using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NSKeyedArchive
{
    internal class BinaryPListReader
    {
        private readonly BinaryReader _reader;
        private readonly int _offsetSize;
        private readonly int _objectRefSize;
        private readonly int _numObjects;
        private readonly int _topObject;
        private readonly long _offsetTableOffset;
        private readonly List<long> _offsetTable;

        private static readonly DateTime Apple2001Reference =
            new(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private BinaryPListReader(
            BinaryReader reader,
            int offsetSize,
            int objectRefSize,
            int numObjects,
            int topObject,
            long offsetTableOffset,
            List<long> offsetTable)
        {
            _reader = reader;
            _offsetSize = offsetSize;
            _objectRefSize = objectRefSize;
            _numObjects = numObjects;
            _topObject = topObject;
            _offsetTableOffset = offsetTableOffset;
            _offsetTable = offsetTable;
        }

        public static BinaryPListReader Create(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            // Verify magic number "bplist00"
            byte[] magic = reader.ReadBytes(8);
            byte[] expectedMagic = Encoding.UTF8.GetBytes("bplist00");

            if (!magic.AsSpan().SequenceEqual(expectedMagic))
            {
                throw new PListFormatException("Not a binary plist file");
            }

            // Read trailer (last 32 bytes)
            stream.Position = stream.Length - 32;
            byte[] trailer = reader.ReadBytes(32);

            // Parse trailer
            byte offsetSize = trailer[6];
            byte objectRefSize = trailer[7];
            int numObjects = BitConverter.ToInt32(trailer.Skip(24).Take(4).Reverse().ToArray());
            int topObject = BitConverter.ToInt32(trailer.Skip(28).Take(4).Reverse().ToArray());
            int offsetTableOffset = BitConverter.ToInt32(trailer.Skip(32 - 8).Take(4).Reverse().ToArray());

            // Read offset table
            stream.Position = offsetTableOffset;
            List<long> offsetTable = new();
            for (int i = 0; i < numObjects; i++)
            {
                long offset = ReadSizedInt(reader, offsetSize);
                offsetTable.Add(offset);
            }

            return new BinaryPListReader(
                reader,
                offsetSize,
                objectRefSize,
                numObjects,
                topObject,
                offsetTableOffset,
                offsetTable);
        }

        public PNode Read()
        {
            return ParseObject(_topObject);
        }

        private PNode ParseObject(int objectIndex)
        {
            // consistency checks
            if (objectIndex >= _numObjects)
                throw new PListFormatException($"Invalid object reference: {objectIndex}");
            if (objectIndex >= _offsetTable.Count || _offsetTable[objectIndex] < 0 || _offsetTable[objectIndex] >= _reader.BaseStream.Length)
                throw new PListFormatException($"Invalid offset for object index {objectIndex}");

            _reader.BaseStream.Position = _offsetTable[objectIndex];
            byte marker = _reader.ReadByte();
            byte objectType = (byte)(marker & 0xF0);
            byte objectInfo = (byte)(marker & 0x0F);

            return objectType switch
            {
                0x00 => ParseSimpleType(objectInfo),
                0x10 => ParseInteger(objectInfo),
                0x20 => ParseReal(objectInfo),
                0x30 => ParseDate(),
                0x40 => ParseData(objectInfo),
                0x50 => ParseAsciiString(objectInfo),
                0x60 => ParseUnicodeString(objectInfo),
                0xA0 => ParseArray(objectInfo),
                0xD0 => ParseDictionary(objectInfo),
                _ => throw new PListFormatException($"Unknown object type: {objectType:X2}")
            };
        }

        private PNode ParseSimpleType(byte objectInfo)
        {
            return objectInfo switch
            {
                0x00 => new PNull(),
                0x08 => new PBoolean { Value = false },
                0x09 => new PBoolean { Value = true },
                _ => throw new PListFormatException($"Unknown simple type: {objectInfo}")
            };
        }

        private PNode ParseInteger(byte objectInfo)
        {
            int intSize = 1 << objectInfo;
            byte[] intBytes = _reader.ReadBytes(intSize).Reverse().ToArray();
            long value = BitConverter.ToInt64(intBytes.PadLeft(8));
            return new PNumber { Value = value };
        }

        private PNode ParseReal(byte objectInfo)
        {
            int realSize = 1 << objectInfo;
            byte[] realBytes = _reader.ReadBytes(realSize).Reverse().ToArray();
            double value = BitConverter.ToDouble(realBytes.PadLeft(8));
            return new PNumber { Value = (decimal)value };
        }

        private PNode ParseDate()
        {
            byte[] dateBytes = _reader.ReadBytes(8).Reverse().ToArray();
            double seconds = BitConverter.ToDouble(dateBytes);
            var date = Apple2001Reference.AddSeconds(seconds);
            return new PDate { Value = date };
        }

        private PNode ParseData(byte objectInfo)
        {
            int count = GetCount(objectInfo);
            byte[] data = _reader.ReadBytes(count);
            return new PData { Value = data };
        }

        private PNode ParseAsciiString(byte objectInfo)
        {
            int count = GetCount(objectInfo);
            byte[] stringBytes = _reader.ReadBytes(count);
            string value = Encoding.ASCII.GetString(stringBytes);
            return new PString { Value = value };
        }

        private PNode ParseUnicodeString(byte objectInfo)
        {
            int count = GetCount(objectInfo);
            byte[] stringBytes = _reader.ReadBytes(count * 2);
            string value = Encoding.BigEndianUnicode.GetString(stringBytes);
            return new PString { Value = value };
        }

        private PNode ParseArray(byte objectInfo)
        {
            int count = GetCount(objectInfo);
            PArray array = new();

            for (int i = 0; i < count; i++)
            {
                int objRef = ReadObjectRef();
                array.Add(ParseObject(objRef));
            }

            return array;
        }

        private PNode ParseDictionary(byte objectInfo)
        {
            int count = GetCount(objectInfo);
            PDictionary dict = new();

            // Read keys
            string[] keys = new string[count];
            for (int i = 0; i < count; i++)
            {
                int keyRef = ReadObjectRef();
                var keyNode = ParseObject(keyRef);
                if (keyNode is not PString keyString)
                {
                    throw new PListFormatException("Dictionary key must be a string");
                }
                keys[i] = keyString.Value;
            }

            // Read values
            for (int i = 0; i < count; i++)
            {
                int valueRef = ReadObjectRef();
                dict.Add(keys[i], ParseObject(valueRef));
            }

            return dict;
        }

        private int GetCount(byte objectInfo)
        {
            if (objectInfo != 0xF)
            {
                return objectInfo;
            }

            byte marker = _reader.ReadByte();
            int intType = (marker & 0xF0) >> 4;
            if (intType != 0x1)
            {
                throw new PListFormatException("Expected int marker");
            }

            int intInfo = marker & 0x0F;
            int intSize = 1 << intInfo;
            byte[] intBytes = _reader.ReadBytes(intSize).Reverse().ToArray();
            return BitConverter.ToInt32(intBytes.PadLeft(4));
        }

        private int ReadObjectRef()
        {
            byte[] refBytes = _reader.ReadBytes(_objectRefSize);
            return BitConverter.ToInt32(refBytes.PadLeft(4));
        }

        private static long ReadSizedInt(BinaryReader reader, int size)
        {
            byte[] intBytes = reader.ReadBytes(size).Reverse().ToArray();
            return BitConverter.ToInt64(intBytes.PadLeft(8));
        }
    }

    internal static class ByteArrayExtensions
    {
        public static byte[] PadLeft(this byte[] bytes, int length, byte padByte = 0)
        {
            if (bytes.Length >= length)
            {
                return bytes;
            }

            byte[] padded = new byte[length];
            Array.Copy(bytes, 0, padded, length - bytes.Length, bytes.Length);
            return padded;
        }
    }
}

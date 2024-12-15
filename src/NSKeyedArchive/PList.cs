using System;
using System.IO;
using System.Text;

namespace NSKeyedArchive
{
    /// <summary>
    /// Represents a property list document and provides methods for reading different plist formats.
    /// </summary>
    public class PList
    {
        /// <summary>
        /// Gets the root node of the property list.
        /// </summary>
        public PNode Root { get; }

        private PList(PNode root)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
        }

        /// <summary>
        /// Creates a new property list from a file path.
        /// </summary>
        /// <param name="path">The path to the property list file.</param>
        /// <returns>A new PList instance.</returns>
        /// <exception cref="PListException">Thrown when the file cannot be read or parsed.</exception>
        public static PList FromFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be null or empty", nameof(path));
            }

            try
            {
                using var stream = File.OpenRead(path);
                return FromStream(stream);
            }
            catch (IOException ex)
            {
                throw new PListException($"Failed to read file: {path}", ex);
            }
        }

        /// <summary>
        /// Creates a new property list from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the property list data.</param>
        /// <returns>A new PList instance.</returns>
        /// <exception cref="PListException">Thrown when the stream cannot be read or parsed.</exception>
        public static PList FromStream(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (!stream.CanRead)
            {
                throw new ArgumentException("Stream must be readable", nameof(stream));
            }

            try
            {
                // Try to detect format
                var format = DetectFormat(stream);

                // Reset stream position after detection
                stream.Position = 0;

                // Parse according to detected format
                var root = format switch
                {
                    PListFormat.Binary => BinaryPListReader.Create(stream).Read(),
                    PListFormat.Xml => XmlPListReader.Create(stream).Read(),
                    _ => throw new PListException("Unknown or unsupported plist format")
                };

                return new PList(root);
            }
            catch (Exception ex) when (ex is not PListException)
            {
                throw new PListException("Failed to parse property list", ex);
            }
        }

        /// <summary>
        /// Creates a new property list from a byte array.
        /// </summary>
        /// <param name="data">The byte array containing the property list data.</param>
        /// <returns>A new PList instance.</returns>
        /// <exception cref="PListException">Thrown when the data cannot be parsed.</exception>
        public static PList FromBytes(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);

            using MemoryStream stream = new MemoryStream(data);
            return FromStream(stream);
        }

        /// <summary>
        /// Creates a new property list from an XML string.
        /// </summary>
        /// <param name="xml">The XML string containing the property list data.</param>
        /// <returns>A new PList instance.</returns>
        /// <exception cref="PListException">Thrown when the XML cannot be parsed.</exception>
        public static PList FromXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentException("XML cannot be null or empty", nameof(xml));
            }

            using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            return FromStream(stream);
        }

        private static PListFormat DetectFormat(Stream stream)
        {
            // Read first few bytes to check format
            byte[] buffer = new byte[8];
            int read = stream.Read(buffer, 0, buffer.Length);

            if (read < 8)
            {
                throw new PListException("Invalid plist: file too short");
            }

            // Check for binary format ("bplist00")
            byte[] expectedMagic = Encoding.UTF8.GetBytes("bplist00");

            if (buffer.AsSpan().SequenceEqual(expectedMagic))
            {
                return PListFormat.Binary;
            }

            // Check for XML format (should start with <?xml or <!DOCTYPE)
            string possibleXml = Encoding.ASCII.GetString(buffer);
            if (possibleXml.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) ||
                possibleXml.StartsWith("<!DOC", StringComparison.OrdinalIgnoreCase))
            {
                return PListFormat.Xml;
            }

            throw new PListException("Unknown or unsupported plist format");
        }

        private enum PListFormat
        {
            Binary,
            Xml
        }
    }
}

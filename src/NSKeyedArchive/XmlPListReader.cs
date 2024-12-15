using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace NSKeyedArchive
{
    internal class XmlPListReader
    {
        private readonly XDocument _document;
        private readonly XNamespace _namespace;

        private XmlPListReader(XDocument document, XNamespace ns)
        {
            _document = document;
            _namespace = ns;
        }

        public static XmlPListReader Create(Stream stream)
        {
            XDocument document;
            try
            {
                // Load and validate the XML document
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Parse,
                    ValidationType = ValidationType.None
                };

                using var reader = XmlReader.Create(stream, settings);
                document = XDocument.Load(reader);
            }
            catch (XmlException ex)
            {
                throw new PListFormatException("Failed to parse XML plist", ex);
            }

            // Verify root element
            var root = document.Root;
            if (root?.Name.LocalName != "plist")
            {
                throw new PListFormatException("Root element must be 'plist'");
            }

            // Get XML namespace if present
            var ns = root.GetDefaultNamespace();

            return new XmlPListReader(document, ns);
        }

        public PNode Read()
        {
            var root = _document.Root;
            if (root == null)
            {
                throw new PListFormatException("Empty document");
            }

            // Get the first child element (should be only one)
            var value = root.Elements().FirstOrDefault();
            if (value == null)
            {
                throw new PListFormatException("Empty plist");
            }

            return ParseNode(value);
        }

        private PNode ParseNode(XElement element)
        {
            return element.Name.LocalName switch
            {
                "dict" => ParseDict(element),
                "array" => ParseArray(element),
                "string" => ParseString(element),
                "integer" => ParseInteger(element),
                "real" => ParseReal(element),
                "true" => new PBoolean { Value = true },
                "false" => new PBoolean { Value = false },
                "date" => ParseDate(element),
                "data" => ParseData(element),
                _ => throw new PListFormatException($"Unknown element type: {element.Name.LocalName}")
            };
        }

        private PNode ParseDict(XElement element)
        {
            var dict = new PDictionary();
            var children = element.Elements().ToList();

            for (int i = 0; i < children.Count; i += 2)
            {
                if (i + 1 >= children.Count)
                {
                    throw new PListFormatException("Dictionary has odd number of elements");
                }

                var keyElement = children[i];
                var valueElement = children[i + 1];

                if (keyElement.Name.LocalName != "key")
                {
                    throw new PListFormatException($"Expected 'key' element, found '{keyElement.Name.LocalName}'");
                }

                var key = keyElement.Value;
                if (string.IsNullOrEmpty(key))
                {
                    throw new PListFormatException("Empty dictionary key");
                }

                var value = ParseNode(valueElement);
                dict.Add(key, value);
            }

            return dict;
        }

        private PNode ParseArray(XElement element)
        {
            var array = new PArray();
            foreach (var child in element.Elements())
            {
                array.Add(ParseNode(child));
            }
            return array;
        }

        private PNode ParseString(XElement element)
        {
            return new PString { Value = element.Value };
        }

        private PNode ParseInteger(XElement element)
        {
            if (!decimal.TryParse(
                element.Value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out decimal value))
            {
                throw new PListFormatException($"Invalid integer value: {element.Value}");
            }

            return new PNumber { Value = value };
        }

        private PNode ParseReal(XElement element)
        {
            if (!decimal.TryParse(
                element.Value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out decimal value))
            {
                throw new PListFormatException($"Invalid real value: {element.Value}");
            }

            return new PNumber { Value = value };
        }

        private PNode ParseDate(XElement element)
        {
            if (!DateTime.TryParse(
                element.Value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out DateTime value))
            {
                throw new PListFormatException($"Invalid date value: {element.Value}");
            }

            return new PDate { Value = value };
        }

        private PNode ParseData(XElement element)
        {
            try
            {
                var value = Convert.FromBase64String(element.Value);
                return new PData { Value = value };
            }
            catch (FormatException ex)
            {
                throw new PListFormatException($"Invalid base64 data: {element.Value}", ex);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NSKeyedArchive
{
    /// <summary>
    /// Provides handlers for specialized NS class types encountered in property lists.
    /// </summary>
    internal static class SpecializedHandlers
    {
        // Dictionary mapping class names to their handlers
        private static readonly Dictionary<string, Func<PDictionary, PNode>> Handlers =
            new(StringComparer.Ordinal)
            {
                ["NSColor"] = HandleNSColor,
                ["NSAttributedString"] = HandleNSAttributedString,
                ["NSMutableAttributedString"] = HandleNSAttributedString,
                ["NSURLRequest"] = HandleNSURLRequest,
                ["NSURL"] = HandleNSURL,
                ["NSValue"] = HandleNSValue,
                ["NSNumber"] = HandleNSNumber,
                ["NSDecimalNumber"] = HandleNSDecimalNumber,
                ["NSTimeZone"] = HandleNSTimeZone,
                ["NSLocale"] = HandleNSLocale,
                ["NSRange"] = HandleNSRange,
                ["NSPoint"] = HandleNSPoint,
                ["NSSize"] = HandleNSSize,
                ["NSRect"] = HandleNSRect
            };

        /// <summary>
        /// Attempts to handle a specialized NS class.
        /// </summary>
        /// <param name="dict">The dictionary containing class data.</param>
        /// <param name="className">The NS class name.</param>
        /// <returns>A handled PNode if successful, null if no handler exists.</returns>
        public static PNode? TryHandle(PDictionary dict, string className)
        {
            if (Handlers.TryGetValue(className, out var handler))
            {
                return handler(dict);
            }
            return null;
        }

        private static PNode HandleNSColor(PDictionary dict)
        {
            // NSColor can be stored in different color spaces
            if (dict.TryGetValue("NSRGB", out var rgbData) && rgbData is PData rgb)
            {
                var bytes = rgb.Value;
                if (bytes.Length >= 3)
                {
                    return new PDictionary
                    {
                        ["Red"] = new PNumber { Value = bytes[0] / 255m },
                        ["Green"] = new PNumber { Value = bytes[1] / 255m },
                        ["Blue"] = new PNumber { Value = bytes[2] / 255m },
                        ["Alpha"] = new PNumber { Value = bytes.Length >= 4 ? bytes[3] / 255m : 1m }
                    };
                }
            }
            return new PNull();
        }

        private static PNode HandleNSAttributedString(PDictionary dict)
        {
            var result = new PDictionary();

            // Get the base string
            if (dict.TryGetValue("NSString", out var str))
            {
                result["string"] = str;
            }

            // Get the attributes
            if (dict.TryGetValue("NSAttributes", out var attrs))
            {
                result["attributes"] = attrs;
            }

            return result;
        }

        private static PNode HandleNSURLRequest(PDictionary dict)
        {
            var result = new PDictionary();

            if (dict.TryGetValue("URL", out var url))
            {
                result["URL"] = url;
            }

            if (dict.TryGetValue("HTTPMethod", out var method))
            {
                result["method"] = method;
            }

            if (dict.TryGetValue("HTTPBody", out var body))
            {
                result["body"] = body;
            }

            return result;
        }

        private static PNode HandleNSURL(PDictionary dict)
        {
            if (dict.TryGetValue("NS.string", out var str) && str is PString urlStr)
            {
                if (dict.TryGetValue("NS.base", out var baseUrl) && baseUrl is PString baseStr)
                {
                    // Handle relative URLs
                    return new PString { Value = new Uri(new Uri(baseStr.Value), urlStr.Value).ToString() };
                }
                return new PString { Value = urlStr.Value };
            }
            return new PNull();
        }

        private static PNode HandleNSValue(PDictionary dict)
        {
            // NSValue is a wrapper for various C structures
            if (dict.TryGetValue("NS.special-type", out var type))
            {
                return type switch
                {
                    PString s when s.Value == "CGPoint" => HandleNSPoint(dict),
                    PString s when s.Value == "CGSize" => HandleNSSize(dict),
                    PString s when s.Value == "CGRect" => HandleNSRect(dict),
                    PString s when s.Value == "_NSRange" => HandleNSRange(dict),
                    _ => new PNull()
                };
            }
            return new PNull();
        }

        private static PNode HandleNSNumber(PDictionary dict)
        {
            if (dict.TryGetValue("NS.number", out var num))
            {
                return num;
            }
            return new PNull();
        }

        private static PNode HandleNSDecimalNumber(PDictionary dict)
        {
            if (dict.TryGetValue("NS.decimal", out var dec) && dec is PString decStr)
            {
                if (decimal.TryParse(decStr.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                {
                    return new PNumber { Value = value };
                }
            }
            return new PNull();
        }

        private static PNode HandleNSTimeZone(PDictionary dict)
        {
            if (dict.TryGetValue("NS.name", out var name))
            {
                return name;
            }
            return new PNull();
        }

        private static PNode HandleNSLocale(PDictionary dict)
        {
            if (dict.TryGetValue("NS.identifier", out var id))
            {
                return id;
            }
            return new PNull();
        }

        private static PNode HandleNSRange(PDictionary dict)
        {
            var range = new PDictionary();

            if (dict.TryGetValue("NS.location", out var location))
            {
                range["location"] = location;
            }

            if (dict.TryGetValue("NS.length", out var length))
            {
                range["length"] = length;
            }

            return range;
        }

        private static PNode HandleNSPoint(PDictionary dict)
        {
            var point = new PDictionary();

            if (dict.TryGetValue("NS.x", out var x))
            {
                point["x"] = x;
            }

            if (dict.TryGetValue("NS.y", out var y))
            {
                point["y"] = y;
            }

            return point;
        }

        private static PNode HandleNSSize(PDictionary dict)
        {
            var size = new PDictionary();

            if (dict.TryGetValue("NS.width", out var width))
            {
                size["width"] = width;
            }

            if (dict.TryGetValue("NS.height", out var height))
            {
                size["height"] = height;
            }

            return size;
        }

        private static PNode HandleNSRect(PDictionary dict)
        {
            var rect = new PDictionary();

            if (dict.TryGetValue("NS.x", out var x) &&
                dict.TryGetValue("NS.y", out var y))
            {
                rect["origin"] = new PDictionary
                {
                    ["x"] = x,
                    ["y"] = y
                };
            }

            if (dict.TryGetValue("NS.width", out var width) &&
                dict.TryGetValue("NS.height", out var height))
            {
                rect["size"] = new PDictionary
                {
                    ["width"] = width,
                    ["height"] = height
                };
            }

            return rect;
        }
    }
}

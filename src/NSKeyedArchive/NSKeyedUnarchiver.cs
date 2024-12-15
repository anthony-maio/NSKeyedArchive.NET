
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Text;

namespace NSKeyedArchive
{
    /// <summary>
    /// NSKeyedUnarchiver is a decoder that restores objects and data from an NSKeyedArchive.
    /// This class provides mechanisms to interpret a binary or XML property list (plist) file
    /// encoded using the NSKeyedArchiver format, commonly used in iOS/macOS applications.
    /// </summary>
    /// <remarks>
    /// - Handles recursive structures and object references.
    /// - Supports decoding various NS types such as arrays, dictionaries, strings, dates, and more.
    /// - Allows removal of class name metadata for output customization.
    /// </remarks>
    public class NSKeyedUnarchiver : IDisposable
    {
        /// <summary>
        /// Caches decoded objects by their unique IDs to manage object references and prevent duplication.
        /// </summary>
        private readonly Dictionary<int, PNode> _objectCache = [];

        /// <summary>
        /// Tracks the current processing stack to detect recursive references.
        /// </summary>
        private readonly Stack<int> _processingStack = new();

        /// <summary>
        /// Stores the top-level dictionary of the archive being decoded.
        /// </summary>
        private readonly PDictionary _archive;

        /// <summary>
        /// Contains the list of objects in the archive.
        /// </summary>
        private readonly PArray _objects;

        /// <summary>
        /// Indicates whether to strip class name metadata from the output.
        /// </summary>
        private readonly bool _removeClassNames;

        /// <summary>
        /// Initializes a new instance of the NSKeyedUnarchiver class.
        /// </summary>
        /// <param name="plist">The property list to unarchive.</param>
        /// <param name="removeClassNames">Whether to remove class name information from the output.</param>
        /// <exception cref="ArgumentNullException">If plist is null.</exception>
        /// <exception cref="PListException">If the plist is not a valid NSKeyedArchiver archive.</exception>
        public NSKeyedUnarchiver(PList plist, bool removeClassNames = true)
        {
            ArgumentNullException.ThrowIfNull(plist);

            if (plist.Root is not PDictionary root)
                throw new PListException("Root must be a dictionary");

            _removeClassNames = removeClassNames;

            // Verify this is an NSKeyedArchiver plist
            if (!root.TryGetValue("$archiver", out var archiver) ||
                archiver is not PString archiverStr ||
                archiverStr.Value != "NSKeyedArchiver")
            {
                throw new PListException("Not an NSKeyedArchiver plist");
            }

            // Get the objects array
            if (!root.TryGetValue("$objects", out var objects) ||
                objects is not PArray objectsArray)
            {
                throw new PListException("Missing or invalid $objects array");
            }
            _objects = objectsArray;

            // Get the top object
            if (!root.TryGetValue("$top", out var top) ||
                top is not PDictionary topDict)
            {
                throw new PListException("Missing or invalid $top dictionary");
            }
            _archive = topDict;
        }


        /// <summary>
        /// Decodes the property list into its original object structure.
        /// </summary>
        /// <returns>The root object of the unarchived data.</returns>
        public PNode Unarchive()
        {
            try
            {
                if (_archive.Count == 1 && _archive.ContainsKey("root"))
                {
                    return UnarchiveObject(_archive["root"]);
                }

                PDictionary result = [];
                foreach (var kvp in _archive)
                {
                    result.Add(kvp.Key, UnarchiveObject(kvp.Value));
                }
                return result;
            }
            finally
            {
                _objectCache.Clear();
                _processingStack.Clear();
            }
        }

        private void ValidateArchive(PDictionary root)
        {
            if (!root.TryGetValue("$archiver", out var archiver) ||
                archiver is not PString archiverStr ||
                archiverStr.Value != "NSKeyedArchiver")
            {
                throw new PListException("Not an NSKeyedArchiver plist");
            }

            if (!root.ContainsKey("$objects") || !root.ContainsKey("$top"))
            {
                throw new PListException("Missing required archive keys");
            }
        }

        /// <summary>
        /// Recursively decodes a node, resolving references and handling object types.
        /// </summary>
        /// <param name="node">The node to decode.</param>
        /// <returns>The decoded PNode.</returns>
        /// <exception cref="PListException">Thrown for malformed or unsupported nodes.</exception>
        private PNode UnarchiveObject(PNode node)
        {
            // Handle UID references
            if (IsUID(node, out int index))
            {
                if (_processingStack.Count > 100)
                {
                    // Create a placeholder for the truncated object to maintain hierarchy
                    var truncatedNode = new PDictionary
                    {
                        ["error"] = new PString { Value = "Recursion limit exceeded" },
                        ["partial"] = _objectCache.ContainsKey(index) ? _objectCache[index] : new PNull()
                    };

                    throw new NSArchiveRecursionException(_processingStack.Count, index.ToString(), truncatedNode);
                }

                if (_processingStack.Contains(index))
                    return new PString { Value = "$ref" + index };

                if (_objectCache.TryGetValue(index, out var cached))
                    return cached;

                _processingStack.Push(index);
                try
                {
                    var resolved = UnarchiveObject(_objects[index]);
                    _objectCache[index] = resolved;
                    return resolved;
                }
                finally
                {
                    _processingStack.Pop();
                }
            }

            // Handle special cases
            if (node is PString str && str.Value == "$null")
                return new PNull();

            // Handle containers
            return node switch
            {
                PDictionary dict => UnarchiveDictionary(dict),
                PArray array => UnarchiveArray(array),
                _ => node
            };
        }

        /// <summary>
        /// Decodes a dictionary node, resolving object references and class-specific structures.
        /// </summary>
        /// <param name="dict">The dictionary to decode.</param>
        /// <returns>The decoded PDictionary.</returns>
        private PNode UnarchiveDictionary(PDictionary dict)
        {
            // Handle class instances
            if (dict.TryGetValue("$class", out var classRef))
            {
                PDictionary? classDict = GetReferencedObject(classRef) as PDictionary;
                PString? className = (classDict?["$classes"] as PArray)?[0] as PString;

                if (className != null)
                {
                    // Handle specific class types
                    var result = HandleSpecialClass(dict, className.Value);
                    if (result != null)
                    {
                        return result;
                    }

                    // For other classes, keep the dictionary structure
                    if (_removeClassNames)
                    {
                        dict = new PDictionary(dict);
                        dict.Remove("$class");
                    }
                }
            }
            else
            {
                throw new NSArchiveMalformedNodeException("Missing $class reference in dictionary.", "$class", dict);
            }

            // Process regular dictionary
            PDictionary resultDict = [];
            foreach (var kvp in dict)
            {
                resultDict.Add(kvp.Key, UnarchiveObject(kvp.Value));
            }
            return resultDict;
        }

        private PNode HandleSpecialClass(PDictionary dict, string className)
        {
            // Try basic types first
            var basicHandler = className switch
            {
                "NSArray" or "NSMutableArray" => UnarchiveNSArray(dict),
                "NSDictionary" or "NSMutableDictionary" => UnarchiveNSDictionary(dict),
                "NSString" or "NSMutableString" => UnarchiveNSString(dict),
                "NSDate" => UnarchiveNSDate(dict),
                "NSData" or "NSMutableData" => UnarchiveNSData(dict),
                "NSSet" or "NSMutableSet" => UnarchiveNSSet(dict),
                _ => null
            };

            if (basicHandler != null)
            {
                return basicHandler;
            }

            // Try specialized handlers
            return SpecializedHandlers.TryHandle(dict, className) ?? dict;
        }

        private PNode UnarchiveNSArray(PDictionary dict)
        {
            PArray array = [];
            var objects = UnarchiveObject(dict["NS.objects"]);
            if (objects is PArray objArray)
            {
                foreach (var item in objArray)
                {
                    array.Add(item);
                }
            }
            return array;
        }

        private PNode UnarchiveNSDictionary(PDictionary dict)
        {
            PDictionary result = [];
            PArray? keys = UnarchiveObject(dict["NS.keys"]) as PArray;
            PArray? values = UnarchiveObject(dict["NS.objects"]) as PArray;

            if (keys != null && values != null)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    if (keys[i] is PString key)
                    {
                        result.Add(key.Value, values[i]);
                    }
                }
            }
            return result;
        }

        private PNode UnarchiveNSString(PDictionary dict)
        {
            return new PString { Value = (dict["NS.string"] as PString)?.Value ?? "" };
        }

        private PNode UnarchiveNSDate(PDictionary dict)
        {
            if (dict["NS.time"] is PNumber time)
            {
                var date = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddSeconds((double)time.Value);
                return new PDate { Value = date };
            }
            return new PNull();
        }

        private PNode UnarchiveNSData(PDictionary dict)
        {
            return dict["NS.data"] as PData ?? new PData { Value = [] };
        }

        private PNode UnarchiveNSSet(PDictionary dict)
        {
            // Convert NSSet to array for simplicity
            return UnarchiveNSArray(dict);
        }

        private PNode UnarchiveArray(PArray array)
        {
            PArray result = [];
            foreach (var item in array)
            {
                result.Add(UnarchiveObject(item));
            }
            return result;
        }

        private bool IsUID(PNode node, out int index)
        {
            index = -1;
            if (node is PDictionary dict &&
                dict.Count == 1 &&
                dict.TryGetValue("CF$UID", out var uid) &&
                uid is PNumber num)
            {
                index = (int)num.Value;
                return true;
            }
            return false;
        }

        private PNode GetReferencedObject(PNode reference)
        {
            return IsUID(reference, out int index) ? _objects[index] : reference;
        }

        /// <summary>
        /// Releases all resources used by the NSKeyedUnarchiver.
        /// </summary>
        public void Dispose()
        {
            _objectCache.Clear();
            _processingStack.Clear();
            GC.SuppressFinalize(this) ;
        }
    }
}

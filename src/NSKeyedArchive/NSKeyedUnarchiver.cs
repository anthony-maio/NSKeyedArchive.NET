/*
 * NSKeyedArchive Unarchiver
 * Version: 2.1
 * Date: December 7, 2024
 * 
 * Changes from v2.0:
 * - Added support for recursive structure handling
 * - Enhanced input flexibility (string/bytes/file)
 * - Added direct NSKeyedArchiver reference handling
 * - Improved memory efficiency
 * - Added support for more NS types
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NSKeyedArchive
{
    public class NSKeyedUnarchiver : IDisposable
    {
        private readonly Dictionary<int, PNode> _objectCache = new();
        private readonly Stack<int> _processingStack = new();
        private readonly PDictionary _archive;
        private readonly PArray _objects;
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
            if (plist == null)
                throw new ArgumentNullException(nameof(plist));

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
        /// Unarchives the property list to a regular format.
        /// </summary>
        /// <returns>The unarchived root object.</returns>
        public PNode Unarchive()
        {
            try
            {
                if (_archive.Count == 1 && _archive.ContainsKey("root"))
                {
                    return UnarchiveObject(_archive["root"]);
                }

                var result = new PDictionary();
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

        private PNode UnarchiveObject(PNode node)
        {
            // Handle UID references
            if (IsUID(node, out int index))
            {
                // Check for recursive references
                if (_processingStack.Contains(index))
                {
                    return new PString { Value = $"$ref{index}" };
                }

                // Check cache
                if (_objectCache.TryGetValue(index, out var cached))
                {
                    return cached;
                }

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
            {
                return new PNull();
            }

            // Handle containers
            return node switch
            {
                PDictionary dict => UnarchiveDictionary(dict),
                PArray array => UnarchiveArray(array),
                _ => node
            };
        }

        private PNode UnarchiveDictionary(PDictionary dict)
        {
            // Handle class instances
            if (dict.TryGetValue("$class", out var classRef))
            {
                var classDict = GetReferencedObject(classRef) as PDictionary;
                var className = (classDict?["$classes"] as PArray)?[0] as PString;

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

            // Process regular dictionary
            var resultDict = new PDictionary();
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
            var array = new PArray();
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
            var result = new PDictionary();
            var keys = UnarchiveObject(dict["NS.keys"]) as PArray;
            var values = UnarchiveObject(dict["NS.objects"]) as PArray;

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
            return dict["NS.data"] as PData ?? new PData { Value = Array.Empty<byte>() };
        }

        private PNode UnarchiveNSSet(PDictionary dict)
        {
            // Convert NSSet to array for simplicity
            return UnarchiveNSArray(dict);
        }

        private PNode UnarchiveArray(PArray array)
        {
            var result = new PArray();
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

        public void Dispose()
        {
            _objectCache.Clear();
            _processingStack.Clear();
            GC.SuppressFinalize(this) ;
        }
    }
}

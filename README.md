# NSKeyedArchive.NET

A .NET library for working with Apple NSKeyedArchive property lists. This library provides cross-platform support for reading both binary and XML property lists, with special handling for NSKeyedArchiver format.

## Features

- Read binary and XML property lists
- Convert NSKeyedArchive plists to regular property lists
- Strong typing with full type safety
- Cross-platform compatibility
- Easy-to-use API

## Installation

Install via NuGet:

```bash
dotnet add package NSKeyedArchive
```

## Usage

### Basic Usage

```csharp
using NSKeyedArchive;

// Read a property list file
var plist = PList.FromFile("archive.plist");

// Unarchive NSKeyedArchiver format
var unarchiver = new NSKeyedUnarchiver(plist);
var root = unarchiver.Unarchive();

// Work with the unarchived data
if (root is PDictionary dict)
{
    foreach (var kvp in dict)
    {
        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
    }
}

// Access specific values
if (dict.TryGetValue("name", out var nameNode) && nameNode is PString nameStr)
{
    string name = nameStr.Value;
}

// Work with arrays
if (dict.TryGetValue("items", out var itemsNode) && itemsNode is PArray items)
{
    foreach (var item in items)
    {
        // Process each item
    }
}
```

### Stream Support

```csharp
using (var stream = File.OpenRead("archive.plist"))
{
    var plist = PList.FromStream(stream);
    // Process the plist...
}
```

### Working with Different Node Types

```csharp
// Strings
if (node is PString str)
{
    string value = str.Value;
}

// Numbers
if (node is PNumber num)
{
    decimal value = num.Value;
    // or use specific types
    int intValue = num.GetValue<int>();
    double doubleValue = num.GetValue<double>();
}

// Dates
if (node is PDate date)
{
    DateTime value = date.Value;
}

// Binary Data
if (node is PData data)
{
    byte[] value = data.Value;
}

// Arrays
if (node is PArray array)
{
    foreach (var item in array)
    {
        // Process each item
    }
}

// Dictionaries
if (node is PDictionary dict)
{
    foreach (var kvp in dict)
    {
        string key = kvp.Key;
        PNode value = kvp.Value;
    }
}
```

### Error Handling

```csharp
try
{
    var plist = PList.FromFile("archive.plist");
    var unarchiver = new NSKeyedUnarchiver(plist);
    var root = unarchiver.Unarchive();
}
catch (PListException ex)
{
    // Handle property list specific errors
    Console.WriteLine($"PList error: {ex.Message}");
}
catch (IOException ex)
{
    // Handle file system errors
    Console.WriteLine($"File error: {ex.Message}");
}
```

## Type Safety

The library provides strong typing through the `PNode` hierarchy:

- `PString`: String values
- `PNumber`: Numeric values (supports int, long, float, double, decimal)
- `PBoolean`: Boolean values
- `PDate`: DateTime values
- `PData`: Binary data
- `PArray`: Lists of nodes
- `PDictionary`: Key-value pairs
- `PNull`: Null values

Each type provides type-safe access to its values and appropriate conversion methods.

## Performance Considerations

- The library reads the entire file into memory
- For large files, consider using streams
- Binary plists are parsed lazily when possible
- Consider memory usage when working with large property lists

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

MIT License - see LICENSE file for details

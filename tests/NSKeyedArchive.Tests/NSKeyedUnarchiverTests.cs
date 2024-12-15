using System;
using System.Text;
using NSKeyedArchive;

namespace NSKeyedArchive.Tests
{
    public class NSKeyedUnarchiverTests
    {
        [Fact]
        public void Unarchive_SimpleString_ReturnsCorrectValue()
        {
            // This XML represents a simple archived NSString
            var xmlPlist = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
    <key>$archiver</key>
    <string>NSKeyedArchiver</string>
    <key>$objects</key>
    <array>
        <string>$null</string>
        <dict>
            <key>$class</key>
            <dict>
                <key>$classes</key>
                <array>
                    <string>NSString</string>
                    <string>NSObject</string>
                </array>
                <key>$classname</key>
                <string>NSString</string>
            </dict>
            <key>NS.string</key>
            <string>Hello World</string>
        </dict>
    </array>
    <key>$top</key>
    <dict>
        <key>root</key>
        <dict>
            <key>CF$UID</key>
            <integer>1</integer>
        </dict>
    </dict>
    <key>$version</key>
    <integer>100000</integer>
</dict>
</plist>";

            // Act
            var plist = PList.FromXml(xmlPlist);
            var unarchiver = new NSKeyedUnarchiver(plist);
            var result = unarchiver.Unarchive();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PString>(result);
            Assert.Equal("Hello World", ((PString)result).Value);
        }

        [Fact]
        public void Unarchive_SimpleArray_ReturnsCorrectValues()
        {
            // This XML represents an archived NSArray with two strings
            var xmlPlist = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
    <key>$archiver</key>
    <string>NSKeyedArchiver</string>
    <key>$objects</key>
    <array>
        <string>$null</string>
        <dict>
            <key>$class</key>
            <dict>
                <key>$classes</key>
                <array>
                    <string>NSArray</string>
                    <string>NSObject</string>
                </array>
                <key>$classname</key>
                <string>NSArray</string>
            </dict>
            <key>NS.objects</key>
            <array>
                <dict>
                    <key>CF$UID</key>
                    <integer>2</integer>
                </dict>
                <dict>
                    <key>CF$UID</key>
                    <integer>3</integer>
                </dict>
            </array>
        </dict>
        <string>First</string>
        <string>Second</string>
    </array>
    <key>$top</key>
    <dict>
        <key>root</key>
        <dict>
            <key>CF$UID</key>
            <integer>1</integer>
        </dict>
    </dict>
    <key>$version</key>
    <integer>100000</integer>
</dict>
</plist>";

            // Act
            var plist = PList.FromXml(xmlPlist);
            var unarchiver = new NSKeyedUnarchiver(plist);
            var result = unarchiver.Unarchive();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PArray>(result);

            var array = (PArray)result;
            Assert.Equal(2, array.Count);
            Assert.Equal("First", ((PString)array[0]).Value);
            Assert.Equal("Second", ((PString)array[1]).Value);
        }

        [Fact]
        public void Unarchive_SimpleDictionary_ReturnsCorrectValues()
        {
            // This XML represents an archived NSDictionary with two key-value pairs
            var xmlPlist = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
    <key>$archiver</key>
    <string>NSKeyedArchiver</string>
    <key>$objects</key>
    <array>
        <string>$null</string>
        <dict>
            <key>$class</key>
            <dict>
                <key>$classes</key>
                <array>
                    <string>NSDictionary</string>
                    <string>NSObject</string>
                </array>
                <key>$classname</key>
                <string>NSDictionary</string>
            </dict>
            <key>NS.keys</key>
            <array>
                <dict>
                    <key>CF$UID</key>
                    <integer>2</integer>
                </dict>
                <dict>
                    <key>CF$UID</key>
                    <integer>3</integer>
                </dict>
            </array>
            <key>NS.objects</key>
            <array>
                <dict>
                    <key>CF$UID</key>
                    <integer>4</integer>
                </dict>
                <dict>
                    <key>CF$UID</key>
                    <integer>5</integer>
                </dict>
            </array>
        </dict>
        <string>name</string>
        <string>age</string>
        <string>John</string>
        <integer>42</integer>
    </array>
    <key>$top</key>
    <dict>
        <key>root</key>
        <dict>
            <key>CF$UID</key>
            <integer>1</integer>
        </dict>
    </dict>
    <key>$version</key>
    <integer>100000</integer>
</dict>
</plist>";

            // Act
            var plist = PList.FromXml(xmlPlist);
            var unarchiver = new NSKeyedUnarchiver(plist);
            var result = unarchiver.Unarchive();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PDictionary>(result);

            var dict = (PDictionary)result;
            Assert.Equal(2, dict.Count);
            Assert.Equal("John", ((PString)dict["name"]).Value);
            Assert.Equal(42m, ((PNumber)dict["age"]).Value);
        }

        [Fact]
        public void Create_InvalidXml_ThrowsPListException()
        {
            var invalidXml = "<not-a-plist></not-a-plist>";

            // Assert that creating a PList from invalid XML throws the correct exception
            Assert.Throws<PListException>(() => PList.FromXml(invalidXml));
        }

        [Fact]
        public void Create_NonArchiverPlist_ThrowsPListException()
        {
            // This is a valid plist but not an NSKeyedArchiver plist
            var xmlPlist = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
    <key>simple</key>
    <string>value</string>
</dict>
</plist>";

            var plist = PList.FromXml(xmlPlist);

            // Assert that creating an unarchiver from a non-NSKeyedArchiver plist throws
            Assert.Throws<PListException>(() => new NSKeyedUnarchiver(plist));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Tests.Unit
{
    public class DirectoryPathTests
    {
        [Theory]
        [InlineData("relative/path", "/relative/path/")]
        [InlineData("/already/absolute", "/already/absolute/")]
        [InlineData("/already/with/slash/", "/already/with/slash/")]
        [InlineData("/weird/scene/path/2½", "/weird/scene/path/2½/")]
        [InlineData("/", "/")]
        public void DirectoryPath_ToString_Returns_ExpectedPath(string path1, string expectedPath)
        {
            // Act
            var directoryPath = new DirectoryPath(path1);

            // Assert
            directoryPath.ToString().Should().Be(expectedPath);
        }

        [Theory]
        [InlineData("/", "/")]
        [InlineData("/valid/path", "path")]        
        public void DirectoryPath_Has_ExpectedName(string path, string name)
        {
            //Arrange
            var dirPath = new DirectoryPath(path);

            //Assert
            dirPath.DirectoryName.Should().Be(name);
        }

        [Theory]
        [InlineData("\\invalid\\directory\\path")]
        [InlineData("C:\\invalid\\directory\\path")]
        public void When_PathInvalid_ShouldThrowException(string path)
        {
            // Arrange
            Action act = () => new DirectoryPath(path);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Must be valid Unix-style path*")
                .And.ParamName.Should().Be("directoryPath");
        }

        [Fact]
        public void DirectoryPath_RootPath_ShouldBeRootPath()
        {
            // Arrange
            var expectedPath = "/";

            // Act
            var directoryPath = new DirectoryPath(expectedPath);

            // Assert
            directoryPath.Value.Should().Be(expectedPath);
        }

        [Theory]
        [InlineData("valid/path", "valid/path")]
        [InlineData("/another/valid/path", "another/valid/path/")]
        [InlineData("another/valid/path", "/another/valid/path/")]
        [InlineData("/", "/")]
        [InlineData("", "")]
        public void DirectoryPaths_ShouldBeEqual_WithSimilarPaths(string path1, string path2)
        {
            // Arrange
            var directoryPath1 = new DirectoryPath(path1);
            var directoryPath2 = new DirectoryPath(path2);

            // Act & Assert
            directoryPath1.Equals(directoryPath2).Should().BeTrue();
        }

        [Theory]
        [InlineData("/path1", "/path2", "/path1/path2/")]
        [InlineData("/path1/path2", "/path3", "/path1/path2/path3/")]
        [InlineData("/path1/path2/", "/path3/", "/path1/path2/path3/")]
        [InlineData("/", "/path3/", "/path3/")]
        [InlineData("", "/path2/", "/path2/")]
        public void DirectoryPaths_ShouldConcatenate_WithTwoPaths(string path1, string path2, string expectedResult)
        {

            var directoryPath1 = new DirectoryPath(path1);
            var directoryPath2 = new DirectoryPath(path2);
            var directoryPath3 = new DirectoryPath(expectedResult);


            var combinedPath = directoryPath1.Combine(directoryPath2);


            combinedPath.ToString().Should().Be(expectedResult);
            combinedPath.Equals(directoryPath3).Should().BeTrue();
        }

        [Theory]
        [InlineData("/path1", "/path2", "/path3", "/path1/path2/path3/")]
        [InlineData("/path1/path2", "/path3", "/path4", "/path1/path2/path3/path4/")]
        [InlineData("/path1/path2/", "/path3/", "/path4", "/path1/path2/path3/path4/")]
        [InlineData("/", "/path3/", "/path4/", "/path3/path4/")]
        [InlineData("", "/path2/", "/path3/", "/path2/path3/")]
        public void DirectoryPaths_ShouldConcatenate_WithMoreThanTwoPaths(string path1, string path2, string path3, string expectedResult)
        {
            // Arrange
            var directoryPath1 = new DirectoryPath(path1);
            var directoryPath2 = new DirectoryPath(path2);
            var directoryPath3 = new DirectoryPath(path3);
            var directoryPathExpected = new DirectoryPath(expectedResult);

            // Act
            var combinedPath = directoryPath1.Combine(directoryPath2, directoryPath3);

            // Assert
            combinedPath.ToString().Should().Be(expectedResult);
            combinedPath.Equals(directoryPathExpected).Should().BeTrue();
        }

        [Theory]
        [InlineData("/path1", "/path2/test.txt", "/path1/path2/test.txt")]
        [InlineData("/", "/another/file.txt", "/another/file.txt")]
        [InlineData("", "file.txt", "/file.txt")]
        public void DirectoryPath_CombinedWithFilePath_ShouldReturnCombinedFilePath(string directoryPathString, string filePathString, string expectedPath)
        {
            // Arrange
            var dirPath = new DirectoryPath(directoryPathString);
            var filePath = new FilePath(filePathString);
            var expectedCombinedPath = new FilePath(expectedPath);

            // Act
            var combinedFilePath = dirPath.Combine(filePath);

            // Assert
            combinedFilePath.ToString().Should().Be(expectedCombinedPath.ToString());
            combinedFilePath.Equals(expectedCombinedPath).Should().BeTrue();
        }

        [Theory]
        [InlineData("/valid/path", "/valid/path/")]
        [InlineData("another/valid/path/", "/another/valid/path/")]
        [InlineData("valid/path/", "/valid/path/")]
        [InlineData("/", "/")]
        [InlineData("", "")]
        public void DirectoryPath_ToString_ShouldReturnCorrectStringRepresentation(string path, string expectedPath)
        {
            // Arrange
            var directoryPath = new DirectoryPath(path);

            // Act
            var result = directoryPath.ToString();

            // Assert
            result.Should().Be(expectedPath);
        }

        [Fact]
        public void DirectoryPath_ShouldReturnIntHashCode_OfInternalStringValue()
        {
            // Arrange
            var directoryPath = new DirectoryPath("/valid/path/");
            var expectedHashCode = directoryPath.Value.GetHashCode();

            // Act            
            var hashCode = directoryPath.GetHashCode();

            // Assert
            hashCode.Should().Be(expectedHashCode);
        }

        [Fact]
        public void DirectoryPath_Parent_ShouldBeCorrectlyCalculated()
        {
            // Arrange
            var directoryPath = new DirectoryPath("/valid/path/to/directory/");
            var expectedParentPath = new DirectoryPath("/valid/path/to/");

            // Act
            var parentPath = directoryPath.ParentPath;

            // Assert
            parentPath.Equals(expectedParentPath).Should().BeTrue();
        }

        [Fact]
        public void DirectoryPath_Parent_ShouldBeNull_WhenPathIsRoot()
        {
            // Arrange
            var directoryPath = new DirectoryPath("/");

            // Assert
            directoryPath.ParentPath.Should().BeNull();
        }

        [Fact]
        public void DirectoryPath_IsRoot_ShouldBeTrue_WhenValueIsRoot() 
        {
            // Act
            var directoryPath = new DirectoryPath("/");

            // Assert
            directoryPath.IsRoot.Should().BeTrue();
        }

        [Fact]
        public void DirectoryPath_Contains_ShouldReturnTrue_WhenPathIsContained()
        {
            // Arrange
            var path1 = new DirectoryPath("/parent/");
            var path2 = new DirectoryPath("/parent/child/");
            
            // Act
            var contains = path2.Contains(path1);
            
            // Assert
            contains.Should().BeTrue();
        }

        [Fact]
        public void DirectoryPath_Contains_ShouldReturnFalse_WhenPathIsNotContained()
        {
            // Arrange
            var path1 = new DirectoryPath("/parent/");
            var path2 = new DirectoryPath("/other/child/");
            
            // Act
            var contains = path2.Contains(path1);
            
            // Assert
            contains.Should().BeFalse();
        }

        [Theory]
        [InlineData("/parent/child/", "/parent/", true)]
        [InlineData("/parent/child/", "/other/", false)]
        [InlineData("/", "/", true)]
        public void DirectoryPath_Contains_ShouldResultTrue_WhenStringCompared(string path1, string path2, bool contained) 
        {
            // Arrange
            var directoryPath = new DirectoryPath(path1);
            
            // Act
            var contains = directoryPath.Contains(path2);
            
            // Assert
            contains.Should().Be(contained);
        }

        [Theory]        
        [InlineData("/", "/")]
        [InlineData("", "")]
        [InlineData("/parent/child/", "child")]
        [InlineData("/parent/child", "child")]
        [InlineData("parent/child/", "child")]
        [InlineData("child", "child")]
        public void DirectoryPath_DirectoryName_ShouldReturnCorrectDirectoryName(string path, string expectedDirectoryName)
        {
            // Arrange
            var directoryPath = new DirectoryPath(path);

            // Act
            var directoryName = directoryPath.DirectoryName;

            // Assert
            directoryName.Should().Be(expectedDirectoryName);
        }

        [Theory]
        [InlineData("/path1/path2/", "Path2")]
        [InlineData("path1/path2/", "Path2")]
        [InlineData("", "")]
        public void DirectoryPath_ShouldHave_ExpectedDirectoryTitle(string fileName, string expectedValue)
        {
            //Arrange
            var filePath = new DirectoryPath(fileName);

            //Assert
            filePath.DirectoryTitle.Should().Be(expectedValue);
        }
    }
}

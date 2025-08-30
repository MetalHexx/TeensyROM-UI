using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Tests.Unit
{
    public class FilePathTests
    {
        [Theory]
        [InlineData("/valid/file/path", "/valid/file/path")]
        [InlineData("valid/directory/path/file.txt", "/valid/directory/path/file.txt")]
        [InlineData("/valid/directory/path/file.txt", "/valid/directory/path/file.txt")]
        [InlineData("file.txt", "/file.txt")]
        [InlineData("/games/4M Arena (Darro99 & Erazzking - 2021).crt", "/games/4M Arena (Darro99 & Erazzking - 2021).crt")]
        [InlineData("/games/Extras/OfficialCRTs/C64-GS(Fiendish Freddy's Big Top o' Fun, Flimbo’s Quest, Klax & International Soccer).crt", "/games/Extras/OfficialCRTs/C64-GS(Fiendish Freddy's Big Top o' Fun, Flimbo’s Quest, Klax & International Soccer).crt")]
        [InlineData("/games/Extras/OtherCRTs/Arcade Compilation #1 [EasyFlash].crt", "/games/Extras/OtherCRTs/Arcade Compilation #1 [EasyFlash].crt")]
        [InlineData("", "")]
        public void FilePath_ToString_Returns_ExpectedPath(string path1, string expectedPath)
        {
            // Act
            var filePath = new FilePath(path1);

            // Assert
            filePath.ToString().Should().Be(expectedPath);
        }

        [Theory]        
        [InlineData("\\invalid\\file\\path")]
        [InlineData("C:\\invalid\\file\\path")]
        public void When_PathInvalid_ShouldThrowException(string path)
        {
            // Arrange
            Action act = () => new FilePath(path);

            // Assert
            act.Should().Throw<ArgumentException>()
                .And.ParamName.Should().Be("filePath");
        }

        [Theory]
        [InlineData("valid/path/file.txt", "valid/path/file.txt")]
        [InlineData("/valid/path/file.txt", "/valid/path/file.txt")]
        [InlineData("valid/path/file.txt", "/valid/path/file.txt")]
        [InlineData("", "")]
        public void FilePath_ShouldBeEqual_WithSimilarPaths(string path1, string path2)
        {
            // Arrange
            var directoryPath1 = new DirectoryPath(path1);
            var directoryPath2 = new DirectoryPath(path2);

            // Act & Assert
            directoryPath1.Equals(directoryPath2).Should().BeTrue();
        }

        [Theory]
        [InlineData("/path1", "/path2", "/path1/path2/file.txt")]
        [InlineData("/path1/path2", "/path3", "/path1/path2/path3/file.txt")]
        [InlineData("/path1/path2/", "/path3/", "/path1/path2/path3/file.txt")]
        [InlineData("/", "/path3/", "/path3/file.txt")]
        [InlineData("", "/path2/", "/path2/file.txt")]
        public void FilePath_ShouldConcatenate_WithTwoPaths(string path1, string path2, string expectedString)
        {
            var filePath1 = new FilePath("/file.txt");
            var directoryPath1 = new DirectoryPath(path1);
            var directoryPath2 = new DirectoryPath(path2);
            var expectedPath = new FilePath(expectedString);

            var combinedPath = filePath1.Combine(directoryPath1, directoryPath2);

            combinedPath.ToString().Should().Be(expectedString);
            combinedPath.Equals(expectedPath).Should().BeTrue();
        }

        [Theory]
        [InlineData("/path1", "/path2", "/path3", "/path1/path2/path3/file.txt")]
        [InlineData("/path1/path2", "/path3", "/path4", "/path1/path2/path3/path4/file.txt")]
        [InlineData("/path1/path2/", "/path3/", "/path4", "/path1/path2/path3/path4/file.txt")]
        [InlineData("/", "/path3/", "/path4/", "/path3/path4/file.txt")]
        [InlineData("", "/path2/", "/path3/", "/path2/path3/file.txt")]
        public void FilePath_ShouldConcatenate_WithMoreThanTwoPaths(string path1, string path2, string path3, string expectedFilePathString)
        {
            // Arrange
            var filePath1 = new FilePath("/file.txt");
            var directoryPath1 = new DirectoryPath(path1);
            var directoryPath2 = new DirectoryPath(path2);
            var directoryPath3 = new DirectoryPath(path3);
            var expectedFilePath = new FilePath(expectedFilePathString);

            // Act
            var combinedPath = filePath1.Combine(directoryPath1, directoryPath2, directoryPath3);

            // Assert
            combinedPath.ToString().Should().Be(expectedFilePathString);
            combinedPath.Equals(expectedFilePath).Should().BeTrue();
        }

        [Theory]
        [InlineData("/valid/path/file.txt", "/valid/path/file.txt")]
        [InlineData("another/valid/path/file.txt", "/another/valid/path/file.txt")]
        [InlineData("valid/path/file.txt", "/valid/path/file.txt")]
        [InlineData("/file.txt", "/file.txt")]
        [InlineData("file.txt", "/file.txt")]
        [InlineData("", "")]
        public void FilePath_ToString_ShouldReturnCorrectStringRepresentation(string path, string expectedPath)
        {
            // Arrange
            var filePath = new FilePath(path);

            // Act
            var result = filePath.ToString();

            // Assert
            result.Should().Be(expectedPath);
        }

        [Fact]
        public void FilePath_ShouldReturnIntHashCode_OfInternalStringValue()
        {
            // Arrange
            var directoryPath = new FilePath("/valid/path/file.txt");
            var expectedHashCode = directoryPath.Value.GetHashCode();

            // Act            
            var hashCode = directoryPath.GetHashCode();

            // Assert
            hashCode.Should().Be(expectedHashCode);
        }

        [Fact]
        public void FilePath_Parent_ShouldBeCorrectlyCalculated()
        {
            // Arrange
            var filePath = new FilePath("/valid/path/to/file.txt");
            var expectedParentPath = new DirectoryPath("/valid/path/to/");

            // Act
            var parentPath = filePath.Directory;

            // Assert
            parentPath.Equals(expectedParentPath).Should().BeTrue();
        }

        [Fact]
        public void FilePath_Contains_ShouldReturnTrue_WhenPathIsContained()
        {
            // Arrange
            var path1 = new DirectoryPath("/parent/");
            var path2 = new FilePath("/parent/child/file.txt");

            // Act
            var contains = path2.Contains(path1);

            // Assert
            contains.Should().BeTrue();
        }

        [Fact]
        public void FilePath_Contains_ShouldReturnFalse_WhenPathIsNotContained()
        {
            // Arrange
            var path1 = new DirectoryPath("/parent1/");
            var path2 = new FilePath("/parent/child/file.txt");

            // Act
            var contains = path2.Contains(path1);

            // Assert
            contains.Should().BeFalse();
        }

        [Theory]
        [InlineData("/parent/child/file.txt", "/parent/", true)]
        [InlineData("/parent/child/file.txt", "/other/", false)]
        public void FilePath_Contains_ShouldResultTrue_WhenStringCompared(string path1, string path2, bool contained)
        {
            // Arrange
            var directoryPath = new FilePath(path1);

            // Act
            var contains = directoryPath.Contains(path2);

            // Assert
            contains.Should().Be(contained);
        }

        [Fact]
        public void FilePath_ShouldHave_ExpectedFileName() 
        {
            //Arrange
            var filePath = new FilePath("parent/file.txt");

            //Assert
            filePath.FileName.Should().Be("file.txt");
        }

        [Fact]
        public void FilePath_ShouldHave_NoFileExtension_When_FileHasNoExtension() 
        {
            //Arrange
            var filePath = new FilePath("parent/file");

            //Assert
            filePath.Extension.Should().BeEmpty();
        }

        [Theory]
        [InlineData("parent/file.txt", ".txt")]
        [InlineData("parent/file", "")]
        public void FilePath_ShouldHave_ExpectedFileExtension(string path, string expectedExtension)
        {
            //Arrange
            var filePath = new FilePath(path);

            //Assert
            filePath.Extension.Should().Be(expectedExtension);
        }

        [Theory]
        [InlineData("parent/file.txt", "file")]
        [InlineData("parent/file", "file")]
        public void FilePath_ShouldHave_ExpectedFileNameWithoutExtension(string path, string expectedResult)
        {
            //Arrange
            var filePath = new FilePath(path);

            //Assert
            filePath.FileNameWithoutExtension.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("parent/file", "File")]
        [InlineData("parent/file.txt", "File")]
        [InlineData("parent/f.txt", "F")]
        [InlineData("", "")]
        public void FilePath_ShouldHave_ExpectedFileTitle(string fileName, string expectedValue)
        {
            //Arrange
            var filePath = new FilePath(fileName);

            //Assert
            filePath.FileTitle.Should().Be(expectedValue);
        }
    }
}

using System.Reflection;
using TeensyRom.Cli.Commands.Main.Asid;

namespace TeensyRom.Cli.Tests
{
    public class GeneratePresetsSettingsTests
    {
        [Fact]
        public void When_SourcePath_Empty_ValidationSucceeds()
        {
            //Arrrange
            var settings = new GeneratePresetsSettings
            {
                SourcePath = string.Empty
            };

            //Act
            var result = settings.Validate();
            
            //Assert
            result.Message.Should().BeNullOrEmpty();
        }

        [Fact]
        public void When_SourcePath_Valid_ValidationSucceeds()
        {
            //Arrrange
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;

            var settings = new GeneratePresetsSettings
            {
                SourcePath = Path.GetDirectoryName(assemblyLocation)!
            };

            //Act
            var result = settings.Validate();

            //Assert
            result.Message.Should().BeNullOrEmpty();
        }

        [Fact]
        public void When_SourcePath_Invalid_ValidationFails()
        {
            //Arrrange
            var settings = new GeneratePresetsSettings
            {
                SourcePath = "invalid"
            };

            //Act
            var result = settings.Validate();

            //Assert
            result.Message.Should().Contain("The source path 'invalid' does not exist.");
        }

        [Fact]
        public void When_TargetPath_Empty_ValidationSucceeds()
        {
            //Arrrange
            var settings = new GeneratePresetsSettings
            {
                TargetPath = string.Empty
            };

            //Act
            var result = settings.Validate();

            //Assert
            result.Message.Should().BeNullOrEmpty();
        }
        [Fact]
        public void When_TargetPath_IsNotRelative_ValidationFails()
        {
            //Arrrange
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;

            var settings = new GeneratePresetsSettings
            {
                SourcePath = Path.GetDirectoryName(assemblyLocation)!,
                TargetPath = Path.GetDirectoryName(assemblyLocation)!
            };

            //Act
            var result = settings.Validate();

            //Assert
            result.Message.Should().Contain($"The target path '{Path.GetDirectoryName(assemblyLocation)}' must be a relative path.");
        }

        [Fact]
        public void When_TargetPath_IsRelative_ValidationSucceeds()
        {
            //Arrrange
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;

            var settings = new GeneratePresetsSettings
            {
                SourcePath = Path.GetDirectoryName(assemblyLocation)!,
                TargetPath = "relative"
            };

            //Act
            var result = settings.Validate();

            //Assert
            result.Message.Should().BeNullOrEmpty();
        }

        [Fact]
        public void When_Clock_Empty_ValidationSucceeds()
        {
            //Arrrange
            var settings = new GeneratePresetsSettings
            {
                Clock = string.Empty
            };

            //Act
            var result = settings.Validate();

            //Assert
            result.Message.Should().BeNullOrEmpty();
        }

        [Fact]
        public void When_Clock_PAL_ValidationSucceeds()
        {
            //Arrrange
            var settings = new GeneratePresetsSettings
            {
                Clock = "PAL"
            };

            //Act
            var result = settings.Validate();

            //Assert
            result.Message.Should().BeNullOrEmpty();
        }

        [Fact]
        public void When_Clock_NTSC_ValidationSucceeds()
        {
            //Arrrange
            var settings = new GeneratePresetsSettings
            {
                Clock = "NTSC"
            };

            //Act
            var result = settings.Validate();

            //Assert
            result.Message.Should().BeNullOrEmpty();
        }

        [Fact]
        public void When_Clock_Invalid_ValidationFails()
        {
            //Arrrange
            var settings = new GeneratePresetsSettings
            {
                Clock = "invalid"
            };

            //Act
            var result = settings.Validate();

            //Assert
            result.Message.Should().Contain("The clock 'invalid' must be 'PAL' or 'NTSC'");
        }

        [Fact]
        public void When_Clock_LowerCase_ValidationSucceeds()
        {
            //Arrrange
            var settings = new GeneratePresetsSettings
            {
                Clock = "pal"
            };

            //Act
            var result = settings.Validate();

            //Assert
            result.Message.Should().BeNullOrEmpty();           
        }

        [Fact]
        public void When_Clock_MixedCase_ValidationSucceeds()
        {
            //Arrrange
            var settings = new GeneratePresetsSettings
            {
                Clock = "Pal"
            };

            //Act
            var result = settings.Validate();

            //Assert
            result.Message.Should().BeNullOrEmpty();
        }
    }
}
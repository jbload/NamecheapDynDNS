using System;
using Xunit;

namespace NamecheapDynDNS.Tests
{
    public class CommandLineOptionsTests
    {
        public CommandLineOptionsTests()
        {
            Options = new CommandLineOptions();    
        }

        private CommandLineOptions Options { get; }

        [Fact]
        public void DomainConfigFile_HasDefaultValue_WhenArgNotProvided()
        {
            var result = ParseArgs();

            Assert.True(result);
            Assert.Equal(CommandLineOptions.DomainConfigFileDefault, Options.DomainConfigFile);
        }

        [Fact]
        public void DomainConfigFile_IsNotNull_WhenArgIsProvidedUsingShortName()
        {
            var expectedValue = @"C:\some\path\config.json";
            var result = ParseArgs($"-{CommandLineOptions.DomainConfigFileShortName}", expectedValue);

            Assert.True(result);
            Assert.NotNull(Options.DomainConfigFile);
            Assert.Equal(expectedValue, Options.DomainConfigFile);
        }

        [Fact]
        public void DomainConfigFile_IsNotNull_WhenArgIsProvidedUsingLongName()
        {
            var expectedValue = @"C:\some\path\config.json";
            var result = ParseArgs($"--{CommandLineOptions.DomainConfigFileLongName}", expectedValue);

            Assert.True(result);
            Assert.NotNull(Options.DomainConfigFile);
            Assert.Equal(expectedValue, Options.DomainConfigFile);
        }

        [Fact]
        public void UpdateInterval_HasDefaultValue_WhenArgNotProvided()
        {
            var result = ParseArgs();

            Assert.True(result);
            Assert.NotNull(Options.UpdateInterval);
            Assert.Equal(CommandLineOptions.UpdateIntervalDefault, Options.UpdateInterval);
        }

        [Fact]
        public void UpdateInterval_IsNotNull_WhenArgIsProvidedUsingShortName()
        {
            var expectedValue = "27";
            var result = ParseArgs($"-{CommandLineOptions.UpdateIntervalShortName}", expectedValue);

            Assert.True(result);
            Assert.NotNull(Options.UpdateInterval);
            Assert.Equal(expectedValue, Options.UpdateInterval);
        }

        [Fact]
        public void UpdateInterval_IsNotNull_WhenArgIsProvidedUsingLongName()
        {
            var expectedValue = "27";
            var result = ParseArgs($"--{CommandLineOptions.UpdateIntervalLongName}", expectedValue);

            Assert.True(result);
            Assert.NotNull(Options.UpdateInterval);
            Assert.Equal(expectedValue, Options.UpdateInterval);
        }

        private bool ParseArgs(params string[] args)
        {
            return CommandLine.Parser.Default.ParseArguments(args, Options);
        }
    }
}

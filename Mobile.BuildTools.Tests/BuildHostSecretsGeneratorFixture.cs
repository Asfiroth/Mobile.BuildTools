using System;
using Microsoft.Build.Utilities;
using Mobile.BuildTools.Generators;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Mobile.BuildTools.Tests
{
    public class BuildHostSecretsGeneratorFixture
    {
        private const string SecretsPrefix = "UNIT_TEST_";
        private const string SecretsJsonFilePath = "Samples/secrets.json";

        private ITestOutputHelper _testOutputHelper { get; }

        public BuildHostSecretsGeneratorFixture(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private BuildHostSecretsGenerator GetGenerator() =>
            new BuildHostSecretsGenerator()
            {
                SecretsPrefix = SecretsPrefix,
                SecretsJsonFilePath = SecretsJsonFilePath
            };

        private void SetTestVariable() =>
            Environment.SetEnvironmentVariable($"{SecretsPrefix}Test1", "SomeValue");

        [Fact]
        public void DoesNotThrowException()
        {
            SetTestVariable();
            var generator = GetGenerator();
            var ex = Record.Exception(() => generator.Execute());

            if(ex != null)
            {
                _testOutputHelper.WriteLine(ex.ToString());
            }

            Assert.Null(ex);
        }

        [Fact]
        public void CreatesJObject()
        {
            SetTestVariable();
            var generator = GetGenerator();
            var secrets = generator.GetSecrets();

            var jsonObject = generator.GetJObjectFromSecrets(secrets);

            Assert.NotNull(jsonObject);
            Assert.IsType<JObject>(jsonObject);
            Assert.Equal("SomeValue", jsonObject["Test1"].ToString());

        }

        [Fact]
        public void RetrievesEnvironmentVariables()
        {
            SetTestVariable();
            var generator = GetGenerator();
            var secrets = generator.GetSecrets();

            Assert.NotNull(secrets);
            Assert.NotEmpty(secrets);
            foreach (var s in secrets)
            {
                _testOutputHelper.WriteLine($"{s.GetType()}: {s}");
                _testOutputHelper.WriteLine($"     ---- {Environment.GetEnvironmentVariable(s.ToString())}");
            }

            Assert.Contains($"{SecretsPrefix}Test1", secrets);
        }
    }
}

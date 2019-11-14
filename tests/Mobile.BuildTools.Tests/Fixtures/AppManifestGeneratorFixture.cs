﻿using System;
using System.IO;
using System.Linq;
using System.Xml;
using Mobile.BuildTools.Build;
using Mobile.BuildTools.Generators;
using Mobile.BuildTools.Generators.Manifests;
using Mobile.BuildTools.Tests.Mocks;
using Mobile.BuildTools.Utils;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0040 // Add accessibility modifiers
namespace Mobile.BuildTools.Tests.Fixtures
{
    public class AppManifestGeneratorFixture : FixtureBase
    {
        private const string TestPrefix = "XunitTest_";
        private static readonly string TemplateAndroidManifestPath = @"Templates/MockAndroidManifest.xml";
        private static readonly string TemplateAndroidManifestOutputPath = @"Generated/AndroidManifest.xml";
        private static readonly string TemplateManifestPath = @"Templates/MockManifestTemplate.json";
        private static readonly string TemplateManifestOutputPath = @"Generated/MockManifest.json";

        public AppManifestGeneratorFixture(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            Environment.SetEnvironmentVariable($"{TestPrefix}TemplatedParameter", nameof(AppManifestGeneratorFixture));
            Environment.SetEnvironmentVariable($"{TestPrefix}CustomTokenParameter", nameof(AppManifestGeneratorFixture));
        }

        private BaseTemplatedManifestGenerator CreateGenerator() =>
            CreateGenerator(GetConfiguration());

        private BaseTemplatedManifestGenerator CreateGenerator(IBuildConfiguration config) =>
            new DefaultTemplatedManifestGenerator(config)
            {
                ManifestOutputPath = TemplateManifestOutputPath,
            };

        [Fact]
        public void GetMatches()
        {
            var generator = CreateGenerator();
            var template = File.ReadAllText(TemplateManifestPath);
            _testOutputHelper.WriteLine(template);
            var matches = generator.GetMatches(template);
            Assert.NotNull(matches);
            Assert.NotEmpty(matches);
            Assert.Single(matches);
            var match = matches.FirstOrDefault();
            Assert.Equal("$$TemplatedParameter$$", match.Value);
        }

        [Fact]
        public void ProcessesStandardTokenizedVariables()
        {
            var generator = CreateGenerator();
            var template = File.ReadAllText(TemplateManifestPath);
            var match = generator.GetMatches(template).FirstOrDefault();
            var variables = EnvironmentAnalyzer.GatherEnvironmentVariables(Directory.GetCurrentDirectory(), true);
            var processedTemplate = generator.ProcessMatch(template, match, variables);
            var json = JsonConvert.DeserializeAnonymousType(processedTemplate, new
            {
                TemplatedParameter = "",
                CustomTokenParameter = ""
            });

            Assert.Equal("%%CustomTokenParameter%%", json.CustomTokenParameter);
            Assert.Equal(nameof(AppManifestGeneratorFixture), json.TemplatedParameter);
        }

        [Fact]
        public void ProcessCustomTokenizedVariables()
        {
            var config = GetConfiguration();
            config.Configuration.Manifests.Token = @"%%";
            var generator = CreateGenerator(config);

            var template = File.ReadAllText(TemplateManifestPath);
            var match = generator.GetMatches(template).FirstOrDefault();
            var variables = EnvironmentAnalyzer.GatherEnvironmentVariables(Directory.GetCurrentDirectory(), true);
            var processedTemplate = generator.ProcessMatch(template, match, variables);
            var json = JsonConvert.DeserializeAnonymousType(processedTemplate, new
            {
                TemplatedParameter = "",
                CustomTokenParameter = ""
            });

            Assert.Equal(nameof(AppManifestGeneratorFixture), json.CustomTokenParameter);
            Assert.Equal("$$TemplatedParameter$$", json.TemplatedParameter);
        }

        //[Fact]
        public void ProcessingDoesNotCorruptAndroidManifest()
        {
            var config = GetConfiguration();
            var generator = CreateGenerator(config);
            var template = File.ReadAllText(TemplateAndroidManifestPath);
            var guid = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable("XunitTest_AADClientId", guid);

            var matches = generator.GetMatches(template);
            var match = matches.First();
            Assert.Equal("$$AADClientId$$", match.Value);

            generator.ManifestOutputPath = TemplateAndroidManifestOutputPath;
            config.Configuration.Debug = true;

            var ex = Record.Exception(() => ((IGenerator)generator).Execute());
            Assert.Null(ex);

            var generatedTemplate = File.ReadAllText(TemplateAndroidManifestOutputPath);
            ex = Record.Exception(() =>
            {
                var doc = new XmlDocument();
                doc.LoadXml(generatedTemplate);
            });

            Assert.Null(ex);

            Assert.DoesNotContain("msal$$AADClientId$$", generatedTemplate);
            Assert.Contains($"msal{guid}", generatedTemplate);
        }
    }
}
#pragma warning restore IDE0040 // Add accessibility modifiers
#pragma warning restore IDE1006 // Naming Styles

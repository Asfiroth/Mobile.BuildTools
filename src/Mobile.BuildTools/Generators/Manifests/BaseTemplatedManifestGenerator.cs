using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Utilities;
using Mobile.BuildTools.Build;
using Mobile.BuildTools.Logging;
using Newtonsoft.Json;
using Xamarin.MacDev;
using Mobile.BuildTools.Extensions;

namespace Mobile.BuildTools.Generators.Manifests
{
    internal abstract class BaseTemplatedManifestGenerator : GeneratorBase
    {
        internal const string DefaultToken = @"\$";

        public BaseTemplatedManifestGenerator(IBuildConfiguration configuration)
            : base(configuration)
        {
            var token = configuration.Configuration.Manifests.Token;
            if (string.IsNullOrEmpty(token))
            {
                token = DefaultToken;
            }

            Token = token;
        }

        public string Token { get; }

        public string ProjectDirectory => Build.ProjectDirectory;

        public string ManifestSourcePath { get; set; }

        public string ManifestOutputPath { get; set; }

        protected override void ExecuteInternal()
        {
            if (!File.Exists(ManifestSourcePath))
            {
                Log?.LogWarning("There is no Template Manifest at the path: '{0}'", ManifestSourcePath);
            }

            var template = ReadManifest();

            // Includes manifest.json and secrets.json
            var variables = Utils.EnvironmentAnalyzer.GatherEnvironmentVariables(ProjectDirectory, true);

            var matches = GetMatches(template);
            var manifestFileName = Path.GetFileName(ManifestSourcePath);
            if (matches.Count > 0)
            {
                Log.LogMessage($"Found {matches.Count} Tokens in the {manifestFileName}");
                foreach (Match match in matches)
                {
                    template = ProcessMatch(template, match, variables);
                }
            }
            else
            {
                Log.LogMessage($"Did not find any Tokens in the {manifestFileName}. To use Tokens ");
            }

            var outputFile = new FileInfo(ManifestOutputPath);
            outputFile.Directory.Create();
            SaveManifest(template);
        }

        protected abstract string ReadManifest();

        protected abstract void SaveManifest(string manifest);

        internal MatchCollection GetMatches(string template)
        {
            var token = Regex.Escape(Token);
            var pattern = $@"{token}(.*?){token}";
            return Regex.Matches(template, pattern);
        }

        internal string ProcessMatch(string template, Match match, IDictionary<string, string> variables)
        {
            var tokenId = match.Groups[1].Value;
            var key = GetKey(tokenId, variables);
            var token = Regex.Escape(Token);

            if (!string.IsNullOrWhiteSpace(key))
            {
                Log?.LogMessage($"Replacing token '{tokenId}'");
                var value = variables[key];
                template = Regex.Replace(template, $@"{token}{tokenId}{token}", value);
            }
            else
            {
                Log?.LogWarning($"Unable to locate replacement value for {tokenId}");
            }

            return template;
        }

        internal string GetKey(string matchValue, IDictionary<string, string> variables)
        {
            if(variables.ContainsKey(matchValue))
            {
                return matchValue;
            }

            var prefixes = Utils.EnvironmentAnalyzer.GetManifestPrefixes(Build.Platform, Build.Configuration.Manifests.VariablePrefix);

            foreach (var manifestPrefix in prefixes)
            {
                if (variables.ContainsKey($"{manifestPrefix}{matchValue}"))
                    return $"{manifestPrefix}{matchValue}";
            }

            return null;
        }
    }
}
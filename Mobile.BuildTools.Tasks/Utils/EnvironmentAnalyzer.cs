﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mobile.BuildTools.Tasks.Utils;
using Newtonsoft.Json.Linq;

namespace Mobile.BuildTools.Utils
{
    internal static class EnvironmentAnalyzer
    {
        private const string DefaultSecretPrefix = "Secret_";
        private const string DefaultManifestPrefix = "Manifest_";

        public static IDictionary<string, string> GatherEnvironmentVariables(string projectPath = null, bool includeManifest = false)
        {
            var env = new Dictionary<string, string>();
            foreach(var key in Environment.GetEnvironmentVariables().Keys)
            {
                env.Add(key.ToString(), Environment.GetEnvironmentVariable(key.ToString()));
            }

            if(!string.IsNullOrWhiteSpace(projectPath))
            {
                LoadSecrets(Path.Combine(projectPath, "secrets.json"), ref env);
            }

            if (includeManifest)
            {
                LoadSecrets(Path.Combine(projectPath, "manifest.json"), ref env);
            }

            return env;
        }

        public static IEnumerable<string> GetManifestPrefixes(string sdkShortFrameworkIdentifier)
        {
            var prefixes = new List<string>(GetSecretPrefixes(sdkShortFrameworkIdentifier, forceIncludeDefault: true))
            {
                DefaultManifestPrefix
            };
            var platformPrefix = GetPlatformManifestPrefix(sdkShortFrameworkIdentifier);
            if(!string.IsNullOrWhiteSpace(platformPrefix))
            {
                prefixes.Add(platformPrefix);
            }

            return prefixes;
        }

        private static string GetPlatformManifestPrefix(string sdkShortFrameworkIdentifier)
        {
            switch (sdkShortFrameworkIdentifier.GetTargetPlatform())
            {
                case Platform.Android:
                    return "DroidManifest_";
                case Platform.iOS:
                    return "iOSManifest_";
                case Platform.UWP:
                    return "UWPManifest_";
                case Platform.macOS:
                    return "MacManifest_";
                case Platform.Tizen:
                    return "TizenManifest_";
                default:
                    return null;
            }
        }

        public static string GetSecretPrefix(string sdkShortFrameworkIdentifier)
        {
            switch (sdkShortFrameworkIdentifier.GetTargetPlatform())
            {
                case Platform.Android:
                    return "DroidSecret_";
                case Platform.iOS:
                    return "iOSSecret_";
                case Platform.UWP:
                    return "UWPSecret_";
                case Platform.macOS:
                    return "MacSecret_";
                case Platform.Tizen:
                    return "TizenSecret_";
                default:
                    return DefaultSecretPrefix;
            }
        }

        private static bool IsPlatformProject(string sdkShortFrameworkIdentifier)
        {
            return sdkShortFrameworkIdentifier.GetTargetPlatform() != Platform.Unsupported;
        }

        public static IEnumerable<string> GetSecretPrefixes(string sdkShortFrameworkIdentifier, string runtimePrefix = null, bool forceIncludeDefault = false)
        {
            if(string.IsNullOrWhiteSpace(runtimePrefix))
            {
                runtimePrefix = GetSecretPrefix(sdkShortFrameworkIdentifier);
            }

            var prefixes = new List<string>
            {
                runtimePrefix,
                "SharedSecret_"
            };

            if(IsPlatformProject(sdkShortFrameworkIdentifier))
            {
                prefixes.Add("PlatformSecret_");
            }

            if(forceIncludeDefault && !prefixes.Contains(DefaultSecretPrefix))
            {
                prefixes.Add(DefaultSecretPrefix);
                prefixes.Add($"MB{DefaultManifestPrefix}");
            }

            return prefixes;
        }

        public static IEnumerable<string> GetSecretKeys(IEnumerable<string> prefixes)
        {
            var variables = GatherEnvironmentVariables();
            return variables.Keys.Where(k => prefixes.Any(p => k.StartsWith(p)));
        }

        public static IDictionary<string, string> GetSecrets(string sdkShortFrameworkIdentifier, string runtimePrefix = null)
        {
            var prefixes = GetSecretPrefixes(sdkShortFrameworkIdentifier, runtimePrefix);
            var keys = GetSecretKeys(prefixes);
            var variables = GatherEnvironmentVariables().Where(p => keys.Any(k => k == p.Key));
            var output = new Dictionary<string, string>();
            foreach(var prefix in prefixes)
            {
                foreach(var pair in variables.Where(v => v.Key.StartsWith(prefix)))
                {
                    var key = Regex.Replace(pair.Key, prefix, "");
                    output.Add(key, pair.Value);
                }
            }
            return output;
        }

        private static void LoadSecrets(string path, ref Dictionary<string, string> env)
        {
            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            var secrets = JObject.Parse(json);
            foreach(var secret in secrets)
            {
                if (!env.ContainsKey(secret.Key))
                {
                    env.Add(secret.Key, secret.Value.ToString());
                }
            }
        }
    }
}

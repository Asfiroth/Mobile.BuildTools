﻿using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Mobile.BuildTools.Utils
{
    internal static class EnvironmentAnalyzer
    {
        private const string DefaultSecretPrefix = "Secret_";
        private const string DefaultManifestPrefix = "Manifest_";

        public static IDictionary<string, string> GatherEnvironmentVariables(string projectPath, bool includeManifest = false)
        {
            var env = new Dictionary<string, string>();
            foreach(var key in Environment.GetEnvironmentVariables().Keys)
            {
                env.Add(key.ToString(), Environment.GetEnvironmentVariable(key.ToString()));
            }

            LoadSecrets(Path.Combine(projectPath, "secrets.json"), ref env);

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
            switch (sdkShortFrameworkIdentifier)
            {
                case "monoandroid":
                case "xamarinandroid":
                case "xamarin.android":
                    return "DroidManifest_";
                case "xamarinios":
                case "xamarin.ios":
                    return "iOSManifest_";
                case "win":
                case "uap":
                    return "UWPManifest_";
                case "xamarinmac":
                case "xamarin.mac":
                    return "MacManifest_";
                case "tizen":
                    return "TizenManifest_";
                default:
                    return null;

            }
        }

        public static string GetSecretPrefix(string sdkShortFrameworkIdentifier)
        {
            switch (sdkShortFrameworkIdentifier)
            {
                case "monoandroid":
                case "xamarinandroid":
                case "xamarin.android":
                    return "DroidSecret_";
                case "xamarinios":
                case "xamarin.ios":
                    return "iOSSecret_";
                case "win":
                case "uap":
                    return "UWPSecret_";
                case "xamarinmac":
                case "xamarin.mac":
                    return "MacSecret_";
                case "tizen":
                    return "TizenSecret_";
                default:
                    return DefaultSecretPrefix;

            }
        }

        private static bool IsPlatformProject(string sdkShortFrameworkIdentifier)
        {
            switch (sdkShortFrameworkIdentifier)
            {
                case "monoandroid":
                case "xamarinandroid":
                case "xamarin.android":
                case "xamarinios":
                case "xamarin.ios":
                case "win":
                case "uap":
                case "xamarinmac":
                case "xamarin.mac":
                case "tizen":
                    return true;
                default:
                    return false;

            }
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
            }

            return prefixes;
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

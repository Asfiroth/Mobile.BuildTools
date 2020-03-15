using System.IO;
using Mobile.BuildTools.Build;
using Xamarin.MacDev;

namespace Mobile.BuildTools.Generators.Versioning
{
    internal class iOSAutomaticBuildVersionGenerator : BuildVersionGeneratorBase
    {
        public iOSAutomaticBuildVersionGenerator(IBuildConfiguration buildConfiguration)
            : base(buildConfiguration)
        {
        }

        protected override void ProcessManifest(string buildNumber)
        {
            var infoPlist = PDictionary.FromFile(ManifestInputPath);

            var shortVersion = infoPlist.GetCFBundleShortVersionString();
            var bundleVersion = infoPlist.GetCFBundleVersion();
            var hasShortVersion = !string.IsNullOrEmpty(shortVersion);
            var hasBundleVersion = !string.IsNullOrEmpty(bundleVersion);

            if(hasBundleVersion && hasShortVersion)
            {
                bundleVersion = GetBundleVersion(bundleVersion, buildNumber);
                shortVersion = GetShortBundleVersion(shortVersion, buildNumber);
            }
            else if(!hasBundleVersion && hasShortVersion)
            {
                var semVerPartCount = shortVersion.Split('.').Length;
                bundleVersion = GetBundleVersion(shortVersion, buildNumber);
                shortVersion = GetShortBundleVersion( semVerPartCount > 1 ? shortVersion : "1.0", buildNumber);
            }
            else if(hasBundleVersion && !hasShortVersion)
            {
                bundleVersion = GetBundleVersion(bundleVersion, buildNumber);
                shortVersion = GetShortBundleVersion(bundleVersion, buildNumber);
            }
            else
            {
                bundleVersion = GetBundleVersion("1", buildNumber);
                shortVersion = GetShortBundleVersion("1.0.0", buildNumber);
            }

            Log.LogMessage($"Setting the CFBundleVersion to: {shortVersion}");
            Log.LogMessage($"Setting the CFBundleShortVersion to: {shortVersion}");
            infoPlist.SetCFBundleVersion(bundleVersion);
            infoPlist.SetCFBundleShortVersionString(shortVersion);
            File.WriteAllText(ManifestOutputPath, infoPlist.ToXml());
        }

        private string GetBundleVersion(string currentVersion, string buildNumber)
        {
            var semVerParts = currentVersion.Split('.');
            if(semVerParts.Length > 1)
            {
                if(int.TryParse(semVerParts[0], out var major) && int.TryParse(semVerParts[1], out var minor))
                {
                    return $"{major}.{minor}.{buildNumber}";
                }
                else
                {
                    Log.LogWarning($"Unable to properly parse the first to elements of the Bundle Version: '{currentVersion}'.");
                }
            }

            return buildNumber;
        }

        private string GetShortBundleVersion(string currentVersion, string buildNumber)
        {
            var semVerParts = currentVersion.Split('.');
            string version;
            switch(semVerParts.Length)
            {
                case 1:
                    version = $"{semVerParts[0]}.0";
                    break;
                case 2:
                case 3:
                    version = $"{semVerParts[0]}.{semVerParts[1]}";
                    break;
                default:
                    throw new System.Exception($"Unable to validate existing CFShortBundleVersion '{currentVersion}'.");
            }

            // TODO: Add configuration value to control whether we should add the build number here
            return $"{version}.{buildNumber}";
        }
    }
}

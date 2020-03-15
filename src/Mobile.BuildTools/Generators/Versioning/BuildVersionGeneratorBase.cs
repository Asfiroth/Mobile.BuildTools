using System;
using System.IO;
using System.Linq;
using Mobile.BuildTools.Build;
using Mobile.BuildTools.Models;
using Mobile.BuildTools.Utils;

namespace Mobile.BuildTools.Generators.Versioning
{
    internal abstract class BuildVersionGeneratorBase : GeneratorBase
    {
        public BuildVersionGeneratorBase(IBuildConfiguration buildConfiguration)
            : base(buildConfiguration)
        {
        }

        private static DateTimeOffset EPOCOffset => new DateTimeOffset(new DateTime(2018, 1, 1));

        public string ManifestInputPath { get; set; }
        public string ManifestOutputPath { get; set; }
        public VersionBehavior Behavior => Build.Configuration.AutomaticVersioning.Behavior;
        public VersionEnvironment VersionEnvironment => Build.Configuration.AutomaticVersioning.Environment;
        public int VersionOffset => Build.Configuration.AutomaticVersioning.VersionOffset;

        internal string BuildNumber { get; private set; }

        protected override void ExecuteInternal()
        {
            if(Behavior == VersionBehavior.Off)
            {
                Log.LogMessage("Automatic versioning has been disabled. Please update your project settings to enable automatic versioning.");
                return;
            }

            if(string.IsNullOrWhiteSpace(ManifestInputPath))
            {
                Log.LogMessage("No Manifest Path was provided.");
                return;
            }

            if(!File.Exists(ManifestInputPath))
            {
                Log.LogWarning($"The '{Path.GetFileName(ManifestInputPath)}' could not be found at the path '{ManifestInputPath}'.");
                return;
            }

            BuildNumber = GetBuildNumber();
            Log.LogMessage($"Build Number: {BuildNumber}");

            LogManifestContents();

            Log.LogMessage("Processing Manifest");
            var outputFile = new FileInfo(ManifestOutputPath);
            outputFile.Directory.Create();
            ProcessManifest(BuildNumber);

            LogManifestContents();
        }

        private void LogManifestContents()
        {
            if (Build.Configuration.Debug)
            {
                Log.LogMessage(File.ReadAllText(ManifestInputPath));
            }
        }

        protected abstract void ProcessManifest(string buildNumber);

        protected string GetBuildNumber()
        {
            if(Behavior == VersionBehavior.PreferBuildNumber && CIBuildEnvironmentUtils.IsBuildHost)
            {
                if(int.TryParse(CIBuildEnvironmentUtils.BuildNumber, out var buildId))
                {
                    return $"{buildId + VersionOffset}";
                }

                return CIBuildEnvironmentUtils.BuildNumber;
            }

            var timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds() - EPOCOffset.ToUnixTimeSeconds();
            return $"{VersionOffset + timeStamp}";
        }

        protected string SanitizeVersion(string version)
        {
            try
            {
                var parts = version.Split('.');
                return parts.LastOrDefault().Count() > 4 ?
                    string.Join(".", parts.Where(p => p != parts.LastOrDefault())) :
                    version;
            }
            catch(Exception e)
            {
                Log.LogMessage($"Error Sanitizing Version String");
                Log.LogWarning(e.ToString());
                return version;
            }
        }
    }
}

using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mobile.BuildTools.Build;
using Mobile.BuildTools.Generators;
using Mobile.BuildTools.Generators.Manifests;
using Mobile.BuildTools.Utils;

namespace Mobile.BuildTools.Tasks
{
    public class TemplateManifestTask : BuildToolsTaskBase
    {
        public string[] ReferenceAssemblyPaths { get; set; }

        [Required]
        public string ManifestPath { get; set; }

        [Required]
        public string OutputManifestPath { get; set; }

        [Output]
        public string ProcessedManifest => OutputManifestPath;

        internal override void ExecuteInternal(IBuildConfiguration config)
        {
            if(string.IsNullOrEmpty(ManifestPath))
            {
                Log.LogWarning("No value was provided for the Manifest. Unable to process Manifest Tokens");
                return;
            }

            if(!File.Exists(ManifestPath))
            {
                Log.LogWarning($"Unable to process Manifest Tokens, no manifest was found at the path '{ManifestPath}'");
                return;
            }

            IGenerator generator = null;
            switch(config.Platform)
            {
                case Platform.iOS:
                    generator = new TemplatedPlistGenerator(config)
                    {
                        ManifestSourcePath = ManifestPath,
                        ManifestOutputPath = OutputManifestPath
                    };
                    break;
                case Platform.Android:
                    generator = new TemplatedAndroidAppManifestGenerator(config, ReferenceAssemblyPaths)
                    {
                        ManifestSourcePath = ManifestPath,
                        ManifestOutputPath = OutputManifestPath,
                        AndroidVersions = new AndroidVersions(ReferenceAssemblyPaths)
                    };
                    break;
            }

            generator?.Execute();
        }
    }
}
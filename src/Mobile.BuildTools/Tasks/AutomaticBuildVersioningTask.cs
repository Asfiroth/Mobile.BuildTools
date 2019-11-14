﻿using Mobile.BuildTools.Build;
using Mobile.BuildTools.Generators;
using Mobile.BuildTools.Generators.Versioning;
using Mobile.BuildTools.Models;
using Mobile.BuildTools.Tasks.Utils;
using Mobile.BuildTools.Utils;

namespace Mobile.BuildTools.Tasks
{
    public class AutomaticBuildVersioningTask : BuildToolsTaskBase
    {
        public string[] ReferenceAssemblyPaths { get; set; }

        internal override void ExecuteInternal(IBuildConfiguration buildConfiguration)
        {
            if (buildConfiguration.Platform != Platform.Android && buildConfiguration.Platform != Platform.iOS && buildConfiguration.Platform != Platform.macOS)
            {
                Log.LogMessage($"The current Platform '{buildConfiguration.Platform}' is not supported for Automatic Versioning");
            }
            else if (buildConfiguration.Configuration.AutomaticVersioning.Behavior == VersionBehavior.Off)
            {
                Log.LogMessage("Automatic versioning has been disabled.");
            }
            else if (CIBuildEnvironmentUtils.IsBuildHost && buildConfiguration.Configuration.AutomaticVersioning.Environment == VersionEnvironment.Local)
            {
                Log.LogMessage("Your current settings are to only build on your local machine, however it appears you are building on a Build Host.");
            }
            else
            {
                Log.LogMessage("Executing Automatic Version Generator");
                GetGenerator(buildConfiguration.Platform)?.Execute();
            }
        }

        private IGenerator GetGenerator(Platform platform)
        {
            switch (platform)
            {
                case Platform.Android:
                    return new AndroidAutomaticBuildVersionGenerator(this)
                    {
                        ReferenceAssemblyPaths = ReferenceAssemblyPaths
                    };
                case Platform.iOS:
                case Platform.macOS:
                    return new iOSAutomaticBuildVersionGenerator(this);
                default:
                    return null;

            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Mobile.BuildTools.Logging;
using Mobile.BuildTools.Utils;
using Mobile.BuildTools.Versioning;

namespace Mobile.BuildTools.Generators
{
    public abstract class BuildVersionGeneratorBase : IGenerator
    {
        public Behavior Behavior { get; set; }
        public VersionEnvironment VersionEnvironment { get; set; }
        public string ProjectPath { get; set; }
        public int VersionOffset { get; set; }
        public ILog Log { get; set; }
        public bool? DebugOutput { get; set; }

        public void Execute()
        {
            if (DebugOutput == null)
            {
                DebugOutput = false;
            }

            if(Behavior == Behavior.Off)
            {
                Log.LogMessage("Automatic versioning has been disabled. Please update your project settings to enable automatic versioning.");
                return;
            }

            var manifestPath = GetManifestPath();
            if(string.IsNullOrWhiteSpace(manifestPath))
            {
                Log.LogMessage("This platform is unsupported");
                return;
            }

            if(!File.Exists(manifestPath))
            {
                Log.LogWarning($"The '{Path.GetFileName(manifestPath)}' could not be found at the path '{manifestPath}'.");
                return;
            }

            var buildNumber = GetBuildNumber();

            ProcessManifest(manifestPath, buildNumber);
        }

        protected abstract void ProcessManifest(string path, string buildNumber);

        protected abstract string GetManifestPath();

        protected string GetBuildNumber()
        {
            if(Behavior == Behavior.PreferBuildNumber && CIBuildEnvironmentUtils.IsBuildHost)
            {
                var buildId = int.Parse(CIBuildEnvironmentUtils.BuildNumber);
                return $"{buildId + VersionOffset}";
            }

            return $"{VersionOffset + DateTimeOffset.Now.ToUnixTimeSeconds()}";
        }

        protected string SanitizeVersion(string version)
        {
            var parts = version.Split('.');
            return parts.LastOrDefault().Count() > 4 ?
                   string.Join(".", parts.Where(p => p != parts.LastOrDefault())) :
                   version;
        }
    }
}

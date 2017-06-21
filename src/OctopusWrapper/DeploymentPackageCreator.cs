
namespace NuGetPackager
{
    using System;
    using System.IO;
    using NuGet;

    class DeploymentPackageCreator
    {
        readonly string nugetsFolderFullPath;
        readonly string metadataFolderFullPath;
        readonly string deployFolderFullPath;
        readonly string productName;
        readonly string version;
        readonly string branch;

        public DeploymentPackageCreator(string nugetsFolderFullPath, string metadataFolderFullPath, string deployFolderFullPath, string productName, string version, string branch, string commitHash)
        {
            this.nugetsFolderFullPath = nugetsFolderFullPath;
            this.metadataFolderFullPath = metadataFolderFullPath;
            this.deployFolderFullPath = deployFolderFullPath;
            this.productName = productName;
            this.version = version;
            this.branch = branch;
        }

        public void CreateDeploymentPackage()
        {
            if (Directory.Exists(nugetsFolderFullPath))
            {
                foreach (var nupkg in Directory.GetFiles(nugetsFolderFullPath, "*.nupkg"))
                {
                    File.Copy(nupkg, nupkg + ".nzip", true);
                }
            }
            try
            {
                CreateDeployPackage(productName + ".Deploy", "Octopus package for release " + productName + ".");
            }
            finally
            {
                // Clean up
                if (Directory.Exists(nugetsFolderFullPath))
                {
                    foreach (var nupkg in Directory.GetFiles(nugetsFolderFullPath, "*.nzip"))
                    {
                        File.Delete(nupkg);
                    }
                }
            }
        }

        void CreateDeployPackage(string id, string description)
        {
            var packageBuilder = new PackageBuilder
            {
                Id = id,
                Description = description,
                Version = SemanticVersion.Parse(version)
            };
            packageBuilder.Authors.Add("Particular Software");
            AddScript(packageBuilder, GenerateMetadataScript(), "Metadata.ps1");
            AddContent(packageBuilder);
            SavePackage(packageBuilder, deployFolderFullPath, ".nupkg", "Package created -> {0}");
        }

        string GenerateMetadataScript()
        {
            var versionParts = version.Split('.');
            var major = versionParts[0];
            var minor = versionParts[1];

            return $@"$Branch = ""{branch}""
$Version = ""{version}""
$Product = ""{productName}""
$Major = ""{major}""
$Minor = ""{minor}""
";
        }

        void AddContent(PackageBuilder packageBuilder)
        {
            if (Directory.Exists(nugetsFolderFullPath))
            {
                foreach (var nupkg in Directory.GetFiles(nugetsFolderFullPath, "*.nzip"))
                {
                    packageBuilder.PopulateFiles("", new[]
                    {
                        new ManifestFile
                        {
                            Source = nupkg,
                            Target = "content"
                        }
                    });
                }
            }
        }

        void AddScript(PackageBuilder packageBuilder, string scriptBody, string scriptName)
        {
            var metadataFile = Path.Combine(metadataFolderFullPath, scriptName);
            if (!File.Exists(metadataFile))
            {
                metadataFile = Path.Combine(Path.GetTempPath(), scriptName);
                File.WriteAllText(metadataFile, scriptBody);
            }
            packageBuilder.PopulateFiles("", new[] { new ManifestFile { Source = metadataFile, Target = scriptName } });
        }

        static void SavePackage(PackageBuilder packageBuilder, string destinationFolder, string filenameSuffix, string logMessage)
        {
            var filename = Path.Combine(destinationFolder, packageBuilder.GetFullName()) + filenameSuffix;
            if (Directory.Exists(destinationFolder))
            {
                Directory.Delete(destinationFolder, true);
            }
            Directory.CreateDirectory(destinationFolder);

            using (var file = new FileStream(filename, FileMode.Create))
            {
                packageBuilder.Save(file);
            }
            Console.WriteLine(logMessage, filename);
        }
    }
}

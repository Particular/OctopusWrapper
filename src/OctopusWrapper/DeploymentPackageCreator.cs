
namespace NuGetPackager
{
    using System;
    using System.IO;
    using NuGet;

    class DeploymentPackageCreator
    {
        readonly string nugetsFolderFullPath;
        readonly string chocosFolderFullPath;
        readonly string deployFolderFullPath;
        readonly string productName;
        readonly string version;
        readonly string branch;

        public DeploymentPackageCreator(string nugetsFolderFullPath, string chocosFolderFullPath, string deployFolderFullPath, string productName, string version, string branch)
        {
            this.chocosFolderFullPath = chocosFolderFullPath;
            this.nugetsFolderFullPath = nugetsFolderFullPath;
            this.deployFolderFullPath = deployFolderFullPath;
            this.productName = productName;
            this.version = version;
            this.branch = branch;
        }

        public void CreateDeploymentPackage()
        {
            foreach (var nupkg in Directory.GetFiles(nugetsFolderFullPath, "*.nupkg"))
            {
                File.Copy(nupkg, nupkg + ".nzip", true);
            }
            foreach (var nupkg in Directory.GetFiles(chocosFolderFullPath, "*.nupkg"))
            {
                File.Copy(nupkg, nupkg + ".czip", true);
            }

            try
            {
                CreateDeployPackage(productName + ".Deploy", "Octopus package for release " + productName + ".");
            }
            finally
            {
                // Clean up
                foreach (var nupkg in Directory.GetFiles(nugetsFolderFullPath, "*.nzip"))
                {
                    File.Delete(nupkg);
                }
                foreach (var nupkg in Directory.GetFiles(chocosFolderFullPath, "*.czip"))
                {
                    File.Delete(nupkg);
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

            return string.Format(@"$Branch = ""{0}""
$Version = ""{1}""
$Product = ""{2}""
$Major = ""{3}""
$Minor = ""{4}""
", branch, version, productName, major, minor);
        }

        void AddContent(PackageBuilder packageBuilder)
        {
            foreach (var nupkg in Directory.GetFiles(nugetsFolderFullPath, "*.nzip"))
            {
                packageBuilder.PopulateFiles("", new[] { new ManifestFile { Source = nupkg, Target = "content" } });
            }
            foreach (var nupkg in Directory.GetFiles(chocosFolderFullPath, "*.czip"))
            {
                packageBuilder.PopulateFiles("", new[] { new ManifestFile { Source = nupkg, Target = "content" } });
            }
        }

        void AddScript(PackageBuilder packageBuilder, string scriptBody, string scriptName)
        {
            var deployFile = Path.Combine(Path.GetTempPath(), scriptName);
            File.WriteAllText(deployFile, scriptBody);
            packageBuilder.PopulateFiles("", new[] { new ManifestFile { Source = deployFile, Target = scriptName } });
        }

        void SavePackage(PackageBuilder packageBuilder, string destinationFolder, string filenameSuffix, string logMessage)
        {
            var filename = Path.Combine(destinationFolder, packageBuilder.GetFullName()) + filenameSuffix;

            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }
            using (var file = new FileStream(filename, FileMode.Create))
            {
                packageBuilder.Save(file);
            }
            Console.WriteLine(logMessage, filename);
        }
    }
}

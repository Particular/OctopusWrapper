namespace OctopusWrapper
{
    using System;
    using System.IO;
    using NuGetPackager;

    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: OctopusWrapper [ProductName] [Version] [Branch] [CommitHash?]");
                Console.WriteLine("  e.g. OctopusWrapper NServiceBus 5.3.1 master f0adf07787dd099f0a401516a7d19c65c1b2cf7b");
                return 1;
            }

            string commitHash = null;
            if (args.Length > 3)
            {
                commitHash = args[3];
            }

            var packageCreator = new DeploymentPackageCreator(
                Path.Combine(Environment.CurrentDirectory, "nugets"),
                Path.Combine(Environment.CurrentDirectory, "assets"),
                Path.Combine(Environment.CurrentDirectory),
                Path.Combine(Environment.CurrentDirectory, "deploy"),
                args[0], args[1], args[2], commitHash
                );

            packageCreator.CreateDeploymentPackage();
            return 0;
        }
    }
}

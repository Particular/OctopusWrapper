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
                Console.WriteLine("Usage: OctopusWrapper [ProductName] [Version] [Branch]");
                Console.WriteLine("  e.g. OctopusWrapper NServiceBus 5.3.1 master");
                return 1;
            }

            var packageCreator = new DeploymentPackageCreator(
                Path.Combine(Environment.CurrentDirectory, "nugets"),
                Path.Combine(Environment.CurrentDirectory, "chocos"),
                Path.Combine(Environment.CurrentDirectory, "assets"),
                Path.Combine(Environment.CurrentDirectory, "deploy"),
                args[0], args[1], args[2]
                );

            packageCreator.CreateDeploymentPackage();
            return 0;
        }
    }
}

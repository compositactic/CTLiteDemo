using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CTLite.Tools.CTGen
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CTGen - code generation for CTLite");

            var proc = Process.GetCurrentProcess();

            var rootDirectory = string.Empty;
            var applicationType = string.Empty;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-r":
                        rootDirectory = args[i + 1];
                        break;
                    case "-a":
                        applicationType = args[i + 1];
                        break;
                    default:
                        break;
                }
            }

            if (string.IsNullOrEmpty(rootDirectory))
            {
                Console.Error.WriteLine($"Usage: {proc.ProcessName} -r [root directory] -a [application type]");
                return;
            }

            var rootDirectoryInfo = new DirectoryInfo(rootDirectory);
            var workingDirectory = rootDirectoryInfo.Parent.FullName;

            if (!rootDirectoryInfo.Exists)
            {
                Console.Error.WriteLine($"The directory {rootDirectory} does not exist");
                return;
            }

            var directories = new DirectoryInfo[] { rootDirectoryInfo }.Union(rootDirectoryInfo.GetDirectories(string.Empty, SearchOption.AllDirectories));

            if (!Regex.IsMatch(rootDirectoryInfo.Name, @"s|es$") || !directories.All(d => Regex.IsMatch(d.Name, @"s|es$")))
            {
                Console.Error.WriteLine("All directory names must be in plural form, and end with 's' or 'es'");
                return;
            }

            var rootDirectoryNameSingular = Regex.Replace(rootDirectoryInfo.Name, @"s|es$", string.Empty);
            var dotNet = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Console.WriteLine($"Creating solution file {rootDirectoryNameSingular}.sln ...");
            dotNet.Arguments = $"new sln --name {rootDirectoryNameSingular}";
            var dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();

            Console.Write($"Creating project {rootDirectoryNameSingular}.Model ...");
            dotNet.Arguments = $"new classlib --name {rootDirectoryNameSingular}.Model -- output {rootDirectoryNameSingular}.Model";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();

            var newDirs = directories.Select(d => d.FullName.Replace(workingDirectory, Path.Combine(workingDirectory, $"{rootDirectoryNameSingular}.Model")));

            foreach (var newDir in newDirs)
                Directory.CreateDirectory(newDir);

            dotNet.Arguments = $"sln add .\\{rootDirectoryNameSingular}.Model\\{rootDirectoryNameSingular}.Model.csproj";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();


        }
    }
}

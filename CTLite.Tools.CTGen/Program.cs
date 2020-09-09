﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

            if (!Regex.IsMatch(rootDirectoryInfo.Name, @"s$|es$") || !directories.All(d => Regex.IsMatch(d.Name, @"s$|es$")))
            {
                Console.Error.WriteLine("All directory names must be in plural form, and end with 's' or 'es'");
                return;
            }

            CreateSolutionAndProjects(rootDirectoryInfo, workingDirectory, directories);

            Console.WriteLine("Generating model code ...");
            GenerateModelCode(new DirectoryInfo[] { rootDirectoryInfo }, workingDirectory, true, string.Empty, string.Empty);

            Console.WriteLine("Code generation complete!");
        }

        private static void GenerateModelCode(IEnumerable<DirectoryInfo> rootDirectoryInfos, string workingDirectory, bool isRootDirectory, string rootNamespace, string rootClassName)
        {
            var modelGTcsTemplate = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Model.g.tcs"));
            var modelTcsTemplate = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Model.tcs"));

            foreach(var directory in rootDirectoryInfos)
            {
                var childNamespaces = new StringBuilder();
                var childModelFactoryMethods = new StringBuilder();
                var childModelClassDictionaries = new StringBuilder();
                var publicConstructorCode = new StringBuilder();
                var modelOnDeserailizedMethod = new StringBuilder();
                var internalConstructorCode = new StringBuilder();

                var modelClassName = Regex.Replace(directory.Name, @"s$|es$", string.Empty);
                rootNamespace = isRootDirectory ? $"{modelClassName}.Model" : rootNamespace;
                rootClassName = isRootDirectory ? modelClassName : rootClassName;
                var modelClassNamespace = rootNamespace + directory.FullName.Replace(workingDirectory, string.Empty).Replace(Path.DirectorySeparatorChar, '.');
                var childModelClassDirectories = directory.GetDirectories();

                var parentModelClassName = isRootDirectory ? string.Empty : Regex.Replace(directory.Parent.Name, @"s$|es$", string.Empty);
                var parentModelIdPropertyName = string.IsNullOrEmpty(parentModelClassName) ? string.Empty : $"{parentModelClassName}Id";

                if (!string.IsNullOrEmpty(parentModelClassName))
                {
                    var parentModelClassNameCamelCase = char.ToLowerInvariant(parentModelClassName[0]) + parentModelClassName.Substring(1);
                    internalConstructorCode.AppendLine($"internal {modelClassName}({parentModelClassName} {parentModelClassNameCamelCase}) ");
                    internalConstructorCode.AppendLine("\t\t{");
                    internalConstructorCode.AppendLine($"\t\t\t{parentModelIdPropertyName} = {parentModelClassNameCamelCase}.Id;");
                    internalConstructorCode.AppendLine($"\t\t\t{parentModelClassName} = {parentModelClassNameCamelCase} ?? throw new ArgumentNullException(nameof({parentModelClassNameCamelCase}));");
                    internalConstructorCode.AppendLine($"\t\t\t{parentModelClassName}.{directory.Name.ToLower()}.Load(this, _ => {{ return new long().NewId(); }});");
                    internalConstructorCode.AppendLine();
                }

                publicConstructorCode.AppendLine($"public {modelClassName}() ");
                publicConstructorCode.AppendLine("\t\t{");
                if (string.IsNullOrEmpty(parentModelClassName))
                    publicConstructorCode.AppendLine("\t\t\tId = new long().NewId();");

                foreach (var childDirectory in childModelClassDirectories)
                {
                    var childModelClassName = Regex.Replace(childDirectory.Name, @"s$|es$", string.Empty);
                    var concurrentDictionaryName = childDirectory.Name.ToLower();
                    var readOnlyDictionaryName = $"_{concurrentDictionaryName}";
                    var iReadOnlyDictionaryName = childDirectory.Name;
                    childNamespaces.AppendLine($"using {modelClassNamespace}.{childDirectory.Name};");
                    childModelFactoryMethods.AppendLine($"public {childModelClassName} CreateNew{childModelClassName}() {{ return new {childModelClassName}(this); }}");

                    childModelClassDictionaries.AppendLine($"[DataMember] internal ConcurrentDictionary<long, {childModelClassName}> {concurrentDictionaryName};");
                    childModelClassDictionaries.AppendLine($"\t\tprivate ReadOnlyDictionary<long, {childModelClassName}> {readOnlyDictionaryName};");
                    childModelClassDictionaries.AppendLine($"\t\tpublic IReadOnlyDictionary<long, {childModelClassName}> {iReadOnlyDictionaryName} {{ get {{ return {readOnlyDictionaryName}; }} }}");
                    childModelClassDictionaries.AppendLine();
                    
                    publicConstructorCode.AppendLine($"\t\t\t{concurrentDictionaryName} = new ConcurrentDictionary<long, {childModelClassName}>();");
                    publicConstructorCode.AppendLine($"\t\t\t{readOnlyDictionaryName} = new ReadOnlyDictionary<long, {childModelClassName}>({concurrentDictionaryName});");

                    if(modelOnDeserailizedMethod.Length == 0)
                    {
                        modelOnDeserailizedMethod.AppendLine("[OnDeserialized]");
                        modelOnDeserailizedMethod.AppendLine("\t\tprivate void OnDeserialized(StreamingContext context)");
                        modelOnDeserailizedMethod.AppendLine("\t\t{");
                    }

                    modelOnDeserailizedMethod.AppendLine($"\t\t\t{readOnlyDictionaryName} = new ReadOnlyDictionary<long, {childModelClassName}>({concurrentDictionaryName});");

                    if(!string.IsNullOrEmpty(parentModelClassName))
                    {
                        internalConstructorCode.AppendLine($"\t\t\t{concurrentDictionaryName} = new ConcurrentDictionary<long, {childModelClassName}>();");
                        internalConstructorCode.AppendLine($"\t\t\t{readOnlyDictionaryName} = new ReadOnlyDictionary<long, {childModelClassName}>({concurrentDictionaryName});");
                    }
                }

                if(!string.IsNullOrEmpty(parentModelClassName))
                    internalConstructorCode.AppendLine("\t\t}");
                
                if(childModelClassDirectories.Length > 0)
                    modelOnDeserailizedMethod.AppendLine("\t\t}");

                publicConstructorCode.AppendLine("\t\t}");

                var modelGTcs = modelGTcsTemplate
                    .Replace("{modelClassChildNamespaces}", childNamespaces.ToString())
                    .Replace("{modelClassNamespace}", modelClassNamespace)
                    .Replace("{modelClassName}", modelClassName)
                    .Replace("{modelParentPropertyNameAttribute}", string.IsNullOrEmpty(parentModelClassName) ? string.Empty : $"[ParentProperty(nameof({modelClassName}.{parentModelClassName}))]")
                    .Replace("{publicConstructorCode}", publicConstructorCode.ToString())
                    .Replace("{childModelClassDictionaries}", childModelClassDictionaries.ToString())
                    .Replace("{childModelFactoryMethods}", childModelFactoryMethods.ToString())
                    .Replace("{modelOnDeserailizedMethod}", modelOnDeserailizedMethod.ToString())
                    .Replace("{parentIdProperty}", string.IsNullOrEmpty(parentModelClassName) ? string.Empty : $"{(parentModelClassName != rootClassName ? "[DataMember] " : string.Empty)}public long {parentModelIdPropertyName} {{ get; set;}}")
                    .Replace("{parentProperty}", string.IsNullOrEmpty(parentModelClassName) ? string.Empty :  $"public {parentModelClassName} {parentModelClassName} {{ get; internal set;}}")
                    .Replace("{modelRemoveMethod}", string.IsNullOrEmpty(parentModelClassName) ? string.Empty : $"public void Remove() {{ {parentModelClassName}.{directory.Name.ToLower()}.TryRemove(Id, out _); }}")
                    .Replace("{internalConstructorCode}", internalConstructorCode.ToString());

                var modelTcs = modelTcsTemplate.Replace("{modelClassNamespace}", modelClassNamespace).Replace("{modelClassName}", modelClassName);

                var modelGeneratedClassFileName = Path.Combine(workingDirectory, rootNamespace, directory.FullName.Replace(workingDirectory + Path.DirectorySeparatorChar, string.Empty), modelClassName + ".g.cs");
                var modelClassFileName = Path.Combine(workingDirectory, rootNamespace, directory.FullName.Replace(workingDirectory + Path.DirectorySeparatorChar, string.Empty), modelClassName + ".cs");
                
                File.WriteAllText(modelGeneratedClassFileName, modelGTcs);
                if (!File.Exists(modelClassFileName))
                    File.WriteAllText(modelClassFileName, modelTcs);

                GenerateModelCode(directory.GetDirectories(), workingDirectory, false, rootNamespace, rootClassName);
            }
        }

        private static void CreateSolutionAndProjects(DirectoryInfo rootDirectoryInfo, string workingDirectory, IEnumerable<DirectoryInfo> directories)
        {
            var rootDirectoryNameSingular = Regex.Replace(rootDirectoryInfo.Name, @"s$|es$", string.Empty);
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

            // ---------------  Model
            Console.Write($"Creating project {rootDirectoryNameSingular}.Model ...");
            dotNet.Arguments = $"new classlib --framework netcoreapp3.1 --name {rootDirectoryNameSingular}.Model --output {rootDirectoryNameSingular}.Model";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();
            File.Delete(Path.Combine(dotNet.WorkingDirectory, $"{rootDirectoryNameSingular}.Model","class1.cs"));

            dotNet.WorkingDirectory = Path.Combine(dotNet.WorkingDirectory, $"{rootDirectoryNameSingular}.Model");
            dotNet.Arguments = $"add package CTLite";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();
            dotNet.WorkingDirectory = workingDirectory;


            var newDirs = directories.Select(d => d.FullName.Replace(workingDirectory, Path.Combine(workingDirectory, $"{rootDirectoryNameSingular}.Model")));

            foreach (var newDir in newDirs)
                Directory.CreateDirectory(newDir);

            dotNet.Arguments = $"sln add .\\{rootDirectoryNameSingular}.Model\\{rootDirectoryNameSingular}.Model.csproj";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();
            // -------------------

            // --------------- Presentation
            Console.Write($"Creating project {rootDirectoryNameSingular}.Presentation ...");
            dotNet.Arguments = $"new classlib --framework netcoreapp3.1 --name {rootDirectoryNameSingular}.Presentation --output {rootDirectoryNameSingular}.Presentation";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();
            File.Delete(Path.Combine(dotNet.WorkingDirectory, $"{rootDirectoryNameSingular}.Presentation", "class1.cs"));

            dotNet.WorkingDirectory = Path.Combine(dotNet.WorkingDirectory, $"{rootDirectoryNameSingular}.Presentation");
            dotNet.Arguments = $"add package CTLite";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();
            dotNet.Arguments = $"add package CTLite.Data.MicrosoftSqlServer";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();

            dotNet.WorkingDirectory = workingDirectory;

            newDirs = directories.Select(d => d.FullName.Replace(workingDirectory, Path.Combine(workingDirectory, $"{rootDirectoryNameSingular}.Presentation")));

            foreach (var newDir in newDirs)
                Directory.CreateDirectory(newDir);

            // dotnet add app/app.csproj reference lib/lib.csproj
            dotNet.Arguments = $"add .\\{rootDirectoryNameSingular}.Presentation\\{rootDirectoryNameSingular}.Presentation.csproj reference .\\{rootDirectoryNameSingular}.Model\\{rootDirectoryNameSingular}.Model.csproj";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();

            dotNet.Arguments = $"sln add .\\{rootDirectoryNameSingular}.Presentation\\{rootDirectoryNameSingular}.Presentation.csproj";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();
            // -------------------

            // --------------- Service
            Console.Write($"Creating project {rootDirectoryNameSingular}.Service ...");
            dotNet.Arguments = $"new classlib --framework netcoreapp3.1 --name {rootDirectoryNameSingular}.Service --output {rootDirectoryNameSingular}.Service";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();
            File.Delete(Path.Combine(dotNet.WorkingDirectory, $"{rootDirectoryNameSingular}.Service", "class1.cs"));

            dotNet.WorkingDirectory = Path.Combine(dotNet.WorkingDirectory, $"{rootDirectoryNameSingular}.Service");
            dotNet.Arguments = $"add package CTLite";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();
            dotNet.WorkingDirectory = workingDirectory;

            newDirs = directories.Select(d => d.FullName.Replace(workingDirectory, Path.Combine(workingDirectory, $"{rootDirectoryNameSingular}.Service")));

            foreach (var newDir in newDirs)
                Directory.CreateDirectory(newDir);

            // dotnet add app/app.csproj reference lib/lib.csproj
            dotNet.Arguments = $"add .\\{rootDirectoryNameSingular}.Service\\{rootDirectoryNameSingular}.Service.csproj reference .\\{rootDirectoryNameSingular}.Model\\{rootDirectoryNameSingular}.Model.csproj";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();

            // dotnet add app/app.csproj reference lib/lib.csproj
            dotNet.Arguments = $"add .\\{rootDirectoryNameSingular}.Service\\{rootDirectoryNameSingular}.Service.csproj reference .\\{rootDirectoryNameSingular}.Presentation\\{rootDirectoryNameSingular}.Presentation.csproj";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();

            dotNet.Arguments = $"sln add .\\{rootDirectoryNameSingular}.Service\\{rootDirectoryNameSingular}.Service.csproj";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();
            // -------------------

            // --------------- Test
            Console.Write($"Creating project {rootDirectoryNameSingular}.Test ...");
            dotNet.Arguments = $"new mstest --framework netcoreapp3.1 --name {rootDirectoryNameSingular}.Test --output {rootDirectoryNameSingular}.Test";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();

            // dotnet add app/app.csproj reference lib/lib.csproj
            dotNet.Arguments = $"add .\\{rootDirectoryNameSingular}.Test\\{rootDirectoryNameSingular}.Test.csproj reference .\\{rootDirectoryNameSingular}.Model\\{rootDirectoryNameSingular}.Model.csproj";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();

            // dotnet add app/app.csproj reference lib/lib.csproj
            dotNet.Arguments = $"add .\\{rootDirectoryNameSingular}.Test\\{rootDirectoryNameSingular}.Test.csproj reference .\\{rootDirectoryNameSingular}.Presentation\\{rootDirectoryNameSingular}.Presentation.csproj";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();


            // dotnet add app/app.csproj reference lib/lib.csproj
            dotNet.Arguments = $"add .\\{rootDirectoryNameSingular}.Test\\{rootDirectoryNameSingular}.Test.csproj reference .\\{rootDirectoryNameSingular}.Service\\{rootDirectoryNameSingular}.Service.csproj";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();


            dotNet.Arguments = $"sln add .\\{rootDirectoryNameSingular}.Test\\{rootDirectoryNameSingular}.Test.csproj";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();
            // -------------------

            // -------------------- WebApi
            Console.Write($"Creating project {rootDirectoryNameSingular}.WebApi ...");
            dotNet.Arguments = $"new webapi --framework netcoreapp3.1 --name {rootDirectoryNameSingular}.WebApi --output {rootDirectoryNameSingular}.WebApi";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();
            Directory.Delete(Path.Combine(dotNet.WorkingDirectory, $"{rootDirectoryNameSingular}.WebApi", "Controllers"), true);
            File.Delete(Path.Combine(dotNet.WorkingDirectory, $"{rootDirectoryNameSingular}.WebApi", "WeatherForecast.cs"));

            // dotnet add app/app.csproj reference lib/lib.csproj
            dotNet.Arguments = $"add .\\{rootDirectoryNameSingular}.WebApi\\{rootDirectoryNameSingular}.WebApi.csproj reference .\\{rootDirectoryNameSingular}.Presentation\\{rootDirectoryNameSingular}.Presentation.csproj";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();


            // dotnet add app/app.csproj reference lib/lib.csproj
            dotNet.Arguments = $"add .\\{rootDirectoryNameSingular}.WebApi\\{rootDirectoryNameSingular}.WebApi.csproj reference .\\{rootDirectoryNameSingular}.Service\\{rootDirectoryNameSingular}.Service.csproj";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();

            dotNet.WorkingDirectory = Path.Combine(dotNet.WorkingDirectory, $"{rootDirectoryNameSingular}.WebApi");
            dotNet.Arguments = $"add package CTLite.AspNetCore";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();
            dotNet.Arguments = $"add package Microsoft.AspNetCore.Mvc.NewtonsoftJson";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();
            dotNet.WorkingDirectory = workingDirectory;

            dotNet.Arguments = $"sln add .\\{rootDirectoryNameSingular}.WebApi\\{rootDirectoryNameSingular}.WebApi.csproj";
            dotNetProc = Process.Start(dotNet);
            Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
            dotNetProc.WaitForExit();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CTLite.Tools.CTGen
{
    public class Program
    {
        private static readonly string  _modelGTcsTemplate = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Model.g.tcs"));
        private static readonly string  _modelTcsTemplate = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Model.tcs"));
        private static readonly string _presentationlGTcsTemplate = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Composite.g.tcs"));
        private static readonly string _presentationTcsTemplate = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Composite.tcs"));
        private static readonly string _presentationContainerGTcsTemplate = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "CompositeContainer.g.tcs"));
        private static readonly string _presentationContainerTcsTemplate = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "CompositeContainer.tcs"));

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

            //CreateSolutionAndProjects(rootDirectoryInfo, workingDirectory, directories);

            //Console.WriteLine("Generating model code ...");
            //GenerateModelCode(new DirectoryInfo[] { rootDirectoryInfo }, workingDirectory, true, string.Empty, string.Empty);

            Console.WriteLine("Generating presentation code ...");
            GeneratePresentationCode(new DirectoryInfo[] { rootDirectoryInfo }, workingDirectory, true, string.Empty, string.Empty, string.Empty, string.Empty);

            Console.WriteLine("Code generation complete!");
        }

        private static void GeneratePresentationCode(IEnumerable<DirectoryInfo> rootDirectoryInfos, string workingDirectory, bool isRootDirectory, string rootPresentationNamespace, string modelNamespace, string rootClassName, string parentClass)
        {
            foreach (var directory in rootDirectoryInfos)
            {
                var createContainers = new StringBuilder();
                var constructorsCode = new StringBuilder();
                var compositeRootInitializeCode = new StringBuilder();
                var compositeContainers = new StringBuilder();
                var compositeChildNamespaces = new StringBuilder();



                var childModelClassDirectories = directory.GetDirectories();

                var modelClassName = Regex.Replace(directory.Name, @"s$|es$", string.Empty);
                var baseCompositeClass = isRootDirectory ? "CompositeRoot" : "Composite";
                rootPresentationNamespace = isRootDirectory ? $"{modelClassName}.Presentation" : rootPresentationNamespace;
                var testAssemblyName = string.Empty;
                testAssemblyName = isRootDirectory ? $"{modelClassName}.Test" : testAssemblyName;
                modelNamespace = string.IsNullOrEmpty(modelNamespace) ? $"using {modelClassName}.Model.{directory.Name}" : modelNamespace += $".{directory.Name};";
                rootClassName = isRootDirectory ? $"{modelClassName}{baseCompositeClass}" : rootClassName;


                var compositeClassNamespace = rootPresentationNamespace + directory.FullName.Replace(workingDirectory, string.Empty).Replace(Path.DirectorySeparatorChar, '.');

                foreach (var childDirectory in childModelClassDirectories)
                {
                    var childModelClassName = Regex.Replace(childDirectory.Name, @"s$|es$", string.Empty);
                    createContainers.AppendLine($"\t\t\t{childDirectory.Name} = new {childModelClassName}CompositeContainer(this);");
                    compositeContainers.AppendLine($"[DataMember] public {childModelClassName}CompositeContainer {childDirectory.Name} {{ get; private set; }}");
                    compositeChildNamespaces.AppendLine($"using {compositeClassNamespace}.{childDirectory.Name};");
                }

                var compositeIdProperty = string.Empty;
                var compositeStateProperty = string.Empty;
                var compositeOriginalIdProperty = string.Empty;
                var compositeRemoveMethod = string.Empty;
                var keyPropertyAttribute = string.Empty;
                var compositeParentPropertyNameAttribute = string.Empty;
                var compositeParentProperty = string.Empty;
                var compositeInternalConstructor = string.Empty;

                if (isRootDirectory)
                {
                    constructorsCode.AppendLine($"public {modelClassName}{baseCompositeClass}() : base() {{ Initialize(); }}");
                    constructorsCode.AppendLine($"\t\tpublic {modelClassName}{baseCompositeClass}(params IService[] services) : base(services) {{ Initialize(); }}");
                    compositeRootInitializeCode.AppendLine("private void Initialize()");
                    compositeRootInitializeCode.AppendLine("\t\t{");
                    compositeRootInitializeCode.AppendLine($"\t\t\t{modelClassName}Model = new {modelClassName}();");
                    compositeRootInitializeCode.AppendLine(createContainers.ToString());
                    compositeRootInitializeCode.AppendLine("\t\t}");
                    compositeRootInitializeCode.AppendLine();
                    compositeRootInitializeCode.AppendLine("\t\tpublic override void InitializeCompositeModel(object model)");
                    compositeRootInitializeCode.AppendLine("\t\t{");
                    compositeRootInitializeCode.AppendLine($"\t\t\t{modelClassName}Model = model as {modelClassName};");
                    compositeRootInitializeCode.AppendLine(createContainers.ToString());
                    compositeRootInitializeCode.AppendLine("\t\t}");
                    compositeIdProperty = isRootDirectory ? $"public override long Id => {modelClassName}Model.Id;" : $"[DataMember] public long Id {{ get {{ return {modelClassName}Model.Id; }} }}";
                    compositeStateProperty = "public override CompositeState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }";
                }
                else
                {
                    compositeIdProperty = $"[DataMember] public long Id {{ get {{ return {modelClassName}Model.Id; }} }}";
                    compositeOriginalIdProperty = $"public long OriginalId {{ get {{ return {modelClassName}Model.OriginalId; }} }}";
                    compositeRemoveMethod = $"[Command] public void Remove() {{ Blogs.blogs.Remove(Id, true); }}";
                    compositeStateProperty = $"public override CompositeState State {{ get => {modelClassName}Model.State; set => {modelClassName}Model.State = value; }}";
                    keyPropertyAttribute = $"[KeyProperty(nameof({modelClassName}{baseCompositeClass}.Id), nameof({modelClassName}{baseCompositeClass}.OriginalId))]";
                    compositeParentPropertyNameAttribute = $"[ParentProperty(nameof({modelClassName}{baseCompositeClass}.{directory.Name}))]";
                    compositeParentProperty = $"public {modelClassName}CompositeContainer {directory.Name} {{ get; }}";
                    compositeInternalConstructor = $"internal {modelClassName}{baseCompositeClass}({modelClassName} {char.ToLowerInvariant(modelClassName[0]) + modelClassName.Substring(1)}, {modelClassName}CompositeContainer {char.ToLowerInvariant(modelClassName[0]) + modelClassName.Substring(1)}CompositeContainer ) {{ {modelClassName}Model = {char.ToLowerInvariant(modelClassName[0]) + modelClassName.Substring(1)}; {directory.Name} = {char.ToLowerInvariant(modelClassName[0]) + modelClassName.Substring(1)}CompositeContainer; {createContainers} }}";
                }

                var presentationlGTcs = _presentationlGTcsTemplate
                                            .Replace("{modelClassName}", modelClassName)
                                            .Replace("{baseCompositeClass}", baseCompositeClass)
                                            .Replace("{compositeRootConstructors}", constructorsCode.ToString())
                                            .Replace("{compositeIdProperty}", compositeIdProperty)
                                            .Replace("{compositeRootInitializeCode}", compositeRootInitializeCode.ToString())
                                            .Replace("{compositeContainers}", compositeContainers.ToString())
                                            .Replace("{compositeParentPropertyNameAttribute}", compositeParentPropertyNameAttribute)
                                            .Replace("{compositeClassNamespace}", compositeClassNamespace)
                                            .Replace("{compositeClassChildNamespaces}", compositeChildNamespaces.ToString())
                                            .Replace("{modelNamespace}", modelNamespace + ";")
                                            .Replace("{internalsVisibleToAttribute}", isRootDirectory ?  $"[assembly: InternalsVisibleTo(\"{testAssemblyName}\")]" : string.Empty)
                                            .Replace("{compositeStateProperty}", compositeStateProperty)
                                            .Replace("{compositeOriginalIdProperty}", compositeOriginalIdProperty)
                                            .Replace("{compositeRemoveMethod}", compositeRemoveMethod)
                                            .Replace("{compositeInternalConstructor}", compositeInternalConstructor)
                                            .Replace("{keyPropertyAttribute}", keyPropertyAttribute);


                var presentationTcs = _presentationTcsTemplate
                                        .Replace("{modelClassName}", modelClassName)
                                        .Replace("{baseCompositeClass}", baseCompositeClass)
                                        .Replace("{compositeClassNamespace}", compositeClassNamespace);

                var compositeGeneratedClassFileName = Path.Combine(workingDirectory, rootPresentationNamespace, directory.FullName.Replace(workingDirectory + Path.DirectorySeparatorChar, string.Empty), modelClassName + baseCompositeClass + ".g.cs");
                var compositeClassFileName = Path.Combine(workingDirectory, rootPresentationNamespace, directory.FullName.Replace(workingDirectory + Path.DirectorySeparatorChar, string.Empty), modelClassName + baseCompositeClass + ".cs");

                Directory.CreateDirectory(Path.GetDirectoryName(compositeGeneratedClassFileName));

                File.WriteAllText(compositeGeneratedClassFileName, presentationlGTcs);
                if (!File.Exists(compositeClassFileName))
                    File.WriteAllText(compositeClassFileName, _presentationTcsTemplate);

                if(!isRootDirectory)
                {
                    var presentationContainerGTcs = _presentationContainerGTcsTemplate
                                .Replace("{compositeClassNamespace}", compositeClassNamespace)
                                .Replace("{modelClassName}", modelClassName)
                                .Replace("{modelNamespace}", modelNamespace + ";")
                                .Replace("{folderName}", directory.Name)
                                .Replace("{folderNameCamel}", $"{char.ToLowerInvariant(directory.Name[0]) + directory.Name.Substring(1)}")
                                .Replace("{parentClass}", parentClass);
                }

                parentClass = $"{modelClassName}{baseCompositeClass}";

                GeneratePresentationCode(directory.GetDirectories(), workingDirectory, false, rootPresentationNamespace, modelNamespace, rootClassName, parentClass);
            }


        }

        private static void GenerateModelCode(IEnumerable<DirectoryInfo> rootDirectoryInfos, string workingDirectory, bool isRootDirectory, string rootNamespace, string rootClassName)
        {
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

                var modelGTcs = _modelGTcsTemplate
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

                var modelTcs = _modelTcsTemplate.Replace("{modelClassNamespace}", modelClassNamespace).Replace("{modelClassName}", modelClassName);

                var modelGeneratedClassFileName = Path.Combine(workingDirectory, rootNamespace, directory.FullName.Replace(workingDirectory + Path.DirectorySeparatorChar, string.Empty), modelClassName + ".g.cs");
                var modelClassFileName = Path.Combine(workingDirectory, rootNamespace, directory.FullName.Replace(workingDirectory + Path.DirectorySeparatorChar, string.Empty), modelClassName + ".cs");

                Directory.CreateDirectory(Path.GetDirectoryName(modelGeneratedClassFileName));

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

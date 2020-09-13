// CTLiteDemo - Made in the USA - Indianapolis, IN  - Copyright (c) 2020 Matt J. Crouch

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN 
// NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CTLite.Data.MicrosoftSqlServer;

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
        private static readonly string _webApiControllerTcsTemplate = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "WebApiController.tcs"));
        private static readonly string _webApiStartupTcsTemplate = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "WebApiStartup.tcs"));
        private static readonly string _webApiStartupBaseTcsTemplate = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "WebApiStartupBase.tcs"));
        private static readonly string _sqlDatabaseCreateTsqlTemplate = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "SqlDatabaseCreate.tsql"));

        static void Main(string[] args)
        {
            Console.WriteLine("CTGen - code generation for CTLite");

            var proc = Process.GetCurrentProcess();

            var rootDirectory = string.Empty;
            var applicationType = string.Empty;
            var shouldGenerateProjects = false;
            var shouldGenerateCode = false;
            var shouldRunSqlScripts = false;
            var shouldCreateDatabase = false;
            var masterConnectionString = string.Empty;
            var dbConnectionString = string.Empty;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-r":
                        rootDirectory = args[i + 1];
                        break;
                    case "-a":
                        applicationType = args[i + 1];
                        break;
                    case "-p":
                        shouldGenerateProjects = true;
                        break;
                    case "-c":
                        shouldGenerateCode = true;
                        break;
                    case "-s":
                        shouldRunSqlScripts = true;
                        break;
                    case "-sc":
                        shouldCreateDatabase = true;
                        break;
                    case "-mcs":
                        masterConnectionString = args[i + 1];
                        break;
                    case "-dbcs":
                        dbConnectionString = args[i + 1];
                        break;
                    default:
                        break;
                }
            }

            if (string.IsNullOrEmpty(rootDirectory))
            {
                Console.Error.WriteLine($"Usage: {proc.ProcessName} -r [root directory] -a [application type] -p -c -s -mcs [master db connection string]");
                Console.Error.WriteLine($"-r : root directory of model hierarchy{Environment.NewLine}-a : application type to generate (ex. webapi){Environment.NewLine}-p : generate solution and projects{Environment.NewLine}-c : generate code{Environment.NewLine}-s : run sql scripts{Environment.NewLine}-mcs : master db connection string");
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

            if(shouldGenerateProjects)
                CreateSolutionAndProjects(rootDirectoryInfo, workingDirectory, directories, applicationType);

            if (shouldGenerateCode)
            {
                Console.WriteLine("Generating model code ...");
                GenerateModelCode(new DirectoryInfo[] { rootDirectoryInfo }, workingDirectory, true, string.Empty, string.Empty, string.Empty);

                Console.WriteLine("Generating presentation code ...");
                GeneratePresentationCode(new DirectoryInfo[] { rootDirectoryInfo }, workingDirectory, true, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

                switch (applicationType)
                {
                    case "webapi":
                        Console.WriteLine("Generating ASP.NET API code ...");
                        GenerateWebApiCode(rootDirectoryInfo, workingDirectory);
                        break;
                    default:
                        break;
                }

                Console.WriteLine("Code generation complete!");
            }

            var dbName = Regex.Replace(rootDirectoryInfo.Name, @"s$|es$", string.Empty);
            dbConnectionString += $"Initial Catalog={dbName};";

            if (shouldCreateDatabase)
            {
                Console.WriteLine("Creating database ...");
                CreateDatabase(workingDirectory, masterConnectionString, dbName);
            }

            if(shouldRunSqlScripts)
            {
                Console.WriteLine("Running SQL scripts ...");
                RunSqlScripts(Path.Combine(workingDirectory, $"{dbName}.Model", rootDirectoryInfo.Name), dbConnectionString);

                Console.WriteLine("SQL scripts complete!");
            }
        }

        private static void CreateDatabase(string workingDirectory, string masterDbConnectionString, string dbName)
        {
            var createDatabaseSqlFile = Path.Combine(workingDirectory, $"{dbName}.Model", $"000-{dbName}Database-Create.sql");
            var repository = MicrosoftSqlServerRepository.Create();
            using var connection = repository.OpenConnection(masterDbConnectionString);
            repository.Execute<object>(connection, null, File.ReadAllText(createDatabaseSqlFile), null);
        }

        private static void RunSqlScripts(string rootDirectory, string dbConnectionString)
        {
            var repository = MicrosoftSqlServerRepository.Create();

            using (var connection = repository.OpenConnection(dbConnectionString))
            using (var transaction = repository.BeginTransaction(connection))
            {
                repository.CreateHelperStoredProcedures(connection, transaction);
                repository.CommitTransaction(transaction);
            }

            using (var connection = repository.OpenConnection(dbConnectionString))
            using (var transaction = repository.BeginTransaction(connection))
            {
                var directories =
                    Directory.GetDirectories(rootDirectory, string.Empty, SearchOption.AllDirectories)
                    .GroupBy(d => new { Depth = d.Split(Path.DirectorySeparatorChar).Count(), Directory = d })
                    .OrderBy(g => g.Key.Depth).ThenBy(g => g.Key.Directory)
                    .Select(g => g.Key.Directory);

                foreach (var directory in directories)
                {
                    foreach (var sqlScriptFile in Directory.GetFiles(directory, "*.sql"))
                    {
                        var script = File.ReadAllText(sqlScriptFile);
                        repository.Execute<object>(connection, transaction, script, null);
                    }
                }

                repository.CommitTransaction(transaction);
            }
        }

        private static void GenerateWebApiCode(DirectoryInfo rootDirectoryInfo, string workingDirectory)
        {
            var modelClassName = Regex.Replace(rootDirectoryInfo.Name, @"s$|es$", string.Empty);
            var webApiProjectDirectory = Path.Combine(workingDirectory, $"{modelClassName}.WebApi");
            var webApiStartupCsFilePath = Path.Combine(webApiProjectDirectory, "Startup.cs");
            var webApiStartupBaseCsFilePath = Path.Combine(webApiProjectDirectory, "StartupBase.cs");
            var webApiControllerCsFilePath = Path.Combine(webApiProjectDirectory, $"{modelClassName}Controller.cs");

            if (!File.Exists(webApiStartupCsFilePath))
            {
                var _webApiStartupTcsCode = _webApiStartupTcsTemplate.Replace("{modelClassName}", modelClassName);
                File.WriteAllText(webApiStartupCsFilePath, _webApiStartupTcsCode);
            }

            var webApiStartupBaseCsCode = _webApiStartupBaseTcsTemplate.Replace("{modelClassName}", modelClassName);
            File.WriteAllText(webApiStartupBaseCsFilePath, webApiStartupBaseCsCode);

            var rootPresentationNamespace = $"{modelClassName}.Presentation";
            var rootFolderName = rootDirectoryInfo.Name;

            var webApiControllerCode = _webApiControllerTcsTemplate
                                        .Replace("{modelClassName}", modelClassName)
                                        .Replace("{rootFolderName}", rootFolderName)
                                        .Replace("{rootPresentationNamespace}", rootPresentationNamespace);

            if (!File.Exists(webApiControllerCsFilePath))
                File.WriteAllText(webApiControllerCsFilePath, webApiControllerCode);

        }

        private static void GeneratePresentationCode(IEnumerable<DirectoryInfo> rootDirectoryInfos, string workingDirectory, bool isRootDirectory, string rootPresentationNamespace, string rootModelNamespace, string rootClassName, string parentClass, string parentClassPropertyName)
        {
            for(int directoryIndex = 0; directoryIndex < rootDirectoryInfos.Count(); directoryIndex++)
            {
                var directory = rootDirectoryInfos.ElementAt(directoryIndex);
                var createContainers = new StringBuilder();
                var constructorsCode = new StringBuilder();
                var compositeRootInitializeCode = new StringBuilder();
                var compositeContainers = new StringBuilder();
                var compositeChildNamespaces = new StringBuilder();

                var childModelClassDirectories = directory.GetDirectories();

                var modelClassName = Regex.Replace(directory.Name, @"s$|es$", string.Empty);
                var baseCompositeClass = isRootDirectory ? "CompositeRoot" : "Composite";
                rootPresentationNamespace = isRootDirectory ? $"{modelClassName}.Presentation" : rootPresentationNamespace;

                rootModelNamespace = isRootDirectory ? $"using {modelClassName}.Model" : rootModelNamespace;

                var testAssemblyName = string.Empty;
                testAssemblyName = isRootDirectory ? $"{modelClassName}.Test" : testAssemblyName;
                var modelNamespace = rootModelNamespace + directory.FullName.Replace(workingDirectory, string.Empty).Replace(Path.DirectorySeparatorChar, '.');
                rootClassName = isRootDirectory ? $"{modelClassName}{baseCompositeClass}" : rootClassName;

                var compositeClassNamespace = rootPresentationNamespace + directory.FullName.Replace(workingDirectory, string.Empty).Replace(Path.DirectorySeparatorChar, '.');

                foreach (var childDirectory in childModelClassDirectories)
                {
                    var childModelClassName = Regex.Replace(childDirectory.Name, @"s$|es$", string.Empty);
                    createContainers.AppendLine($"\t\t\t{childDirectory.Name} = new {childModelClassName}CompositeContainer(this);");
                    compositeContainers.AppendLine($"[DataMember] public {childModelClassName}CompositeContainer {childDirectory.Name} {{ get; private set; }}");
                    compositeChildNamespaces.AppendLine($"using {compositeClassNamespace}.{childDirectory.Name};");
                }

                var compositeOriginalIdProperty = string.Empty;
                var compositeRemoveMethod = string.Empty;
                var keyPropertyAttribute = string.Empty;
                var compositeParentPropertyNameAttribute = string.Empty;
                var compositeParentProperty = string.Empty;
                var compositeInternalConstructor = string.Empty;
                string compositeIdProperty;

                string compositeStateProperty;
                string internalModel;

                if (isRootDirectory)
                {
                    constructorsCode.AppendLine($"public {modelClassName}{baseCompositeClass}() : base() {{ Initialize(); }}");
                    constructorsCode.AppendLine($"\t\tpublic {modelClassName}{baseCompositeClass}(params IService[] services) : base(services) {{ Initialize(); }}");
                    compositeRootInitializeCode.AppendLine("private void Initialize()");
                    compositeRootInitializeCode.AppendLine("\t\t{");
                    compositeRootInitializeCode.AppendLine($"\t\t\t{modelClassName}Model = new Model.{directory.Name}.{modelClassName}();");
                    compositeRootInitializeCode.AppendLine(createContainers.ToString());
                    compositeRootInitializeCode.AppendLine("\t\t}");
                    compositeRootInitializeCode.AppendLine();
                    compositeRootInitializeCode.AppendLine("\t\tpublic override void InitializeCompositeModel(object model)");
                    compositeRootInitializeCode.AppendLine("\t\t{");
                    compositeRootInitializeCode.AppendLine($"\t\t\t{modelClassName}Model = model as Model.{directory.Name}.{modelClassName};");
                    compositeRootInitializeCode.AppendLine(createContainers.ToString());
                    compositeRootInitializeCode.AppendLine("\t\t}");
                    compositeIdProperty = isRootDirectory ? $"public override long Id => {modelClassName}Model.Id;" : $"[DataMember] public long Id {{ get {{ return {modelClassName}Model.Id; }} }}";
                    compositeStateProperty = "public override CompositeState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }";
                    internalModel = $"internal Model.{directory.Name}.{modelClassName} {modelClassName}Model;";
                }
                else
                {
                    compositeIdProperty = $"[DataMember] public long Id {{ get {{ return {modelClassName}Model.Id; }} }}";
                    compositeOriginalIdProperty = $"public long OriginalId {{ get {{ return {modelClassName}Model.OriginalId; }} }}";
                    compositeRemoveMethod = $"[Command] public void Remove() {{ {directory.Name}.{char.ToLowerInvariant(directory.Name[0]) + directory.Name.Substring(1)}.Remove(Id, true); }}";
                    compositeStateProperty = $"public override CompositeState State {{ get => {modelClassName}Model.State; set => {modelClassName}Model.State = value; }}";
                    keyPropertyAttribute = $"[KeyProperty(nameof({modelClassName}{baseCompositeClass}.Id), nameof({modelClassName}{baseCompositeClass}.OriginalId))]";
                    compositeParentPropertyNameAttribute = $"[ParentProperty(nameof({modelClassName}{baseCompositeClass}.{directory.Name}))]";
                    compositeParentProperty = $"public {modelClassName}CompositeContainer {directory.Name} {{ get; }}";
                    compositeInternalConstructor = $"internal {modelClassName}{baseCompositeClass}({modelClassName} {char.ToLowerInvariant(modelClassName[0]) + modelClassName.Substring(1)}, {modelClassName}CompositeContainer {char.ToLowerInvariant(modelClassName[0]) + modelClassName.Substring(1)}CompositeContainer ) {{ {modelClassName}Model = {char.ToLowerInvariant(modelClassName[0]) + modelClassName.Substring(1)}; {directory.Name} = {char.ToLowerInvariant(modelClassName[0]) + modelClassName.Substring(1)}CompositeContainer; {createContainers} }}";
                    internalModel = $"internal {modelClassName} {modelClassName}Model;";
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
                                            .Replace("{keyPropertyAttribute}", keyPropertyAttribute)
                                            .Replace("{compositeParentProperty}", compositeParentProperty)
                                            .Replace("{internalModel}", internalModel);


                var presentationTcs = _presentationTcsTemplate
                                        .Replace("{modelClassName}", modelClassName)
                                        .Replace("{baseCompositeClass}", baseCompositeClass)
                                        .Replace("{compositeClassNamespace}", compositeClassNamespace);

                var compositeGeneratedClassFileName = Path.Combine(workingDirectory, rootPresentationNamespace, directory.FullName.Replace(workingDirectory + Path.DirectorySeparatorChar, string.Empty), modelClassName + baseCompositeClass + ".g.cs");
                var compositeClassFileName = Path.Combine(workingDirectory, rootPresentationNamespace, directory.FullName.Replace(workingDirectory + Path.DirectorySeparatorChar, string.Empty), modelClassName + baseCompositeClass + ".cs");

                Directory.CreateDirectory(Path.GetDirectoryName(compositeGeneratedClassFileName));

                File.WriteAllText(compositeGeneratedClassFileName, presentationlGTcs);
                if (!File.Exists(compositeClassFileName))
                    File.WriteAllText(compositeClassFileName, presentationTcs);

                if(!isRootDirectory)
                {
                    var presentationContainerGTcs = _presentationContainerGTcsTemplate
                                .Replace("{compositeClassNamespace}", compositeClassNamespace)
                                .Replace("{modelClassName}", modelClassName)
                                .Replace("{modelNamespace}", modelNamespace + ";")
                                .Replace("{folderName}", directory.Name)
                                .Replace("{folderNameCamel}", $"{char.ToLowerInvariant(directory.Name[0]) + directory.Name.Substring(1)}")
                                .Replace("{parentClass}", parentClass)
                                .Replace("{parentClassCamel}", $"{char.ToLowerInvariant(parentClass[0]) + parentClass.Substring(1)}")
                                .Replace("{compositeParentProperty}", $"public {parentClass} {parentClassPropertyName} {{ get; private set; }}")
                                .Replace("{parentClassPropertyName}", parentClassPropertyName);

                    var presentationContainerTcs = _presentationContainerTcsTemplate
                                .Replace("{compositeClassNamespace}", compositeClassNamespace)
                                .Replace("{modelClassName}", modelClassName)
                                .Replace("{folderNameCamel}", $"{char.ToLowerInvariant(directory.Name[0]) + directory.Name.Substring(1)}");

                    var compositeGeneratedContainerClassFileName = Path.Combine(workingDirectory, rootPresentationNamespace, directory.FullName.Replace(workingDirectory + Path.DirectorySeparatorChar, string.Empty), modelClassName + baseCompositeClass + "Container.g.cs");
                    var compositeContainerClassFileName = Path.Combine(workingDirectory, rootPresentationNamespace, directory.FullName.Replace(workingDirectory + Path.DirectorySeparatorChar, string.Empty), modelClassName + baseCompositeClass + "Container.cs");

                    File.WriteAllText(compositeGeneratedContainerClassFileName, presentationContainerGTcs);
                    if (!File.Exists(compositeContainerClassFileName))
                        File.WriteAllText(compositeContainerClassFileName, presentationContainerTcs);
                }

                if(directoryIndex == rootDirectoryInfos.Count() - 1)
                {
                    parentClass = $"{modelClassName}{baseCompositeClass}";
                    parentClassPropertyName = modelClassName;
                }

                GeneratePresentationCode(directory.GetDirectories(), workingDirectory, false, rootPresentationNamespace, rootModelNamespace, rootClassName, parentClass, parentClassPropertyName);
            }
        }

        private static void GenerateModelCode(IEnumerable<DirectoryInfo> rootDirectoryInfos, string workingDirectory, bool isRootDirectory, string rootNamespace, string rootClassName, string parentClass)
        {

            for (int directoryIndex = 0; directoryIndex < rootDirectoryInfos.Count(); directoryIndex++)
            {
                var directory = rootDirectoryInfos.ElementAt(directoryIndex);

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
                
                if(isRootDirectory)
                {
                    var sqlDatabaseCreateFile = Path.Combine(workingDirectory, rootNamespace, $"000-{modelClassName}Database-Create.sql");
                    var sqlDatabaseCreateTsql = _sqlDatabaseCreateTsqlTemplate
                                                .Replace("{modelClassName}", modelClassName);

                    if (!File.Exists(sqlDatabaseCreateFile))
                        File.WriteAllText(sqlDatabaseCreateFile, sqlDatabaseCreateTsql);
                }
                
                Directory.CreateDirectory(Path.GetDirectoryName(modelGeneratedClassFileName));

                File.WriteAllText(modelGeneratedClassFileName, modelGTcs);
                if (!File.Exists(modelClassFileName))
                    File.WriteAllText(modelClassFileName, modelTcs);

                if(!isRootDirectory)
                {
                    var createTableSqlFilename = Path.Combine(workingDirectory, rootNamespace, directory.FullName.Replace(workingDirectory + Path.DirectorySeparatorChar, string.Empty), directoryIndex.ToString().PadLeft(3,'0') + "-Table-" + modelClassName + ".sql");
                    var createTableSql = parentClass != rootClassName ? $"EXEC CreateTable '{modelClassName}', '{parentClass}'" : $"EXEC CreateTable '{modelClassName}'";
                    File.WriteAllText(createTableSqlFilename, createTableSql);
                }

                if (directoryIndex == rootDirectoryInfos.Count() - 1)
                    parentClass = modelClassName;
                

                GenerateModelCode(directory.GetDirectories(), workingDirectory, false, rootNamespace, rootClassName, parentClass);
            }
        }

        private static void CreateSolutionAndProjects(DirectoryInfo rootDirectoryInfo, string workingDirectory, IEnumerable<DirectoryInfo> directories, string applicationType)
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
            {
                Directory.CreateDirectory(newDir);
                File.WriteAllText(Path.Combine(newDir, "README.txt"), $"TODO: Create IService implementations in this directory");
            }

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

            if(applicationType == "webapi")
            {
                // -------------------- WebApi
                Console.Write($"Creating project {rootDirectoryNameSingular}.WebApi ...");
                dotNet.Arguments = $"new webapi --framework netcoreapp3.1 --name {rootDirectoryNameSingular}.WebApi --output {rootDirectoryNameSingular}.WebApi";
                dotNetProc = Process.Start(dotNet);
                Console.WriteLine(dotNetProc.StandardOutput.ReadToEnd());
                dotNetProc.WaitForExit();

                var defaultWebApiControllerDirectory = Path.Combine(dotNet.WorkingDirectory, $"{rootDirectoryNameSingular}.WebApi", "Controllers");
                if (Directory.Exists(defaultWebApiControllerDirectory))
                    Directory.Delete(Path.Combine(dotNet.WorkingDirectory, $"{rootDirectoryNameSingular}.WebApi", "Controllers"), true);

                File.Delete(Path.Combine(dotNet.WorkingDirectory, $"{rootDirectoryNameSingular}.WebApi", "WeatherForecast.cs"));
                File.Delete(Path.Combine(dotNet.WorkingDirectory, $"{rootDirectoryNameSingular}.WebApi", "Startup.cs"));

                var launchSettingsJsonFileName = Path.Combine(dotNet.WorkingDirectory, $"{rootDirectoryNameSingular}.WebApi", "Properties", "launchSettings.json");
                var launchSettingsJson = JObject.Parse(File.ReadAllText(launchSettingsJsonFileName));
                launchSettingsJson.Remove("profiles");
                File.WriteAllText(launchSettingsJsonFileName, launchSettingsJson.ToString());

                File.Delete(Path.Combine(dotNet.WorkingDirectory, $"{rootDirectoryNameSingular}.WebApi", "appsettings.json"));
                File.Delete(Path.Combine(dotNet.WorkingDirectory, $"{rootDirectoryNameSingular}.WebApi", "appsettings.Development.json"));

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
}

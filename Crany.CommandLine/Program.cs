using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Xml.Linq;

namespace Crany.CommandLine;

internal static class Program
{
    static async Task<int> Main(string[] args)
    {
        // RootCommand initialization
        var rootCommand = new RootCommand("Crany CLI: Generate .nuspec and .targets files for NuGet packages");

        #region Generate Nuspec Command

        var generateNuspecCommand = new Command("generate-nuspec", "Generate a .nuspec file from a proto file")
        {
            new Argument<string>("protoPath", "Path to the .proto file"),
            new Option<bool>("--generate-targets", "Optionally generate a corresponding .targets file")
        };
        generateNuspecCommand.Handler = CommandHandler.Create<string, bool>(GenerateNuspec);

        #endregion

        #region Generate Targets Command

        var generateTargetsCommand = new Command("generate-targets", "Generate a .targets file from a .nuspec file")
        {
            new Argument<string>("nuspecPath", "Path to the .nuspec file")
        };
        generateTargetsCommand.Handler = CommandHandler.Create<string>(nuspecPath =>
        {
            var targetsPath = Path.Combine(Path.GetDirectoryName(nuspecPath) ?? ".",
                $"{Path.GetFileNameWithoutExtension(nuspecPath)}.targets");
            var targetsGenerator = new TargetsGenerator();
            targetsGenerator.Generate(nuspecPath, targetsPath);
        });

        #endregion

        #region Pack Command

        var packCommand = new Command("pack", "Pack a .nuspec file into a NuGet package")
        {
            new Argument<string>("nuspecPath", "Path to the .nuspec file"),
            new Option<string?>("--output-dir", "Directory to save the .nupkg file"),
            new Option<bool>("--push", "Push the package to a NuGet server"),
            new Option<string?>("--api-key", "API key for NuGet server"),
            new Option<string?>("--source", "NuGet server source URL")
        };
        packCommand.Handler = CommandHandler.Create<string, string?, bool, string?, string?>(PackNuspec);

        #endregion

        // Add commands to root
        rootCommand.Add(generateNuspecCommand);
        rootCommand.Add(generateTargetsCommand);
        rootCommand.Add(packCommand);

        // CLI Version Option
        var versionOption = new Option<bool>("--version", "Show version information");
        rootCommand.AddOption(versionOption);

        rootCommand.SetHandler((InvocationContext context) =>
        {
            if (context.ParseResult.HasOption(versionOption))
            {
                Console.WriteLine("Crany CLI v2.1.0");
                context.ExitCode = 0;
            }
        });

        // CLI Configuration
        var configuration = new CommandLineConfiguration(rootCommand);
        var parser = new Parser(configuration);

        return await parser.InvokeAsync(args);
    }

    static void GenerateNuspec(string protoPath, bool generateTargets)
    {
        if (!File.Exists(protoPath))
        {
            Console.WriteLine($"Error: Proto file not found: {protoPath}");
            return;
        }

        // Collect metadata from the user
        var id = PromptInput("Package ID", Path.GetFileNameWithoutExtension(protoPath) + "Proto");
        var version = PromptInput("Version (Major.Minor.Patch)", "1.0.0");
        var authors = PromptInput("Authors", "Unknown");
        var description = PromptInput("Description", "No description provided");
        var tags = PromptInput("Tags (comma-separated)", "protobuf,grpc");
        var formattedTags = string.Join(" ", tags.Split(',').Select(t => t.Trim()));

        // Parse proto file for imports
        var imports = ExtractProtoImports(protoPath);

        // Generate .nuspec file
        var nuspecPath = Path.Combine(Path.GetDirectoryName(protoPath) ?? ".", $"{id}.nuspec");
        var nuspecGenerator = new NuspecGenerator();
        nuspecGenerator.Generate(protoPath, nuspecPath, id, version, authors, description, formattedTags, imports);

        Console.WriteLine($"Nuspec file generated: {nuspecPath}");

        // Optionally generate .targets file
        if (generateTargets)
        {
            GenerateTargets(nuspecPath);
        }

        Console.WriteLine("Please review the dependencies in the nuspec file.");
    }

    static void GenerateTargets(string nuspecPath)
    {
        if (!File.Exists(nuspecPath))
        {
            Console.WriteLine($"Error: Nuspec file not found: {nuspecPath}");
            return;
        }

        var id = XDocument.Load(nuspecPath)
            .Root?.Element("metadata")?.Element("id")?.Value?.ToLower();

        if (string.IsNullOrEmpty(id))
        {
            Console.WriteLine("Error: Could not extract ID from the nuspec file.");
            return;
        }

        var targetsPath = Path.Combine(Path.GetDirectoryName(nuspecPath) ?? ".", $"{id}.targets");
        var targetsGenerator = new TargetsGenerator();
        targetsGenerator.Generate(nuspecPath, targetsPath);
    }

    static void PackNuspec(string nuspecPath, string outputDir, bool push, string? apiKey, string? source)
    {
        if (!File.Exists(nuspecPath))
        {
            Console.WriteLine($"Error: .nuspec file not found: {nuspecPath}");
            return;
        }

        Console.WriteLine($"Packing .nuspec file: {nuspecPath}");

        var nugetPath = "nuget"; // Assumes `nuget` CLI is available in the PATH
        var dotnetPath = "dotnet"; // Assumes `dotnet` CLI is available in the PATH
        var packArguments = $"pack {nuspecPath} -OutputDirectory {outputDir}";

        // Step 1: Pack the NuGet package
        var packProcess = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = nugetPath,
                Arguments = packArguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        packProcess.Start();
        var packOutput = packProcess.StandardOutput.ReadToEnd();
        var packError = packProcess.StandardError.ReadToEnd();
        packProcess.WaitForExit();

        if (packProcess.ExitCode != 0)
        {
            Console.WriteLine(packError);
            Console.WriteLine("Error: Failed to pack .nuspec file.");
            return;
        }

        Console.WriteLine(packOutput);
        Console.WriteLine("NuGet package successfully created.");

        // Extract version from nuspec file
        var nuspecXml = XDocument.Load(nuspecPath);
        var version = nuspecXml.Root?
            .Element("metadata")?
            .Element("version")?
            .Value;

        if (string.IsNullOrEmpty(version))
        {
            Console.WriteLine("Error: Version could not be extracted from the .nuspec file.");
            return;
        }

        // Step 2: Push the NuGet package if --push is specified
        if (push)
        {
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(source))
            {
                Console.WriteLine("Error: --api-key and --source must be provided to push the package.");
                return;
            }

            var packageFileName = $"{Path.GetFileNameWithoutExtension(nuspecPath)}.{version}.nupkg";
            var packageFilePath = Path.Combine(outputDir, packageFileName);

            if (!File.Exists(packageFilePath))
            {
                Console.WriteLine($"Error: NuGet package file not found: {packageFilePath}");
                return;
            }

            var pushArguments = $"nuget push {packageFilePath} --source \"{source}\" --api-key {apiKey}";

            var pushProcess = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = dotnetPath,
                    Arguments = pushArguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            pushProcess.Start();
            var pushOutput = pushProcess.StandardOutput.ReadToEnd();
            var pushError = pushProcess.StandardError.ReadToEnd();
            pushProcess.WaitForExit();

            if (pushProcess.ExitCode != 0)
            {
                Console.WriteLine(pushError);
                Console.WriteLine("Error: Failed to push the NuGet package.");
                return;
            }

            Console.WriteLine(pushOutput);
            Console.WriteLine("NuGet package successfully pushed.");
        }
    }

    static string PromptInput(string prompt, string defaultValue)
    {
        Console.Write($"{prompt} [{defaultValue}]: ");
        var input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? defaultValue : input;
    }

    static IEnumerable<string> ExtractProtoImports(string protoPath)
    {
        var imports = new List<string>();

        foreach (var line in File.ReadLines(protoPath))
        {
            if (line.TrimStart().StartsWith("import"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(line, "\"(.*?)\"");
                if (match.Success)
                {
                    imports.Add(match.Groups[1].Value);
                }
            }
        }

        return imports;
    }
}

internal class NuspecGenerator
{
    public void Generate(string protoPath, string outputPath, string id, string version, string authors,
        string description, string tags, IEnumerable<string> imports)
    {
        var doc = new XDocument(
            new XElement("package",
                new XElement("metadata",
                    new XElement("id", id),
                    new XElement("version", version),
                    new XElement("authors", authors),
                    new XElement("description", description),
                    new XElement("tags", tags),
                    new XElement("developmentDependency", true),
                    new XElement("dependencies",
                        imports.Select(import => new XElement("dependency",
                            new XAttribute("id", Path.GetFileNameWithoutExtension(import) + "Proto"),
                            new XAttribute("version", "1.0.0"),
                            new XAttribute("include", $"contentFiles/any/any/Protos/{import}")
                        ))
                    ),
                    new XElement("contentFiles",
                        new XElement("files",
                            new XAttribute("include", $"contentFiles/any/any/Protos/{Path.GetFileName(protoPath)}"),
                            new XAttribute("buildAction", "None"),
                            new XAttribute("copyToOutput", false),
                            new XAttribute("flatten", false)
                        )
                    )
                ),
                new XElement("files",
                    new XElement("file",
                        new XAttribute("src", Path.GetFileName(protoPath)),
                        new XAttribute("target", $"contentFiles/any/any/Protos/{Path.GetFileName(protoPath)}")
                    ),
                    new XElement("file",
                        new XAttribute("src", $"{id}.targets"),
                        new XAttribute("target", "build/" + $"{id}.targets")
                    )
                )
            )
        );

        doc.Save(outputPath);
    }
}

internal class TargetsGenerator
{
    public void Generate(string nuspecPath, string outputPath)
    {
        if (!File.Exists(nuspecPath))
        {
            Console.WriteLine($"Error: File not found: {nuspecPath}");
            return;
        }

        Console.WriteLine($"Processing .nuspec file: {nuspecPath}");

        var nuspec = XDocument.Load(nuspecPath);
        var metadata = nuspec.Root?.Element("metadata");

        if (metadata == null)
        {
            Console.WriteLine("Error: Invalid .nuspec file. 'metadata' element is missing.");
            return;
        }

        var id = metadata.Element("id")?.Value;
        var version = metadata.Element("version")?.Value;
        var dependencies = metadata.Element("dependencies")?.Elements("dependency");
        var contentFiles = metadata.Element("contentFiles")?.Elements("files");

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(version))
        {
            Console.WriteLine("Error: 'id' and 'version' must be specified in the .nuspec file.");
            return;
        }

        using var writer = new StreamWriter(outputPath);

        writer.WriteLine(@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">");

        // Write content files and dependencies
        WriteProtobufGroup(writer, id, version, contentFiles, dependencies);

        // Write copy target
        WriteCopyTarget(writer, "CopyProtosToProject", "@(Protobuf)", "$(MSBuildProjectDirectory)/Protos");

        writer.WriteLine("</Project>");

        Console.WriteLine($"Targets file generated: {outputPath}");
    }

    private void WriteProtobufGroup(StreamWriter writer, string id, string version, IEnumerable<XElement>? contentFiles,
        IEnumerable<XElement>? dependencies)
    {
        if (contentFiles == null || !contentFiles.Any())
            return;

        writer.WriteLine("  <ItemGroup>");

        foreach (var file in contentFiles)
        {
            var include = file.Attribute("include")?.Value;
            if (!string.IsNullOrEmpty(include))
            {
                var additionalImportDirs = dependencies != null
                    ? string.Join(";", dependencies.Select(d =>
                    {
                        var depId = d.Attribute("id")?.Value;
                        var depVersion = d.Attribute("version")?.Value;
                        var depInclude = d.Attribute("include")?.Value;
                        return !string.IsNullOrEmpty(depId) && !string.IsNullOrEmpty(depVersion) &&
                               !string.IsNullOrEmpty(depInclude)
                            ? $"$(NuGetPackageRoot){depId.ToLower()}/{depVersion}/{Path.GetDirectoryName(depInclude)}"
                            : null;
                    }).Where(dir => dir != null))
                    : null;

                WriteProtobufElement(
                    writer,
                    $"$(NuGetPackageRoot){id.ToLower()}/{version}/contentFiles/{include}",
                    $"$(NuGetPackageRoot){id.ToLower()}/{version}/contentFiles/{Path.GetDirectoryName(include)}",
                    additionalImportDirs
                );
            }
        }

        if (dependencies != null)
        {
            WriteProtobufDependencies(writer, dependencies);
        }

        writer.WriteLine("  </ItemGroup>");
    }

    private void WriteProtobufDependencies(StreamWriter writer, IEnumerable<XElement> dependencies)
    {
        foreach (var dependency in dependencies)
        {
            var depId = dependency.Attribute("id")?.Value;
            var depVersion = dependency.Attribute("version")?.Value;
            var depInclude = dependency.Attribute("include")?.Value;

            if (!string.IsNullOrEmpty(depId) && !string.IsNullOrEmpty(depVersion) && !string.IsNullOrEmpty(depInclude))
            {
                WriteProtobufElement(
                    writer,
                    $"$(NuGetPackageRoot){depId.ToLower()}/{depVersion}/{depInclude}",
                    $"$(NuGetPackageRoot){depId.ToLower()}/{depVersion}/{Path.GetDirectoryName(depInclude)}"
                );
            }
        }
    }

    private void WriteProtobufElement(StreamWriter writer, string include, string protoRoot,
        string? additionalImportDirs = null)
    {
        writer.WriteLine($"    <Protobuf Include=\"{include}\"");
        writer.WriteLine($"              ProtoRoot=\"{protoRoot}\"");
        if (!string.IsNullOrEmpty(additionalImportDirs))
        {
            writer.WriteLine($"              AdditionalImportDirs=\"{additionalImportDirs.TrimEnd('/')}/\"");
        }

        writer.WriteLine("              GrpcServices=\"Both\" />");
    }

    private void WriteCopyTarget(StreamWriter writer, string name, string sourceFiles, string destinationFolder)
    {
        writer.WriteLine($"  <Target Name=\"{name}\" AfterTargets=\"Build\">");
        writer.WriteLine($"    <Copy SourceFiles=\"{sourceFiles}\"");
        writer.WriteLine($"          DestinationFolder=\"{destinationFolder}\"");
        writer.WriteLine("          SkipUnchangedFiles=\"true\" />");
        writer.WriteLine();
        writer.WriteLine("    <ItemGroup>");
        writer.WriteLine("      <Protobuf Include=\"$(MSBuildProjectDirectory)/Protos/**/*.proto\"");
        writer.WriteLine("                ProtoRoot=\"$(MSBuildProjectDirectory)/Protos\"");
        writer.WriteLine("                GrpcServices=\"Both\" />");
        writer.WriteLine("    </ItemGroup>");
        writer.WriteLine("  </Target>");
    }
}
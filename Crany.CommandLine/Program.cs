using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Xml.Linq;
using CommandHandler = System.CommandLine.NamingConventionBinder.CommandHandler;

namespace Crany.CommandLine;

internal static class Program
{
    static async Task<int> Main(string[] args)
    {
        // RootCommand initialization with custom description
        var rootCommand = new RootCommand("Crany CLI: Generate .targets files for NuGet packages");

        // Command to process .nuspec files
        var processCommand = new Command("process", "Process a .nuspec file to generate a .targets file")
        {
            new Argument<string>("nuspecPath", "Path to the .nuspec file to process")
        };

        processCommand.Handler = CommandHandler.Create<string>(ProcessNuspec);

        // Custom --version option
        var versionOption = new Option<bool>("--version", "Show version information");
        rootCommand.AddOption(versionOption);

        // Add the process command to the root command
        rootCommand.Add(processCommand);

        // Configure custom handler for --version
        rootCommand.SetHandler((InvocationContext context) =>
        {
            if (context.ParseResult.HasOption(versionOption))
            {
                Console.WriteLine("Crany CLI v1.0.0");
                context.ExitCode = 0;
            }
        });

        var configuration = new CommandLineConfiguration(
            rootCommand,
            enablePosixBundling: true,
            enableDirectives: true,
            enableLegacyDoubleDashBehavior: false
        );

        var parser = new Parser(configuration);
        return await parser.InvokeAsync(args);
    }

    static void ProcessNuspec(string nuspecPath)
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

        var targetsPath = Path.Combine(Path.GetDirectoryName(nuspecPath) ?? ".", $"{id}.targets");

        using var writer = new StreamWriter(targetsPath);

        writer.WriteLine(@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">");

        // Write content files
        WriteProtobufGroup(writer, id, version, contentFiles, dependencies);

        // Write copy target
        WriteCopyTarget(writer, "CopyProtosToProject", "@(Protobuf)", "$(MSBuildProjectDirectory)/Protos");

        writer.WriteLine("</Project>");

        Console.WriteLine($"Targets file generated: {targetsPath}");
    }

    static void WriteProtobufGroup(StreamWriter writer, string id, string version, IEnumerable<XElement>? contentFiles, IEnumerable<XElement>? dependencies)
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
                        return !string.IsNullOrEmpty(depId) && !string.IsNullOrEmpty(depVersion) && !string.IsNullOrEmpty(depInclude)
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

    static void WriteProtobufDependencies(StreamWriter writer, IEnumerable<XElement> dependencies)
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

    static void WriteProtobufElement(StreamWriter writer, string include, string protoRoot, string? additionalImportDirs = null)
    {
        writer.WriteLine($"    <Protobuf Include=\"{include}\"");
        writer.WriteLine($"              ProtoRoot=\"{protoRoot}\"");
        if (!string.IsNullOrEmpty(additionalImportDirs))
        {
            writer.WriteLine($"              AdditionalImportDirs=\"{additionalImportDirs.TrimEnd('/')}/\"");
        }
        writer.WriteLine("              GrpcServices=\"Both\" />");
    }

    static void WriteCopyTarget(StreamWriter writer, string name, string sourceFiles, string destinationFolder)
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
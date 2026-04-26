using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace RustMaui.Tool;

internal static class Program
{
    private const string SourceName = "MauiRust";
    private const string CrateToken = "mauirustnativelib";
    private const string GeneratorPackageIdToken = "__GENERATOR_PACKAGE_ID__";
    private const string GeneratorPackageId = "RustMaui.Generators";
    private const string GeneratorIncludeAssets = "runtime; build; native; contentfiles; analyzers; buildtransitive";

    private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs", ".csproj", ".props", ".targets", ".json", ".md", ".txt", ".toml", ".rs", ".xaml", ".xml", ".plist", ".config", ".manifest", ".ps1", ".sh", ".sln", ".slnx", ".svg", ".yml", ".yaml"
    };

    public static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || IsHelp(args[0]))
            {
                PrintHelp();
                return 0;
            }

            return args[0].ToLowerInvariant() switch
            {
                "new" => RunNew(args.Skip(1).ToArray()),
                "init" => RunInit(args.Skip(1).ToArray()),
                _ => Fail($"Unknown command '{args[0]}'.")
            };
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static int RunNew(string[] args)
    {
        if (args.Length == 0)
        {
            return Fail("Missing app name. Usage: rustmaui new <name> [--output <path>]");
        }

        var name = args[0].Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return Fail("App name cannot be empty.");
        }

        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), name);
        for (var index = 1; index < args.Length; index++)
        {
            if (args[index] is "--output" or "-o")
            {
                if (index + 1 >= args.Length)
                {
                    return Fail("Missing value for --output.");
                }

                outputPath = Path.GetFullPath(args[++index]);
                continue;
            }

            return Fail($"Unknown option '{args[index]}'.");
        }

        if (Directory.Exists(outputPath) && Directory.EnumerateFileSystemEntries(outputPath).Any())
        {
            return Fail($"Output directory already exists and is not empty: {outputPath}");
        }

        Directory.CreateDirectory(outputPath);

        var replacements = CreateReplacements(name);
        CopyScaffold(GetScaffoldRoot(), outputPath, replacements, includeEverything: true);

        Console.WriteLine($"Created .NET MAUI + Rust app at {outputPath}");
        return 0;
    }

    private static int RunInit(string[] args)
    {
        if (args.Length == 0)
        {
            return Fail("Missing project path. Usage: rustmaui init <path-to-csproj-or-directory>");
        }

        var projectPath = ResolveProjectPath(args[0]);
        var projectDirectory = Path.GetDirectoryName(projectPath)!;
        var projectName = Path.GetFileNameWithoutExtension(projectPath);
        var replacements = CreateReplacements(projectName);

        EnsureGeneratorReference(projectPath, GetSharedPackageVersion());
        EnsureRustCrateDir(projectPath);
        CopyScaffold(Path.Combine(GetScaffoldRoot(), "rust"), Path.Combine(projectDirectory, "rust"), replacements, includeEverything: false);

        Console.WriteLine($"Initialized RustMaui in {projectPath}");
        return 0;
    }

    private static string ResolveProjectPath(string inputPath)
    {
        var fullPath = Path.GetFullPath(inputPath);
        if (File.Exists(fullPath) && string.Equals(Path.GetExtension(fullPath), ".csproj", StringComparison.OrdinalIgnoreCase))
        {
            return fullPath;
        }

        if (!Directory.Exists(fullPath))
        {
            throw new InvalidOperationException($"Path does not exist: {fullPath}");
        }

        var projectFiles = Directory.GetFiles(fullPath, "*.csproj", SearchOption.TopDirectoryOnly);
        if (projectFiles.Length == 1)
        {
            return projectFiles[0];
        }

        if (projectFiles.Length == 0)
        {
            throw new InvalidOperationException($"No .csproj file found in directory: {fullPath}");
        }

        throw new InvalidOperationException($"Multiple .csproj files found in directory: {fullPath}. Pass the exact project path.");
    }

    private static void EnsureGeneratorReference(string projectPath, string packageVersion)
    {
        var document = XDocument.Load(projectPath, LoadOptions.PreserveWhitespace);
        var projectElement = document.Root ?? throw new InvalidOperationException($"Invalid project file: {projectPath}");

        var packageReference = projectElement
            .Descendants("PackageReference")
            .FirstOrDefault(element => string.Equals((string?)element.Attribute("Include"), GeneratorPackageId, StringComparison.OrdinalIgnoreCase));

        if (packageReference is null)
        {
            var itemGroup = projectElement.Elements("ItemGroup")
                .FirstOrDefault(group => group.Elements("PackageReference").Any())
                ?? new XElement("ItemGroup");

            if (itemGroup.Parent is null)
            {
                projectElement.Add(itemGroup);
            }

            packageReference = new XElement("PackageReference",
                new XAttribute("Include", GeneratorPackageId),
                new XAttribute("Version", packageVersion),
                new XElement("PrivateAssets", "all"),
                new XElement("IncludeAssets", GeneratorIncludeAssets));

            itemGroup.Add(packageReference);
        }
        else
        {
            packageReference.SetAttributeValue("Version", packageVersion);

            EnsureChildElement(packageReference, "PrivateAssets", "all");
            EnsureChildElement(packageReference, "IncludeAssets", GeneratorIncludeAssets);
        }

        document.Save(projectPath);
    }

    private static void EnsureRustCrateDir(string projectPath)
    {
        var document = XDocument.Load(projectPath, LoadOptions.PreserveWhitespace);
        var projectElement = document.Root ?? throw new InvalidOperationException($"Invalid project file: {projectPath}");

        var rustCrateDir = projectElement.Descendants("RustCrateDir").FirstOrDefault();
        if (rustCrateDir is null)
        {
            var propertyGroup = projectElement.Elements("PropertyGroup").FirstOrDefault() ?? new XElement("PropertyGroup");
            if (propertyGroup.Parent is null)
            {
                projectElement.AddFirst(propertyGroup);
            }

            propertyGroup.Add(new XElement("RustCrateDir", "rust"));
            document.Save(projectPath);
        }
    }

    private static void EnsureChildElement(XElement parent, string name, string value)
    {
        var child = parent.Element(name);
        if (child is null)
        {
            parent.Add(new XElement(name, value));
            return;
        }

        child.Value = value;
    }

    private static string GetSharedPackageVersion()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "Config", "RustMaui.Package.props");
        var document = XDocument.Load(configPath);
        return document.Root?
            .Descendants("RustMauiPackageVersion")
            .Select(element => element.Value.Trim())
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
            ?? throw new InvalidOperationException($"Could not read RustMauiPackageVersion from {configPath}");
    }

    private static string GetScaffoldRoot()
        => Path.Combine(AppContext.BaseDirectory, "Scaffold");

    private static Dictionary<string, string> CreateReplacements(string name)
    {
        var crateName = ToCrateName(name);
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceName] = name,
            [CrateToken] = crateName,
            [GeneratorPackageIdToken] = GeneratorPackageId
        };
    }

    private static string ToCrateName(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
            else if (builder.Length == 0 || builder[^1] != '_')
            {
                builder.Append('_');
            }
        }

        var result = builder.ToString().Trim('_');
        return string.IsNullOrWhiteSpace(result) ? "mauirustapp" : result;
    }

    private static void CopyScaffold(string sourceRoot, string targetRoot, IReadOnlyDictionary<string, string> replacements, bool includeEverything)
    {
        if (!Directory.Exists(sourceRoot))
        {
            throw new InvalidOperationException($"Scaffold root not found: {sourceRoot}");
        }

        foreach (var sourcePath in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceRoot, sourcePath);
            if (!includeEverything && relativePath.StartsWith(".template.config", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var targetPath = Path.Combine(targetRoot, ReplaceTokensInPath(relativePath, replacements));
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

            if (File.Exists(targetPath))
            {
                continue;
            }

            if (IsTextFile(sourcePath))
            {
                var content = File.ReadAllText(sourcePath);
                File.WriteAllText(targetPath, ReplaceTokens(content, replacements), new UTF8Encoding(false));
            }
            else
            {
                File.Copy(sourcePath, targetPath);
            }
        }
    }

    private static bool IsTextFile(string path)
        => TextExtensions.Contains(Path.GetExtension(path));

    private static string ReplaceTokensInPath(string path, IReadOnlyDictionary<string, string> replacements)
        => ReplaceTokens(path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar), replacements);

    private static string ReplaceTokens(string value, IReadOnlyDictionary<string, string> replacements)
    {
        var result = value;
        foreach (var replacement in replacements)
        {
            result = result.Replace(replacement.Key, replacement.Value, StringComparison.Ordinal);
        }

        return result;
    }

    private static bool IsHelp(string arg)
        => arg is "help" or "--help" or "-h" or "/?";

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        PrintHelp();
        return 1;
    }

    private static void PrintHelp()
    {
        var version = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "dev";

        Console.WriteLine($"rustmaui v{version}");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  rustmaui new <name> [--output <path>]");
        Console.WriteLine("  rustmaui init <path-to-csproj-or-directory>");
    }
}
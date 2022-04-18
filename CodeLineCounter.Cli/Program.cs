using CodeLineCounter.Cli;
using System.Diagnostics;
using System.Reflection;

Stopwatch? timer = null;
bool showHelp = false;
bool verbose = false;
List<string> extensions = new List<string>() { ".cs" };
string directory = string.Empty;
int exitCode = -1;

try
{
    HandleArguments(args);

    if (showHelp)
    {
        ShowHelp();
    }
    else
    {
        if (string.IsNullOrWhiteSpace(directory)) { throw new ArgumentNullException(nameof(directory)); }
        if (!Directory.Exists(directory)) { throw new ArgumentException($"Directory '{directory}' not found."); }

        timer = Stopwatch.StartNew();
        var dirInfo = new DirectoryInfo(directory);
        var files = dirInfo.GetFiles("*", SearchOption.AllDirectories).Where(f => extensions.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));

        long lineCount = 0L;
        int fileCount = 0;

        foreach (var file in files)
        {
            fileCount++;
            var lines = File.ReadAllLines(file.FullName);
            var countedLines = lines.Where(l => !string.IsNullOrWhiteSpace(l) &
                !l.Trim().Equals("{") && !l.Trim().Equals("}") && !l.Trim().StartsWith("//"));
            int count = countedLines.Count();
            lineCount += count;
            Communicate($"{file.Name}\t{count}\t{lineCount}");
        }

        Console.WriteLine($"Files counted: {fileCount}");
        Console.WriteLine($"Lines of code: {lineCount}");
    }
    exitCode = 0;
}
catch (Exception exc)
{
    Communicate(exc.ToString(), true);
    exitCode = 1;
}
finally
{
    if (timer != null)
    {
        timer.Stop();
        Communicate($"Completed in {timer.Elapsed.ConvertToText()}.");
    }
    Environment.Exit(exitCode);
}

void Communicate(string message, bool force = false)
{
    if (verbose || force)
    {
        Console.WriteLine(message);
    }
}

void HandleArguments(string[] args)
{
    if (args.Length > 0)
    {
        for (int a = 0; a < args.Length; a++)
        {
            string argument = args[a].ToLower();

            switch (argument)
            {
                case "--directory":
                case "--dir":
                case "-d":
                    if (a > args.Length - 1) { throw new ArgumentException($"Expecting a directory after {args[a]}"); }
                    directory = args[++a];
                    break;
                case "--verbose":
                case "-v":
                    verbose = true;
                    break;
                case "--help":
                case "-h":
                case "-?":
                case "?":
                    showHelp = true;
                    break;
                default:
                    throw new ArgumentException($"Unrecognized argument: {args[a]}");
            }
        }
    }
    else
    {
        ShowHelp("No arguments provided.");
    }
}

void ShowHelp(string? message = null)
{
    if (!string.IsNullOrWhiteSpace(message))
    {
        Console.WriteLine(message);
    }

    Dictionary<string, string> helpDefinitions = new()
    {
        { "--directory|d <directory>","The directory to search. Searches are always recursive."},
        { "[-h|-?|?|--help]", "Show this help." },
        { "[-v|--verbose]", "Write details to console." },
    };

    string? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    int maxKeyLength = helpDefinitions.Keys.Max(k => k.Length) + 1;

    Console.WriteLine($"{Environment.NewLine}{assemblyName} {string.Join(' ', helpDefinitions.Keys)}{Environment.NewLine}");

    foreach (KeyValuePair<string, string> helpItem in helpDefinitions)
    {
        Console.WriteLine($"{helpItem.Key.PadRight(maxKeyLength)}\t{helpItem.Value}");
    }
}
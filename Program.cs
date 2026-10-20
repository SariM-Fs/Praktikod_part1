using System;
using System.CommandLine;
var languageOption = new Option<string[]>("--language", "List of programming languages");
languageOption.AllowMultipleArgumentsPerToken = true;
languageOption.IsRequired = true;
languageOption.AddAlias("-l");
var fileOption = new Option<FileInfo>("--output", "File path and name");
fileOption.IsRequired = true;
fileOption.AddAlias("-f");
var noteOption = new Option<bool>("--note", "Notes from files");
noteOption.AddAlias("-n");
var sortOption = new Option<string>("--sort", "Sort order of code files");
sortOption.SetDefaultValue("name");
sortOption.AddAlias("-s");
var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines", "Remove empty lines");
removeEmptyLinesOption.AddAlias("-r");
var authorOption = new Option<string>("--author", "The name");
authorOption.AddAlias("-a");
var bundleCommand = new Command("bundle", "bundle code files to a single file");
var create_rsp = new Command("create-rsp", "create a response file");
create_rsp.AddAlias("-c");
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(fileOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(authorOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.SetHandler((languages, output, note, sort, remove, author) =>
{
    foreach (var language in languages)
        Console.WriteLine(language);
    try
    {
        File.Create(output.FullName).Close();
        Console.WriteLine("File was created");
    }
    catch (DirectoryNotFoundException)
    {
        Console.WriteLine("Error: File path is invalid");
        return;
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error: " + ex.Message);
        return;
    }
    string currentDir = Directory.GetCurrentDirectory();
    string[] files = Directory.GetFiles(currentDir, "*", SearchOption.AllDirectories);
    string[] excludedFolders = { "bin", "obj", "debug", ".vs", ".git" };
    bool find;
    List<string> newFiles = new List<string>();
    for (int i = 0; i < files.Length; i++)
    {
        find = false;
        for (int j = 0; j < excludedFolders.Length; j++)
        {
            if (files[i].Contains("\\" + excludedFolders[j] + "\\"))
            {
                find = true;
            }

        }
        if (!find)
        {
            newFiles.Add(files[i]);
        }
    }
    Dictionary<string, string> languageExtensions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    { "c#", ".cs" },
    { "java", ".java" },
    { "python", ".py" },
    { "javascript", ".js" },
    { "typescript", ".ts" },
    { "html", ".html" },
    { "css", ".css" },
    { "c++", ".cpp" },
    { "c", ".c" },
    { "sql", ".sql" }
};
    List<string> allowedExtensions = new List<string>();
    List<string> filteredFiles = new List<string>();
    string s = "";
    for (int j = 0; j < languages.Length; j++)
    {
        if (languages[j].Contains("all"))
        {
            allowedExtensions.AddRange(languageExtensions.Values);
        }

        if (!languageExtensions.ContainsKey(languages[j]))
        {
            continue;
        }
        else
        {
            allowedExtensions.Add(languageExtensions[languages[j]]);
        }

    }
    for (int i = 0; i < newFiles.Count; i++)
    {
        s = Path.GetExtension(newFiles[i]);
        if (allowedExtensions.Contains(s))
        {
            filteredFiles.Add(newFiles[i]);
        }
    }
    filteredFiles.Remove(output.FullName);
    foreach (var item in filteredFiles)
    {
        Console.WriteLine(item);
    }
    if (sort == "name")
        filteredFiles = filteredFiles.OrderBy(f => Path.GetFileName(f)).ToList();
    else
        filteredFiles = filteredFiles.OrderBy(f => Path.GetExtension(f)).ToList();
    using StreamWriter writer = new StreamWriter(output.FullName);
    if (!string.IsNullOrEmpty(author))
    {
        writer.WriteLine($"// Author: {author}");
        writer.WriteLine();
    }
    foreach (var file in filteredFiles)
    {
        if (note == true)
        {
            string relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), file);
            writer.WriteLine($"// Source: {Path.GetFileName(file)} | Path: {relativePath}");
        }
        string[] lines = File.ReadAllLines(file);
        foreach (var line in lines)
        {
            if (remove && string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            writer.WriteLine(line);
        }
        writer.WriteLine();
    }
    Console.WriteLine($"File created: {output.FullName}");


}, languageOption, fileOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);
create_rsp.SetHandler(() =>
{
    string command = "bundle";
    Console.WriteLine("Which languages? ");
    var ans1 = Console.ReadLine();
    command += " --language " + ans1;
    Console.WriteLine("What is the output file name?");
    var ans2 = Console.ReadLine();
    command += " --output " + ans2;
    Console.WriteLine("Add notes? (y/n)");
    var ans3 = Console.ReadLine();
    if (ans3 == "y")
        command += " --note ";
    Console.WriteLine("Sort by? (name/type)");
    var ans4 = Console.ReadLine();
    command += " --sort " + ans4;
    Console.WriteLine("Remove empty lines? (y/n)");
    var ans5 = Console.ReadLine();
    if (ans5 == "y")
        command += " --remove-empty-lines ";
    Console.WriteLine("Author name?");
    var ans6 = Console.ReadLine();
    command += " --author " + ans6;
    File.WriteAllText("myfile.rsp", command);
});
var rootCommand = new RootCommand("Root command file bundler CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(create_rsp);
await rootCommand.InvokeAsync(args);

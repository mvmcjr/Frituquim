using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Frituquim.Models;

namespace Frituquim.Helpers;

public static class YtdlpHelper
{
    private static string ExePath { get; } = FindYtdlp();

    private static string FindYtdlp()
    {
        var ytdlpPath = Path.Combine(AppContext.BaseDirectory, "yt-dlp.exe");

        if (File.Exists(ytdlpPath)) return ytdlpPath;

        return Environment.GetEnvironmentVariable("YTDLP_PATH") ?? "yt-dlp";
    }

    public static async Task UpdateYtdlp()
    {
        var commandResult = await CreateBaseCommand()
            .WithArguments(["--update"])
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        Debug.WriteLine(commandResult.ExitCode != 0 ? "Failed to update yt-dlp" : "yt-dlp updated successfully");
    }

    public static async Task<string> GetFileName(string url, ExtractionType selectedExtractionType)
    {
        var commandResult = await CreateBaseCommand()
            .WithArguments([url, "--get-filename"])
            .ExecuteBufferedAsync();
        var fileName = commandResult.StandardOutput.Trim();

        if (selectedExtractionType == ExtractionType.Audio)
            fileName = $"{Path.GetFileNameWithoutExtension(fileName)}.mp3";

        return fileName;
    }

    public static Command CreateYtdlpCommand(string url, string filePath, IEnumerable<string> extraArguments)
    {
        return CreateBaseCommand()
            .WithArguments(new[] { url, "--no-mtime", "-o", filePath }.Concat(extraArguments), true);
    }

    private static Command CreateBaseCommand()
    {
        return Cli.Wrap(ExePath)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(c => Debug.WriteLine(c)))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(c => Debug.WriteLine(c)));
    }
}
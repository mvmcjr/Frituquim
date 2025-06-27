using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Frituquim.Models;

namespace Frituquim.Helpers;

public static class FFmpegHelper
{
    private static string ExePath { get; } = FindFfmpeg();
    private static string FfprobePath { get; } = FindFfprobe();

    private static string FindFfmpeg()
    {
        var ffmpegPath = Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe");

        if (File.Exists(ffmpegPath)) return ffmpegPath;

        return Environment.GetEnvironmentVariable("FFMPEG_PATH") ?? "ffmpeg";
    }

    private static string FindFfprobe()
    {
        var ffprobePath = Path.Combine(AppContext.BaseDirectory, "ffprobe.exe");

        if (File.Exists(ffprobePath)) return ffprobePath;

        return Environment.GetEnvironmentVariable("FFPROBE_PATH") ?? "ffprobe";
    }

    /// <summary>
    ///     Gets the duration of a video file using ffprobe
    /// </summary>
    public static async Task<TimeSpan?> GetVideoDurationAsync(string inputPath)
    {
        try
        {
            var result = await Cli.Wrap(FfprobePath)
                .WithArguments([
                    "-v", "quiet",
                    "-show_entries", "format=duration",
                    "-of", "csv=p=0",
                    inputPath
                ])
                .ExecuteBufferedAsync();

            if (result.ExitCode == 0 && !string.IsNullOrWhiteSpace(result.StandardOutput))
            {
                var durationText = result.StandardOutput.Trim();
                if (double.TryParse(durationText, NumberStyles.Float, CultureInfo.InvariantCulture,
                        out var durationSeconds)) return TimeSpan.FromSeconds(durationSeconds);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting video duration: {ex.Message}");
        }

        return null;
    }

    public static Command ConvertFile(string inputPath, string outputPath, ConversionType conversionType,
        ConversionHardware conversionHardware, Action<string>? onProgress = null)
    {
        var args = new List<string>
        {
            "-i", inputPath, "-progress", "pipe:2"
        };

        if (conversionType == ConversionType.Mp4)
        {
            var codec = conversionHardware switch
            {
                ConversionHardware.Nvidia => "h264_nvenc",
                ConversionHardware.IntelQuickSync => "h264_qsv",
                _ => "libx264"
            };

            if (conversionHardware == ConversionHardware.Nvidia)
            {
                args.AddRange(["-preset", "medium"]);
                args.AddRange(["-rc:v", "vbr", "-cq", "24"]);
            }

            args.AddRange([
                "-c:v", codec, "-c:a", "aac", "-b:a", "128k", "-pix_fmt", "yuv420p", "-movflags", "+faststart"
            ]);
        }

        args.Add(outputPath);

        return Cli.Wrap(ExePath)
            .WithArguments(args)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(c => Debug.WriteLine(c)))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(l =>
            {
                Debug.WriteLine(l);

                if (onProgress != null) onProgress(l);
            }));
    }
}
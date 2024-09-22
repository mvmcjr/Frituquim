using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Frituquim.Models;

namespace Frituquim.Helpers;

public static class FFmpegHelper
{
    private static string ExePath { get; } = FindFfmpeg();

    private static string FindFfmpeg()
    {
        var ffmpegPath = Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe");

        if (File.Exists(ffmpegPath))
        {
            return ffmpegPath;
        }

        return Environment.GetEnvironmentVariable("FFMPEG_PATH") ?? "ffmpeg";
    }
    
    public static Command ConvertFile(string inputPath, string outputPath, ConversionType conversionType, ConversionHardware conversionHardware)
    {
        var args = new List<string>
        {
            "-i", inputPath
        };

        if (conversionType == ConversionType.Mp4)
        {
            var codec = conversionHardware switch
            {
                ConversionHardware.Nvidia => "h264_nvenc",
                ConversionHardware.IntelQuickSync => "h264_qsv",
                _ => "libx264"
            };
            
            args.AddRange(["-c:v", codec, "-c:a", "aac", "-b:a", "128k"]);
        }
        
        args.Add(outputPath);
        
        return Cli.Wrap(ExePath)
            .WithArguments(args)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(c => Debug.WriteLine(c)))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(c => Debug.WriteLine(c)));
    }
}
using System.Diagnostics;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

namespace Frituquim.Helpers;

public static class YtdlpHelper
{
    public static async Task<string> GetFileName(string url)
    {
        var commandResult = await CreateBaseCommand()
            .WithArguments(new[] { url, "--get-filename" })
            .ExecuteBufferedAsync();
        return commandResult.StandardOutput.Trim();
    }

    public static Command CreateYtdlpCommand(string url, string filePath) =>
        CreateBaseCommand()
            .WithArguments(new[] { url, "-o", filePath }, true);

    private static Command CreateBaseCommand() =>
        Cli.Wrap("yt-dlp")
            .WithStandardOutputPipe(PipeTarget.ToDelegate(c => Debug.WriteLine(c)))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(c => Debug.WriteLine(c)));
}
using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrameExtractor;
using FrameExtractor.Extensions;
using Frituquim.Helpers;
using Frituquim.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Frituquim.ViewModels
{
    public record ExecutionMessage(string Title, string Message);

    public record ExecutionMessageMap(ExecutionMessage Success, ExecutionMessage Error);

    public partial class FramesExtractionViewModel(ISnackbarService snackbarService)
        : FrituquimBasePageWithOutputDirectory
    {
        [ObservableProperty] private string _videoPathOrUrl = "https://www.youtube.com/watch?v=vaphaFCyLQI";
        
        [ObservableProperty] private bool _createSubfolders = true;
        
        [ObservableProperty] private TimeSpan? _extractionTimeLimit;
        
        private ISnackbarService SnackbarService { get; } = snackbarService;


        [RelayCommand]
        private void OpenFileDialog()
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() ?? false)
            {
                VideoPathOrUrl = fileDialog.FileName;
            }
        }
        
        [RelayCommand]
        private async Task Execute()
        {
            IsExtractButtonEnabled = false;

            try
            {
                await ExtractFrames();
            }
            finally
            {
                IsExtractButtonEnabled = true;
            }
        }

        private async Task ExtractFrames()
        {
            var isUrl = Uri.IsWellFormedUriString(VideoPathOrUrl, UriKind.Absolute);

            if (isUrl)
            {
                var fileName = await YtdlpHelper.GetFileName(VideoPathOrUrl, ExtractionType.Frames);
                var tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }

                var downloadedVideo = await YtdlpHelper
                    .CreateYtdlpCommand(VideoPathOrUrl, tempFilePath, Array.Empty<string>())
                    .ExecuteAsync()
                    .Task
                    .ContinueWith(t => t.IsCompletedSuccessfully);

                if (!downloadedVideo)
                {
                    SnackbarService.Show("Erro ao baixar o video!",
                        "Ocorreu um erro ao baixar o video, tente novamente.", ControlAppearance.Danger, null,
                        TimeSpan.FromSeconds(3));
                    IsExtractButtonEnabled = true;
                    return;
                }

                SnackbarService.Show("Video baixado com sucesso!",
                    "O video foi baixado com sucesso e a extração de frames será iniciada.", ControlAppearance.Success,
                    null, TimeSpan.FromSeconds(3));
                await ExtractFramesToFolder(tempFilePath);
                File.Delete(tempFilePath);
            }
            else
            {
                await ExtractFramesToFolder(VideoPathOrUrl);
            }
        }

        private async Task ExtractFramesToFolder(string filePath)
        {
            var videoName = Path.GetFileNameWithoutExtension(filePath);
            var basePath = CreateSubfolders ? Path.Combine(OutputDirectory, videoName) : OutputDirectory;

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            var frameExtractionOptions = new FrameExtractionOptions
            {
                TimeLimit = ExtractionTimeLimit,
                FrameFormat = FrameFormat.Png
            };

            await foreach (var frame in FrameExtractionService.Default.GetFrames(filePath,
                               options: frameExtractionOptions, onDurationUpdate: OnDurationUpdate))
            {
                await WriteFrame(frame, basePath);
            }

            CurrentProgress = null;

            if (OpenFolderAfterExecution)
            {
                System.Diagnostics.Process.Start("explorer", basePath);
            }
        }

        private void OnDurationUpdate(TimeSpan maxDuration, TimeSpan currentDuration)
        {
            CurrentProgress = currentDuration.TotalSeconds /
                (ExtractionTimeLimit?.TotalSeconds ?? maxDuration.TotalSeconds) * 100;
        }

        private static async Task WriteFrame(Frame frame, string basePath)
        {
            var outputPath = Path.Combine(basePath,
                $"frame-{frame.Position}{frame.Options.FrameFormat.GetPipeFormat()}");

            await File.WriteAllBytesAsync(outputPath, frame.Data);
        }
    }
}
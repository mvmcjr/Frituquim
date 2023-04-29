using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using CliWrap;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrameExtractor;
using FrameExtractor.Extensions;
using Frituquim.Helpers;
using Wpf.Ui.Common;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Frituquim.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _videoPathOrUrl = "https://www.youtube.com/watch?v=vaphaFCyLQI";

        [ObservableProperty]
        private string _outputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLoadingVisibility))]
        [NotifyPropertyChangedFor(nameof(IsLoading))]
        private bool _isExtractButtonEnabled = true;

        [ObservableProperty]
        private bool _createSubfolders = true;

        [ObservableProperty]
        private bool _openFolderAfterExtraction = true;
        
        [ObservableProperty]
        private TimeSpan? _extractionTimeLimit;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowIndeterminateProgress))]
        private double? _currentProgress;

        public bool ShowIndeterminateProgress => !CurrentProgress.HasValue;

        public DashboardViewModel(ISnackbarService snackbarService)
        {
            SnackbarService = snackbarService;
        }

        public bool IsLoading => !IsExtractButtonEnabled;

        public Visibility IsLoadingVisibility => IsExtractButtonEnabled ? Visibility.Collapsed : Visibility.Visible;

        private ISnackbarService SnackbarService { get; }
        

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
        private void OpenOutputDirectoryDialog()
        {
            var fileDialog = new FolderBrowserDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                OutputDirectory = fileDialog.SelectedPath;
            }
        }

        [RelayCommand]
        private async Task ExtractFrames()
        {
            IsExtractButtonEnabled = false;
            var isUrl = Uri.IsWellFormedUriString(VideoPathOrUrl, UriKind.Absolute);

            if (isUrl)
            {
                var fileName = await YtdlpHelper.GetFileName(VideoPathOrUrl);
                var tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }

                var downloadedVideo = await YtdlpHelper.CreateYtdlpCommand(VideoPathOrUrl, tempFilePath)
                    .ExecuteAsync()
                    .Task
                    .ContinueWith(t => t.IsCompletedSuccessfully);

                if (!downloadedVideo)
                {
                    await SnackbarService.ShowAsync("Erro ao baixar o video!", "Ocorreu um erro ao baixar o video, tente novamente.", SymbolRegular.EqualCircle24, ControlAppearance.Danger);
                    IsExtractButtonEnabled = true;
                    return;
                }

                await SnackbarService.ShowAsync("Video baixado com sucesso!", "O video foi baixado com sucesso e a extração de frames será iniciada.", SymbolRegular.CheckmarkCircle24, ControlAppearance.Success);
                await ExtractFramesToFolder(tempFilePath);
                File.Delete(tempFilePath);
            }
            else
            {
                await ExtractFramesToFolder(VideoPathOrUrl);
            }

            IsExtractButtonEnabled = true;
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
                TimeLimit = ExtractionTimeLimit
            };
            
            await foreach (var frame in FrameExtractionService.Default.GetFrames(filePath, options: frameExtractionOptions, onDurationUpdate: OnDurationUpdate))
            {
                await WriteFrame(frame, basePath);
            }

            CurrentProgress = null;

            if (OpenFolderAfterExtraction)
            {
                System.Diagnostics.Process.Start("explorer", basePath);
            }
        }

        private void OnDurationUpdate(TimeSpan maxDuration, TimeSpan currentDuration)
        {
            CurrentProgress = currentDuration.TotalSeconds / (ExtractionTimeLimit?.TotalSeconds ?? maxDuration.TotalSeconds) * 100;
        }

        private static async Task WriteFrame(Frame frame, string basePath)
        {
            var outputPath = Path.Combine(basePath, $"frame-{frame.Position}{frame.Options.FrameFormat.GetPipeFormat()}");

            await File.WriteAllBytesAsync(outputPath, frame.Data);
        }
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frituquim.Helpers;
using Frituquim.Models;
using Microsoft.Win32;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Frituquim.ViewModels;

public partial class ExtractionViewModel(
    ExtractionType extractionType,
    ISnackbarService snackbarService,
    ExecutionMessageMap messageMap) : FrituquimBasePageWithOutputDirectory
{
    [ObservableProperty]
    private string _videoPathOrUrl = "https://www.youtube.com/watch?v=vaphaFCyLQI";

    [RelayCommand]
    private void OpenFileDialog()
    {
        var fileDialog = new OpenFileDialog();
        if (fileDialog.ShowDialog() ?? false) VideoPathOrUrl = fileDialog.FileName;
    }

    [RelayCommand]
    private async Task Execute()
    {
        IsExtractButtonEnabled = false;

        try
        {
            var fileName = await YtdlpHelper.GetFileName(VideoPathOrUrl, ExtractionType.Audio);
            var downloadFilePath = Path.Combine(OutputDirectory, fileName);

            if (File.Exists(downloadFilePath)) File.Delete(downloadFilePath);

            var downloadedFile = await YtdlpHelper.CreateYtdlpCommand(VideoPathOrUrl, downloadFilePath,
                    extractionType == ExtractionType.Audio ? ["-x", "--audio-format", "mp3"] : [])
                .ExecuteAsync()
                .Task
                .ContinueWith(t => t.IsCompletedSuccessfully);

            if (!downloadedFile)
            {
                snackbarService.Show(messageMap.Error.Title, messageMap.Error.Message, ControlAppearance.Danger,
                    null, TimeSpan.FromSeconds(3));
                IsExtractButtonEnabled = true;
                return;
            }

            snackbarService.Show(messageMap.Success.Title, messageMap.Success.Message, ControlAppearance.Success,
                null, TimeSpan.FromSeconds(3));

            if (OpenFolderAfterExecution) Process.Start("explorer", OutputDirectory);
        }
        finally
        {
            IsExtractButtonEnabled = true;
        }
    }
}
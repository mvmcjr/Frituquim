using System;
using System.Collections.Generic;
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

    [ObservableProperty]
    private bool _isPlaylistUrl;

    [ObservableProperty]
    private bool _shouldDownloadPlaylist;

    partial void OnVideoPathOrUrlChanged(string value)
    {
        IsPlaylistUrl = !string.IsNullOrWhiteSpace(value) && (value.Contains("list=") || value.Contains("&list="));
        if (!IsPlaylistUrl)
        {
            ShouldDownloadPlaylist = false;
        }
    }

    [RelayCommand]
    private async Task Execute()
    {
        IsExtractButtonEnabled = false;

        try
        {
            bool success;
            if (ShouldDownloadPlaylist)
            {
                var outputTemplate = Path.Combine(OutputDirectory, "%(title)s.%(ext)s");
                var args = new List<string> { "--yes-playlist" };
                if (extractionType == ExtractionType.Audio)
                {
                    args.AddRange(["-x", "--audio-format", "mp3"]);
                }

                success = await YtdlpHelper.CreateYtdlpCommand(VideoPathOrUrl, outputTemplate, args)
                    .ExecuteAsync()
                    .Task
                    .ContinueWith(t => t.IsCompletedSuccessfully);
            }
            else
            {
                var fileName = await YtdlpHelper.GetFileName(VideoPathOrUrl, extractionType, false);
                var downloadFilePath = Path.Combine(OutputDirectory, fileName);

                if (File.Exists(downloadFilePath)) File.Delete(downloadFilePath);

                var args = new List<string> { "--no-playlist" };
                if (extractionType == ExtractionType.Audio)
                {
                    args.AddRange(["-x", "--audio-format", "mp3"]);
                }

                success = await YtdlpHelper.CreateYtdlpCommand(VideoPathOrUrl, downloadFilePath, args)
                    .ExecuteAsync()
                    .Task
                    .ContinueWith(t => t.IsCompletedSuccessfully);
            }

            if (!success)
            {
                snackbarService.Show(messageMap.Error.Title, messageMap.Error.Message, ControlAppearance.Danger,
                    null, TimeSpan.FromSeconds(3));
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
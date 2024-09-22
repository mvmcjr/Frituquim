using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnumerableAsyncProcessor.Extensions;
using Frituquim.Helpers;
using Frituquim.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Frituquim.ViewModels;

public partial class ConvertViewModel(ISnackbarService snackbarService) : FrituquimBasePageWithOutputDirectory
{
    public Dispatcher? Dispatcher => UiApplication.Current.MainWindow?.Dispatcher;
    
    private ISnackbarService SnackbarService { get; } = snackbarService;

    [ObservableProperty] private string? _inputDirectory;

    [ObservableProperty] private string _inputFilter = "*.MOV";

    [ObservableProperty] private bool _inputIncludeSubDirectories = true;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ShowOutputDirectory))]
    private bool _saveInSameDirectory = true;

    [ObservableProperty] private ConversionType _conversionType = ConversionType.Mp4;

    [ObservableProperty] private ConversionHardware _conversionHardware = ConversionHardware.Cpu;

    [ObservableProperty] private ICollection<ConversionHardware> _conversionHardwares = new List<ConversionHardware>
    {
        ConversionHardware.Nvidia,
        ConversionHardware.IntelQuickSync,
        ConversionHardware.Cpu
    };

    public bool ShowOutputDirectory => !SaveInSameDirectory;
    
    [RelayCommand]
    private void OpenInputDirectoryDialog()
    {
        var fileDialog = new FolderBrowserDialog();
        if (fileDialog.ShowDialog() == DialogResult.OK)
        {
            InputDirectory = fileDialog.SelectedPath;
        }
    }

    [RelayCommand]
    private async Task ConvertVideos()
    {
        IsExtractButtonEnabled = false;
        CurrentProgress = null;

        try
        {
            if (InputDirectory == null)
            {
                SnackbarService.Show("Diretório de entrada não selecionado!",
                    "Selecione um diretório de entrada para continuar.", ControlAppearance.Danger, null,
                    TimeSpan.FromSeconds(3));
                IsExtractButtonEnabled = true;
                return;
            }

            if (!Directory.Exists(InputDirectory))
            {
                SnackbarService.Show("Diretório de entrada não encontrado!",
                    "O diretório de entrada selecionado não foi encontrado.", ControlAppearance.Danger, null,
                    TimeSpan.FromSeconds(3));
                IsExtractButtonEnabled = true;
                return;
            }

            var files = Directory.GetFiles(InputDirectory, InputFilter,
                InputIncludeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            if (files.Length == 0)
            {
                SnackbarService.Show("Nenhum arquivo encontrado!",
                    "Nenhum arquivo foi encontrado no diretório de entrada.", ControlAppearance.Danger, null,
                    TimeSpan.FromSeconds(3));
                IsExtractButtonEnabled = true;
                return;
            }
            
            int processedFiles = 0;

            await files.ToAsyncProcessorBuilder()
                .ForEachAsync(async file =>
                {
                    var fileName = Path.GetFileName(file);

                    var outputDirectory = SaveInSameDirectory
                        ? Path.GetDirectoryName(file) ?? OutputDirectory
                        : OutputDirectory;

                    var outputFilePath =
                        Path.Combine(outputDirectory, Path.ChangeExtension(fileName, ".mp4"));

                    if (File.Exists(outputFilePath))
                    {
                        File.Delete(outputFilePath);
                    }

                    var ffmpegCommand =
                        FFmpegHelper.ConvertFile(file, outputFilePath, ConversionType, ConversionHardware);
                    var convertedFile = await ffmpegCommand
                        .ExecuteAsync()
                        .Task
                        .ContinueWith(t => t.IsCompletedSuccessfully);

                    if (!convertedFile)
                    {
                        SnackbarService.Show("Erro ao converter video!",
                            "Ocorreu um erro ao converter o video, tente novamente.", ControlAppearance.Danger, null,
                            TimeSpan.FromSeconds(3));
                        IsExtractButtonEnabled = true;
                    }

                    Interlocked.Increment(ref processedFiles);
                    
                    Dispatcher?.Invoke(() => CurrentProgress = (double) processedFiles / files.Length * 100);
                })
                .ProcessInParallel(3);

            SnackbarService.Show("Conversão concluída!",
                "Todos os vídeos foram convertidos com sucesso.", ControlAppearance.Success, null,
                TimeSpan.FromSeconds(3));

            if (OpenFolderAfterExecution)
            {
                var directory = SaveInSameDirectory ? InputDirectory : OutputDirectory;
                System.Diagnostics.Process.Start("explorer.exe", directory);
            }
        }
        finally
        {
            IsExtractButtonEnabled = true;
        }
    }
}
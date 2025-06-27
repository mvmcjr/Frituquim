using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frituquim.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Application = System.Windows.Application;

namespace Frituquim.ViewModels;

public partial class ConvertViewModel(ISnackbarService snackbarService) : FrituquimBasePageWithOutputDirectory
{
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    private ConversionHardware _conversionHardware = ConversionHardware.Cpu;

    [ObservableProperty]
    private ICollection<ConversionHardware> _conversionHardwares = new List<ConversionHardware>
    {
        ConversionHardware.Nvidia,
        ConversionHardware.IntelQuickSync,
        ConversionHardware.Cpu
    };

    [ObservableProperty]
    private ConversionManager? _conversionManager;

    [ObservableProperty]
    private string? _conversionSpeed;

    [ObservableProperty]
    private ConversionType _conversionType = ConversionType.Mp4;

    [ObservableProperty]
    private string? _currentFile;

    [ObservableProperty]
    private string? _estimatedTimeRemaining;

    [ObservableProperty]
    private string? _inputDirectory;

    [ObservableProperty]
    private string _inputFilter = "*.MOV";

    [ObservableProperty]
    private bool _inputIncludeSubDirectories = true;

    [ObservableProperty]
    private bool _isCancelButtonEnabled;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ShowOutputDirectory))]
    private bool _saveInSameDirectory = true;

    public Dispatcher Dispatcher => Application.Current.Dispatcher;

    private ISnackbarService SnackbarService { get; } = snackbarService;

    public bool ShowOutputDirectory => !SaveInSameDirectory;

    [RelayCommand]
    private void OpenInputDirectoryDialog()
    {
        var fileDialog = new FolderBrowserDialog();
        if (fileDialog.ShowDialog() == DialogResult.OK) InputDirectory = fileDialog.SelectedPath;
    }

    [RelayCommand]
    private void CancelConversion()
    {
        _cancellationTokenSource?.Cancel();
        IsCancelButtonEnabled = false;
    }

    [RelayCommand]
    private async Task ConvertVideos()
    {
        IsExtractButtonEnabled = false;
        IsCancelButtonEnabled = true;
        CurrentProgress = null;
        ConversionSpeed = null;
        EstimatedTimeRemaining = null;
        CurrentFile = null;

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            if (InputDirectory == null)
            {
                SnackbarService.Show("Diretório de entrada não selecionado!",
                    "Selecione um diretório de entrada para continuar.", ControlAppearance.Danger, null,
                    TimeSpan.FromSeconds(3));
                return;
            }

            if (!Directory.Exists(InputDirectory))
            {
                SnackbarService.Show("Diretório de entrada não encontrado!",
                    "O diretório de entrada selecionado não foi encontrado.", ControlAppearance.Danger, null,
                    TimeSpan.FromSeconds(3));
                return;
            }

            var files = Directory.GetFiles(InputDirectory, InputFilter,
                InputIncludeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            if (files.Length == 0)
            {
                SnackbarService.Show("Nenhum arquivo encontrado!",
                    "Nenhum arquivo foi encontrado no diretório de entrada.", ControlAppearance.Danger, null,
                    TimeSpan.FromSeconds(3));
                return;
            }

            // Create and configure conversion manager
            ConversionManager = new ConversionManager(Dispatcher);

            // Bind manager properties to ViewModel properties
            ConversionManager.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ConversionManager.CurrentStatus):
                        CurrentFile = ConversionManager?.CurrentStatus;
                        break;
                    case nameof(ConversionManager.AverageSpeed):
                        ConversionSpeed = ConversionManager?.AverageSpeed;
                        break;
                    case nameof(ConversionManager.EstimatedTimeRemaining):
                        EstimatedTimeRemaining = ConversionManager?.EstimatedTimeRemaining;
                        break;
                    case nameof(ConversionManager.OverallProgress):
                        CurrentProgress = ConversionManager?.OverallProgress;
                        break;
                }
            };

            // Start conversions
            await ConversionManager.StartConversions(
                files,
                OutputDirectory,
                SaveInSameDirectory,
                ConversionType,
                ConversionHardware,
                _cancellationTokenSource.Token);

            if (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var failedConversions = ConversionManager.CompletedConversions
                    .Where(c => c.Status == ConversionStatus.Failed)
                    .ToList();

                if (failedConversions.Any())
                    SnackbarService.Show("Conversão concluída com erros!",
                        $"{failedConversions.Count} arquivo(s) falharam na conversão.", ControlAppearance.Caution, null,
                        TimeSpan.FromSeconds(5));
                else
                    SnackbarService.Show("Conversão concluída!",
                        "Todos os vídeos foram convertidos com sucesso.", ControlAppearance.Success, null,
                        TimeSpan.FromSeconds(3));

                if (OpenFolderAfterExecution)
                {
                    var directory = SaveInSameDirectory ? InputDirectory : OutputDirectory;
                    Process.Start("explorer.exe", directory);
                }
            }
            else
            {
                SnackbarService.Show("Conversão cancelada!",
                    "A conversão foi cancelada pelo usuário.", ControlAppearance.Info, null,
                    TimeSpan.FromSeconds(3));
            }
        }
        catch (OperationCanceledException)
        {
            SnackbarService.Show("Conversão cancelada!",
                "A conversão foi cancelada pelo usuário.", ControlAppearance.Info, null,
                TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            SnackbarService.Show("Erro durante a conversão!",
                $"Ocorreu um erro ao tentar converter os vídeos: {ex.Message}", ControlAppearance.Danger, null,
                TimeSpan.FromSeconds(5));
            Debug.WriteLine($"Error during conversion: {ex}");
        }
        finally
        {
            IsExtractButtonEnabled = true;
            IsCancelButtonEnabled = false;
            CurrentFile = null;
            ConversionSpeed = null;
            EstimatedTimeRemaining = null;
            ConversionManager?.Reset();
            ConversionManager = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}
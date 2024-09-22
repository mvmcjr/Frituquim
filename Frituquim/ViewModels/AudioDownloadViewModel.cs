using Frituquim.Models;
using Wpf.Ui;

namespace Frituquim.ViewModels;

public class AudioDownloadViewModel(ISnackbarService snackbarService) : ExtractionViewModel(ExtractionType.Audio,
    snackbarService, new ExecutionMessageMap(
        new ExecutionMessage("Audio extraído com sucesso!",
            "O audio foi extraído com sucesso e salvo no diretório de saída."),
        new ExecutionMessage("Erro ao extrair audio!",
            "Ocorreu um erro ao extrair o audio, tente novamente.")
    ));
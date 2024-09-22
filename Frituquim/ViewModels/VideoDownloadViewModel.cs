using Frituquim.Models;
using Wpf.Ui;

namespace Frituquim.ViewModels;

public class VideoDownloadViewModel(ISnackbarService snackbarService) : ExtractionViewModel(ExtractionType.Video,
    snackbarService, new ExecutionMessageMap(
        new ExecutionMessage("Vídeo extraído com sucesso!",
            "O vídeo foi extraído com sucesso e salvo no diretório de saída."),
        new ExecutionMessage("Erro ao extrair vídeo!",
            "Ocorreu um erro ao extrair o vídeo, tente novamente.")
    ));
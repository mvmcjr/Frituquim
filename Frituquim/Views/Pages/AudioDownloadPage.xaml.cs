using Frituquim.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace Frituquim.Views.Pages;

public partial class AudioDownloadPage : INavigableView<AudioDownloadViewModel>
{
    public AudioDownloadPage(AudioDownloadViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public AudioDownloadViewModel ViewModel { get; }
}
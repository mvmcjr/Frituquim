using Frituquim.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace Frituquim.Views.Pages;

public partial class VideoDownloadPage : INavigableView<VideoDownloadViewModel>
{
    public VideoDownloadPage(VideoDownloadViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public VideoDownloadViewModel ViewModel { get; }
}
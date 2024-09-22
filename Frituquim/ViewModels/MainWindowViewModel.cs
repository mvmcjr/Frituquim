using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace Frituquim.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty] private string _applicationTitle = "Frituquim";

        [ObservableProperty] private ObservableCollection<object> _menuItems =
        [
            new NavigationViewItem
            {
                Content = "Extrair Frames",
                Icon = new SymbolIcon { Symbol = SymbolRegular.ImageMultiple24 },
                TargetPageType = typeof(Views.Pages.FramesExtractionPage)
            },
            new NavigationViewItem
            {
                Content = "Extrair Áudio",
                Icon = new SymbolIcon { Symbol = SymbolRegular.MusicNote120 },
                TargetPageType = typeof(Views.Pages.AudioDownloadPage),
            },
            new NavigationViewItem
            {
                Content = "Extrair Vídeo",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Video24 },
                TargetPageType = typeof(Views.Pages.VideoDownloadPage)
            },
            new NavigationViewItem
            {
                Content = "Converter",
                Icon = new SymbolIcon { Symbol = SymbolRegular.ArrowsBidirectional24 },
                TargetPageType = typeof(Views.Pages.ConvertPage)
            },
        ];

        [ObservableProperty] private ObservableCollection<object> _footerMenuItems = [];

        [ObservableProperty] private ObservableCollection<MenuItem> _trayMenuItems = [];
    }
}
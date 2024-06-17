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
                Content = "Home",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                TargetPageType = typeof(Views.Pages.DashboardPage)
            }
        ];

        [ObservableProperty] private ObservableCollection<object> _footerMenuItems = [];

        [ObservableProperty] private ObservableCollection<MenuItem> _trayMenuItems =
            [new MenuItem { Header = "Home", Tag = "tray_home" }];
    }
}
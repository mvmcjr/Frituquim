using System;
using System.Windows;
using System.Windows.Media;
using Frituquim.ViewModels;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Frituquim.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INavigationWindow
    {
        public MainWindowViewModel ViewModel { get; }

        public MainWindow(
            MainWindowViewModel viewModel,
            INavigationViewPageProvider pageProvider,
            INavigationService navigationService,
            ISnackbarService snackbarService,
            IThemeService themeService
        )
        {
            ViewModel = viewModel;
            DataContext = this;

            SystemThemeWatcher.Watch(this);

            InitializeComponent();
            SetPageService(pageProvider);

            navigationService.SetNavigationControl(RootNavigation);
            snackbarService.SetSnackbarPresenter(SnackbarPresenter);

            themeService.SetAccent((SolidColorBrush)new BrushConverter().ConvertFrom("#9a00e5")!);
        }

        #region INavigationWindow methods

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);
        
        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider)
            => RootNavigation.SetPageProviderService(navigationViewPageProvider);

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }

        INavigationView INavigationWindow.GetNavigation()
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}
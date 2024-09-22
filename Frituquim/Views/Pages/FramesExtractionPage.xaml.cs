using Frituquim.ViewModels;
using Wpf.Ui.Controls;

namespace Frituquim.Views.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class FramesExtractionPage : INavigableView<FramesExtractionViewModel>
    {
        public FramesExtractionViewModel ViewModel
        {
            get;
        }

        public FramesExtractionPage(FramesExtractionViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
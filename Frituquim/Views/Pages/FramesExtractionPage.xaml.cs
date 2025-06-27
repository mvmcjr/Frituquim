using Frituquim.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace Frituquim.Views.Pages;

/// <summary>
///     Interaction logic for DashboardPage.xaml
/// </summary>
public partial class FramesExtractionPage : INavigableView<FramesExtractionViewModel>
{
    public FramesExtractionPage(FramesExtractionViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public FramesExtractionViewModel ViewModel { get; }
}
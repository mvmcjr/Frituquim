using Frituquim.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace Frituquim.Views.Pages;

public partial class ConvertPage : INavigableView<ConvertViewModel>
{
    public ConvertPage(ConvertViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public ConvertViewModel ViewModel { get; }
}
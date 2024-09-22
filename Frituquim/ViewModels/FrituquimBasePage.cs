using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Frituquim.ViewModels;

public partial class FrituquimBasePage : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoadingVisibility))]
    [NotifyPropertyChangedFor(nameof(IsLoading))]
    private bool _isExtractButtonEnabled = true;

    public bool IsLoading => !IsExtractButtonEnabled;

    public Visibility IsLoadingVisibility => IsExtractButtonEnabled ? Visibility.Collapsed : Visibility.Visible;
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ShowIndeterminateProgress))]
    private double? _currentProgress;
    
    public bool ShowIndeterminateProgress => !CurrentProgress.HasValue;
}
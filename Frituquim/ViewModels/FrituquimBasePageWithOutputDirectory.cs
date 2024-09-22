using System;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Frituquim.ViewModels;

public partial class FrituquimBasePageWithOutputDirectory : FrituquimBasePage
{
    [ObservableProperty] private string _outputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
    
    [ObservableProperty] private bool _openFolderAfterExecution = true;
    
    [RelayCommand]
    private void OpenOutputDirectoryDialog()
    {
        var fileDialog = new FolderBrowserDialog();
        if (fileDialog.ShowDialog() == DialogResult.OK)
        {
            OutputDirectory = fileDialog.SelectedPath;
        }
    }
}
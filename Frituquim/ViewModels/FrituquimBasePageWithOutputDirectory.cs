using System;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Syroot.Windows.IO;

namespace Frituquim.ViewModels;

public partial class FrituquimBasePageWithOutputDirectory : FrituquimBasePage
{
    [ObservableProperty] private string _outputDirectory = KnownFolders.Downloads.Path;
    
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
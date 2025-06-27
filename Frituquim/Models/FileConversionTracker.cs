using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Frituquim.Models;

public partial class FileConversionTracker : ObservableObject
{
    [ObservableProperty] private string _fileName;
    
    [ObservableProperty] private string _filePath;
    
    [ObservableProperty] private string _outputPath;
    
    [ObservableProperty] private ConversionStatus _status;
    
    [ObservableProperty] private double _progress;
    
    [ObservableProperty] private double? _speed;
    
    [ObservableProperty] private TimeSpan? _elapsed;
    
    [ObservableProperty] private string? _errorMessage;
    
    [ObservableProperty] private TimeSpan? _duration;
    
    [ObservableProperty] private TimeSpan? _currentTime;
    
    [ObservableProperty] private TimeSpan? _estimatedTimeRemaining;
    
    public DateTime StartTime { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    public FileConversionTracker(string filePath, string outputPath)
    {
        FilePath = filePath;
        OutputPath = outputPath;
        FileName = System.IO.Path.GetFileName(filePath);
        Status = ConversionStatus.Pending;
        StartTime = DateTime.Now;
    }
    
    
    public void UpdateProgress(double? speed, TimeSpan? currentTime) 
    {
        if (speed != null && Speed != speed)
        {
            Speed = speed;
        }
        
        if (currentTime != null && CurrentTime != currentTime)
        {
            CurrentTime = currentTime;
        }
        
        var newElapsed = DateTime.Now - StartTime;
        if (Elapsed != newElapsed)
        {
            Elapsed = newElapsed;
        }
        
        if (Duration.HasValue && CurrentTime.HasValue && Duration.Value.TotalSeconds > 0)
        {
            var progressRatio = CurrentTime.Value.TotalSeconds / Duration.Value.TotalSeconds;
            Progress = progressRatio;
            
            if (progressRatio is > 0 and < 1 && Elapsed is { TotalSeconds: > 0 })
            {
                var totalEstimatedTime = Elapsed.Value.TotalSeconds / progressRatio;
                var remainingTime = totalEstimatedTime - Elapsed.Value.TotalSeconds;
                TimeSpan? newEta = remainingTime > 0 ? TimeSpan.FromSeconds(remainingTime) : null;
                
                if (newEta.HasValue)
                {
                    EstimatedTimeRemaining = newEta;
                }
            }
        }
    }
    
    public void SetDuration(TimeSpan duration)
    {
        Duration = duration;
    }
    
    public void SetCompleted(bool success, string? errorMessage = null)
    {
        Status = success ? ConversionStatus.Completed : ConversionStatus.Failed;
        EndTime = DateTime.Now;
        ErrorMessage = errorMessage;
        Elapsed = EndTime - StartTime;
    }
    
    public void SetCancelled()
    {
        Status = ConversionStatus.Cancelled;
        EndTime = DateTime.Now;
        Elapsed = EndTime - StartTime;
    }
}

public enum ConversionStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Cancelled
}

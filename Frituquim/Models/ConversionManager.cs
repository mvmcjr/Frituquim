using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using CliWrap;
using CommunityToolkit.Mvvm.ComponentModel;
using EnumerableAsyncProcessor.Extensions;
using Frituquim.Helpers;

namespace Frituquim.Models;

public partial class ConversionManager : ObservableObject, IDisposable
{
    private readonly Dispatcher _dispatcher;
    private readonly IDisposable _globalSubscription;

    private readonly Subject<(FileConversionTracker Tracker, double? Speed, TimeSpan? CurrentTime)> _progressSubject =
        new();

    private readonly IDisposable _progressSubscription;
    private readonly List<double> _speedHistory = new();
    private readonly object _speedLock = new();
    private readonly Regex _speedRegex = new(@"speed=\s*([0-9.]+)x", RegexOptions.Compiled);
    private readonly DateTime _startTime;
    private readonly Regex _timeMs = new(@"out_time_ms=(\d+)", RegexOptions.Compiled);
    private readonly Regex _timeRegex = new(@"out_time_us=(\d+)", RegexOptions.Compiled);
    private readonly Regex _timeRegexAlt = new(@"time=(\d{2}:\d{2}:\d{2}.\d{2})", RegexOptions.Compiled);

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ShouldBeVisible))]
    private ObservableCollection<FileConversionTracker> _activeConversions = new();

    [ObservableProperty]
    private string? _averageSpeed;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ShouldBeVisible))]
    private ObservableCollection<FileConversionTracker> _completedConversions = new();

    [ObservableProperty]
    private string? _currentStatus;

    [ObservableProperty]
    private string? _estimatedTimeRemaining;

    [ObservableProperty]
    private double _overallProgress;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ShouldBeVisible))]
    private int _totalFiles;

    public ConversionManager(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
        _startTime = DateTime.Now;

        _progressSubscription = _progressSubject
            .GroupBy(update => update.Tracker)
            .SelectMany(trackerGroup => trackerGroup
                .Scan((prev, current) =>
                    (current.Tracker,
                        Speed: current.Speed ?? prev.Speed,
                        CurrentTime: current.CurrentTime ?? prev.CurrentTime)
                )
                .Sample(TimeSpan.FromMilliseconds(500))
                .Where(update => update.Speed != null && update.CurrentTime != null)
                .ObserveOn(Scheduler.Default))
            .Subscribe(update =>
            {
                dispatcher.Invoke(() => update.Tracker.UpdateProgress(update.Speed, update.CurrentTime));

                if (update.Speed.HasValue)
                    lock (_speedLock)
                    {
                        _speedHistory.Add(update.Speed.Value);
                        if (_speedHistory.Count > 20)
                            _speedHistory.RemoveAt(0);
                    }
            });

        _globalSubscription = _progressSubject
            .Where(update => update.Speed.HasValue)
            .Throttle(TimeSpan.FromSeconds(1))
            .ObserveOn(Scheduler.Default)
            .Subscribe(_ => _dispatcher.BeginInvoke(UpdateAverageSpeedAndETA, DispatcherPriority.Background));
    }

    public bool ShouldBeVisible => TotalFiles > 0 || ActiveConversions.Count > 0 || CompletedConversions.Count > 0;

    public void Dispose()
    {
        _globalSubscription.Dispose();
        _progressSubscription.Dispose();
        _progressSubject.Dispose();

        GC.SuppressFinalize(this);
    }

    public async Task StartConversions(
        string[] filePaths,
        string outputDirectory,
        bool saveInSameDirectory,
        ConversionType conversionType,
        ConversionHardware conversionHardware,
        CancellationToken cancellationToken,
        int maxParallelism = 3)
    {
        TotalFiles = filePaths.Length;

        var trackers = filePaths.Select(filePath =>
        {
            var outputDir = saveInSameDirectory
                ? Path.GetDirectoryName(filePath) ?? outputDirectory
                : outputDirectory;
            var outputPath = Path.Combine(outputDir, Path.ChangeExtension(Path.GetFileName(filePath), ".mp4"));
            return new FileConversionTracker(filePath, outputPath);
        }).ToArray();

        await trackers.ToAsyncProcessorBuilder()
            .ForEachAsync(async tracker =>
                await ProcessFile(tracker, conversionType, conversionHardware, cancellationToken))
            .ProcessInParallel(maxParallelism);
    }

    private async Task ProcessFile(
        FileConversionTracker tracker,
        ConversionType conversionType,
        ConversionHardware conversionHardware,
        CancellationToken cancellationToken)
    {
        try
        {
            await _dispatcher.InvokeAsync(() =>
            {
                ActiveConversions.Add(tracker);
                tracker.Status = ConversionStatus.InProgress;
                UpdateCurrentStatus();
            });

            var duration = await FFmpegHelper.GetVideoDurationAsync(tracker.FilePath);
            if (duration.HasValue) await _dispatcher.InvokeAsync(() => tracker.SetDuration(duration.Value));

            if (File.Exists(tracker.OutputPath)) File.Delete(tracker.OutputPath);

            var ffmpegCommand = FFmpegHelper.ConvertFile(
                    tracker.FilePath,
                    tracker.OutputPath,
                    conversionType,
                    conversionHardware,
                    progressLine => HandleProgressUpdate(tracker, progressLine))
                .WithValidation(CommandResultValidation.None);

            var result = await ffmpegCommand.ExecuteAsync(cancellationToken);
            var success = result.ExitCode == 0;

            if (!success && !cancellationToken.IsCancellationRequested)
                tracker.SetCompleted(false, $"FFmpeg exited with code {result.ExitCode}");
            else if (!cancellationToken.IsCancellationRequested) tracker.SetCompleted(true);
        }
        catch (OperationCanceledException)
        {
            tracker.SetCancelled();

            if (File.Exists(tracker.OutputPath))
                try
                {
                    File.Delete(tracker.OutputPath);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Failed to delete output file {tracker.OutputPath}: {e.Message}");
                }
        }
        catch (Exception ex)
        {
            tracker.SetCompleted(false, ex.Message);
        }
        finally
        {
            await _dispatcher.InvokeAsync(() =>
            {
                ActiveConversions.Remove(tracker);
                CompletedConversions.Add(tracker);
                UpdateOverallProgress();
                UpdateCurrentStatus();
            });
        }
    }

    private void HandleProgressUpdate(FileConversionTracker tracker, string progressLine)
    {
        if (string.IsNullOrWhiteSpace(progressLine)) return;

        double? speed = null;
        TimeSpan? currentTime = null;
        var hasUpdate = false;

        var speedMatch = _speedRegex.Match(progressLine);
        if (speedMatch.Success && double.TryParse(speedMatch.Groups[1].Value, out var speedValue) && speedValue > 0)
        {
            speed = speedValue;
            hasUpdate = true;
        }

        var timeMatch = _timeRegex.Match(progressLine);
        if (timeMatch.Success && long.TryParse(timeMatch.Groups[1].Value, out var timeMicroseconds))
        {
            currentTime = TimeSpan.FromMicroseconds(timeMicroseconds);
            hasUpdate = true;
        }
        else
        {
            var timeMsMatch = _timeMs.Match(progressLine);
            if (timeMsMatch.Success && long.TryParse(timeMsMatch.Groups[1].Value, out var timeMilliseconds))
            {
                currentTime = TimeSpan.FromMicroseconds(timeMilliseconds);
                hasUpdate = true;
            }
            else
            {
                var timeAltMatch = _timeRegexAlt.Match(progressLine);
                if (timeAltMatch.Success && TimeSpan.TryParse(timeAltMatch.Groups[1].Value, out var timeSpan))
                {
                    currentTime = timeSpan;
                    hasUpdate = true;
                }
            }
        }

        if (hasUpdate) _progressSubject.OnNext((tracker, speed, currentTime));
    }

    private void UpdateOverallProgress()
    {
        if (TotalFiles == 0) return;

        var completed = CompletedConversions.Count;
        OverallProgress = (double)completed / TotalFiles * 100;
    }

    private void UpdateAverageSpeedAndETA()
    {
        lock (_speedLock)
        {
            if (_speedHistory.Count > 0)
            {
                var avgSpeed = _speedHistory.Average();
                AverageSpeed = ActiveConversions.Count > 1
                    ? $"{avgSpeed:F1}x (média, {ActiveConversions.Count} arquivos)"
                    : $"{avgSpeed:F1}x";
            }
        }

        var activeFilesWithETA = ActiveConversions.Where(c => c.EstimatedTimeRemaining.HasValue).ToList();
        var completedFiles = CompletedConversions.Count;
        var totalFiles = TotalFiles;

        if (activeFilesWithETA.Count > 0)
        {
            var maxActiveETA = activeFilesWithETA.Max(c => c.EstimatedTimeRemaining!.Value);

            var remainingFiles = totalFiles - completedFiles - ActiveConversions.Count;
            if (remainingFiles > 0 && completedFiles > 0)
            {
                var elapsed = DateTime.Now - _startTime;
                var avgTimePerFile = elapsed.TotalSeconds / completedFiles;
                var estimatedTimeForRemainingFiles = TimeSpan.FromSeconds(avgTimePerFile * remainingFiles);

                var totalETA = maxActiveETA.Add(estimatedTimeForRemainingFiles);
                EstimatedTimeRemaining = totalETA.TotalSeconds > 0
                    ? totalETA.TotalHours >= 1 ? totalETA.ToString(@"hh\:mm\:ss") : totalETA.ToString(@"mm\:ss")
                    : null;
            }
            else
            {
                EstimatedTimeRemaining = maxActiveETA.TotalSeconds > 0
                    ? maxActiveETA.TotalHours >= 1
                        ? maxActiveETA.ToString(@"hh\:mm\:ss")
                        : maxActiveETA.ToString(@"mm\:ss")
                    : null;
            }
        }
        else if (completedFiles > 0 && ActiveConversions.Count > 0)
        {
            var elapsed = DateTime.Now - _startTime;
            var completionRate = completedFiles / elapsed.TotalSeconds;
            var remaining = totalFiles - completedFiles;

            if (completionRate > 0)
            {
                var remainingSeconds = remaining / completionRate;
                var eta = TimeSpan.FromSeconds(remainingSeconds);

                EstimatedTimeRemaining = eta.TotalSeconds > 0
                    ? eta.TotalHours >= 1 ? eta.ToString(@"hh\:mm\:ss") : eta.ToString(@"mm\:ss")
                    : null;
            }
        }
    }

    private void UpdateCurrentStatus()
    {
        if (ActiveConversions.Count == 0)
        {
            CurrentStatus = null;
        }
        else if (ActiveConversions.Count == 1)
        {
            CurrentStatus = $"Convertendo: {ActiveConversions.First().FileName}";
        }
        else
        {
            var first = ActiveConversions.First();
            CurrentStatus = $"Convertendo: {first.FileName} (+{ActiveConversions.Count - 1} outros)";
        }
    }

    public void Reset()
    {
        ActiveConversions.Clear();
        CompletedConversions.Clear();
        TotalFiles = 0;
        OverallProgress = 0;
        AverageSpeed = null;
        EstimatedTimeRemaining = null;
        CurrentStatus = null;

        lock (_speedLock)
        {
            _speedHistory.Clear();
        }
    }
}
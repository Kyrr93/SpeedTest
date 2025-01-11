using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpeedTest.Services;
using System;
using System.Threading.Tasks;

namespace SpeedTest.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ISpeedTestService _speedTestService;

    [ObservableProperty]
    private double downloadSpeed;

    [ObservableProperty]
    private double uploadSpeed;

    [ObservableProperty]
    private double currentSpeed;

    [ObservableProperty]
    private double progress;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private int ping;

    [ObservableProperty]
    private string serverLocation = string.Empty;

    [ObservableProperty]
    private string buttonText = "Start Speed Test";

    [ObservableProperty]
    private bool isTestRunning;

    public MainViewModel(ISpeedTestService speedTestService)
    {
        _speedTestService = speedTestService;
    }

    [RelayCommand(CanExecute = nameof(CanStartTest))]
    private async Task StartTestAsync()
    {
        try
        {
            IsTestRunning = true;
            ButtonText = "Testing...";
            ResetValues();

            // Test ping
            StatusMessage = "Testing ping...";
            Progress = 5;
            Ping = await _speedTestService.TestPingAsync();
            ServerLocation = _speedTestService.GetServerLocation();

            // Test download speed
            StatusMessage = "Testing download speed...";
            Progress = 10;
            var downloadProgress = new Progress<SpeedTestResult>(result =>
            {
                DownloadSpeed = Math.Round(result.Speed, 1);
                CurrentSpeed = Math.Round(result.InstantSpeed, 1);
                Progress = 10 + (result.Speed / 100.0 * 40); // 40% of progress bar for download
            });

            await _speedTestService.TestDownloadSpeedAsync(downloadProgress);

            // Test upload speed
            StatusMessage = "Testing upload speed...";
            Progress = 50;
            var uploadProgress = new Progress<SpeedTestResult>(result =>
            {
                UploadSpeed = Math.Round(result.Speed, 1);
                CurrentSpeed = Math.Round(result.InstantSpeed, 1);
                Progress = 50 + (result.Speed / 100.0 * 40); // 40% of progress bar for upload
            });

            await _speedTestService.TestUploadSpeedAsync(uploadProgress);

            // Complete
            Progress = 100;
            StatusMessage = "Test completed";
            ButtonText = "Start New Test";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error occurred: {ex.Message}";
            ButtonText = "Retry Test";
        }
        finally
        {
            IsTestRunning = false;
        }
    }

    private void ResetValues()
    {
        DownloadSpeed = 0;
        UploadSpeed = 0;
        CurrentSpeed = 0;
        Progress = 0;
        StatusMessage = "Initializing...";
    }

    private bool CanStartTest() => !IsTestRunning;
}
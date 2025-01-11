using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SpeedTest.Services
{
    public class SpeedTestService : ISpeedTestService
    {
        private readonly HttpClient _httpClient;
        private const string TEST_SERVER = "https://speed.cloudflare.com";
        private const string DOWNLOAD_TEST_FILE = "/__down?bytes=";
        private const string UPLOAD_TEST_FILE = "/__up";
        private readonly int[] FILE_SIZES = { 1000000, 2000000, 5000000, 10000000 };

        public SpeedTestService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(2)
            };
        }

        public async Task<double> TestDownloadSpeedAsync(IProgress<SpeedTestResult> progress)
        {
            var speeds = new List<double>();
            var speedWatch = new Stopwatch();
            var totalBytes = 0L;

            foreach (var fileSize in FILE_SIZES)
            {
                var url = $"{TEST_SERVER}{DOWNLOAD_TEST_FILE}{fileSize}";
                speedWatch.Restart();

                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                using var stream = await response.Content.ReadAsStreamAsync();
                var buffer = new byte[8192];
                var bytesRead = 0;
                var lastReportTime = TimeSpan.Zero;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    totalBytes += bytesRead;
                    var elapsed = speedWatch.Elapsed;

                    // Report instant speed every 100ms
                    if (elapsed - lastReportTime > TimeSpan.FromMilliseconds(100))
                    {
                        var instantSpeed = CalculateSpeed(bytesRead, elapsed - lastReportTime);
                        var averageSpeed = CalculateSpeed(totalBytes, elapsed);

                        progress.Report(new SpeedTestResult
                        {
                            Speed = averageSpeed,
                            InstantSpeed = instantSpeed
                        });

                        lastReportTime = elapsed;
                    }
                }

                speedWatch.Stop();
                var finalSpeed = CalculateSpeed(totalBytes, speedWatch.Elapsed);
                speeds.Add(finalSpeed);
            }

            return speeds.Average();
        }

        public async Task<double> TestUploadSpeedAsync(IProgress<SpeedTestResult> progress)
        {
            var speeds = new List<double>();
            var speedWatch = new Stopwatch();
            var totalBytes = 0L;

            foreach (var fileSize in FILE_SIZES)
            {
                var testData = new byte[8192]; // Use smaller chunks for more frequent updates
                new Random().NextBytes(testData);
                speedWatch.Restart();
                var lastReportTime = TimeSpan.Zero;

                for (int i = 0; i < fileSize; i += testData.Length)
                {
                    var content = new ByteArrayContent(testData);
                    await _httpClient.PostAsync($"{TEST_SERVER}{UPLOAD_TEST_FILE}", content);

                    totalBytes += testData.Length;
                    var elapsed = speedWatch.Elapsed;

                    // Report instant speed every 100ms
                    if (elapsed - lastReportTime > TimeSpan.FromMilliseconds(100))
                    {
                        var instantSpeed = CalculateSpeed(testData.Length, elapsed - lastReportTime);
                        var averageSpeed = CalculateSpeed(totalBytes, elapsed);

                        progress.Report(new SpeedTestResult
                        {
                            Speed = averageSpeed,
                            InstantSpeed = instantSpeed
                        });

                        lastReportTime = elapsed;
                    }
                }

                speedWatch.Stop();
                var finalSpeed = CalculateSpeed(totalBytes, speedWatch.Elapsed);
                speeds.Add(finalSpeed);
            }

            return speeds.Average();
        }

        public async Task<int> TestPingAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            await _httpClient.GetAsync(TEST_SERVER);
            stopwatch.Stop();
            return (int)stopwatch.ElapsedMilliseconds;
        }

        public string GetServerLocation()
        {
            return "Cloudflare Speed Test Server";
        }

        private double CalculateSpeed(long bytes, TimeSpan timeSpan)
        {
            var seconds = timeSpan.TotalSeconds;
            if (seconds == 0) return 0;
            return bytes * 8.0 / 1000000 / seconds; // Convert to Mbps
        }
    }
}

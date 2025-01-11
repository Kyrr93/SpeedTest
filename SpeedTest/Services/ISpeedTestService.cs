using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedTest.Services
{
    public interface ISpeedTestService
    {
        Task<double> TestDownloadSpeedAsync(IProgress<SpeedTestResult> progress);
        Task<double> TestUploadSpeedAsync(IProgress<SpeedTestResult> progress);
        Task<int> TestPingAsync();
        string GetServerLocation();
    }
}

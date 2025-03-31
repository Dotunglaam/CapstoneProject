using Microsoft.AspNetCore.Mvc;
using System.Management;

namespace ProjectCapstone.Controllers
{
    [ApiController]
    [Route("api/system")]
    public class SystemController : ControllerBase
    {
        [HttpGet("GetStatus")]
        public IActionResult GetSystemStatus()
        {
            var status = new
            {
                Uptime = DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime,
                CpuUsage = GetCpuUsage(),
                MemoryUsage = GetMemoryUsage(),
                StatusMessage = "The system is operating stably."
            };

            return Ok(status);
        }

        private string GetCpuUsage()
        {
            try
            {
                var cpuQuery = new ManagementObjectSearcher("select * from Win32_Processor");
                double totalUsage = 0;
                int coreCount = 0;

                foreach (var item in cpuQuery.Get())
                {
                    var usage = item["LoadPercentage"];
                    if (usage != null)
                    {
                        totalUsage += Convert.ToDouble(usage);
                        coreCount++;
                    }
                }

                return coreCount > 0 ? $"{totalUsage / coreCount:0.0}%" : "N/A";
            }
            catch
            {
                return "Cannot get CPU Usage";
            }
        }

        private string GetMemoryUsage()
        {
            // Lấy bộ nhớ riêng của tiến trình hiện tại (MB)
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var usedMemory = process.PrivateMemorySize64 / (1024 * 1024);

            // Tổng dung lượng RAM của hệ thống (MB)
            var totalMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024);

            return $"{usedMemory} MB / {totalMemory} MB";
        }
    }

}

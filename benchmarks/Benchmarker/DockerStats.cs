using System.Diagnostics;
using System.Text.Json;

namespace Benchmarker;

public class DockerStats
{
    public static async Task<decimal> GetMemoryUsageForContainer(string container)
    {
        var proc = Process.Start(new ProcessStartInfo("docker", "stats " + container + " --no-stream --format json")
        {
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true
        })!;
        await proc.WaitForExitAsync();
        var json = JsonSerializer.Deserialize<DockerStatOutput>(proc.StandardOutput.BaseStream)!;
        // Find the usage
        var memUsage = json.MemUsage;
        var xibStart = memUsage.IndexOf('M');
        var isMiB = true;
        if (xibStart == -1)
        {
            xibStart = memUsage.IndexOf('G');
            isMiB = false;
        }
        var memUsageStr = memUsage.Substring(0, xibStart);
        var memUsageNum = decimal.Parse(memUsageStr);
        if (!isMiB)
        {
            memUsageNum *= 1024;
        }
        return memUsageNum;
    }
}


public class DockerStatOutput
{
    public string MemUsage { get; set; }
}
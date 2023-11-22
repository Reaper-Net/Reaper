namespace BenchmarkWeb.Services;

public class GetMeAStringService
{
    /// <summary>
    /// This purely exists to simulate some work.
    /// </summary>
    /// <returns>"Hello, World!"</returns>
    public async Task<string> GetMeAString()
    {
        using var str = new MemoryStream();
        await using var writer = new StreamWriter(str);
        using var reader = new StreamReader(str);

        await writer.WriteAsync("Hello, ");
        await writer.WriteAsync("World!");
        await writer.FlushAsync();
        str.Seek(0, SeekOrigin.Begin);
        return await reader.ReadToEndAsync();
    }
}
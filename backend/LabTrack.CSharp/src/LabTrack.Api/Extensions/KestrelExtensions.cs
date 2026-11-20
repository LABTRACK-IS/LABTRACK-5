using Microsoft.AspNetCore.Hosting;

namespace LabTrack.Api.Extensions;

public static class KestrelExtensions
{
    public static IWebHostBuilder ConfigureCrossDeviceKestrel(this IWebHostBuilder webHost)
    {
        return webHost.ConfigureKestrel(options =>
        {
            // Listen on 0.0.0.0:5000 and 0.0.0.0:5001 so Desktop, Laptop, Mobile QR Scanners, and LAN clients connect seamlessly
            options.ListenAnyIP(5000);
            options.ListenAnyIP(5001);
        });
    }
}

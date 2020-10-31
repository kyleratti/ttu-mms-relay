using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ttu_mms_relay
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
              webBuilder.UseKestrel(options =>
              {
                options.Listen(System.Net.IPAddress.Any, 5000);

              });
              webBuilder.UseStartup<Startup>();
            });
  }
}

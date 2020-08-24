using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace core_api
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
                    webBuilder.UseStartup<Startup>()
                    .UseKestrel()
                    .UseIISIntegration();
                });
        // todo
        //.ConfigureWebHost(x =>
        //x.UseUrls("https://localhost:4000", "http://localhost:4001"));
    }
}

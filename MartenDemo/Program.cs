using System.Threading.Tasks;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weasel.Core;

namespace MartenDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddMarten(options =>
                {
                    options.Connection("host=localhost;port=5432;database=postgres;user id=postgres;password=mysecretpassword");
                    options.AutoCreateSchemaObjects = AutoCreate.All; // For demo purposes only
                });
                services.AddHostedService<DemoService>();
            }).RunConsoleAsync();
        }
    }
}

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using App.Metrics.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Sillycore;
using Sillycore.AppMetrics;
using Sillycore.NLog;
using Sillycore.Web;
using WebApplication.Domain;

namespace WebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /*
             * To enable authentication & authorization:
             *  1) Comment in the WithAuthentication and its following options
             *  2) Comment in the Authorization attribute above the SampleController class declaration
             */
            RunRequestsToWebApplication();

            SillycoreAppBuilder.Instance
                .UseUtcTimes()
                .UseNLog()
                .WithAppMetrics()
                .UseWebApi("WebApplication")
                    .WithSwagger(false)
                    .WithAuthentication()
                        .As("AuthServer")
                        .WithPolicy("defaultPolicy", "lookup")
                        .Then()
                    .Build();
            
        }

        public static readonly Uri ApiBaseAddress = new Uri("http://localhost:5000/");

        private static void RunRequestsToWebApplication()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = ApiBaseAddress
            };

            var requestSamplesScheduler = new AppMetricsTaskScheduler(TimeSpan.FromMilliseconds(10), async () =>
            {
                Sample data = new Sample(){CreatedOn = DateTime.UtcNow, Id = Guid.NewGuid(), Name = "Test"};

                var samplesGet = httpClient.GetStringAsync("samples");
                var samplesPost = httpClient.PostAsync("samples", new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
                await Task.WhenAll(samplesGet,samplesPost);
            });

            requestSamplesScheduler.Start();
        }

    }
}

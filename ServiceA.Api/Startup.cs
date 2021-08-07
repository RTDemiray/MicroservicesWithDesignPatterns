using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

namespace ServiceA.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ServiceA.Api", Version = "v1" });
            });
            
            services.AddHttpClient<ProductService>(opt =>
            {
                opt.BaseAddress = new Uri("https://localhost:5003/api/products/");
            }).AddPolicyHandler(GetAdvanceCircuitBreakerPolicy());
        }
        
        private IAsyncPolicy<HttpResponseMessage> GetAdvanceCircuitBreakerPolicy()
        {
            //@TODO: 10 saniye içerisinde 100 tane istekden 10 tanesi başarısız ise çalışır.
            return HttpPolicyExtensions.HandleTransientHttpError().AdvancedCircuitBreakerAsync(0.1, TimeSpan.FromSeconds(30), 30, TimeSpan.FromSeconds(30), onBreak: (arg1, arg2) =>
            {
                Debug.WriteLine("Circuit Breaker Status => On Break");
            }, onReset: () =>
            {
                Debug.WriteLine("Circuit Breaker Status => On Reset");
            }, onHalfOpen: () =>
            {
                Debug.WriteLine("Circuit Breaker Status => On Half Open");
            });
        }

        private IAsyncPolicy<HttpResponseMessage> getCircuitBreakerPolicy()
        {
            //@TODO: Art arda 3 tane başarısız istek olduğunda 10 saniye bekle.
            return HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(3, TimeSpan.FromSeconds(10),
                onBreak:
                (arg1,arg2) =>
                {
                    Debug.WriteLine("Circuit Breaker Status => On Break");
                }
                , onHalfOpen: () =>
                {
                    Debug.WriteLine("Circuit Breaker Status => On Half Open");
                }, onReset:
                () =>
                {
                    Debug.WriteLine("Circuit Breaker Status => On Reset");
                });
        }

        private IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            //@TODO: 10 saniye bekle 5 kez tekrar et.
            return HttpPolicyExtensions.HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound).WaitAndRetryAsync(5,
                    retryAttempt =>
                    {
                        Debug.WriteLine($"Retry Count: {retryAttempt}");
                        return TimeSpan.FromSeconds(10);
                    });
        }

        //@TODO: Retry yapmadan önce işlenecek business kodu.
        private Task onRetryAsync(DelegateResult<HttpResponseMessage> arg1, TimeSpan arg2)
        {
            Debug.WriteLine($"Request is made again: {arg2.TotalMilliseconds}");
            return Task.CompletedTask;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceA.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
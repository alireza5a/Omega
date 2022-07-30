using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sentry;
using Sentry.AspNetCore;

namespace App
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseSentryTracing();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async (context) =>
                {
                    SentryId sentryId = SentrySdk.GetSpan().TraceId;
                    string traceId = sentryId.ToString();
                    
                    string filePath = Path.Combine(Path.GetFullPath("."), "FrontEnd", "index.html");
                    string content = File.ReadAllText(filePath);
                    content = content.Replace("#Trace_Id", traceId);
                    
                    var sentry_dsn = Environment.GetEnvironmentVariable("SENTRY_DSN");
                    context.Response.Headers.Append("Set-Cookie", "SENTRY_DSN" + "=" + sentry_dsn + "; path=/");
                    await context.Response.WriteAsync(content);
                });
                
                endpoints.MapGet("/Hello", async context =>
                {
                    var transaction = SentrySdk.StartTransaction(
                        "HelloPage",
                        "HelloPage-operation"
                    );
                    var span = transaction.StartChild("HelloPage-child-operation");
                    SentrySdk.GetTraceHeader();
                    await context.Response.WriteAsync("Hello World!");
                    span.Finish();
                    transaction.Finish();
                });
                endpoints.MapGet("/POW", async context =>
                {
                    var transaction = SentrySdk.StartTransaction(
                        "Omega",
                        "POW"
                    );
                    var getParametersSpan = transaction.StartChild("Get-parameters-from-querystring");
                    Console.WriteLine(SentrySdk.GetSpan().TraceId);
                    Console.WriteLine("**" + SentrySdk.GetTraceHeader().TraceId);
                    var queryCollection = context.Request.Query;
                    var a = Convert.ToInt32(queryCollection["a"]);
                    var b = Convert.ToInt32(queryCollection["b"]);
                    getParametersSpan.Finish(SpanStatus.Ok);
                    var powerSpan = transaction.StartChild("Power-operation");
                    if (a == 2 && b == 3)
                        System.Threading.Thread.Sleep(5000);
                    await context.Response.WriteAsync(Math.Pow(a, b).ToString());
                    powerSpan.Finish(SpanStatus.Ok);
                    transaction.Finish(SpanStatus.Ok);
                });	
                endpoints.MapGet("/Exception", async context =>
                {
                    throw new ApplicationException("An Exception Occured!");
                });
            });
        }
    }
}

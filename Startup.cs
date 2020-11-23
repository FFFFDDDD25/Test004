using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.IO;
using FluentEmail.Core;
using FluentEmail.Mailgun;

namespace Test004
{
    public class Startup
    {
        public FluentEmail.Core.Models.SendResponse Send(string Subject, string Body)
        {
            return Email
                      .From("killuplus300@gmail.com")
                      .To("killuplus300@gmail.com")
                      .Subject(Subject)
                      .Body(Body).Send();
        }

        ConcurrentDictionary<string,string> urls = new ConcurrentDictionary<string, string>();
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Email.DefaultSender = new MailgunSender(
                               Configuration["Mailgun:Domain"], // Mailgun Domain
                               Configuration["Mailgun:APIKey"] // Mailgun API Key
                       );

            new Thread(() =>
            {
                while (true)
                {
                    List<string> fail_urls = new List<string>();
                    foreach(KeyValuePair<string,string> kv in urls)
                    {
                        var url = kv.Key;
                    
                        try{
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            using (Stream stream = response.GetResponseStream())
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                var content = reader.ReadToEnd();
                            }
                        }
                        catch{
                                fail_urls.Add(url);
                        }

                    }

                    foreach(var url in fail_urls)
                    {
                        Send("失敗:"+url, "失敗:"+url);
                        //Console.WriteLine("失敗:"+url);
                        urls.TryRemove(url, out string _);
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(60));
                }
            }).Start();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello");
                });


                endpoints.MapGet("/Submit", async context =>
                {
                    var qs = context.Request.QueryString;
                    var parse = System.Web.HttpUtility.ParseQueryString(qs.Value);
                    var url = parse["checkThisUrlEveryMin"];
                    urls.TryAdd(url,"");
                    await context.Response.WriteAsync(url);
                });
            });
        }
    }
}

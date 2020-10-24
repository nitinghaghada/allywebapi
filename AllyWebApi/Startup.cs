using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AllyWebApi
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
      services.AddCors();

      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2).ConfigureApiBehaviorOptions(options =>
        {
          options.SuppressConsumesConstraintForFormFileParameters = true;
          options.SuppressInferBindingSourcesForParameters = true;
          options.SuppressModelStateInvalidFilter = true;
          options.SuppressUseValidationProblemDetailsForInvalidModelStateResponses = true;
        });
      services.AddMemoryCache();


      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddAuthentication().AddCookie();
      services.Configure<CookiePolicyOptions>(options =>
      {
        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        options.CheckConsentNeeded = context => false;
        options.MinimumSameSitePolicy = SameSiteMode.None;
      });
      //services.AddScoped<ICookieService, CookieService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddFile("Logs/myapp-{Date}.txt");
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseCors(builder =>
      {
        builder.WithOrigins("http://localhost:4200");
        builder.WithOrigins(Configuration["AllyApi:PortalUrl"]); 
        builder.AllowAnyMethod();
        builder.AllowAnyHeader();
      });
      app.UseMvc();
    }
  }
}

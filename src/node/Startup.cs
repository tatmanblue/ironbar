using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using node.Ledger;

namespace node
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddGrpc();
            services.AddControllers();

            // For now, all nodes have ledger manager,  there might be some differences in behavior
            // between a bootnode ledger manager and a child nodes ledger manager that will
            // make us want to split this out
            services.AddSingleton<ILedgerManager, LedgerManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifeTime, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder
               .AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());

            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // TODO: seems like this is bootnode configuration but not going to work
                // for the other nodes
                endpoints.MapGrpcService<BootNodeRPCService>();
                endpoints.MapControllers();
            });

            

            app.StartLedger();
            lifeTime.ApplicationStopping.Register(() => {
                logger.LogInformation("Application is shutting down");
            });

        }

    }
}

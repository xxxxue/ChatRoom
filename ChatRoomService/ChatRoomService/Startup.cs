using ChatRoomService.EF;
using ChatRoomService.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace ChatRoomService
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // 添加 SignalR 服务 
            services.AddSignalR().AddNewtonsoftJsonProtocol(options =>
            {
                // 忽略json实体循环引用
                options.PayloadSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; 
            });

            services.AddDatabaseAccessor(options =>
            {
                // 将自定义的DbContext加入到DbPool
                options.AddDbPool<SqliteDbContext>();
            },"ChatRoomService");
            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
           
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
              
                // SignalR 路由
                endpoints.MapHub<ChatHub>("/Chat");
            });
        }
    }
}
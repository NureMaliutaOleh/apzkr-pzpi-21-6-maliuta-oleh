using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SmartInlet.Server.Services.DB;
using SmartInlet.Server.Services.Email;

namespace SmartInlet.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<DbApp>(options =>
            {
                options.UseSqlServer(builder.Configuration["EF:ConnectionString"]);
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            builder.Services.AddEmailService(e =>
            {
                e.Email = builder.Configuration["Email:EmailAddress"];
                e.SenderName = builder.Configuration["Email:SenderName"];
                e.Password = builder.Configuration["Email:Password"];
                e.Host = builder.Configuration["Email:Host"];
                e.Port = int.Parse(builder.Configuration["Email:Port"]);
                e.EmailTemplatesFolder = builder.Configuration["Email:EmailTemplatesFolder"];
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DbApp>();

                if (!db.Database.CanConnect())
                {
                    throw new NotImplementedException("Can not connect to the DB!");
                }
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseAuthentication();
            app.UseCors(builder =>
                builder.AllowCredentials().AllowAnyHeader().AllowAnyMethod().WithOrigins(
                    "https://localhost:5173"
                ));

            app.MapControllers();
            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}

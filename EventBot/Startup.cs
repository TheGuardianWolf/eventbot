using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Serilog;
using System.Globalization;
using MongoDB.Driver;
using EventBot.Data.Bot;
using Telegram.Bot;
using EventBot.Repositories.DataSources;
using Microsoft.Extensions.Configuration;
using EventBot.Services.Hosted;
using EventBot.Repositories;
using EventBot.Services;
using EventBot.Services.Bot;
using NodaTime;

namespace EventBot
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Environment = env;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var dataInstanceName = Configuration.GetValue<string>("DataInstanceName") ?? throw new NullReferenceException("Data instance name is null");
            var mongoConfig = Configuration.GetConnectionString("Mongo") ?? throw new NullReferenceException("Mongo config is null");

            var telegramConfigSection = Configuration.GetSection("Telegram");
            var telegramConfig = telegramConfigSection.Get<TelegramConfiguration>() ?? throw new NullReferenceException("Telegram config is null");

            services.Configure<TelegramConfiguration>(telegramConfigSection);

            services.AddRazorPages(options =>
            {
                options.RootDirectory = "/View/Pages";
            });
            services.AddServerSideBlazor();

            services.AddServerSideBlazor().AddHubOptions((o) =>
            {
                o.MaximumReceiveMessageSize = 1024 * 1024 * 100;
            });

            AddBlazorise(services);
            
            services.AddSingleton<IMongoDataSource, MongoDataSource>(sc =>
            {
                return new MongoDataSource(mongoConfig, dataInstanceName);
            });

            services.AddSingleton<IClock>(SystemClock.Instance);

            // App
            services.AddHostedService<ConfigureTelegramWebhookHostedService>();
            services.AddSingleton<ICalendarEventRepository>();
            services.AddSingleton<IEventUserRepository>();

            services.AddBotCortex();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<ITelegramBotService, TelegramBotService>();

            services.AddHttpClient("tgwebhook")
                   .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(telegramConfig.BotToken, httpClient));

            services.AddControllers().AddNewtonsoftJson();

            services.AddDistributedMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var cultureInfo = new CultureInfo("en-NZ");

            if (cultureInfo is not null)
            {
                CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;
            }

            Log.Information("Culture set to {culture}", Thread.CurrentThread.CurrentCulture.DisplayName);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
                app.UseExceptionHandler("/Error");
            }

            app.UseSerilogRequestLogging();
            app.UseStatusCodePages("application/json", "{0}");

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // this is required to be here or otherwise the messages between server and client will be too large and
            // the connection will be lost.
            //app.UseSignalR( route => route.MapHub<ComponentHub>( ComponentHub.DefaultPath, o =>
            //{
            //    o.ApplicationMaxBufferSize = 1024 * 1024 * 100; // larger size
            //    o.TransportMaxBufferSize = 1024 * 1024 * 100; // larger size
            //} ) );

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        public void AddBlazorise(IServiceCollection services)
        {
            services
                .AddBlazorise();
            services
                .AddBootstrap5Providers()
                .AddFontAwesomeIcons();
        }
    }
}

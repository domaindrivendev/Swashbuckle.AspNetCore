using Autofac;

namespace CliExampleWithFactory;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen();
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule(new MyAutofacModule());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseSwaggerUI();
    }
}

public class MyAutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Register your services here
    }
}

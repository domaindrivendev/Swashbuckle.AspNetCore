using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace CustomUIConfig
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

            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                // Core
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");

                // Display
                c.DefaultModelExpandDepth(2);
                c.DefaultModelRendering(ModelRendering.Model);
                c.DefaultModelsExpandDepth(-1);
                c.DisplayOperationId();
                c.DisplayRequestDuration();
                c.DocExpansion(DocExpansion.None);
                c.EnableDeepLinking();
                c.EnableFilter();
                c.ShowExtensions();

                // Network
                c.EnableValidator();
                c.SupportedSubmitMethods(SubmitMethod.Get);

                // Other
                c.DocumentTitle = "CustomUIConfig";
                c.InjectStylesheet("/ext/custom-stylesheet.css");
                c.InjectJavascript("/ext/custom-javascript.js");
                c.UseRequestInterceptor("(req) => { req.headers['x-my-custom-header'] = 'MyCustomValue'; return req; }");
                c.UseResponseInterceptor("(res) => { console.log('Custom interceptor intercepted response from:', res.url); return res; }");
                c.EnablePersistAuthorization();

                c.ConfigObject.AdditionalItems.Add("syntaxHighlight", false);
                c.ConfigObject.AdditionalItems.Add("charProperty", 'c');
                c.ConfigObject.AdditionalItems.Add("stringProperty", "value");
                c.ConfigObject.AdditionalItems.Add("byteProperty", (byte)1);
                c.ConfigObject.AdditionalItems.Add("signedByteProperty", (sbyte)1);
                c.ConfigObject.AdditionalItems.Add("int16Property", (short)1);
                c.ConfigObject.AdditionalItems.Add("uint16Property", (ushort)1);
                c.ConfigObject.AdditionalItems.Add("int32Property", 1);
                c.ConfigObject.AdditionalItems.Add("uint32Property", 1u);
                c.ConfigObject.AdditionalItems.Add("int64Property", 1L);
                c.ConfigObject.AdditionalItems.Add("uint64Property", 1uL);
                c.ConfigObject.AdditionalItems.Add("floatProperty", 1f);
                c.ConfigObject.AdditionalItems.Add("doubleProperty", 1d);
                c.ConfigObject.AdditionalItems.Add("decimalProperty", 1m);
                c.ConfigObject.AdditionalItems.Add("dateTimeProperty", DateTime.UtcNow);
                c.ConfigObject.AdditionalItems.Add("dateTimeOffsetProperty", DateTimeOffset.UtcNow);
                c.ConfigObject.AdditionalItems.Add("timeSpanProperty", new TimeSpan(12, 34, 56));
                c.ConfigObject.AdditionalItems.Add("jsonArray", new JsonArray() { "string" });
                c.ConfigObject.AdditionalItems.Add("jsonObject", new JsonObject() { ["foo"] = "bar" });
                c.ConfigObject.AdditionalItems.Add("jsonDocument", JsonDocument.Parse("""{ "foo": "bar" }"""));

#if NET8_0_OR_GREATER
                c.ConfigObject.AdditionalItems.Add("dateOnlyProperty", new DateOnly(1977, 05, 25));
                c.ConfigObject.AdditionalItems.Add("timeOnlyProperty", new TimeOnly(12, 34, 56));
                c.ConfigObject.AdditionalItems.Add("halfProperty", Half.CreateChecked(1));
                c.ConfigObject.AdditionalItems.Add("int128Property", Int128.CreateChecked(1));
                c.ConfigObject.AdditionalItems.Add("unt128Property", UInt128.CreateChecked(1));
#endif
            });
        }
    }
}

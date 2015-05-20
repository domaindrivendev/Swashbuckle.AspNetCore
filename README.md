# Ahoy - Swashbuckle for AspNet VNext

This is a working repo for development of the next major version of [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle), targetted to [AspNet VNext](https://github.com/aspnet/Mvc)

## Installation on existing app
1. Add Swagger project to app solution
	*  Visual Studio - Right click Solution and Add Existing Project
	*  Command Line - Add path to projects in global.json
1. Add Swagger reference to app project
   *  Both - Add Swashbuckle to project.json in app project
1. Restore packages on Swagger  
   *  Visual Studio - In Solution Explorer right click on References and Restore Packages
   *  Command Line - `dnu restore` in directory
1. (Visual Studio Only) Restore packages on app to fix error in Error List
1. Restore dependencies (bower and npm) in Swagger
	* Visual Studio - Dependencies > Bower (and NPM) Right click and Restore Packages (Close and reopen solution when done)
	* Command Line - ?
1. Add service to ConfigureServices in Start.cs
       	
       	services.AddSwagger(s =>            {                s.SwaggerGenerator(c =>                {                    c.Schemes = new[] { "http", "https" };                    c.SingleApiVersion(new Info                    {                        Version = "v1",                        Title = "Swashbuckle Sample API",                        Description = "A sample API for testing Swashbuckle",                        TermsOfService = "Some terms ..."                    });                });                s.SchemaGenerator(opt => opt.DescribeAllEnumsAsStrings = true);            });     
1.  Add endpoints to Configure in Start.cs

		app.UseSwagger();		        app.UseSwaggerUi();      
1.  (This step involves adding an index file, but there appears to be problems getting this working)     
1.  Fire up the app and navigate to http://myapp.com/swagger/ui/index.html
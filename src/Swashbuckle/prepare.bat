rd lib /S /Q
mkdir lib\net451
mkdir lib\netcoreapp1.0

copy /Y ..\Swashbuckle.Swagger\bin\debug\net451\*.* lib\net451\*.*
copy /Y ..\Swashbuckle.Swagger\bin\debug\netcoreapp1.0 lib\netcoreapp1.0\*.*

copy /Y ..\Swashbuckle.SwaggerGen\bin\debug\net451\*.* lib\net451\*.*
copy /Y ..\Swashbuckle.SwaggerGen\bin\debug\netcoreapp1.0 lib\netcoreapp1.0\*.*

copy /Y ..\Swashbuckle.SwaggerUi\bin\debug\net451\*.* lib\net451\*.*
copy /Y ..\Swashbuckle.SwaggerUi\bin\debug\netcoreapp1.0 lib\netcoreapp1.0\*.*
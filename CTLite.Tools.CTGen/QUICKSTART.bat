CTGen -r "NorthwindDemo\NorthwindApplications" -a webapi -p -c -cc -cr -csvc -sc -sc -sr -srcdb -mcs "Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=SSPI;" -dbcs "Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=SSPI;"
cd NorthwindDemo
dotnet build
cd NorthwindApplication.WebApi
dotnet run
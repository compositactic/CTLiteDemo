CTGen -r "NorthwindDemo\NorthwindApplications" -a webapi -c -cc -cr -csvc -sc -sr -mcs "Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=SSPI;" -dbcs "Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=SSPI;"
cd NorthwindDemo
dotnet build
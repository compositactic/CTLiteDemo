# CTLite CTGen Quick Start

Prerequisites:
* .NET Core 3.1 must be installed (https://dotnet.microsoft.com/download/dotnet-core/3.1)
* LocalDB (https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver15) (For Microsoft SQL Server database functionality)

[Download the latest build of CTLite's **CTGen** command-line utility (~1.39 MB .ZIP)](https://github.com/compositactic/CTLiteDemo/raw/master/CTLite.Tools.CTGen/CTGen.zip)


The **Quick Start Batch Script** will:
* generate the Northwind Solution from the Northwind Domain Model (located in ```NorthwindDemo\NorthwindApplications```) with all Projects, including an ASP.NET Core API application
* generate database on MS SQL Server LocalDB
* run SQL scripts
* build the Northwind Solution
* run the ASP.NET Core API application


From a command-line window in this directory, run the Quick Start Batch Script (```quickstart.bat```) file 

The command window should indicate the URL endpoint for the ASP.NET Core API application. 

To call the API, issue an HTTP request like the following to:

	GET https://localhost:5001/NorthwindApplication

The ASP.NET Code API application will return a new ```NorthwindApplicaitonCompositeRoot``` in the ```returnValue``` of the first response 

```json
[
    {
        "success": true,
        "errors": null,
        "returnValue": {
            "id": 637369091534417819,
            "categories": {
                "categories": {}
            },
            "customers": {
                "customers": {}
            },
            "employees": {
                "employees": {}
            },
            "regions": {
                "regions": {}
            },
            "suppliers": {
                "suppliers": {}
            }
        },
        "returnValueContentType": null,
        "returnValueContentEncoding": null,
        "id": 1
    }
```

Note the ```CompositeRoot``` ```Id``` property value. 

This is the identifier of our new ```NorthwindApplicaitonCompositeRoot```. We may retrieve this new instance again by supplying the identifier after ```NorthwindApplication``` in the URL for subsequent requests:

```json
POST /NorthwindApplication/637369091534417819 HTTP/1.1
Host: localhost:5001
Content-Type: application/json

[
    { "CommandPath": "Customers/CreateNewCustomer", "Id" : 1 },
    { "CommandPath": "Customers/Customers/[{1/Id}]/Orders/CreateNewOrder", "Id" : 2 },
    { "CommandPath": "Customers/Customers/[{1/Id}]/Orders/Orders/[{2/Id}]/Items/CreateNewItem", "Id" : 3 }
]
```

This example demonstrates CTLite.AspNetCore's ability to send multiple API commands inside one HTTP request.

This request creates a new *Customer*, with a new *Order*, with a new *Item* in the new *Order*.

CTLite.AspNetCore returns a response for each command:

```json
[
    {
        "success": true,
        "errors": null,
        "returnValue": {
            "id": 637369101501755817,
            "orders": {
                "orders": {}
            }
        },
        "returnValueContentType": null,
        "returnValueContentEncoding": null,
        "id": 1
    },
    {
        "success": true,
        "errors": null,
        "returnValue": {
            "id": 637369101501930893,
            "items": {
                "items": {}
            },
            "employeeReferences": {
                "employeeReferences": {}
            }
        },
        "returnValueContentType": null,
        "returnValueContentEncoding": null,
        "id": 2
    },
    {
        "success": true,
        "errors": null,
        "returnValue": {
            "id": 637369101502041724,
            "productReferences": {
                "productReferences": {}
            }
        },
        "returnValueContentType": null,
        "returnValueContentEncoding": null,
        "id": 3
    }
]
```

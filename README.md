# CTLite

CTLite automatically generates .NET Core applications, SQL databases, and APIs from a **code-free domain model**. CTLite outputs a complete, ready-to-run code solution that implements a layered Presentation Model/Model-View-ViewModel architecture. Use (and reuse!) CTLite code with ASP.NET Core, Windows Presentation Foundation, Xamarin.Forms, Windows Forms, console applications, and more.

CTLite offers:

* Developer productivity - automates and standardizes the application's architecture
* "API-first" development - enables maximum code reuse
* Domain-based code organization - keeps the code focused on functional domain requirements.
* Low dependency, small code library - includes an integrated object-relational mapper, dependency injection system, and SQL DDL management support   

# Domain Model
CTLite domain models are specified using **ordinary, empty directories on your disk**. The directories' and sub-directories' names establish the class names and the one-to-many relationships of the classes in the domain model. The example domain model below is based on Microsoft's "Northwind" sample database, a fictitious e-commerce company:

![CTLite Domain Model of the Microsoft Northwind Sample](DomainModel.png)

CTLite requires domain model directory names to be English **plural nouns** - specifically nouns ending in "*s*", "*es*", or "*ies*". **This naming convention controls CTLite's novel code generation process**.

CTLite domain models begin with a **root directory** that represents the outermost boundary of your system-under-design (ex. an application, microservice, library, etc.). In our Northwind example, we choose the name "NorthwindApplications" to represent an application for the Northwind organization, following CTLite's rule for plural domain model directory names. 

**All relationships in CTLite domain models are one-to-many**. A directory represents a class in the domain model, and the directory's sub-directories define child classes. One-to-one relationships (a special case of a one-to-many relationship) and many-to-many relationships (implemented as one-to-many reference/link classes) are both modeled in similar fashion. In the Northwind example, a *Customer* has *Orders*, an *Order* has *Items*, and an *Item* references a *Product* (where an *Item* has a single *ProductReference*).  

> Successful domain modeling should be an iterative, collaborative effort between domain experts and all the stakeholders of your project. A well-modeled domain that holistically considers all system use cases, integrations, and reporting requirements makes for a system that evolves gracefully over its lifetime.  

# Code Generation
CTLite includes a command-line utility called **CTGen**. CTGen creates and refreshes C# code from the domain model. CTGen may also run generated SQL scripts. Code solutions generated by CTGen contain:

* Visual Studio solution file (.sln)
* Model Project
* Presentation Project
* Service Project
* Test Project
* SQL DDL scripts
* Application project
  * ASP.NET Core API
  * (others to come!)

## CTGen
Syntax for CTGen follows:
```
CTGen Usage

-r : root directory of domain model
-a : application type to generate (ex. webapi)
-p : generate solution file (.sln) and projects (.csproj)
-c : generate code (.cs)
-cd : generate code sample docs
-cc : generate composite (presentation) sample "Create" method
-cr : generate composite (presentation) sample "Remove" method
-csvc : generate service (interfaces and classes) samples
-sc : generate sql scripts
-sr : run sql scripts
-srcdb : run database create script (WARNING: DEFAULT SCRIPT DELETES ANY EXISTING DATABASE)
-mcs : master db connection string
-dbcs : application db connection string
```

Example:
Generate code solution for the domain model in ```C:\NorthwindDemo\NorthwindApplications``` which includes:
* An ASP.NET Core API Project: ```-a webapi```
* C#/SQL code (Model, Presentation, Service, Test): ```-c```
* Projects (Model, Presentation, Service, Test): ```-p```
* Commented-out in-line C# code samples: ```-cd```
* Sample "Create" factory method on Presentation composite container classes: ```-cc```
* Sample "Remove" factory method on Presentation composite classes: ```- cr```
* Sample service interfaces and class: ```-csvc```
* SQL DDL scripts for creating tables for Model classes including foreign key relationships: ```-sc```
* Running all SQL scripts (files with .sql in domain model directory and subdirectories): ```-sr```
* Running the CREATE DATABASE SQL script from the Master DB: ```-srcdb```  

```
CTGen -r "C:\NorthwindDemo\NorthwindApplications" -a webapi -c -p -cd -cc -cr -csvc -sc -sr -srcdb -mcs "Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=SSPI;" -dbcs "Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=SSPI;"
```
## Visual Studio Solution
After code generation is complete, the CTGen generated solution file can be opened in Visual Studio.

![The Northwind Solution generated by CTGen](NorthwindSolution.png)

## Model Project
The **Model Project** represents the base data structures of the domain model. CTGen creates C# (POCO) classes based on the directory names of the domain model, converting the plural directory name to a singular name for the class. CTGen preserves the domain model's directory structure in the generated code. 

Class files with a ```.g.cs``` extension are generated/refreshed each time CTGen is run (with the ```-c``` option), and ```.cs``` files are generated once and are editable by the developer. The ```.g.cs``` files contain boilerplate code that supports CTLite's internal functionality. The ```.cs``` files should contain any properties and methods as required and implemented by the developer.

The generated model classes are C# partial classes that implement the **Composite** design pattern. The generated portion of the model class contains:
* A property named ```Id``` which serves as the class object's unique identifier
* A property named ```[ParentClassName]Id``` where ```[ParentClassName]``` is the name of the parent class (if a parent class exists), which contains the ```Id``` property value of the parent class
* A property named ```[ParentClassName]``` referencing the parent class object (if a parent class exists) 
* Dictionaries for each child model class, with the key being the ```Id``` of those classes
* Factory methods for each child model class, named ```CreateNew[ModelClassName]```, where ```[ModelClassName]``` is the name of the model class
* A ```Remove``` method, which removes the model class object from its containing dictionary

CTGen will generate **SQL DDL Scripts** when the ```-sc``` option is specified. The filename generated is ```[NNN]-Table-[ModelClassName]```, where ```[NNN]``` is a sequential number, and ```[ModelClassName]``` is the name of the model class. 


![The Northwind Model Project, showing an expansion of ](ModelProjectExpanded.png)
## Presentation Project
TODO

## Service Project
TODO

## Test Project
TODO 

## SQL DDL Scripts
TODO

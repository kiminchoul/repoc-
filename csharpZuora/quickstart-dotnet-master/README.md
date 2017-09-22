### Zuora Sample C# REST Client

### Overview:
This client was generated using http://editor.swagger.io/#/ to create C# code for the using the Zuora REST API. All classes were generated via swagger with the exception of the ApplicationManager which was 
written to show and run the example API calls within a C# Console Application.

### Purpose:
This a simple REST client meant to provide examples on the use of Zuora's new REST API offerings in C#. The calls used in this example are C# implementations of the API calls detailed in the exercises on the 
Zuora Developer Quick Start Page (http://zdevelop-zuora.pantheonsite.io/developer/quick-start/).
They are as follows:
- Exercise 1: Query for a Product
- Exercise 2: Create a Customer Account and Subscription
- Exercise 3: Upgrade a Subscription
- Exercise 4: Cancel a Subscription

### Zuora Test Drive

To complete this tutorial, you'll need a Zuora Test Drive tenant.

The Test Drive tenant comes with seed data, such as a sample product catalog, which will be used in the Exercises.
Go to [Zuora Test Drive](https://www.zuora.com/resource/zuora-test-drive/) and sign up for a tenant.

### Set-Up:
The generated code was imported into VisualStudio 2015 for the creation of this sample project. The following steps must be taken to ensure compilation within VisualStudio 2015: 
**This project has not been tested in any other C# IDEs and may require different steps if a different IDE is used**
	- Change target framework to .NET 4.6.1 (Swagger generates to the .NET 2.0 framework but we use version 4.6.1 because it defaults to TLS 1.2)
	- Add Newtonsoft package (Right-click project in the Solution Explorer -> Manage NuGet Packages... -> Search for Newtonsoft in the browse tab and install)
	- Add RestSharp package (Same steps as above except search for RestSharp)
	- Add reference for the System.Runtime.Serialization assembly (Right-click "References" in the Solution Explorer -> Add References... -> Find System.Runtime.Serialization under Framework -> Check the box and hit 'OK')
	- Change reference on line 119 of the ApiClient.cs class from "RestSharp.Contrib.HttpUtility.UrlEncode(str);" to "RestSharp.Extensions.MonoHttp.HttpUtility.UrlEncode(str);"
	- Add the following code black to the beginning of your main method: (This prevents JsonConvert from serializing null values in the API objects which would otherwise cause the API calls to fail)
	        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
	- On line 233 of the ApplicationManager class replace the dummy username and password with the username and password for your Zuora Test Drive tenant


=======
# Zuora Sample C# REST Client

### Overview:
This client was generated using http://editor.swagger.io/#/ to create C# code for the using the Zuora REST API. All classes were generated via swagger with the exception of the ApplicationManager which was 
written to show and run the example API calls within a C# Console Application.

### Purpose:
This a simple REST client meant to provide examples on the use of Zuora's new REST API offerings in C#. The calls used in this example are C# implementations of the API calls detailed in the exercises on the 
Zuora Developer Quick Start Page (http://zdevelop-zuora.pantheonsite.io/developer/quick-start/).
They are as follows:
- Exercise 1: Query for a Product
- Exercise 2: Create a Customer Account and Subscription
- Exercise 3: Upgrade a Subscription
- Exercise 4: Cancel a Subscription

### Zuora Test Drive:

To complete this tutorial, you'll need a Zuora Test Drive tenant.

The Test Drive tenant comes with seed data, such as a sample product catalog, which will be used in the Exercises.
Go to [Zuora Test Drive](https://www.zuora.com/resource/zuora-test-drive/) and sign up for a tenant.

### Set-Up:  
The generated code was imported into VisualStudio 2015 for the creation of this sample project. The following steps were taken to ensure compilation of the code generated from swagger within VisualStudio 2015:

1. Change target framework to .NET 4.6.1 
  - Swagger generates to the .NET 2.0 framework but we use version 4.6.1 because it defaults to TLS 1.2
2. Add Newtonsoft package
  - Right-click project in the Solution Explorer -> Manage NuGet Packages... -> Search for Newtonsoft in the browse tab and install
3. Add RestSharp package
  - Same steps as above except search for RestSharp
4. Add reference for the System.Runtime.Serialization assembly
  - Right-click "References" in the Solution Explorer -> Add References... -> Find System.Runtime.Serialization under Framework -> Check the box and hit 'OK'
5. Change reference on line 119 of the ApiClient.cs class from `RestSharp.Contrib.HttpUtility.UrlEncode(str);` to `RestSharp.Extensions.MonoHttp.HttpUtility.UrlEncode(str);`
6. Add the following code black to the beginning of your main method:

   ```C#
   JsonConvert.DefaultSettings = () => new JsonSerializerSettings
   {
      NullValueHandling = NullValueHandling.Ignore
   };
   ```
   - This prevents JsonConvert from serializing null values in the API objects which would otherwise cause the API calls to fail
7. On line 233 of the ApplicationManager class replace the dummy username and password with the username and password for your Zuora Test Drive tenant. 
 
**This project has not been tested in any other C# IDEs and may require different steps if a different IDE is used**
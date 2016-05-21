# Azure Resource Usage Dashboard 
[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

The AzureBilling solution brings up a website that provides graphical view of expenses incurred by azure resources.

## Features

* Track your Azure expenses with updated dashboard.
* A frequenctly running WebJob refreshes data continously.
* Solution can be extended to include more reports and graphs.
* Website can be secured using Azure App Service Authentication.

## Pre-requisites
* The billing and usage data is fetched using Azure Active Directory (AAD) authentication, you need to either register an application with the default directory
or use details (mentioned below) of existing application which has access to read Active Directory.
You would need the follwing details from the AAD Application.
	* Client Id, 
	* Key
	* A valid User Id

[How to register an App with Azure Active Directory?] (https://azure.microsoft.com/en-in/documentation/articles/active-directory-integrating-applications/#adding-an-application)

You will also need a Tenant Id of your Azure Directory.
[How to get Azure Tenant Id?] (http://stackoverflow.com/questions/26384034/how-to-get-the-azure-account-tenant-id)

## Usage
* Click the 'Deploy to Azure',  provide few details and the website in up and running in minutes.
* Creates Website, an App service plan, a Storage Account and a scheduled WebJob automatically.

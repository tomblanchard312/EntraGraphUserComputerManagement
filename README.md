# EntraGraphUserComputerManagement

# Azure AD Graph API Helper

This project provides a set of methods to interact with the Microsoft Graph API for Azure AD. It is designed as a starting point for developers who want to integrate Azure AD functionality into their applications.

## Disclaimer

**This project is a template and should not be used as is. It is intended as a stub of methods that need to be called outside of the timer trigger or might depend on other external configurations.**

## Getting Started

To use this project, follow these steps:

1. Clone or download the repository.
2. Open the project in your preferred development environment (Visual Studio, Visual Studio Code, etc.).
3. Modify the methods in the `MyGraphService` class to suit your specific requirements.
4. Configure the Azure Blob Storage connection string and other settings in the project.

## Important Notes

- The provided methods are placeholders and require customization to fit your application's logic and requirements.
- Ensure to replace the placeholder configurations and connection strings with your actual values.
- Do not use this project without understanding and adapting it to your specific needs.

## Dependencies

This project utilizes the Microsoft Graph SDK and requires the Azure.Storage.Blobs NuGet package for Azure Blob Storage interaction.

Install dependencies using the following commands:

```bash
dotnet add package Microsoft.Graph -Version 4.0.0
dotnet add package Azure.Storage.Blobs -Version 12.10.0

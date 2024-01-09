# EntraGraphUserComputerManagement

This project provides a set of methods to interact with the Microsoft Graph API for Azure AD and Microsoft Power Platform Dataverse. It is designed as a starting point for developers who want to integrate Azure AD and Dataverse functionality into their applications.

## Disclaimer

**This project is a template and should not be used as is. It is intended as a stub of methods that need to be called outside of the timer trigger or might depend on other external configurations.**

## Getting Started

Before using the functions, make sure to configure the necessary settings in the `local.settings.json` file. This includes Microsoft Graph API credentials and other required configurations.

### Prerequisites

- .NET Core SDK
- Azure Functions Core Tools
- Azure Storage Account (for logging)

### Configuration

Update the `local.settings.json` file with your Microsoft Graph API credentials and Azure Storage Account connection string.

## Important Notes

- The provided methods are placeholders and require customization to fit your application's logic and requirements.
- Ensure to replace the placeholder configurations and connection strings with your actual values.
- Do not use this project without understanding and adapting it to your specific needs.

## Dependencies
Use the Package Manager Console or the command line to install the required dependencies.

dotnet restore
## Functions
### 1. CreateUser
Creates a new user in Azure AD.

### 2. ListUsers
Lists all users in Azure AD.

### 3. FindUserInGroup
Finds a user in a specified group.

### 4. ListGroups
Lists all groups in Azure AD.

### 5. ListGroupsFiltered
Lists groups with a specific filter applied.

### 6. CreateComputer
Creates a new computer object in Azure AD.

### 7. AddComputerToGroup
Adds a computer to a specified group.

### 8. FindComputerInOtherGroups
Finds a computer in groups other than the specified group.

### 9. DeleteComputerFromAllGroups
Deletes a computer from all groups it belongs to.

### 10. ProcessDataverseUsers
Processes user names retrieved from a Dataverse table.

### 11. ProcessDataverseComputers
Processes computer names retrieved from a Dataverse table.

### 12. GetSharePointObjectsForToday
Retrieves user and computer names from a SharePoint list created on the current date.

### 13. UploadLogToBlobStorageAsync
Uploads a log file to Azure Blob Storage asynchronously.

### 14. ProcessSharePointUsers
Processes user names retrieved from a SharePoint list.

### 15. ProcessSharePointComputers
Processes computer names retrieved from a SharePoint list.

## Usage
These functions can be triggered manually or scheduled using Azure Functions Timer Trigger. Make sure to handle the necessary dependencies and configurations for each function.

## Notes
This project is a stub and requires modifications based on your specific requirements.
Ensure that dependencies are up-to-date and compatible with the Microsoft Graph SDK version used.
##License
This project is licensed under the MIT License - see the LICENSE.md file for details.

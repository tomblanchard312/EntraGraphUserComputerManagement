using System;
using System.Threading.Tasks;
using Microsoft.Graph;
using System.Net.Http.Headers;
using Microsoft.Identity.Client;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.SharePoint.Client;
namespace EntraGraphUserComputerManagement
{
    /// <summary>
    /// Provides a set of static methods for interacting with Microsoft Graph API.
    /// </summary>
    public static class MyGraphService
    {
        private static GraphServiceClient _graphServiceClient;
        //TODO: Get values from appsettings/config or From AKV
        private static readonly string BlobStorageConnectionString = "YourBlobStorageConnectionString";
        private static readonly string LogContainerName = "logs";
        private static readonly string DataverseConnectionString = "YourDataverseConnectionString";
        private static readonly string _sharePointSiteUrl = "YourSharePointSiteUrl";
        private static readonly string _sharePointListName = "YourSharePointListName";

        /// <summary>
        /// Initializes the Microsoft Graph Service Client using the provided authentication details.
        /// </summary>
        /// <param name="clientId">The client ID of the Azure AD app.</param>
        /// <param name="clientSecret">The client secret of the Azure AD app.</param>
        /// <param name="tenantId">The ID of the Azure AD tenant.</param>
        public static async Task InitializeGraphServiceClientAsync(string clientId, string clientSecret, string tenantId)
        {
            var confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
                .Build();
            await LogActionAsync("Graph Service Client initialized", "Initialization");


            var authResult = confidentialClientApplication
                .AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" })
                .ExecuteAsync()
                .Result;

            _graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(requestMessage =>
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                return Task.CompletedTask;
            }));
        }
        /// <summary>
        /// Creates a new user in Azure AD.
        /// </summary>
        /// <summary>
        /// Creates a new user in Azure AD and returns the user ID.
        /// </summary>
        /// <returns>The ID of the newly created user.</returns>
        public static async Task<string> CreateUser()
        {
            try
            {
                var newUser = new Microsoft.Graph.User
                {
                    //todo: change these values to match your environment
                    AccountEnabled = true,
                    DisplayName = "John Doe",
                    UserPrincipalName = "john.doe@example.com",
                    PasswordProfile = new PasswordProfile
                    {
                        ForceChangePasswordNextSignIn = true,
                        Password = "StrongPassword123"
                    }
                };

                var createdUser = await _graphServiceClient.Users
                    .Request()
                    .AddAsync(newUser);
                await LogActionAsync($"User created successfully. User Id: {createdUser.Id}", "CreateUser");
                Console.WriteLine($"User created successfully. User Id: {createdUser.Id}");

                // Return the ID of the newly created user
                return createdUser.Id;
            }
            catch (Exception ex)
            {
                await LogActionAsync($"Error creating user: {ex.Message}", "CreateUser");
                Console.WriteLine($"Error creating user: {ex.Message}");
                return null; // Or throw an exception
            }
        }

        public static async Task ListUsers()
        {
            try
            {
                var users = await _graphServiceClient.Users
                    .Request()
                    .GetAsync();

                foreach (var user in users)
                {
                    Console.WriteLine($"User Id: {user.Id}, Display Name: {user.DisplayName}, User Principal Name: {user.UserPrincipalName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing users: {ex.Message}");
            }
        }
        public static async Task FindUserInGroup(string groupId, string userPrincipalName)
        {
            try
            {
                var members = await _graphServiceClient.Groups[groupId].Members
                    .Request()
                    .GetAsync();

                var user = members.OfType<Microsoft.Graph.User>().FirstOrDefault(u => u.UserPrincipalName.Equals(userPrincipalName, StringComparison.OrdinalIgnoreCase));

                if (user != null)
                {
                    Console.WriteLine($"User found in group - User Id: {user.Id}, Display Name: {user.DisplayName}, User Principal Name: {user.UserPrincipalName}");
                }
                else
                {
                    Console.WriteLine($"User with User Principal Name '{userPrincipalName}' not found in group '{groupId}'.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding user in group: {ex.Message}");
            }
        }
        public static async Task ListUsersInGroup(string groupId)
        {
            try
            {
                var groupMemberships = await _graphServiceClient.Me.MemberOf
                    .Request()
                    .GetAsync();

                var groupMemberIds = groupMemberships.OfType<Microsoft.Graph.Group>().Select(g => g.Id).ToList();

                var usersInGroup = await _graphServiceClient.Users
                    .Request()
                    .Filter($"memberOf eq '{groupId}'")
                    .GetAsync();

                foreach (var user in usersInGroup)
                {
                    Console.WriteLine($"User Id: {user.Id}, Display Name: {user.DisplayName}, User Principal Name: {user.UserPrincipalName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing users in group: {ex.Message}");
            }
        }
        /// <summary>
        /// Adds a user to a specified group in Azure AD.
        /// </summary>
        /// <param name="userId">The ID of the user to be added to the group.</param>
        /// <param name="groupId">The ID of the group to which the user will be added.</param>
        public static async Task AddUserToGroup(string userId, string groupId)
        {
            try
            {
                var directoryObject = new DirectoryObject
                {
                    Id = userId
                };

                await _graphServiceClient.Groups[groupId].Members.References
                    .Request()
                    .AddAsync(directoryObject);

                Console.WriteLine($"User with Id '{userId}' added to group with Id '{groupId}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding user to group: {ex.Message}");
            }
        }
        public static async Task ListGroups()
        {
            try
            {
                var groups = await _graphServiceClient.Groups
                    .Request()
                    .GetAsync();

                foreach (var group in groups)
                {
                    Console.WriteLine($"Group Id: {group.Id}, Display Name: {group.DisplayName}, Description: {group.Description}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing groups: {ex.Message}");
            }
        }
        public static async Task ListGroupsFiltered(string filter)
        {
            try
            {
                var filteredGroups = await _graphServiceClient.Groups
                    .Request()
                    .Filter(filter)
                    .GetAsync();

                foreach (var group in filteredGroups)
                {
                    Console.WriteLine($"Group Id: {group.Id}, Display Name: {group.DisplayName}, Description: {group.Description}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing filtered groups: {ex.Message}");
            }
        }
        public static async Task<string> CreateComputer(string displayName)
        {
            try
            {
                var newComputer = new Device
                {
                    DisplayName = displayName
                };

                var createdComputer = await _graphServiceClient.Devices
                    .Request()
                    .AddAsync(newComputer);

                Console.WriteLine($"Computer created successfully. Computer Id: {createdComputer.Id}");

                return createdComputer.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating computer: {ex.Message}");
                return null;
            }
        }
        public static async Task AddComputerToGroup(string computerId, string groupId)
        {
            try
            {
                var directoryObject = new DirectoryObject
                {
                    Id = computerId
                };

                await _graphServiceClient.Groups[groupId].Members.References
                    .Request()
                    .AddAsync(directoryObject);

                Console.WriteLine($"Computer with Id '{computerId}' added to group with Id '{groupId}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding computer to group: {ex.Message}");
            }
        }

        public static async Task FindComputerInOtherGroups(string computerId, string excludeGroupId)
        {
            try
            {
                // Retrieve all groups
                var allGroups = await _graphServiceClient.Groups
                    .Request()
                    .GetAsync();

                // Find the group to exclude
                var excludeGroup = allGroups.FirstOrDefault(group => group.Id == excludeGroupId);

                if (excludeGroup == null)
                {
                    Console.WriteLine($"Group with Id '{excludeGroupId}' not found.");
                    return;
                }

                // Retrieve all members of the exclude group
                var excludeGroupMembers = await _graphServiceClient.Groups[excludeGroup.Id].Members
                    .Request()
                    .GetAsync();

                // Check if the computer is a member of other groups
                var otherGroups = allGroups.Where(group => group.Id != excludeGroupId && !excludeGroupMembers.Any(member => member.Id == computerId));

                foreach (var group in otherGroups)
                {
                    Console.WriteLine($"Computer with Id '{computerId}' is a member of group with Id '{group.Id}'.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding computer in other groups: {ex.Message}");
            }
        }
        /// <summary>
        /// Deletes a computer from all groups it is a member of.
        /// </summary>
        /// <param name="computerId">The ID of the computer to be deleted.</param>
        public static async Task DeleteComputerFromAllGroups(string computerId)
        {
            try
            {
                // Retrieve all groups
                var allGroups = await _graphServiceClient.Groups
                    .Request()
                    .GetAsync();

                // Retrieve all members of the computer
                var computerMemberships = await _graphServiceClient.Devices[computerId].MemberOf
                    .Request()
                    .GetAsync();

                // Iterate over each group the computer is a member of and remove it
                foreach (var group in allGroups.Where(group => computerMemberships.Any(member => member.Id == group.Id)))
                {
                    await _graphServiceClient.Groups[group.Id].Members[computerId]
                        .Reference
                        .Request()
                        .DeleteAsync();

                    Console.WriteLine($"Computer with Id '{computerId}' removed from group with Id '{group.Id}'.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting computer from groups: {ex.Message}");
            }
        }
        private static async Task LogActionAsync(string logMessage, string serviceName)
        {
            try
            {
                // Format log entry as CSV
                var logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss},{serviceName},{logMessage}";

                // Log to Azure Blob Storage
                await LogToAzureBlobStorageAsync(logEntry);
            }
            catch (Exception ex)
            {
                // Handle logging error (you might want to log it to a different location)
                Console.WriteLine($"Error logging action: {ex.Message}");
            }
        }
        private static async Task LogToAzureBlobStorageAsync(string logEntry)
        {
            //  Log to Azure Blob Storage
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(logEntry);
                    writer.Flush();
                    stream.Position = 0;
                    //  Upload to Azure Blob Storage
                    await BlobStorageClient.UploadToBlobStorage(stream, LogContainerName, blobName: $"log_{Guid.NewGuid()}.csv");
                }
            }
        }
        /// <summary>
        /// Gets the names of objects from Dataverse tables based on the create date of today.
        /// </summary>
        /// <returns>A dictionary containing the names of user and computer objects.</returns>
        public static async Task<Dictionary<string, List<string>>> GetDataverseObjectsForTodayAsync()
        {
            try
            {
                var connectionString = DataverseConnectionString;

                using (var service = new CrmServiceClient(connectionString))
                {
                    if (!service.IsReady)
                    {
                        // Handle connection error
                        Console.WriteLine($"Error connecting to Dataverse: {service.LastCrmError}");
                        return null;
                    }

                    // Query useraccount table for today's created users
                    var createdUsers = service.RetrieveMultiple(new QueryExpression("useraccounts")
                    {
                        ColumnSet = new ColumnSet("name"),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                        {
                            new ConditionExpression("createdon", ConditionOperator.Today)
                        }
                        }
                    });

                    // Query computeraccounts table for today's created computers
                    var createdComputers = service.RetrieveMultiple(new QueryExpression("computeraccounts")
                    {
                        ColumnSet = new ColumnSet("name"),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                        {
                            new ConditionExpression("createdon", ConditionOperator.Today)
                        }
                        }
                    });

                    var result = new Dictionary<string, List<string>>
                {
                    { "Users", createdUsers.Entities.Select(entity => entity.GetAttributeValue<string>("name")).ToList() },
                    { "Computers", createdComputers.Entities.Select(entity => entity.GetAttributeValue<string>("name")).ToList() }
                };

                    //  Log Dataverse objects retrieval
                    await LogActionAsync($"Retrieved Dataverse objects for today - Users: {string.Join(", ", result["Users"])}, Computers: {string.Join(", ", result["Computers"])}", "GetDataverseObjectsForToday");

                    return result;
                }
            }
            catch (Exception ex)
            {
                //  Log error during Dataverse object retrieval
                await LogActionAsync($"Error retrieving Dataverse objects: {ex.Message}", "GetDataverseObjectsForToday");
                return null;
            }
        }
        #region SharePoint
        public static async Task<Dictionary<string, List<string>>> GetSharePointObjectsForToday()
        {
            try
            {
                using (var context = new ClientContext(_sharePointSiteUrl))
                {
                    var list = context.Web.Lists.GetByTitle(_sharePointListName);

                    var query = new CamlQuery
                    {
                        ViewXml = $"<View><Query><Where><Geq><FieldRef Name='Created' /><Value Type='DateTime'><Today /></Value></Geq></Where></Query></View>"
                    };

                    var items = list.GetItems(query);
                    context.Load(items);
                    await context.ExecuteQueryAsync();

                    var result = new Dictionary<string, List<string>>
                {
                    { "Users", items.OfType<Microsoft.SharePoint.Client.ListItem>().Select(item => item["UserName"].ToString()).ToList() },

                    { "Computers", items.OfType<Microsoft.SharePoint.Client.ListItem>().Select(item => item["ComputerName"].ToString()).ToList() }
                };

                    await LogActionAsync($"Retrieved SharePoint objects for today - Users: {string.Join(", ", result["Users"])}, Computers: {string.Join(", ", result["Computers"])}", "GetSharePointObjectsForToday");

                    return result;
                }
            }
            catch (Exception ex)
            {
                await LogActionAsync($"Error retrieving SharePoint objects: {ex.Message}", "GetSharePointObjectsForToday");
                return null;
            }
        }
        public static async Task ProcessSharePointUsers(List<string> userNames)
        {
            try
            {
                await LogActionAsync("Processing SharePoint users...", "ProcessSharePointUsers");

                foreach (var userName in userNames)
                {
                    await LogActionAsync($"Processing user: {userName}", "ProcessSharePointUsers");
                    // Additional processing logic
                }

                await LogActionAsync("SharePoint users processed successfully.", "ProcessSharePointUsers");
            }
            catch (Exception ex)
            {
                await LogActionAsync($"Error processing SharePoint users: {ex.Message}", "ProcessSharePointUsers");
            }
        }
        /// <summary>
        /// Process SharePoint computer names.
        /// </summary>
        /// <param name="computerNames">List of computer names.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ProcessSharePointComputers(List<string> computerNames)
        {
            try
            {
                // Example: Log the start of the computer processing
                await LogActionAsync("Processing SharePoint computers...", "ProcessSharePointComputers");

                // Your processing logic for SharePoint computers
                foreach (var computerName in computerNames)
                {
                    // Example: Log each computer name
                   await LogActionAsync($"Processing computer: {computerName}", "ProcessSharePointComputers");

                    // Perform additional processing based on your requirements
                    // For example, call other Graph API methods, update records, etc.
                }

                // Example: Log the completion of the computer processing
                await LogActionAsync("SharePoint computers processed successfully.", "ProcessSharePointComputers");
            }
            catch (Exception ex)
            {
                // Example: Log error during computer processing
                await LogActionAsync($"Error processing SharePoint computers: {ex.Message}", "ProcessSharePointComputers");
            }
        }

        #endregion SharePoint
        #region DataVerse
        /// <summary>
        /// Process Dataverse user names.
        /// </summary>
        /// <param name="userNames">List of user names.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ProcessDataverseUsers(List<string> userNames)
        {
            try
            {
                // Log the start of the user processing
                await LogActionAsync("Processing Dataverse users...", "ProcessDataverseUsers");

                // processing logic for Dataverse users
                foreach (var userName in userNames)
                {
                    // Log each user name
                    await LogActionAsync($"Processing user: {userName}", "ProcessDataverseUsers");

                    // Perform additional processing based on your requirements
                    // For example, call other Graph API methods, update records, etc.
                }

                // Log the completion of the user processing
                await LogActionAsync("Dataverse users processed successfully.", "ProcessDataverseUsers");
            }
            catch (Exception ex)
            {
                // Log error during user processing
                await LogActionAsync($"Error processing Dataverse users: {ex.Message}", "ProcessDataverseUsers");
            }
        }

        /// <summary>
        /// Process Dataverse computer names.
        /// </summary>
        /// <param name="computerNames">List of computer names.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ProcessDataverseComputers(List<string> computerNames)
        {
            try
            {
                //  Log the start of the computer processing
                await LogActionAsync("Processing Dataverse computers...", "ProcessDataverseComputers");

                // Your processing logic for Dataverse computers
                foreach (var computerName in computerNames)
                {
                    //  Log each computer name
                    await LogActionAsync($"Processing computer: {computerName}", "ProcessDataverseComputers");

                    // Perform additional processing based on your requirements
                    // For example, call other Graph API methods, update records, etc.
                }

                //  Log the completion of the computer processing
                await LogActionAsync("Dataverse computers processed successfully.", "ProcessDataverseComputers");
            }
            catch (Exception ex)
            {
                //  Log error during computer processing
                await LogActionAsync($"Error processing Dataverse computers: {ex.Message}", "ProcessDataverseComputers");
            }
        }
       #endregion DataVerse
    }
}
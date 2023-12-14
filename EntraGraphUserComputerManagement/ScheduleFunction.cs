using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
namespace EntraGraphUserComputerManagement
{
    public static class ScheduledFunction
    {
        static ScheduledFunction()
        {
          
        }
        [FunctionName("ScheduledFunction")]
        public static async Task Run(
            [TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log) // Runs every 5 minutes
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // Call your methods here
            string newUserId = await MyGraphService.CreateUser();
            Console.WriteLine($"Newly created user ID: {newUserId}");
            await MyGraphService.ListUsers();
            await MyGraphService.ListUsersInGroup("groupId");
            await MyGraphService.FindUserInGroup("groupId", "user@example.com");
            await MyGraphService.AddUserToGroup("userid","groupId");
            await MyGraphService.ListGroups();
            await MyGraphService.ListGroupsFiltered("filterExpression");
            // Create a computer and get its ID
            string computerId = await MyGraphService.CreateComputer("MyComputer");

            // Add computer to a specific group
            await MyGraphService.AddComputerToGroup("computerId", "groupId");

            // Find computer in other groups (replace "excludeGroupId" with the actual group Id)
            await MyGraphService.FindComputerInOtherGroups("computerId", "excludeGroupId");

            // Delete computer from all groups
            await MyGraphService.DeleteComputerFromAllGroups("computerId");
        }       
    }

}

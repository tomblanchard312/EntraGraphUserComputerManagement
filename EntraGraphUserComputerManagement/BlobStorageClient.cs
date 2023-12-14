using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;

namespace EntraGraphUserComputerManagement
{
    public static class BlobStorageClient
    {
        private static readonly string ConnectionString = "YourBlobStorageConnectionString";

        public static async Task UploadToBlobStorage(Stream stream, string containerName, string blobName)
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(ConnectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

                await blobContainerClient.CreateIfNotExistsAsync();

                var blobClient = blobContainerClient.GetBlobClient(blobName);

                using (var streamReader = new StreamReader(stream))
                {
                    await blobClient.UploadAsync(streamReader.BaseStream, true);
                }
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Error uploading to Azure Blob Storage: {ex.Message}");
                // Handle the exception as per your application's error-handling strategy
            }
        }
    }
}

// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using Azure;
using Azure.Storage.Blobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace FunctionApp2
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
            var createdEvent = ((JObject)eventGridEvent.Data).ToObject<StorageBlobCreatedEventData>();
            var blobName = GetBlobNameFromUrl(createdEvent.Url);
            log.LogInformation(blobName.Name);
            AddBlobMetadataAsync(blobName,log);
            log.LogInformation("Metadata Lista");
        }
        private static BlobClient GetBlobNameFromUrl(string bloblUrl)
        {
            var uri = new Uri(bloblUrl);
            var key = new AzureSasCredential("sp=racwdl&st=2021-10-01T00:44:45Z&se=2021-10-15T08:44:45Z&spr=https&sv=2020-08-04&sr=c&sig=Rds7VvXjshFEywDwHuMaPTqHDTfhynkdsC6LxCFt3fE%3D");
            var blobClient = new BlobClient(uri,key);
            return blobClient;
        }
        private static void AddBlobMetadataAsync(BlobClient blob,ILogger log)
        {
            log.LogInformation("Adding blob metadata...");

            try
            {
                IDictionary<string, string> metadata =
                   new Dictionary<string, string>();

                // Add metadata to the dictionary by calling the Add method
                metadata.Add("docType", "textDocuments");

                // Add metadata to the dictionary by using key/value syntax
                metadata["category"] = "guidance";

                // Set the blob's metadata.
                blob.SetMetadata(metadata);
            }
            catch (RequestFailedException e)
            {
                log.LogInformation($"HTTP error code {e.Status}: {e.ErrorCode}");
                log.LogInformation(e.Message);
            }
        }
    }
}

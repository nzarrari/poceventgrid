// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace poceventgrid
{
    public static class upload2S3
    {
        
        private const string bucketName = "nzbucket123";
        // Specify your bucket region (an example region is shown).
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast1;
        private static IAmazonS3 s3Client;
        

        [FunctionName("upload2S3")]
        public static void Run([EventGridTrigger]EventGridEvent eventGridEvent, 
        [Blob("{data.url}", FileAccess.Read, Connection = "StorageConnectionAppSetting")] Stream myBlob, 
         ILogger log)
        {
            var filename = eventGridEvent.Subject.Split('/').Last();
            log.LogInformation($"file name: {filename}");

            if(myBlob != null){
                log.LogInformation($"BlobInput processed Size: {myBlob.Length} bytes");
            }else{
                log.LogInformation("blob is null!!!");
            }

            log.LogInformation("Upload started");
            uploadtoS3(myBlob, log, filename);
            log.LogInformation("upload ended!!");
        }

        private static void uploadtoS3(Stream fileToUpload, ILogger log, string filename){         
            var awsCredentials = new Amazon.Runtime.BasicAWSCredentials("YOURKEY", "YOURSECRET");   
            s3Client = new AmazonS3Client(awsCredentials, bucketRegion);
            try
            {
                var fileTransferUtility = new TransferUtility(s3Client);
                fileTransferUtility.UploadAsync(fileToUpload, bucketName, filename).Wait();
                log.LogInformation("Upload 3 completed");            
            }
            catch (AmazonS3Exception e)
            {
                log.LogInformation("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                log.LogInformation("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }
    }
}

using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace poceventgrid
{
    public static class uploadToS3_blobTrigger
    {
        private const string bucketName = "nzbucket123";
        // Specify your bucket region (an example region is shown).
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast1;
        private static IAmazonS3 s3Client;
        private static string s3Key = System.Environment.GetEnvironmentVariable("s3Key");
        private static string s3Secret = System.Environment.GetEnvironmentVariable("s3Secret");

        [FunctionName("uploadToS3_blobTrigger")]
        public static void Run([BlobTrigger("addresses/{name}", Connection = "StorageConnectionAppSetting2")]Stream myBlob, 
            string name, 
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            log.LogInformation("Upload started");
            uploadtoS3(myBlob, log, name);
            log.LogInformation("upload ended!!");

        }
        private static void uploadtoS3(Stream fileToUpload, ILogger log, string filename){       

            
            var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(s3Key, s3Secret);   

            s3Client = new AmazonS3Client(awsCredentials, bucketRegion);
            try
            {
                var fileTransferUtility = new TransferUtility(s3Client);
                fileTransferUtility.UploadAsync(fileToUpload, bucketName, filename);
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

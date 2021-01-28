using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace Hypergiant.HIVE.Support
{

    public class S3Client
    {

        private ILogService LogService;
        private AmazonS3Client _s3Client;
        private bool _customS3 = false;
        private string _bucketName, _objectRoot;

        public S3Client(ILogService logService)
        {
            LogService = logService;
            InitS3Client();
        }
        private void InitS3Client()
        {

            _bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME") ?? "hg-gal-hive";

            var clientConfig = new AmazonS3Config();
            clientConfig.RegionEndpoint = RegionEndpoint.USEast2;
            var s3Url = Env.Get("S3_URL");

            if (s3Url != "")
            {
                _customS3 = true;
                clientConfig.ServiceURL = s3Url;
            }

            var accessId = Env.Get("S3_ACCESS_ID"); 
            var secret = Env.Get("S3_ACCESS_SECRET") ;

            _objectRoot = _customS3 ? $"https://{clientConfig.ServiceURL}/" : $"https://{_bucketName}.s3.us-east-2.amazonaws.com/";
            _s3Client = new AmazonS3Client(accessId, secret, clientConfig);
        }
        private string ParseObjectKey(string url)
        {
            return url.Remove(0, _objectRoot.Length);

        }

        private async Task<string> UploadArtifact(string filePath, byte[] payload)
        {
            var memoryStream = new MemoryStream(payload);
            try
            {
                var fileTransferUtility = new TransferUtility(_s3Client);

                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = _bucketName,
                    Key = filePath,
                    InputStream = memoryStream,
                    CannedACL = S3CannedACL.PublicRead //todo: limit this
                };

                await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
                LogService.Information($"{filePath} result uploladed successfully");
                var url = $"{_objectRoot}{filePath}";
                return url;
            }
            catch (AmazonS3Exception e)
            {
                LogService.Error($"Error encountered on server. Message:'{e.Message}' when writing an object");
                throw e;
            }
            catch (Exception e)
            {
                LogService.Error($"Unknown encountered on server. Message:'{e.Message}' when writing an object");
                throw e;
            }
        }

        public async Task<byte[]> GetObject(string payloadUrl)
        {
            var request = new GetObjectRequest();
            request.BucketName = _bucketName;
            request.Key = ParseObjectKey(payloadUrl);
            GetObjectResponse response = await _s3Client.GetObjectAsync(request);
            var stream = new MemoryStream();
            response.ResponseStream.CopyTo(stream); 
            return stream.ToArray();
        }

        public async Task<string> UploadCommand(byte[] payload, string commandID, string satId)
        {
            string filePath = $"{satId}/commands/{commandID}";
            return await UploadArtifact(filePath, payload);
        }
        public async Task<string> UploadCommandResult(byte[] result, string commandID, string satId)
        {
            string filePath = $"{satId}/results/{commandID}";
            return await UploadArtifact(filePath, result);
        }
    }
}

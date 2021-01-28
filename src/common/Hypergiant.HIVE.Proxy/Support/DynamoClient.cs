using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Hypergiant.HIVE.Support
{

    public class DynamoClient
    {

        private ILogService LogService;
        private AmazonDynamoDBClient _dynamoClient;
        const string TABLE_SATELLITE_COMMANDS = "satellite_commands";
        const string TABLE_SATELLITE_COMMAND_RESULTS = "satellite_command_results";

        public DynamoClient(ILogService logService)
        {
            LogService = logService;
            AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig();
            clientConfig.RegionEndpoint = RegionEndpoint.USEast2;
            var dynamoUrl = Env.Get("DYNAMO_URL");

            if (dynamoUrl != "")
            {
                clientConfig.ServiceURL = dynamoUrl;
            }

            var accessId = Env.Get("DYNAMO_ACCESS_ID");
            var secret = Env.Get("DYNAMO_ACCESS_SECRET");

            _dynamoClient = new AmazonDynamoDBClient(accessId, secret, clientConfig);
        }

        public async Task<bool> PutCommand(ICommand command, string payloadUrl)
        {
            var request = new PutItemRequest
            {
                TableName = TABLE_SATELLITE_COMMANDS,
                // Item = command.ToDynamoDBItem(payloadUrl)
            };
            PutItemResponse response = await _dynamoClient.PutItemAsync(request);
            return ((int)response.HttpStatusCode == 200);

        }

        public async Task<bool> PutCommandResult(ICommandResult result, string payloadUrl)
        {
            var request = new PutItemRequest
            {
                TableName = TABLE_SATELLITE_COMMAND_RESULTS,
                // Item = result.ToDynamoDBItem(payloadUrl)
            };
            PutItemResponse response = await _dynamoClient.PutItemAsync(request);
            return ((int)response.HttpStatusCode == 200);
        }

        public async Task<List<Dictionary<string, AttributeValue>>> GetCommands()
        {
            var request = new ScanRequest
            {
                TableName = TABLE_SATELLITE_COMMANDS,
                ScanFilter = new Dictionary<string, Condition>() //for now we're just pulling in all the commands

            };
            ScanResponse response = await _dynamoClient.ScanAsync(request);
            return response.Items;
        }
        public async Task<List<Dictionary<string, AttributeValue>>> GetCommandResults()
        {
            var request = new ScanRequest
            {
                TableName = TABLE_SATELLITE_COMMAND_RESULTS,
                ScanFilter = new Dictionary<string, Condition>() //for now we're just pulling in all the commands

            };
            ScanResponse response = await _dynamoClient.ScanAsync(request);
            return response.Items;
        }

        private async Task<int> AddToDB(string tableName, Dictionary<string, AttributeValue> item)
        {
            var request = new PutItemRequest
            {
                TableName = TABLE_SATELLITE_COMMANDS,
                Item = item
            };
            PutItemResponse response = await _dynamoClient.PutItemAsync(request);
            return ((int)response.HttpStatusCode);
        }

    }
}

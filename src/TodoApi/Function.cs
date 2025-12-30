using System.Text.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TodoApi;

public class Function
{
    private static readonly RegionEndpoint region = RegionEndpoint.USEast1;
    private readonly IDynamoDBContext? _dbContext;

    public Function()
    {
        var config = new AmazonDynamoDBConfig
        {
            RegionEndpoint = region,
            ServiceURL = Environment.GetEnvironmentVariable("DYNAMODB_URL") ?? "http://localhost:8000"
        };

        var client = new AmazonDynamoDBClient(config);
        _dbContext = new DynamoDBContextBuilder()
                    .WithDynamoDBClient(() => client)
                    .Build();
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler
        (APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        var method = request.RequestContext.Http.Method;
        return method switch
        {
            "POST" => await AddItems(request),
            "GET" => await GetItems(request),
            "PUT" => await UpdateItems(request),
            "DELETE" => await DeleteItems(request),
            _ => new() { StatusCode = 405 }
        };
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> AddItems(APIGatewayHttpApiV2ProxyRequest request)
    {
        var todoItem = JsonSerializer.Deserialize<TodoItem>(request.Body!);
        await _dbContext!.SaveAsync(todoItem!);
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 201,
            Body = "Todo Item Added",
            Headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        }
        };
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> GetItems(APIGatewayHttpApiV2ProxyRequest request)
    {
        var itemId = request.QueryStringParameters?["id"];
        if (!string.IsNullOrEmpty(itemId))
        {
            var item = await _dbContext!.LoadAsync<TodoItem>(itemId);
            if (item == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 404,
                    Body = "Item not found",
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    }
                };
            }

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(item),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        var scanConditions = new List<ScanCondition>();
        var allItems = await _dbContext!.ScanAsync<TodoItem>(scanConditions)
            .GetRemainingAsync();

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(allItems),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            }
        };
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> DeleteItems(APIGatewayHttpApiV2ProxyRequest request)
    {
        var itemId = request.QueryStringParameters?["id"];
        if (string.IsNullOrEmpty(itemId))
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 400,
                Body = "Item ID is required for deletion",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }

        await _dbContext!.DeleteAsync<TodoItem>(itemId);

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 204,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            }
        };
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> UpdateItems(APIGatewayHttpApiV2ProxyRequest request)
    {
        var itemId = request.QueryStringParameters?["id"];
        if (string.IsNullOrEmpty(itemId))
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 400,
                Body = "Item ID is required for update",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }

        var modifiedItem = JsonSerializer.Deserialize<TodoItem>(request.Body!);
        var itemToUpdate = await _dbContext!.LoadAsync<TodoItem>(itemId);
        if (itemToUpdate == null)
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 404,
                Body = "Item not found",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }

        itemToUpdate.Name = modifiedItem!.Name;
        itemToUpdate.IsComplete = modifiedItem.IsComplete;
        await _dbContext.SaveAsync(itemToUpdate);

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 204,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            }
        };
    }
}

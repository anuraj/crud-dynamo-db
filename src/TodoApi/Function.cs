using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TodoApi;

public class Function
{
    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler
        (APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        var method = request.RequestContext.Http.Method;
        var body = request.Body;

        context.Logger.LogInformation($"Received {method} request with body: {body}");

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200,
            Body = "Hello from Lambda!",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}

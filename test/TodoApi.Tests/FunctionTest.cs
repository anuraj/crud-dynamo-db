using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using Shouldly;

namespace TodoApi.Tests;

public class FunctionTest
{
    [Fact]
    public async Task TestFunctionHandler()
    {
        // Arrange
        var request = new APIGatewayHttpApiV2ProxyRequest
        {
            RequestContext = new()
            {
                Http = new() { Method = "POST" }
            },
            Body = JsonSerializer.Serialize(new { domain = "example.com" })
        };
        
        var function = new Function();

        // Act
        var response = await function.FunctionHandler(request, new TestLambdaContext());

        // Assert
        response.StatusCode.ShouldBe(200);
        response.Body.ShouldBe("Hello from Lambda!");
        response.Headers.ShouldContainKey("Content-Type");
        response.Headers["Content-Type"].ShouldBe("application/json");
    }
}

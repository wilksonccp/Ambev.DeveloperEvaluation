using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Ambev.DeveloperEvaluation.Integration.Testing;

namespace Ambev.DeveloperEvaluation.Integration.Sales;

public class SalesFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public SalesFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<string> AuthenticateAsync(HttpClient client)
    {
        var authReq = new { email = "admin@local", password = "Admin@123" };
        var resp = await client.PostAsJsonAsync("/api/auth", authReq);
        resp.EnsureSuccessStatusCode();
        var content = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);

        string? found = null;
        var stack = new Stack<JsonElement>();
        stack.Push(doc.RootElement);
        while (stack.Count > 0 && found is null)
        {
            var el = stack.Pop();
            if (el.ValueKind == JsonValueKind.Object)
            {
                foreach (var p in el.EnumerateObject())
                {
                    if (string.Equals(p.Name, "token", StringComparison.OrdinalIgnoreCase))
                    {
                        found = p.Value.GetString();
                        break;
                    }
                    if (p.Value.ValueKind == JsonValueKind.Object || p.Value.ValueKind == JsonValueKind.Array)
                        stack.Push(p.Value);
                }
            }
            else if (el.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in el.EnumerateArray())
                    stack.Push(item);
            }
        }
        Assert.False(string.IsNullOrWhiteSpace(found));
        return found!;
    }

    [Fact]
    public async Task Sales_HappyPath_Works()
    {
        var client = _factory.CreateClient();
        var token = await AuthenticateAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var saleId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var createPayload = new
        {
            id = saleId,
            number = $"S-{Random.Shared.Next(1000, 9999)}",
            customerId = Guid.NewGuid(),
            customerName = "ACME",
            branchId = Guid.NewGuid(),
            branchName = "SP",
            items = new[]
            {
                new { productId, productName = "Beer", unitPrice = 10.0m, quantity = 5 }
            }
        };

        // Create
        var createResp = await client.PostAsJsonAsync("/api/sales", createPayload);
        createResp.EnsureSuccessStatusCode();
        var createJson = JsonDocument.Parse(await createResp.Content.ReadAsStringAsync());
        var createData = createJson.RootElement.GetProperty("data");
        Assert.Equal(saleId.ToString(), createData.GetProperty("id").GetString());
        Assert.Equal(50.0m, createData.GetProperty("totalAmount").GetDecimal());
        Assert.Equal(5.0m, createData.GetProperty("totalDiscount").GetDecimal());
        Assert.Equal(45.0m, createData.GetProperty("totalPayable").GetDecimal());
        var createdItems = createData.GetProperty("items");
        Assert.Equal(JsonValueKind.Array, createdItems.ValueKind);
        Assert.True(createdItems.GetArrayLength() >= 1);
        var createdItem = createdItems[0];
        Assert.Equal(productId.ToString(), createdItem.GetProperty("productId").GetString());
        Assert.Equal(5, createdItem.GetProperty("quantity").GetInt32());
        Assert.Equal(10.0m, createdItem.GetProperty("unitPrice").GetDecimal());
        Assert.Equal(5.0m, createdItem.GetProperty("discountAmount").GetDecimal());
        Assert.Equal(45.0m, createdItem.GetProperty("lineTotal").GetDecimal());

        // Get
        var getResp = await client.GetAsync($"/api/sales/{saleId}");
        getResp.EnsureSuccessStatusCode();
        var getBody = await getResp.Content.ReadAsStringAsync();
        Assert.Contains(saleId.ToString(), getBody);

        // List
        var listResp = await client.GetAsync("/api/sales?page=1&size=10&order=-createdAt");
        listResp.EnsureSuccessStatusCode();
        var listBody = await listResp.Content.ReadAsStringAsync();
        Assert.Contains(saleId.ToString(), listBody);
    }
}

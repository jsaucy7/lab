// Custom code for this connector - implements Microsoft's "Script : ScriptBase" contract.
// Docs: https://learn.microsoft.com/en-us/connectors/custom-connectors/write-code
//
// A few things worth knowing before you write this:
// - Only ONE script file is supported per connector - branch on this.Context.OperationId
//   below if different operations need different handling.
// - Runs on .NET Standard 2.0 with a LIMITED set of namespaces available (see the doc above
//   for the full list) - notably System.Net.Http, System.Text.RegularExpressions,
//   Newtonsoft.Json(.Linq), and Microsoft.Extensions.Logging. Anything outside that list
//   won't compile when the connector is deployed, even if it compiles locally.
// - Execution must finish within 2 minutes; the compiled script can't exceed 1 MB.
// - Always call this.Context.SendAsync(...) to reach the backend - never HttpClient directly.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class Script : ScriptBase
{
    private const string UserAgent = "TALXIS-Lab-WarehousePicking/1.0 (+https://github.com/TALXIS/lab)";

    public override async Task<HttpResponseMessage> ExecuteAsync()
    {
        if (string.Equals(this.Context.OperationId, "GetProductImage", StringComparison.OrdinalIgnoreCase))
        {
            return await GetProductImageAsync().ConfigureAwait(false);
        }

        return await GetProductByBarcodeAsync().ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> GetProductByBarcodeAsync()
    {
        this.Context.Request.Headers.UserAgent.ParseAdd(UserAgent);
        var response = await this.Context.SendAsync(this.Context.Request, this.CancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return response;
        }

        var body = JObject.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        if ((int?)body["status"] != 1)
        {
            response.StatusCode = HttpStatusCode.NotFound;
            response.Content = CreateJsonContent("{\"message\": \"No product found for this barcode\"}");
            return response;
        }

        var product = body["product"];
        var flattened = new JObject
        {
            ["ean"] = (string)body["code"],
            ["name"] = (string)product["product_name"],
            ["brand"] = (string)product["brands"],
            ["quantity"] = (string)product["quantity"],
            ["imageUrl"] = (string)product["image_url"]
        };
        response.Content = CreateJsonContent(flattened.ToString());
        return response;
    }

    private async Task<HttpResponseMessage> GetProductImageAsync()
    {
        var imageUrl = GetQueryParameter(this.Context.Request.RequestUri, "imageUrl");
        if (string.IsNullOrEmpty(imageUrl))
        {
            var badRequest = new HttpResponseMessage(HttpStatusCode.BadRequest);
            badRequest.Content = CreateJsonContent("{\"message\": \"imageUrl query parameter is required\"}");
            return badRequest;
        }

        this.Context.Request.RequestUri = new Uri(imageUrl);
        this.Context.Request.Headers.UserAgent.ParseAdd(UserAgent);
        return await this.Context.SendAsync(this.Context.Request, this.CancellationToken).ConfigureAwait(false);
    }

    private static string GetQueryParameter(Uri requestUri, string name)
    {
        var query = requestUri.Query.TrimStart('?');
        if (string.IsNullOrEmpty(query)) return null;

        foreach (var pair in query.Split('&'))
        {
            var parts = pair.Split(new[] { '=' }, 2);
            if (parts.Length == 2 && parts[0] == name)
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return null;
    }
}

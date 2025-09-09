// Services/GraphSharePointService.cs
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace ProjectPano.Services
{
    public class GraphSharePointService
    {
        private readonly GraphServiceClient _graphClient;

        public GraphSharePointService(IConfiguration configuration)
        {
            var tenantId = configuration["AzureAd:TenantId"];
            var clientId = configuration["AzureAd:ClientId"];
            var clientSecret = configuration["AzureAd:ClientSecret"];

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _graphClient = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });

        }


        //var siteId = "tegrecorp.sharepoint.com,735948da-96fb-46f7-aa06-11e3d61bebdc,38d66edc-e407-4960-a2e8-b67a6c935b0b";
        public async Task<List<ListItem>> GetListItemsAsync(string siteId, string listName)
        {
            // Get the site by ID
            var site = await _graphClient
                .Sites[siteId]
                .GetAsync();

            // Get the list items and expand the "fields" property
            var items = await _graphClient
                .Sites[siteId]
                .Lists[listName]
                .Items
                .GetAsync(req => req.QueryParameters.Expand = new[] { "fields" });

            return items.Value.ToList();
        }


        public GraphServiceClient Client => _graphClient;
    }
}

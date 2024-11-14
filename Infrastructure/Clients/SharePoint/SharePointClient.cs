using Azure.Identity;
using CoreChatbotApp.Utilities.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime;
using System.Threading.Tasks;

namespace CoreChatbotApp.Infrastructure.Clients.SharePoint
{
    public class SharePointClient
    {
        private readonly ApplicationConfig _applicationConfig;
        // App-only auth token credential
        private ClientSecretCredential _clientSecretCredential;
        // Client configured with app-only authentication
        private GraphServiceClient _graphClient;


        public SharePointClient(IOptions<ApplicationConfig> options)
        {
            _applicationConfig = options.Value;
        }

        public async Task InitializeAsync()
        {
            _clientSecretCredential = new ClientSecretCredential(_applicationConfig.MicrosoftAppTenantId, _applicationConfig.MicrosoftAppId, _applicationConfig.MicrosoftAppPassword);
            _graphClient = new GraphServiceClient(_clientSecretCredential,
                    // Use the default scope, which will request the scopes
                    // configured on the app registration
                    new[] { "https://graph.microsoft.com/.default" });
        }

        public async Task<List<string>> GetSharePointSiteInfoAsync(string siteId)
        {
            ListCollectionResponse items = await _graphClient.Sites[siteId].Lists.GetAsync();

            List<string> listNames = new List<string>();

            foreach (var item in items.Value)
            {
                listNames.Add(item.Id + " : " + item.DisplayName);
            }

            return listNames;
        }

        public async Task<int> NumberOfSharePointListsRetrievable(string siteId)
        {
            ListCollectionResponse items = await _graphClient.Sites[siteId].Lists.GetAsync();

            return items.Value.Count;
        }

        public async Task<string> GetListAdditionalData(string siteId, string listId)
        {
            var list = await _graphClient.Sites[siteId].Lists[listId].GetAsync();

            return list.AdditionalData.ToString();
        }

        public async Task<string> GetListFirstItemAllData(string siteId, string listId)
        {
            ListItemCollectionResponse items = await _graphClient.Sites[siteId].Lists[listId].Items.GetAsync();

            var item = items.Value[0];

            return item.ToString() + " :::: " + item.GetFieldDeserializers() + " :::: " + item.AdditionalData;
        }

        public async Task<List<string>> GetFirstTenItemsAsStrings(string siteId, string listId)
        {
            ListItemCollectionResponse items = await _graphClient.Sites[siteId].Lists[listId].Items.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Expand = new string[] { "fields($select=ID,Title,Customer,Customer/Value,CustomerNumber,ProjectGroup,ProjectGroup/Value,PM,PM/Claims,PCLead_x0028_optional_x0029_,PCLead_x0028_optional_x0029_/Claims,Author,Author/Claims)" };
            });
            List<string> itemStrings = new List<string>();

            int i = 0;

            foreach (var item in items.Value)
            {
                string currentItem = "";
                foreach (var field in item.Fields.AdditionalData)
                {
                    currentItem += field.Key + " : " + field.Value + " | ";
                }
                itemStrings.Add(currentItem);
                i++;
                if (i == 10)
                {
                    break;
                }
            }

            return itemStrings;
        }

        public async Task<int> GetNoOfListItems(string siteId, string listId)
        {
            ListItemCollectionResponse items = await _graphClient.Sites[siteId].Lists[listId].Items.GetAsync();

            return items.Value.Count;
        }

        public async Task<List<string>> GetPCMListNormal()
        {
            string siteId = "940a17d8-cb61-40e9-b7c2-ff83d827da83";
            string listId = "c0e20201-b146-41c3-90fb-7f59ae513e92";

            ListItemCollectionResponse items = await _graphClient.Sites[siteId].Lists[listId].Items.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Expand = new string[] { "fields" };
            });

            List<string> itemStrings = new List<string>();

            foreach (var item in items.Value)
            {
                string currentItem = "";
                foreach (var field in item.Fields.AdditionalData)
                {
                    currentItem += field.Key + " : " + field.Value + " | ";
                }
                itemStrings.Add(currentItem);
            }
            return itemStrings;
        }

        public async Task<List<string>> GetPCMListSpecial()
        {
            string siteId = "940a17d8-cb61-40e9-b7c2-ff83d827da83";
            string listId = "c0e20201-b146-41c3-90fb-7f59ae513e92";

            ListItemCollectionResponse items = await _graphClient.Sites[siteId].Lists[listId].Items.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Expand = new string[] { "fields($select=Title,ProjectLead,ProjectLeadLookupId)" };
            });

            List<string> itemStrings = new List<string>();

            foreach (var item in items.Value)
            {
                string currentItem = "";
                foreach (var field in item.Fields.AdditionalData)
                {
                    currentItem += field.Key + " : " + field.Value + " | ";
                }
                itemStrings.Add(currentItem);
            }
            return itemStrings;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using CoreChatbotApp.Utilities.Configurations.Timelog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using static CoreChatbotApp.Infrastructure.Clients.Timelog.ResponseObjects.CustomerResponseObjects;

namespace CoreChatbotApp.Infrastructure.Clients.Timelog
{
    public enum Tenant
    {
        DACH,
        UK,
        ServiceLayers
    }

    public class TimelogClient
    {
        private readonly HttpClient _httpClient;
        private readonly TimelogTenantsConfig _tenantsConfig;

        public TimelogClient(HttpClient httpClient, IOptions<TimelogTenantsConfig> options)
        {
            _httpClient = httpClient;
            _tenantsConfig = options.Value;
        }

        private async Task<string> GetCustomersAsync(TimelogTenantConfig timelogTenantConfig)
        {
            var requestUri = $"{timelogTenantConfig.APIURL}/service.asmx/GetCustomersRaw" +
                             $"?siteCode={timelogTenantConfig.APISiteCode}" +
                             $"&apiID={timelogTenantConfig.APIID}" +
                             $"&apiPassword={timelogTenantConfig.APIPassword}" +
                             "&customerID=0" +
                             "&customerStatusID=-1" +
                             "&accountManagerID=0";

            var response = await _httpClient.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed with status code: {response.StatusCode}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetProjectsByCustomerAsync(TimelogTenantConfig timelogTenantConfig, Customer customer)
        {
            var config = timelogTenantConfig;

            var builder = new UriBuilder($"{config.APIURL}/service.asmx/GetProjectsRaw");
            var query = HttpUtility.ParseQueryString(builder.Query);

            query["siteCode"] = config.APISiteCode;
            query["apiID"] = config.APIID;
            query["apiPassword"] = config.APIPassword;
            query["projectID"] = "0";
            query["status"] = "-2";
            query["customerID"] = customer.ID.ToString();
            query["projectManagerID"] = "0";

            builder.Query = query.ToString();
            var requestUri = builder.ToString();

            var response = await _httpClient.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed with status code: {response.StatusCode}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<List<Customer>> GetFirstTenCustomersAsync(Tenant tenant)
        {
            TimelogTenantConfig config = tenant switch
            {
                Tenant.DACH => _tenantsConfig.DACH,
                Tenant.UK => _tenantsConfig.UK,
                Tenant.ServiceLayers => _tenantsConfig.ServiceLayers,
                _ => throw new ArgumentOutOfRangeException(nameof(tenant), tenant, null)
            };

            string customers;

            try
            {
                customers = await GetCustomersAsync(config);
            }
            catch (Exception e)
            {
                return null;
            }

            var serializer = new XmlSerializer(typeof(CustomersResponse), new XmlRootAttribute
            {
                ElementName = "Customers",
                Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4"
            });

            List<Customer> customersList = new List<Customer>();

            using (var reader = new StringReader(customers))
            {
                CustomersResponse customersResponse = (CustomersResponse)serializer.Deserialize(reader);
                foreach (Customer customer in customersResponse.Customers)
                {
                    customersList.Add(customer);
                }
            }

            if (customersList.Count < 10)
            {
                return customersList;
            }
            return customersList[0..10];
        }

        public string[] GetSettings()
        {
            var config = _tenantsConfig.UK;

            return new string[]
            {
                config.APIURL,
                config.APIID,
                config.APIPassword,
                config.APISiteCode
            };
        }
    }
}

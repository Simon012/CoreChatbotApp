// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;

namespace CoreChatbotApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Build the initial configuration
                    var builtConfig = config.Build();

                    // Retrieve Key Vault settings from configuration
                    string keyVaultName = builtConfig["KeyVaultName"];
                    string kvUri = $"https://{keyVaultName}.vault.azure.net/";

                    string appIdSecretName = builtConfig["MicrosoftAppIdSecretName"];
                    string appPasswordSecretName = builtConfig["MicrosoftAppPasswordSecretName"];
                    string applicationInsightsInstrumentationKeySecretName = builtConfig["ApplicationInsightsInstrumentationKeySecretName"];
                    string mySqlConnectionStringSecretName = builtConfig["MySQLConnectionStringSecretName"];

                    // Set up credentials for accessing Key Vault
                    var credential = new DefaultAzureCredential();

                    // Create a SecretClient to interact with Key Vault
                    var secretClient = new SecretClient(new Uri(kvUri), credential);

                    // Retrieve secrets from Key Vault with individualized names (one name per environment)
                    KeyVaultSecret appIdSecret = secretClient.GetSecret(appIdSecretName);
                    KeyVaultSecret appPasswordSecret = secretClient.GetSecret(appPasswordSecretName);
                    KeyVaultSecret applicationInsightsInstrumentationKeySecret = secretClient.GetSecret(applicationInsightsInstrumentationKeySecretName);
                    KeyVaultSecret mySqlConnectionStringSecret = secretClient.GetSecret(mySqlConnectionStringSecretName);

                    // Retrieve secrets from Key Vault with generic names
                    KeyVaultSecret tenantIdSecret = secretClient.GetSecret("MicrosoftTenantId");
                    KeyVaultSecret cacheConnectionStringSecret = secretClient.GetSecret("RedisCacheConnectionString");

                    KeyVaultSecret timelogDACHTenantAPIURLSecret = secretClient.GetSecret("TimelogDACHTenantAPIURL");
                    KeyVaultSecret timelogDACHTenantAPIIDSecret = secretClient.GetSecret("TimelogDACHTenantAPIID");
                    KeyVaultSecret timelogDACHTenantAPIPasswordSecret = secretClient.GetSecret("TimelogDACHTenantAPIPassword");
                    KeyVaultSecret timelogDACHTenantAPISiteCodeSecret = secretClient.GetSecret("TimelogDACHTenantAPISiteCode");

                    KeyVaultSecret timelogUKTenantAPIURLSecret = secretClient.GetSecret("TimelogUKTenantAPIURL");
                    KeyVaultSecret timelogUKTenantAPIIDSecret = secretClient.GetSecret("TimelogUKTenantAPIID");
                    KeyVaultSecret timelogUKTenantAPIPasswordSecret = secretClient.GetSecret("TimelogUKTenantAPIPassword");
                    KeyVaultSecret timelogUKTenantAPISiteCodeSecret = secretClient.GetSecret("TimelogUKTenantAPISiteCode");

                    KeyVaultSecret timelogServiceLayersTenantAPIURLSecret = secretClient.GetSecret("TimelogServiceLayersTenantAPIURL");
                    KeyVaultSecret timelogServiceLayersTenantAPIIDSecret = secretClient.GetSecret("TimelogServiceLayersTenantAPIID");
                    KeyVaultSecret timelogServiceLayersTenantAPIPasswordSecret = secretClient.GetSecret("TimelogServiceLayersTenantAPIPassword");
                    KeyVaultSecret timelogServiceLayersTenantAPISiteCodeSecret = secretClient.GetSecret("TimelogServiceLayersTenantAPISiteCode");


                    // Add secrets to configuration
                    var keyVaultSecrets = new Dictionary<string, string>
                    {
                        { "MicrosoftAppId", appIdSecret.Value },
                        { "MicrosoftAppPassword", appPasswordSecret.Value },
                        { "MicrosoftAppTenantId", tenantIdSecret.Value },
                        { "ApplicationInsights:InstrumentationKey", applicationInsightsInstrumentationKeySecret.Value },
                        { "RedisCacheConnectionString", cacheConnectionStringSecret.Value },
                        { "MySQLConnectionString", mySqlConnectionStringSecret.Value },

                        { "ApplicationConfig:MicrosoftAppId", appIdSecret.Value },
                        { "ApplicationConfig:MicrosoftAppPassword", appPasswordSecret.Value },
                        { "ApplicationConfig:MicrosoftAppTenantId", tenantIdSecret.Value },


                        { "TimelogTenants:DACH:APIURL", timelogDACHTenantAPIURLSecret.Value },
                        { "TimelogTenants:DACH:APIID", timelogDACHTenantAPIIDSecret.Value },
                        { "TimelogTenants:DACH:APIPassword", timelogDACHTenantAPIPasswordSecret.Value },
                        { "TimelogTenants:DACH:APISiteCode", timelogDACHTenantAPISiteCodeSecret.Value },

                        { "TimelogTenants:UK:APIURL", timelogUKTenantAPIURLSecret.Value },
                        { "TimelogTenants:UK:APIID", timelogUKTenantAPIIDSecret.Value },
                        { "TimelogTenants:UK:APIPassword", timelogUKTenantAPIPasswordSecret.Value },
                        { "TimelogTenants:UK:APISiteCode", timelogUKTenantAPISiteCodeSecret.Value },

                        { "TimelogTenants:ServiceLayers:APIURL", timelogServiceLayersTenantAPIURLSecret.Value },
                        { "TimelogTenants:ServiceLayers:APIID", timelogServiceLayersTenantAPIIDSecret.Value },
                        { "TimelogTenants:ServiceLayers:APIPassword", timelogServiceLayersTenantAPIPasswordSecret.Value },
                        { "TimelogTenants:ServiceLayers:APISiteCode", timelogServiceLayersTenantAPISiteCodeSecret.Value }
                    };

                    config.AddInMemoryCollection(keyVaultSecrets);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureLogging((logging) =>
                    {
                        logging.AddApplicationInsights();
                        logging.AddDebug();
                        logging.AddConsole();
                        // Necessary so that Application Insights receives Trace level logs
                        logging.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>("", LogLevel.Trace);
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}

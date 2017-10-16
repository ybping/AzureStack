namespace MetricsDemo
{
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net.Http;
    using System.Threading.Tasks;


    class MetricProvider:IMetricProvider
    {
        public string TenantId { get; private set; }
        public string SubscriptionId { get; private set; }
        public string ClientId { get; private set; }
        public string AadAuthority { get; private set; }
        public string AadAudience { get; private set; }

        private HttpClient httpClient;
        private string ClientSecret;

        private string ApiVersion = "2015-07-01";
        private string ListMetricsDefinitionTempalte = @"{0}/providers/microsoft.insights/metricDefinitions?api-version={1}";
        // For more info about filter syntas: https://docs.microsoft.com/en-us/rest/api/monitor/filter-syntax
        private string ListMetricsDefinitionWithFilterTempalte = @"{0}/providers/microsoft.insights/metricDefinitions?api-version={1}&$filter={2}";

        /// <summary>
        /// Init a Metic Provider, authorize and authentication by AAD
        /// </summary>
        public MetricProvider()
        {
            this.TenantId = ConfigurationManager.AppSettings["TenantId"];
            this.SubscriptionId = ConfigurationManager.AppSettings["SubscriptionId"];
            this.ClientId = ConfigurationManager.AppSettings["ClientId"];
            this.ClientSecret = ConfigurationManager.AppSettings["ClientSecret"];
            this.AadAudience = ConfigurationManager.AppSettings["AadAudience"];
            this.AadAuthority = ConfigurationManager.AppSettings["AadAuthority"];


            var frontdoorUrl = ConfigurationManager.AppSettings["FrontdoorUrl"];
            Uri baseUrl;
            if (!Uri.TryCreate(frontdoorUrl, UriKind.Absolute, out baseUrl))
            {
                throw new UriFormatException("Invalid FrontdoorUrl: " + frontdoorUrl);
            }

            var token = GetAuthorizationHeaderToken();
            this.httpClient = new HttpClientWrapper(baseUrl, token);

        }

        /// <summary>
        /// Query the Bear Token
        /// </summary>
        /// <returns></returns>
        public string GetAuthorizationHeaderToken()
        {
            AuthenticationResult result = null;
            var authenticationContext = new AuthenticationContext(string.Format(this.AadAuthority, this.TenantId));
            var credential = new ClientCredential(this.ClientId, this.ClientSecret);
            result = authenticationContext.AcquireTokenAsync(this.AadAudience, credential).Result;
            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }
            return result.AccessToken;
        }

        /// <summary>
        /// Query the metrics Definition by RestApi: https://docs.microsoft.com/en-us/rest/api/monitor/metricdefinitions
        /// </summary>
        /// <param name="resourceUrl"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public Task<string> GetMetricsDefinitions(string resourceUrl, string filter)
        {
            string url;
            if (filter == "")
            {
                url = string.Format(this.ListMetricsDefinitionTempalte, resourceUrl, this.ApiVersion);
            }
            else
            {
                url = string.Format(this.ListMetricsDefinitionWithFilterTempalte, resourceUrl, this.ApiVersion, filter);
            }
            Console.WriteLine(url);
            var response = this.httpClient.GetAsync(url).Result;
            return Task.FromResult<string>(response.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// Query the metrics Values by RestApi: https://docs.microsoft.com/en-us/rest/api/monitor/metrics
        /// </summary>
        /// <param name="resourceUrl"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public Task<IEnumerable<string>> GetMetricsValues(string resourceUrl, string filter)
        {
            List<string> results = new List<string>();

            var metricsDefinitions = GetMetricsDefinitions(resourceUrl, filter).Result;
            var jsonData = JObject.Parse(metricsDefinitions);
            var measurementValues = jsonData["value"];
            foreach (var measurement in measurementValues)
            {
                var metricAvailabilities = measurement["metricAvailabilities"];
                foreach(var metrics in metricAvailabilities)
                {
                    var location = metrics["location"];
                    // Parse the table info where the metrics value stored
                    var tableEndpoint = location["tableEndpoint"].ToString();
                    var tableInfo = location["tableInfo"][0];
                    var tablename = tableInfo["tableName"].ToString();
                    var startTime = tableInfo["startTime"].ToString();
                    var endTime = tableInfo["endTime"].ToString();
                    var sasToken = tableInfo["sasToken"].ToString();
                    var sasTokenExpirationTime = tableInfo["sasTokenExpirationTime"].ToString();

                    // Query table storage to get the real metrics data
                    var tableClient = new CloudTableClient(new Uri(tableEndpoint), new StorageCredentials(sasToken));
                    var table = tableClient.GetTableReference(tablename);
                   
                    // Just for demo, we only query 10 records and return the row key;
                    // you can add more DIY operation to process the metrics values
                    TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>().Take(10);
                    var requestOptions = new TableRequestOptions()
                    {
                        ServerTimeout = TimeSpan.FromSeconds(20.0),
                    };
                    var records = table.ExecuteQuery(query, requestOptions);
                   
                    foreach(var record in records)
                    {
                        results.Add(record.RowKey);
                    }
                }
            }
            return Task.FromResult<IEnumerable<string>>(results);
        }
    }

}

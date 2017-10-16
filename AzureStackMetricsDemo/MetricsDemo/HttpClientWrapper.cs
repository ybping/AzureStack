namespace MetricsDemo
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    class HttpClientWrapper: HttpClient
    {
        private static readonly MediaTypeWithQualityHeaderValue Type = new MediaTypeWithQualityHeaderValue("application/json");

        public HttpClientWrapper(Uri baseUrl, string token)
        {
            this.BaseAddress = baseUrl;
            this.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            this.DefaultRequestHeaders.Accept.Add(Type);
        }
    }
}

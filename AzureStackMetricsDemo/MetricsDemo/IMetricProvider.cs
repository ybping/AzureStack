namespace MetricsDemo
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    interface IMetricProvider
    {
        Task<string> GetMetricsDefinitions(string resourceUri, string filter);
        Task<IEnumerable<string>> GetMetricsValues(string resourceUri, string filter);
    }
}

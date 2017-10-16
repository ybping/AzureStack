namespace MetricsDemo
{
    class MetricProviderFactory
    {
        public static IMetricProvider GetProvider()
        {
            return new MetricProvider();
        }
    }
}

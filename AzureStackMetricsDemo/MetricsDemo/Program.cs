namespace MetricsDemo
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            IMetricProvider metricProvier =  MetricProviderFactory.GetProvider();
            string resourceId;

            // Query VM Metircs
            resourceId = "/subscriptions/{subscriptionId}/resourceGroups/{RG}/providers/Microsoft.Compute/virtualMachines/{vmName}";
            Console.WriteLine(metricProvier.GetMetricsDefinitions(resourceId, ""));
            var values = metricProvier.GetMetricsValues(resourceId, "").Result;
            foreach(var value in values)
            {
                Console.WriteLine(value);
            }

            // Query Storage MetricsDefinitions
            resourceId = "/subscriptions/{subsriptionId}/resourcegroups/{RG}/providers/Microsoft.Storage/storageaccounts/{accountName}/services/{serviceType}";
            Console.WriteLine(metricProvier.GetMetricsDefinitions(resourceId, "").Result);

            // Query Storage Metrics Values
            values = metricProvier.GetMetricsValues(resourceId, "").Result;
            foreach(var value in values)
            {
                Console.WriteLine(value);
            }
            Console.ReadLine();
        }
    }
}
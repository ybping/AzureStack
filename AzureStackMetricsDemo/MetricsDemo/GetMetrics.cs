namespace MetricsDemo
{
    using System;

    class GetMetrics
    {
        static void Main(string[] args)
        {
            if(args.Length != 1)
            {
                Console.WriteLine("Usage ./MetricsDemo.exe {ArmId}");
                Console.WriteLine("For VM: ArmId format is /subscriptions/{subscriptionId}/resourceGroups/{RG}/providers/Microsoft.Compute/virtualMachines/{vmName}");
                Console.WriteLine("For Storage: ArmId format is /subscriptions/{subsriptionId}/resourcegroups/{RG}/providers/Microsoft.Storage/storageaccounts/{accountName}/services/{serviceType}");
                return ;
            }
            IMetricProvider metricProvier =  MetricProviderFactory.GetProvider();

            // Query  Metircs
            var resourceId = args[0];
            Console.WriteLine(metricProvier.GetMetricsDefinitions(resourceId, ""));
            var values = metricProvier.GetMetricsValues(resourceId, "").Result;
            foreach(var value in values)
            {
                Console.WriteLine(value);
            }
            Console.ReadLine();
        }
    }
}
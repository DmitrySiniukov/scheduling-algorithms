using System;

namespace ModelingApplication
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            const int n = 11;
            const int m = 3;
            const int iterNum = 1000000;
            Console.WriteLine("Efficiency = {0:f3}", Modeler.CalculateOptimalityCriterionEfficiency(n, m, iterNum,
                () => Modeler.NextGamma(5.0, 0.5), () => Modeler.NextGamma(5.0, 0.5))*100);

            Console.ReadKey();
        }
    }
}
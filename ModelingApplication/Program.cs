using System;

namespace ModelingApplication
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            const int n = 1000;
            const int m = 3;
            const int iterNum = 7777;
            Console.WriteLine("Efficiency = {0:f3}", Modeler.CalculateOptimalityCriterionEfficiency(n, m, iterNum,
                () => Modeler.NextNormal(2.0, 1.0), () => Modeler.NextExponential(0.5))*100);

            Console.ReadKey();
        }
    }
}
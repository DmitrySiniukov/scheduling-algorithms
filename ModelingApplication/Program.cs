using System;
using System.IO;

namespace ModelingApplication
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            const int n = 16;
            const int m = 3;
            const int iterNum = 20;
            const int schedulesNum = 1000;

            for (var i = 9; i < schedulesNum; i++)
            {
                var currentScale = (i + 1)*1.0;
                InitialAlgorithmStatistics newAlgorithmStatistics;
                InitialAlgorithmStatistics primaryAlgorithmStatistics;
                InitialAlgorithmStatistics primaryFirstCriterion;
                Modeler.CalculateOptimalityCriterionEfficiency(n, m, iterNum,
                    () => Modeler.NextGamma(2.0, 8.0), () => Modeler.NextExponential(currentScale),
                    out newAlgorithmStatistics,
                    out primaryAlgorithmStatistics, out primaryFirstCriterion);

                using (var fileStream = File.AppendText("stats2.txt"))
                {
                    fileStream.WriteLine("{0} {1} {2} {3} {4} {5} {6}", currentScale,
                        primaryFirstCriterion.SuccessfulPercent, primaryFirstCriterion.FeasibleExistsPercent,
                        primaryAlgorithmStatistics.SuccessfulPercent, primaryAlgorithmStatistics.FeasibleExistsPercent,
                        newAlgorithmStatistics.SuccessfulPercent, newAlgorithmStatistics.FeasibleExistsPercent);
                }
            }

            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}
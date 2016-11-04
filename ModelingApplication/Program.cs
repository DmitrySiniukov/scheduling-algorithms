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
            const int iterNum = 200;

            using (var fileStream = File.AppendText("stats.txt"))
            {
                fileStream.WriteLine("n={0},m={1},iterNum={2}", n, m, iterNum);
                var currentScale = 0.1;
                for (var i = 0; i < 10000; i++)
                {

                    var result = Modeler.CalculateOptimalityCriterionEfficiency(n, m, iterNum,
                        () => Modeler.NextGamma(5.0, 5.0), () => Modeler.NextExponential(currentScale));

                    fileStream.WriteLine("scale: {0}, success: {1}, sign.: {2}, av.time: {3}", currentScale,
                        result.SuccessfulPercent,
                        result.FeasibleExistsPercent, result.AverageTime);
                    currentScale += 0.1;
                }
            }

            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}
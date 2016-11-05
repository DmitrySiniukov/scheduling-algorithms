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
            const int iterNum = 100;
            const int schedulesNum = 1000;

            using (var fileStream = File.AppendText("stats.txt"))
            {
                fileStream.WriteLine("n={0},m={1},iterNum={2}", n, m, iterNum);
                for (var i = 0; i < schedulesNum; i++)
                {
                    var currentScale = (i + 1)*0.5;
                    var result = Modeler.CalculateOptimalityCriterionEfficiency(n, m, iterNum,
                        () => Modeler.NextGamma(5.0, 5.0), () => Modeler.NextExponential(currentScale));

                    fileStream.WriteLine("scale: {0}, success: {1}, sign.: {2}, av.time: {3}", currentScale,
                        result.SuccessfulPercent, result.FeasibleExistsPercent, result.AverageTime);
                }
            }

            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}
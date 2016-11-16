using MathNet.Numerics.Distributions;
using OptimalSchedulingLogic;
using System;
using System.Collections.Generic;

namespace ModelingApplication
{
    internal class Modeler
    {
        public static void CalculateOptimalityCriterionEfficiency(int n, int m, int testsNumber,
            Func<double> lengthGenerator, Func<double> nextDeadlineGenerator, out InitialAlgorithmStatistics newAlgorithm,
            out InitialAlgorithmStatistics primaryAlgorithm, out InitialAlgorithmStatistics primaryFirstCriterion)
        {
            // Generate machines
            var machines = new List<Machine>();
            for (var i = 0; i < m; i++)
            {
                machines.Add(new Machine(i + 1, string.Format("Machine #{0}", i + 1)));
            }

            newAlgorithm = new InitialAlgorithmStatistics {TestsNumber = testsNumber};
            primaryAlgorithm = new InitialAlgorithmStatistics {TestsNumber = testsNumber};
            primaryFirstCriterion = new InitialAlgorithmStatistics {TestsNumber = testsNumber};
            for (var i = 0; i < testsNumber; i++)
            {
                var tasks = new List<Task>();
                var currentDeadline = new DateTime(2016, 12, 28, 12, 0, 0);

                for (var j = 0; j < n; j++)
                {
                    currentDeadline = currentDeadline.AddMinutes(nextDeadlineGenerator());
                    double length;
                    do
                    {
                        length = lengthGenerator();
                    } while (!(length > 0));
                    tasks.Add(new Task(j + 1, string.Format("Task #{0}", j + 1), length, currentDeadline));
                }
                
                bool firstCriterion;
                var primaryAlg = Schedule.OptimalInitialSchedulePrimary(tasks, machines, out firstCriterion);
                var newAlg = Schedule.OptimalInitialSchedule(tasks, machines, 3);
                var resAcc = Schedule.BuildOptimalSchedule(tasks, machines);
                
                if (primaryAlg != null)
                {
                    primaryAlgorithm.SuccessfulNumber++;
                    if (firstCriterion)
                    {
                        primaryFirstCriterion.SuccessfulNumber++;
                    }
                    var primaryCompareRes = primaryAlg.CompareTo(resAcc);
                    if (primaryCompareRes < 0)
                    {
                        Console.WriteLine("Error");
                        var t = Schedule.OptimalInitialSchedule(tasks, machines, 3);

                        using (var fileStream = System.IO.File.AppendText("log.txt"))
                        {
                            fileStream.WriteLine("\t(id) \"Name\"\tl\td");
                            foreach (var task in tasks)
                            {
                                fileStream.WriteLine("\t({0}) \"{1}\"\t{2}\t{3}", task.Id, task.Name, task.Duration,
                                    (task.Deadline - currentDeadline).TotalMinutes);
                            }
                        }
                    }
                    if (primaryCompareRes == 0)
                    {
                        primaryAlgorithm.FeasibleExistsNumber++;
                        if (firstCriterion)
                        {
                            primaryFirstCriterion.FeasibleExistsNumber++;
                        }
                    }
                }
                if (newAlg != null)
                {
                    newAlgorithm.SuccessfulNumber++;
                    var newCompareRes = newAlg.CompareTo(resAcc);
                    if (newCompareRes < 0)
                    {
                        Console.WriteLine("Error");
                        var t = Schedule.OptimalInitialSchedule(tasks, machines, 3);

                        using (var fileStream = System.IO.File.AppendText("log.txt"))
                        {
                            fileStream.WriteLine("\t(id) \"Name\"\tl\td");
                            foreach (var task in tasks)
                            {
                                fileStream.WriteLine("\t({0}) \"{1}\"\t{2}\t{3}", task.Id, task.Name, task.Duration,
                                    (task.Deadline - currentDeadline).TotalMinutes);
                            }
                        }
                    }
                    if (newCompareRes == 0)
                    {
                        newAlgorithm.FeasibleExistsNumber++;
                    }
                }
            }
        }

        public static double NextNormal(double mean, double standartDeviation, bool allowZero = false)
        {
            if (standartDeviation < 0)
            {
                throw new ArgumentException("Invalid value of standart deviation parameter");
            }

            var distribution = Normal.WithMeanStdDev(mean, standartDeviation);

            var value = 0d;
            var process = true;
            while (process)
            {
                value = distribution.Sample();
                process = value < 0d || !(allowZero || value > 0d);
            }

            return value;
        }

        public static double NextExponential(double scale)
        {
            if (!(scale > 0))
            {
                throw new ArgumentException("Invalid value of scale parameter");
            }

            return Exponential.Sample(1/scale);
        }

        public static double NextGamma(double shape, double scale)
        {
            if (!(shape > 0))
            {
                throw new ArgumentException("Invalid value of shape parameter");
            }
            if (!(scale > 0))
            {
                throw new ArgumentException("Invalid value of scale parameter");
            }

            return Gamma.Sample(shape, 1/scale);
        }
    }
}
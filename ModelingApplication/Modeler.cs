using MathNet.Numerics.Distributions;
using OptimalSchedulingLogic;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ModelingApplication
{
    internal class Modeler
    {
        public static InitialAlgorithmStatistics CalculateOptimalityCriterionEfficiency(int n, int m, int testsNumber,
            Func<double> lengthGenerator, Func<double> nextDeadlineGenerator)
        {
            // Generate machines
            var machines = new List<Machine>();
            for (var i = 0; i < m; i++)
            {
                machines.Add(new Machine(i + 1, string.Format("Machine #{0}", i + 1)));
            }

            var successfulNumber = 0;
            var feasibleExistsNumber = 0;
            long totalTime = 0;
            var sw = new Stopwatch();
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

                sw.Start();
                var res = Schedule.OptimalInitialSchedule(tasks, machines, 3);
                sw.Stop();
                totalTime += sw.ElapsedMilliseconds;
                sw.Reset();

                if (res == null)
                {
                    continue;
                }

                var resAcc = Schedule.BuildOptimalSchedule(tasks, machines);

                var compareResult = res.CompareTo(resAcc);
                if (compareResult < 0)
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
                if (compareResult == 0)
                {
                    feasibleExistsNumber++;
                }

                successfulNumber++;
            }

            return new InitialAlgorithmStatistics
            {
                TestsNumber = testsNumber,
                SuccessfulNumber = successfulNumber,
                FeasibleExistsNumber = feasibleExistsNumber,
                AverageTime = totalTime/(double) testsNumber
            };
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
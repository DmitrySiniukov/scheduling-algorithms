using MathNet.Numerics.Distributions;
using OptimalSchedulingLogic;
using System;
using System.Collections.Generic;

namespace ModelingApplication
{
    internal class Modeler
    {
        public static decimal CalculateOptimalityCriterionEfficiency(int n, int m, int iterationsNumber,
            Func<double> lengthGenerator, Func<double> nextDeadlineGenerator)
        {
            // Generate machines
            var machines = new List<Machine>();
            for (var i = 0; i < m; i++)
            {
                machines.Add(new Machine(i + 1, string.Format("Machine #{0}", i + 1)));
            }

            var succeedCounter = 0;
            for (var i = 0; i < iterationsNumber; i++)
            {
                var tasks = new List<Task>();
                var currentDeadline = new DateTime(2016, 12, 28, 12, 0, 0);

                for (var j = 0; j < n; j++)
                {
                    currentDeadline = currentDeadline.AddMinutes((int) Math.Round(nextDeadlineGenerator()));
                    int length;
                    do
                    {
                        length = (int) Math.Round(lengthGenerator());
                    } while (!(length > 0));
                    tasks.Add(new Task(j + 1, string.Format("Task #{0}", j + 1), length, currentDeadline));
                }

                var res = Schedule.OptimalInitialSchedule(tasks, machines, 3);
                var resAcc = Schedule.BuildOptimalSchedule(tasks, machines);
                if (res.CompareTo(resAcc) < 0)
                {
                    Console.WriteLine("Error");
                    var t = Schedule.OptimalInitialSchedule(tasks, machines, 3);
                }

                if (res != null)
                {
                    succeedCounter++;
                }
            }

            var numerator = (decimal) succeedCounter;
            var denominator = (decimal) iterationsNumber;
            return numerator/denominator;
        }

        public static double NextNormal(double mean, double standartDeviation, bool allowZero = false)
        {
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

        public static double NextExponential(double rate)
        {
            return Exponential.Sample(rate);
        }

        public static double NextGamma(double shape, double rate)
        {
            return Gamma.Sample(shape, rate);
        }
    }
}
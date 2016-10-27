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
                    currentDeadline = currentDeadline.AddMinutes(nextDeadlineGenerator());
                    tasks.Add(new Task(j + 1, string.Format("Task #{0}", j + 1), lengthGenerator(), currentDeadline));
                }

                var res = Schedule.OptimalInitialSchedulePrimary(tasks, machines);
                if (res != null)
                {
                    succeedCounter++;
                }
            }

            var numerator = (decimal) succeedCounter;
            var denominator = (decimal) iterationsNumber;
            return numerator/denominator;
        }

        public static double NextNormal(double mean, double standartDeviation)
        {
            var distribution = Normal.WithMeanStdDev(mean, standartDeviation);

            double value;
            do
            {
                value = distribution.Sample();
                
            } while (!(value > 0));

            return value;
        }

        public static double NextExponential(double rate)
        {
            return Exponential.Sample(rate);
        }
    }
}
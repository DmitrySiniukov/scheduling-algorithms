using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingApplication
{
    public class InitialAlgorithmStatistics
    {
        public int TestsNumber { get; set; }

        public int SuccessfulNumber { get; set; }

        public int FeasibleExistsNumber { get; set; }

        public double AverageTime { get; set; }

        public double SuccessfulPercent
        {
            get
            {
                if (TestsNumber == 0)
                {
                    return 0d;
                }
                return SuccessfulNumber*100d/TestsNumber;
            }
        }

        public double FeasibleExistsPercent
        {
            get
            {
                if (TestsNumber == 0)
                {
                    return 0d;
                }
                return FeasibleExistsNumber * 100d / TestsNumber;
            }
        }

        public override string ToString()
        {
            return
                string.Format(
                    "Number of tests: {0}\nNumber of successful: {1} ({2:F3}%)\nNumber of significant: {3} ({4:F3}%)\nAverage time: {5}",
                    TestsNumber, SuccessfulNumber, SuccessfulPercent, FeasibleExistsNumber, FeasibleExistsPercent,
                    AverageTime);
        }
    }
}
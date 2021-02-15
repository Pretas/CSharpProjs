using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public class RandomGenerator
    {
        public int ScenCnt { get; private set; }
        public int EndMth { get; private set; }

        public RandomGenerator(int scenCnt, int endMth)
        {
            ScenCnt = scenCnt;
            EndMth = endMth;
        }

        public MathNet.Numerics.Random.MersenneTwister RD { get; private set; } = new MathNet.Numerics.Random.MersenneTwister();

        public void SaveToCSV(List<List<double>> rd, string filePath)
        {
            List<object[]> dt = new List<object[]>();

            int mthCnt = rd.Count();
            int scenCnt = rd[0].Count();

            for (int scenNo = 0; scenNo < scenCnt; scenNo++)
            {
                dt.Add(new object[mthCnt]);
            }

            for (int mth = 0; mth < mthCnt; mth++)
            {
                for (int scenNo = 0; scenNo< scenCnt; scenNo++)
                {
                    dt[scenNo][mth] = rd[mth][scenNo];
                }
            }

            Tools.LibFile.WriteFile(filePath, dt, ',');
        }

        // list(시나리오별)<list(월별)>
        public List<List<double>> GetRandomFullSet()
        {
            List<List<double>> rd = new List<List<double>>();

            List<double> normalAccumulated = new List<double>();
            double[] temp = new double[ScenCnt];

            for (int i = 0; i < EndMth; i++)
            {
                if (i == 0)
                {
                    var a = GetRandomOfNormal();
                    rd.Add(a);
                    a.CopyTo(temp);
                    normalAccumulated = temp.ToList();
                }
                else
                {
                    rd.Add(GetRandomAccumulatedOfNormal(ref normalAccumulated));
                }
            }

            return rd;
        }

        public List<double> GetRandomAccumulatedOfNormal(ref List<double> normalAccumulated)
        {   
            var normalD = new MathNet.Numerics.Distributions.Normal();

            int testCnt = 0;
            double pValue = 0.0;

            double[] temp = new double[ScenCnt];
            List<double> normal = null;

            while (pValue < 0.95)
            {
                normalAccumulated.CopyTo(temp);
                normal = GetRandomOfNormal();

                for (int scenSeq = 0; scenSeq < normalAccumulated.Count(); scenSeq++)
                {
                    temp[scenSeq] += normal[scenSeq];
                }

                var sw = new Accord.Statistics.Testing.ShapiroWilkTest(temp.ToArray());
                pValue = sw.PValue;
                testCnt++;
            }

            for (int scenSeq = 0; scenSeq < normalAccumulated.Count(); scenSeq++)
            {
                normalAccumulated[scenSeq] += normal[scenSeq];
            }

            return normal;
        }

        // 1000개 랜덤
        public List<double> GetRandomOfNormal()
        {
            List<double> rdSet = new List<double>();

            var normalD = new MathNet.Numerics.Distributions.Normal();

            double pValue = 0.0;

            int testCnt = 0;
            while (pValue < 0.9)
            {
                rdSet.Clear();

                for (int i = 0; i < ScenCnt; i++)
                {
                    rdSet.Add(normalD.InverseCumulativeDistribution(RD.NextDouble()));
                }

                // Create a new Shapiro-Wilk test:
                var sw = new Accord.Statistics.Testing.ShapiroWilkTest(rdSet.ToArray());
                pValue = sw.PValue;
                testCnt++;
            }

            return rdSet;
        }
    }
}

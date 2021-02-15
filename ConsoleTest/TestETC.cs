using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    public class TestETC
    {
        public static void TestDB()
        {
            //Tools.DBConnParams dbPMS = new Tools.DBConnParams();
            //dbPMS.InitMS();
            //Tools.IDbConnection connMS = new Tools.MsSqlDbConn(dbPMS);

            Tools.DBConnParams dbPMY = new Tools.DBConnParams();
            dbPMY.InitMY();
            Tools.IDbConnection connMY = new Tools.MySqlDBConn(dbPMY);

            var dbP = new Tools.DBConnParams() { ServerName = @"kkhproj-db.czuxn57jn8hk.ap-northeast-2.rds.amazonaws.com", ServerID = "admin", ServerPW = "kkh198400", DbName = "proj" };
            Tools.MsSqlDbConn connMS2 = new Tools.MsSqlDbConn(dbP);
            
            IDataReader dr = connMY.GetDataReader($@"select * from lapse");
            connMS2.Insert(dr, "lapse");

            dr = connMY.GetDataReader($@"select * from mortality");
            connMS2.Insert(dr, "mortality");

        }

        public static void TestScenGen()
        {
            var irs = new List<double>();
            irs.Add(0.01);
            for (int i = 1; i < 14; i++) irs.Add(0.01 + 0.001 * i);

            var sp = new LiabilityCalc.ScenParams() { SourceCurve = irs, HW_a = 0.1, HW_sigma = 0.01, EqSigma = 0.27};

            var sm = new LiabilityCalc.ScenarioManager(sp, 1200);

            sm.SetScenDataFromRandomGen(1000);

            Console.ReadKey();           
        }
        public static void TestIrCurve()
        {
            var irs = new List<double>();
            irs.Add(0.01);
            for (int i = 1; i < 14; i++) irs.Add(0.01 + 0.001 * i);

            LiabilityCalc.InterestRates ints = new LiabilityCalc.InterestRates(irs, 1200);

            Tools.LibFile.WriteFile(@"d:\irs.csv", ints.SourceCurve);
            Tools.LibFile.WriteFile(@"d:\bootstrappedZero.csv", ints.BootstrappedZeros);
            Tools.LibFile.WriteFile(@"d:\zero.csv", ints.ZeroCurve);
            Tools.LibFile.WriteFile(@"d:\forward.csv", ints.ForwardCurve);
        }

        public static void ConsoleWriteMoving()
        {
            long turn = 1000000000;

            for (long i = 0; i < turn; i++)
            {
                if (i % 1000 == 0)
                { Console.Write("\r" + "finished " + i); }

                
            }

            Console.ReadKey();
            
        }

        public static void TestRandom()
        {
            var rd = new MathNet.Numerics.Random.MersenneTwister();

            var normalD = new MathNet.Numerics.Distributions.Normal();
            List<double> vals = new List<double>();

            for (int i = 0; i < 100000; i++)
            {
                vals.Add(normalD.InverseCumulativeDistribution(rd.NextDouble()));
            }

            // Create a new Shapiro-Wilk test:
            var sw = new Accord.Statistics.Testing.ShapiroWilkTest(vals.ToArray());
        }

        // 50년 누적값이 몇개의 난수셋을 거치면서 안정되는지 확인
        public static void TestRandom2()
        {
            var rd = new MathNet.Numerics.Random.MersenneTwister();

            var normalD = new MathNet.Numerics.Distributions.Normal();

            double val = 0.0;
            double avg = 0.0;
            List<double> avgVal = new List<double>();

            for (int i = 1; i <= 1000000; i++)
            {
                double cumVal = 1.0;
                for (int j = 0; j < 50; j++)
                {
                    val = normalD.InverseCumulativeDistribution(rd.NextDouble());
                    cumVal *= 1.0 + (val / 10.0);
                }

                if (i == 1) avg = cumVal;
                else avg = avg * (Convert.ToDouble(i - 1) / Convert.ToDouble(i))
                        + cumVal * (1 / Convert.ToDouble(i));

                // 1000개마다 평균값을 저장
                if (i%1000 == 0) avgVal.Add(avg);
            }
        }

        public static void TestRandom3()
        {
            LiabilityCalc.RandomGenerator rdg = new LiabilityCalc.RandomGenerator(1000, 1200);

            var set = rdg.GetRandomFullSet();

            string filePath = @"d:\temp\random1.csv";
            rdg.SaveToCSV(set, filePath);
        }
    }
}

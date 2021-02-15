using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiabilityCalc;

namespace ConsoleTest
{
    public class TestCF
    {
        public static void TestProjRun(string[] args)
        {
            if (Guid.TryParse(args[0], out Guid guid)) // args가 제대로 guid로 들어왔다면
            {
                ProjRunner pr = new ProjRunner(args);
                pr.LoadData();
                pr.Run();
            }
            else
            { TestOneCase(); }
        }

        public static void TestOneCase()
        {
            //// 런파라미터
            //var rs = new RunSettings();
            //rs.IdMP = '';
            //rs.MpNoStart = sInf;
            //rs.MpNoEnd = eInf;
            //rs.ResultLabel = jobId;
            //rs.ScenCntOneBatch = 1000;
            //rs.ScenBatchCnt = 1;
            //rs.InsertsByPol = true;
            //rs.InsertsByScen = false;

            //// 계리적가정 테이블
            //tm = new TableManager(rs, dbP);
            //tm.SetTables();

            //// 인포스 로딩 
            //var mphandler = new MPHandler(rs);
            //mphandler.LoadRecords(dbP);
            //pols = Policy.GetPolicies(mphandler.Recs, tm);

            //// 경제적 가정
            //string query = $@"select * from ScenSource where id = '{scenId}'";
            //var res = msConn.GetResult(query)[0];
            //var irs = new List<double>();
            //for (int i = 4; i < 18; i++) irs.Add(Convert.ToDouble(res[i]));
            //var sp = new LiabilityCalc.ScenParams()
            //{
            //    SourceCurve = irs,
            //    EqSigma = Convert.ToDouble(res[1]),
            //    HW_a = Convert.ToDouble(res[2]),
            //    HW_sigma = Convert.ToDouble(res[3])
            //};

            //sm = new ScenarioManager(sp, 1200);
            //Console.WriteLine($@"Load Run Data {machineName}");


            //Runner runner = new Runner(rs, tm, sm, dbP);
            //runner.Execute(rs.IdMP, pols);
            //WriteLog("Finished");
        }

        public static void TestCloud(string[] args)
        {
            Console.WriteLine("프로그램 시작");
            var irs = new List<double> { 0.01 };
            for (int i = 1; i < 14; i++) irs.Add(0.01 + 0.001 * i);

            var sp = new LiabilityCalc.ScenParams() { SourceCurve = irs, HW_a = 0.1, HW_sigma = 0.01, EqSigma = 0.27, BondSigma = 0.01 };

            ScenarioManager sm = new ScenarioManager(sp, 1200);

            Tools.DBConnParams dbP = new Tools.DBConnParams(new string[] { args[1], args[2], args[3], args[4] });

            int scenCnt = 100;
            sm.SetScenDataFromFile(scenCnt);

            Console.WriteLine("시나리오 생성 완료");

            for (int i = 1; i <= scenCnt; i++)
            {
                sm.SetCurrentScen(i);
                InsertScenToDB(args[0], sm.CurrentScenNo, sm.CurrentScenData, dbP);
                Console.WriteLine($@"시나리오 {i}번 입력 완료");
            }
        }

        public static void InsertScenToDB(string id, int currentScenNo, Dictionary<ScenType, double[]> currentScenData, Tools.DBConnParams dbP)
        {
            // 데이터 전환 to list<object[]>
            // id, asset, scenno, val
            var lst = new List<object[]>();
            object[] obj;

            foreach (var kv in currentScenData)
            {
                string asset = kv.Key.ToString();
                for (int i = 0; i < kv.Value.Length; i++)
                {
                    obj = new object[5];
                    obj[0] = id;
                    obj[1] = asset;
                    obj[2] = currentScenNo;
                    obj[3] = i;
                    obj[4] = kv.Value[i];
                    lst.Add(obj);
                }
            }

            // db입력
            Tools.MsSqlDbConn dbConn = new Tools.MsSqlDbConn(dbP);
            //Tools.MySqlDBConn dbConn = new Tools.MySqlDBConn(dbP);
            dbConn.Insert(lst, @"TestScen");
        }


        public static void 데이터테스트()
        {
            //LiabilityCalc.InsertData.InsertTemp();

            //LiabilityCalc.InsertData.InsertMort();
            
            LiabilityCalc.InsertData.InsertLapse();

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public class ProjRunner
    {
        string jobId;
        string runGuid;
        string infId;
        string scenId;
        string machineName;
        Tools.DBConnParams dbP = null;
        Tools.MsSqlDbConn msConn = null;
        RunSettings rs = null;

        TableManager tm = null;
        ScenarioManager sm = null;
        List<Policy> pols = null;
        
        public ProjRunner(string[] args)
        {
            try
            {
                runGuid = args[0];

                // 디비 연결
                dbP = new Tools.DBConnParams() { ServerName = @"kkhproj-db.czuxn57jn8hk.ap-northeast-2.rds.amazonaws.com", ServerID = "admin", ServerPW = "kkh198400", DbName = "proj" };
                msConn = new Tools.MsSqlDbConn(dbP);

                // 런 인포 받아오기
                string query = $@"select top 1 id, arguments from RunInfo where guid = '{runGuid}'";
                var runArgs = msConn.GetResult(query)[0];
                jobId = runArgs[0].ToString();
                infId = runArgs[1].ToString().Split(' ')[0];
                scenId = runArgs[1].ToString().Split(' ')[1];

                // 환경변수 가져오기 
                machineName = Environment.GetEnvironmentVariable("kkhproj", EnvironmentVariableTarget.User);

                // 인포스 시작, 끝 가져오기
                query = $@"select top 1 StartNo, EndNo from RunStatus where guid ='{runGuid}' and machinename = '{machineName}'";
                var res = msConn.GetResult(query)[0];
                int sInf = Convert.ToInt32(res[0]);
                int eInf = Convert.ToInt32(res[1]);

                // 상태 업데이트
                WriteLog("Running");
                Console.WriteLine($@"Ready to Run {machineName}");

                // 런파라미터
                rs = new RunSettings();
                rs.IdMP = infId;
                rs.MpNoStart = sInf;
                rs.MpNoEnd = eInf;
                rs.ResultLabel = jobId;
                rs.ScenCntOneBatch = 1000;
                rs.ScenBatchCnt = 1;
                rs.InsertsByPol = true;
                rs.InsertsByScen = false;
            }
            catch(Exception e)
            {
                WriteLog("Failed_Init \n" + e.ToString());
                throw new Exception("Fail to start program");
            }
        }

        public void LoadData()
        {
            try
            {
                // 계리적가정 테이블
                tm = new TableManager(rs, dbP);
                tm.SetTables();

                // 인포스 로딩 
                var mphandler = new MPHandler(rs);
                mphandler.LoadRecords(dbP);
                pols = Policy.GetPolicies(mphandler.Recs, tm);

                // 경제적 가정
                string query = $@"select * from ScenSource where id = '{scenId}'";
                var res = msConn.GetResult(query)[0];
                var irs = new List<double>();
                for (int i = 4; i < 18; i++) irs.Add(Convert.ToDouble(res[i]));
                var sp = new LiabilityCalc.ScenParams()
                {
                    SourceCurve = irs,
                    EqSigma = Convert.ToDouble(res[1]),
                    HW_a = Convert.ToDouble(res[2]),
                    HW_sigma = Convert.ToDouble(res[3])
                };

                sm = new ScenarioManager(sp, 1200);
                Console.WriteLine($@"Load Run Data {machineName}");
            }
            catch
            {
                WriteLog("Failed_LoadData");
                throw new Exception("Fail to start program");
            }
        }

        public void Run()
        {
            Runner runner = new Runner(rs, tm, sm, dbP);
            runner.Execute(rs.IdMP, pols);
            WriteLog("Finished");          
        }

        private void WriteLog(string msg)
        {
            // 상태 업데이트
            string query = $@"update RunStatus set State = '{msg}' where Guid = '{runGuid}' and machinename = '{machineName}'";
            msConn.Execute(query);
        }
    }

}

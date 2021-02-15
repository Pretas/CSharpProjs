using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public partial class Runner
    {
        public RunSettings RSettings { get; private set; }
        public TableManager TM { get; private set; }
        public ScenarioManager SM { get; private set; }
        public Tools.DBConnParams DbParams { get; private set; }

        public Runner(RunSettings rs, TableManager tm, ScenarioManager sm, Tools.DBConnParams dp)
        {
            RSettings = rs;
            TM = tm;
            SM = sm;
            DbParams = dp;
        }

        /// <summary>
        /// 런실행
        /// </summary>
        /// <param name="id">아이디(사망률_해지율_금리커브_계약번호)</param>
        public void Execute(string id, List<Policy> pols)
        {
            // date4 gets 4/9/1996 5:55:00 PM.
            DateTime DateStart = DateTime.Now;

            // 100번 배치, 총 "BatchCnt * ScenCntByBatch" 번의 시나리오 
            for (int batchSeq = 1; batchSeq <= RSettings.ScenBatchCnt; batchSeq++)
            {
                 //int scenStartSeq = (batchSeq - 1) * RSettings.ScenCntOneBatch + 1;
                SM.SetScenDataFromFile(RSettings.ScenCntOneBatch);

                var outs = new OutputManager(RSettings, DbParams, pols.Select(x=>x.Rec.ContNo).ToList(), 1);

                // 시나리오 루프
                for (int scenNo = 1; scenNo <= RSettings.ScenCntOneBatch; scenNo++)
                {
                    SM.SetCurrentScen(scenNo);

                    // 보유계약 루프
                    foreach (var pol in pols)
                    {
                        ModelBase md = ModelBase.GetSelectedModel(RSettings, pol, SM);
                        md.Run(outs);
                    }

                    if (scenNo % 10 == 0)
                    {
                        DateTime DateAft = DateTime.Now;
                        TimeSpan span = DateAft - DateStart;
                        Console.WriteLine($@"finish {scenNo} Scen of Inf {pols.First().Rec.ContNo} ~ {pols.Last().Rec.ContNo} : Duration {span}");
                    }
                }

                int minPolNo = pols.Select(x => x.Rec.ContNo).Min();
                outs.InsertToDB(RSettings.ResultLabel, minPolNo * batchSeq);
            }
        }

    }
}

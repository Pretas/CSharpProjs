using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public class RunSettings
    {
        public string IdMP { get; set; }
        public int MpNoStart { get; set; }
        public int MpNoEnd { get; set; }

        public string IdMort { get; internal set; }
        public string IdLapse { get; internal set; }
        public bool HasDynamicLapse { get; internal set; }

        public int ScenBatchCnt { get; internal set; }
        public int ScenCntOneBatch { get; set; }

        public string ResultLabel { get; set; }

        public bool InsertsByPol { get; set; } = false;
        public bool InsertsByMth { get; set; } = false;
        public bool InsertsByScen { get; set; } = false;        

        public RunSettings()
        {
            IdMort = "2009";
            IdLapse = "2009";
        }
    }

    public class TableConst
    {
        public string ID { get; private set; }
        public string VarKey { get; private set; }
        public string VarName { get; private set; }
        public double Val { get; private set; }
    }

    public class TableArray
    {
        public string ID { get; private set; }
        public string VarKey { get; private set; }
        public string VarValue { get; private set; }
        public Dictionary<int, double> Vals { get; private set; }

        public void SetValues(string id, string vk, string vv, Dictionary<int, double> vals)
        {
            ID = id; VarKey = vk; VarValue = vv; Vals = vals;
        }
    }

    public class TableManager
    {
        public RunSettings RunSets { get; private set; }

        public Product Prod { get; private set; }
        public List<TableArray> Mort { get; private set; }
        public List<TableArray> Lapse { get; private set; }

        private Tools.IDbConnection DbConn;

        public TableManager(RunSettings rs, Tools.DBConnParams dbParams)
        {
            RunSets = rs;
            DbConn = new Tools.MsSqlDbConn(dbParams);
        }

        public void SetTables()
        {
            SetMort();
            SetLapse();
        }

        public void SetMort()
        {
            string query = $@"select * from mortality where id = '{RunSets.IdMort}'";
            var dt = DbConn.GetResult(query);

            Mort = new List<TableArray>();
            TableArray ta;
            foreach (var line in dt)
            {
                ta = new TableArray();

                var vals = new Dictionary<int, double>();
                for (int i = 3; i < line.Length; i++) vals.Add(i-3, Convert.ToDouble(line[i]));

                ta.SetValues(line[0].ToString(), line[1].ToString(), line[2].ToString(), vals);

                Mort.Add(ta);
            }
        }

        private void SetLapse()
        {
            string query = $@"select * from lapse where id = '{RunSets.IdLapse}'";
            var dt = DbConn.GetResult(query);

            Lapse = new List<TableArray>();
            TableArray ta;
            foreach (var line in dt)
            {
                ta = new TableArray();

                var vals = new Dictionary<int, double>();
                for (int i = 3; i < line.Length; i++) vals.Add(i - 2, Convert.ToDouble(line[i]));

                ta.SetValues(line[0].ToString(), line[1].ToString(), line[2].ToString(), vals);

                Lapse.Add(ta);
            }
        }
    }

    public class InsertData
    {
        public static void InsertMort()
        {
            var dt = new List<object[]>();

            var line = new object[121+3];

            line[0] = "2009";
            line[1] = "UL";
            line[2] = 1;

            for (int age = 0; age <= 120; age++)
            {
                if (age == 120) line[age + 3] = 1.0;
                else line[age + 3] = 0.001 * age;
            }

            dt.Add(line);

            line = new object[121+3];

            line[0] = "2009";
            line[1] = "UL";
            line[2] = 2;

            for (int age = 0; age <= 120; age++)
            {
                if (age == 120) line[age + 3] = 1.0;
                else line[age + 3] = (0.001 * age)*0.8;
            }

            dt.Add(line);

            var dp = new Tools.DBConnParams();
            dp.InitMY();

            Tools.MySqlDBConn conn = new Tools.MySqlDBConn(dp);
            conn.Insert(dt, "mortality");
        }

        public static void InsertLapse()
        {
            var dt = new List<object[]>();

            // 1번째 라인
            var line = new object[120 + 3];

            line[0] = "2009";
            line[1] = "UL";
            line[2] = "InPayment";

            for (int yr = 1; yr <= 120; yr++)
            {
                if (yr >= 20) line[yr + 2] = 0.03;
                else line[yr + 2] = Math.Max(0.03, 0.1 - yr * 0.01);
            }

            dt.Add(line);

            // 2번째 라인
            line = new object[120 + 3];

            line[0] = "2009";
            line[1] = "UL";
            line[2] = "AfterPayment";

            for (int yr = 1; yr <= 120; yr++)
            {
                line[yr + 2] = 0.03;
            }

            dt.Add(line);
            
            var dp = new Tools.DBConnParams();
            dp.InitMY();

            Tools.MySqlDBConn conn = new Tools.MySqlDBConn(dp);
            conn.Insert(dt, "lapse");
        }
    }
}

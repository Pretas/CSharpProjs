using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public class MPHandler
    {
        private RunSettings RSettings;

        public MPHandler(RunSettings rs)
        {
            RSettings = rs;
        }

        public List<Record> Recs { get; private set; }

        public void LoadRecords(Tools.DBConnParams dbParams)
        {
            Tools.IDbConnection dbConn = new Tools.MsSqlDbConn(dbParams);

            //string query = $@"select * from Records where id = '{whereID}' and elapsedmth=60 and premyr=10 and startageofsomething = 60 limit 1";
            string query = $@"select * from Records where id = '{RSettings.IdMP}' and contno >= {RSettings.MpNoStart} and contno <= {RSettings.MpNoEnd}";
            var recsFrom = dbConn.GetResult(query);

            if (recsFrom.Count == 0) throw new Exception("no recs for db");

            Recs = Tools.LibsData.GetInstance<Record>(recsFrom);
        }
    }
}

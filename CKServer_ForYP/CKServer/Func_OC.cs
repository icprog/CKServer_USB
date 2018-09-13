using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CKServer
{
    class Func_OC
    {
        public static DataTable dt_OC_Out = new DataTable();

        public static int OCOutChanNums = 10;

        public static void Init_Table()
        {        

            dt_OC_Out.Columns.Add("序号", typeof(Int32));
            dt_OC_Out.Columns.Add("名称", typeof(String));
            dt_OC_Out.Columns.Add("脉宽", typeof(int));


            for (int i = 0; i < OCOutChanNums; i++)
            {
                DataRow dr = dt_OC_Out.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = Function.GetConfigStr(Data.OCconfigPath, "add", "OC_Channel_" + i.ToString(), "name");
                dr["脉宽"] = Function.GetConfigStr(Data.OCconfigPath, "add", "OC_Channel_" + i.ToString(), "value");
                dt_OC_Out.Rows.Add(dr);
            }
        }




    }
}

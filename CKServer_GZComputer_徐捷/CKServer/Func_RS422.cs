using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CKServer
{
    class Func_RS422
    {

        public static DataTable dt_RS422_Send = new DataTable();
        public static int RS422Nums = 23;
        public static void Init_Table()
        {
            dt_RS422_Send.Columns.Add("序号", typeof(String));
            dt_RS422_Send.Columns.Add("选中发送", typeof(Boolean));
            dt_RS422_Send.Columns.Add("名称", typeof(String));
            dt_RS422_Send.Columns.Add("码本路径", typeof(String));
            for (int i = 0; i < RS422Nums; i++)
            {
                DataRow dr = dt_RS422_Send.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = Function.GetConfigStr(Data.RS422configPath, "add", "RS422_Channel_" + i.ToString(), "name");
                string check = Function.GetConfigStr(Data.RS422configPath, "add", "RS422_Channel_" + i.ToString(), "check");
                dr["选中发送"] = Convert.ToBoolean(check);
                dr["码本路径"] = Function.GetConfigStr(Data.RS422configPath, "add", "RS422_Channel_" + i.ToString(), "path");
                dt_RS422_Send.Rows.Add(dr);
            }
        }


    }
}

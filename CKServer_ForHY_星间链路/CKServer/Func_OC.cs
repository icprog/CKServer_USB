using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CKServer
{
    class Func_OC
    {
        public static DataTable dt_OC = new DataTable();

        public static void Init_Table()
        {
            dt_OC.Columns.Add("序号", typeof(Int32));
            dt_OC.Columns.Add("名称", typeof(String));
            //dt_OC.Columns.Add("脉宽", typeof(int));

            for (int i = 0; i < 8; i++)
            {
                DataRow dr = dt_OC.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = Function.GetConfigStr(Data.OCconfigPath, "add", "OC_Channel_" + i.ToString(), "name");
                //dr["脉宽"] = Function.GetConfigStr(Data.OCconfigPath, "add", "OC_Channel_" + i.ToString(), "value");
                dt_OC.Rows.Add(dr);
            }
        }

        public static void Send2OCBD(byte[] OCData)
        {
            byte[] Send2OCBd = new byte[45];
            Send2OCBd[0] = 0x1D;
            Send2OCBd[1] = 0x00;
            Send2OCBd[2] = 0x00;
            Send2OCBd[3] = 0x25;//32+5 1D000025是机箱帧头
            Send2OCBd[4] = 0xBD;
            Send2OCBd[5] = 0x1D;
            Send2OCBd[6] = 0x00;
            Send2OCBd[7] = 0x0C;//BD1D000C代表OC数据格式
            Send2OCBd[8] = 0x00;//00/01/02/03代表通道，此处使用OC
            OCData.CopyTo(Send2OCBd, 9);
            Send2OCBd[41] = 0xC0;
            Send2OCBd[42] = 0xDE;
            Send2OCBd[43] = 0xC0;
            Send2OCBd[44] = 0xDE;

            USB.SendData(Data.OnlyID, Send2OCBd);

        }

    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CKServer
{
    class Func_DA
    {
        public static DataTable dt_DA1 = new DataTable();
        public static DataTable dt_DA2 = new DataTable();

        public static int DABoard1Nums = 80;
        public static int DABoard2Nums = 80;

        //每块板卡有4个DA芯片，每个芯片有32路，每1路用4Byte表示，DAByteA~DAByteD分别表示4个DA芯片
        public static byte[] DAByteA = new byte[128];
        public static byte[] DAByteB = new byte[128];
        public static byte[] DAByteC = new byte[128];
        public static byte[] DAByteD = new byte[128];

        public static void Init_Table()
        {
            dt_DA1.Columns.Add("序号", typeof(Int32));
            dt_DA1.Columns.Add("名称", typeof(String));
            dt_DA1.Columns.Add("电压", typeof(double));
            for (int i = 0; i < DABoard1Nums; i++)
            {
                DataRow dr = dt_DA1.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = Function.GetConfigStr(Data.DAconfigPath, "add", "DA_Channel_" + i.ToString(), "name");
                dr["电压"] = 0;
                dt_DA1.Rows.Add(dr);
            }

            dt_DA2.Columns.Add("序号", typeof(Int32));
            dt_DA2.Columns.Add("名称", typeof(String));
            dt_DA2.Columns.Add("电压", typeof(double));
            for (int i = 0; i < DABoard2Nums; i++)
            {
                DataRow dr = dt_DA2.NewRow();
                dr["序号"] = i + 81;
                dr["名称"] = Function.GetConfigStr(Data.DAconfigPath, "add", "DA_Channel_" + (i+DABoard1Nums).ToString(), "name");
                dr["电压"] = 0;
                dt_DA2.Rows.Add(dr);
            }


        }

        /// <summary>
        /// 将电压值转化为下发的4Byte
        /// </summary>
        /// <param name="card">0~1:对应板卡1、2</param>
        /// <param name="row">行数</param>
        /// <param name="value">电压值</param>
        public static void clcDAValue(int card, int row, double value)
        {
            double SendValue = 0;
            if (card == 0) SendValue = Data.DA1_value_a[row] + (value / 10.0) * Data.DA1_value_b[row];//第一块卡参数修正
            if (card == 1) SendValue = Data.DA2_value_a[row] + (value / 10.0) * Data.DA2_value_b[row];//第二块卡参数修正

            Int16 temp = Convert.ToInt16(SendValue);


            if(row<32)
            {
                DAByteA[0 + 4 * row] = 0x00;
                DAByteA[1 + 4 * row] = (byte)(0x40 + (row / 4));
                byte a = (byte)((temp & 0x3f00) >> 8);
                byte b = (byte)(((row % 4) & 0x03) << 6);
                DAByteA[2 + 4 * row] = (byte)(b + a);
                DAByteA[3 + 4 * row] = (byte)(temp & 0xff);
            }
            else if(row>=32 && row<64)
            {
                row = row - 32;
                DAByteB[0 + 4 * row] = 0x00;
                DAByteB[1 + 4 * row] = (byte)(0x40 + (row / 4));
                byte a2 = (byte)((temp & 0x3f00) >> 8);
                byte b2 = (byte)(((row % 4) & 0x03) << 6);
                DAByteB[2 + 4 * row] = (byte)(b2 + a2);
                DAByteB[3 + 4 * row] = (byte)(temp & 0xff);
            }
            else if(row>=64 && row <96)
            {
                row = row - 64;
                DAByteC[0 + 4 * row] = 0x00;
                DAByteC[1 + 4 * row] = (byte)(0x40 + (row / 4));
                byte a3 = (byte)((temp & 0x3f00) >> 8);
                byte b3 = (byte)(((row % 4) & 0x03) << 6);
                DAByteC[2 + 4 * row] = (byte)(b3 + a3);
                DAByteC[3 + 4 * row] = (byte)(temp & 0xff);
            }
            else//最大128行
            {
                row = row - 96;
                DAByteD[0 + 4 * row] = 0x00;
                DAByteD[1 + 4 * row] = (byte)(0x40 + (row / 4));
                byte a4 = (byte)((temp & 0x3f00) >> 8);
                byte b4 = (byte)(((row % 4) & 0x03) << 6);
                DAByteD[2 + 4 * row] = (byte)(b4 + a4);
                DAByteD[3 + 4 * row] = (byte)(temp & 0xff);
            }          

        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CKServer
{
    class Func_AD2_BIT
    {

        public static DataTable dt_AD1 = new DataTable();
        public static DataTable dt_AD2 = new DataTable();

        public static int ADNums1 = 48;
        public static int ADNums2 = 48;

        public static void Init_Table()
        {
            dt_AD1.Columns.Add("序号", typeof(String));

            for (int i = 0; i < ADNums1; i++)
            {
                DataRow dr = dt_AD1.NewRow();
                dr["序号"] = "Bit" + (i + 1).ToString();
                dt_AD1.Rows.Add(dr);
            }

            dt_AD2.Columns.Add("序号", typeof(String));

            for (int i = 0; i < ADNums2; i++)
            {
                DataRow dr = dt_AD2.NewRow();
                dr["序号"] = "Bit" + (i + 49).ToString();
                dt_AD2.Rows.Add(dr);
            }

        }


        /// <summary>
        /// 根据采集到的AD源码解析出实际AD值，解析成功返回true否则false
        /// </summary>
        /// <param name="ADList">包含AD数据的帧</param>
        /// <param name="dataRe_AD">解析后的double类型AD数据</param>
        /// <returns></returns>
        public static bool Return_ADValue(ref List<byte> ADList, ref double[] L1, ref double[] L2, ref double[] L3, ref double[] L4)
        {
            int ADFrame = 128 + 4;//64channel=128bytes+2Head+2End
            int NChans = 48;//实际使用通道数48，总通道数64

            double[] dataRe_AD = new double[NChans];

            if (ADList.Count >= 132)
            {
                byte[] buf = new byte[132];
                try
                {
                    buf = ADList.Take(132).ToArray();

                    ADList.RemoveRange(0, 132);
                }
                catch(Exception ex)
                {
                    Trace.WriteLine(ex.Message + "From Return_ADValue Func_AD_BIT");
                }

                byte[] RealData = new byte[96];
                Array.Copy(buf, 2, RealData, 0, 96);
                for (int k = 0; k < NChans; k++)
                {
                    int temp = (RealData[2 * k] & 0x7f) * 256 + RealData[2 * k + 1];
                    double t = (double)(5 * temp) / (double)32767;
                    if ((RealData[2 * k] & 0x80) == 0x80)
                        dataRe_AD[k] = -t;
                    else
                        dataRe_AD[k] = t;
                }

                if (buf[0] == 0xB0 && buf[1] == 0xF0)
                {
                    Array.Copy(dataRe_AD, 0, L1, 0, NChans);
                    return true;
                }
                else if (buf[0] == 0xB0 && buf[1] == 0xF1)
                {
                    Array.Copy(dataRe_AD, 0, L2, 0, NChans);
                    return true;
                }
                else if (buf[0] == 0xB0 && buf[1] == 0xF2)
                {
                    Array.Copy(dataRe_AD, 0, L3, 0, NChans);
                    return true;
                }
                else if (buf[0] == 0xB0 && buf[1] == 0xF3)
                {
                    Array.Copy(dataRe_AD, 0, L4, 0, NChans);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

    }
}

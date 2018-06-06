using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CKServer
{
    class Func_AD
    {
        public static DataTable dt_AD = new DataTable();
        public static void Init_Table()
        {
            dt_AD.Columns.Add("序号", typeof(Int32));
            dt_AD.Columns.Add("名称", typeof(String));
            dt_AD.Columns.Add("测量值", typeof(double));
            for (int i = 0; i < 64; i++)
            {
                DataRow dr = dt_AD.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = Function.GetConfigStr(Data.ADconfigPath, "add", "AD_Channel_" + i.ToString(), "name");
                dr["测量值"] = 0;
                dt_AD.Rows.Add(dr);
            }

        }
        /// <summary>
        /// 根据输入AD源码解析出实际AD值，解析成功返回true否则false
        /// </summary>
        /// <param name="ADList">包含AD数据的帧</param>
        /// <param name="dataRe_AD">解析后的double类型AD数据</param>
        /// <param name="md">校准参数</param>
        /// <returns></returns>
        public static bool Return_ADValue(ref List<byte> ADList, ref double[] dataRe_AD,double[] md)
        {


            int ADFrame = 132;//64channel=128bytes,+2head,+2end

            int _StartPos = ADList.IndexOf(0xB0);

            if (_StartPos >= 0 && ADList.Count >= (_StartPos + ADFrame))
            {
                if (ADList[_StartPos + 1] == 0xFA && ADList[_StartPos + ADFrame - 2] == 0xE0 && ADList[_StartPos + ADFrame - 1] == 0xF5)
                {
                    byte[] buf = new byte[128];
                    for (int t = 0; t < 128; t++)
                    {
                        buf[t] = ADList[_StartPos + 2 + t];
                    }
                    for (int k = 0; k < 63; k++)
                    {
                        int temp = (buf[2 * k] & 0x7f) * 256 + buf[2 * k + 1];
                        double t = (double)(5 * temp) / (double)32767;
                        t = t * 3 / md[k];
                        if ((buf[2 * k] & 0x80) == 0x80)
                            dataRe_AD[k] = -t;
                        else
                            dataRe_AD[k] = t;
                    }
                    lock (ADList)
                    {
                        ADList.RemoveRange(0, _StartPos + ADFrame);
                    }
                }
                return true;
            }
            else
            {
                if (ADList.Count >= 8192)
                {
                    lock (ADList)
                    {
                        ADList.Clear();
                    }
                }
                return false;
            }

        }

    }
}

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
        public static int ADNums = 12;
        public static void Init_Table()
        {
            dt_AD.Columns.Add("序号", typeof(Int32));
            dt_AD.Columns.Add("名称", typeof(String));
            dt_AD.Columns.Add("测量值", typeof(double));
            for (int i = 0; i < ADNums; i++)
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
            int ADFrame = 104;//48channel=96bytes,+4head,+4end
            int NChans = 48*2;//48channel=96bytes
            int _StartPos = ADList.IndexOf(0xB0);

            if (_StartPos >= 0 && ADList.Count >= (_StartPos + ADFrame))
            {
                if (ADList[_StartPos + 1] == 0xFA && ADList[_StartPos + ADFrame - 4] == 0xE0 && ADList[_StartPos + ADFrame - 3] == 0xF5)
                {
                    byte[] buf = new byte[NChans];
                    for (int t = 0; t < NChans; t++)
                    {
                        buf[t] = ADList[_StartPos + 4 + t];
                    }
                    for (int k = 0; k < ADNums; k++)
                    {
                        int temp= (buf[2 * k] & 0x7f) * 256 + buf[2 * k + 1];

                        double value = temp;

                        value = 10 * (value / 32767);


                        if ((buf[2 * k] & 0x80) == 0x80)
                            dataRe_AD[k] = value;
                        else
                            dataRe_AD[k] = -10+value;

                        dataRe_AD[k] = 5.1 / (10 / dataRe_AD[k] - 1);
                    }
                    lock (ADList)
                    {
                        ADList.RemoveRange(0, _StartPos + ADFrame);
                    }
                    return true;
                }
                else
                {
                    ADList.RemoveRange(0, _StartPos+1);
                    return false;
                }
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

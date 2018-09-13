using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CKServer
{
    class Func_DY
    {
        public static bool RunTag = false;
        public static List<byte> DYListBytes = new List<byte>(10 * 1024 * 1024);
        public static ReaderWriterLockSlim DYLock = new ReaderWriterLockSlim();

        public static bool LedOn = false;
        public static bool LedOff = false;
        public static double Real_Vvalue = 23;
        public static double Real_Avalue = 0;

        public static double Strict_Avalue = 0;//设置的限流值

        public static void start()
        {
            RunTag = true;
            new Thread(() => { DealWith_DYData(); }).Start();
        }

        public static void close()
        {
            RunTag = false;
        }
        public static void DealWith_DYData()
        {
            while(RunTag)
            {
                byte[] DYStatus = new byte[12];
                byte[] DYCmdRet = new byte[6];
                if (DYListBytes.Count()>=12)
                {                 
                    if(DYListBytes[0]==0xF5 && DYListBytes[1]==0x0C)//处理查询指令反馈数据
                    {
                        DYLock.EnterReadLock();
                        DYStatus = DYListBytes.Take(12).ToArray();
                        DYListBytes.RemoveRange(0, 12);
                        DYLock.ExitReadLock();

                        Real_Vvalue = (double)DYStatus[2] + (double)DYStatus[3] / (double)100;
                        Real_Avalue = (double)DYStatus[4] + (double)DYStatus[5] / (double)100;
                    }
                    else
                    {                   
                        DYLock.EnterReadLock();
                        DYCmdRet = DYListBytes.Take(6).ToArray();
                        DYListBytes.RemoveRange(0, 6);
                        DYLock.ExitReadLock();

                        if (DYCmdRet[0] == 0xF5 && DYCmdRet[1] == 0x1C)//电源开指令反馈
                        {
                            if (DYCmdRet[2] == 0x01)//指令有效
                            {
                                LedOn = true;
                            }
                            else//指令无效
                            {
                                LedOn = false;
                            }
                        }
                        if (DYCmdRet[0] == 0xF5 && DYCmdRet[1] == 0x2C)//电源关指令反馈
                        {
                            if (DYCmdRet[2] == 0x01)//指令有效
                            {
                                LedOff = true;
                            }
                            else//指令无效
                            {
                                LedOff = false;
                            }
                        }
                        if (DYCmdRet[0] == 0xF5 && DYCmdRet[1] == 0x3C)//电源设置电压指令反馈
                        {
                            double b1 = DYCmdRet[2];
                            double b2 = DYCmdRet[3];
                            double Vvalue = b1 + (b2 / 100);
                            MyLog.Info("设置电压值：" + Vvalue.ToString() + "成功！");
                        }
                        if (DYCmdRet[0] == 0xF5 && DYCmdRet[1] == 0x4C)//电源设置过流指令反馈
                        {
                            double Avalue = (double)DYCmdRet[2] + ((double)DYCmdRet[3] / (double)100);
                            MyLog.Info("设置电流值：" + Avalue.ToString() + "成功！");

                            Strict_Avalue = Avalue;
                        }
                    }                                                  

                }
                Thread.Sleep(100);

            }

        }

    }
}

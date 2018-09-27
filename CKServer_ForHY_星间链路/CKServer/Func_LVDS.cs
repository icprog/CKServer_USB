using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;

namespace CKServer
{
    class Func_LVDS
    {

        public static DataTable dt_LVDS_CP = new DataTable();
        public static byte[] ComPareBuf = new byte[896];//872+24

        public static Queue<byte> DataQueue1 = new Queue<byte>();   //处理FF01第1通道LVDS数据
        public static ReaderWriterLockSlim Lock1 = new ReaderWriterLockSlim();
        public static int ErrorNums1 = 0;
        public static int TotalNums1 = 0;
        public static double ErPert1 = 0;

        public static Queue<byte> DataQueue2 = new Queue<byte>();   //处理FF02数传第2通道LVDS数据
        public static ReaderWriterLockSlim Lock2 = new ReaderWriterLockSlim();
        public static int ErrorNums2 = 0;
        public static int TotalNums2 = 0;
        public static double ErPert2 = 0;

        public static DataTable dt_LVDS_Result = new DataTable();

        public static bool _StartCompare = false;
        public static void Init_Table()
        {
            for (int i = 0; i < 896; i++) ComPareBuf[i] = 0;

            dt_LVDS_CP.Columns.Add("名称", typeof(String));
            dt_LVDS_CP.Columns.Add("设定值", typeof(String));

            for (int i = 0; i < 9; i++)
            {
                DataRow dr = dt_LVDS_CP.NewRow();

                dr["名称"] = Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_Config_" + i.ToString(), "name");
                dr["设定值"] = Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_Config_" + i.ToString(), "value");
                dt_LVDS_CP.Rows.Add(dr);
            }

            dt_LVDS_Result.Columns.Add("名称", typeof(String));
            dt_LVDS_Result.Columns.Add("总字节数", typeof(int));
            dt_LVDS_Result.Columns.Add("错误字节", typeof(int));
            dt_LVDS_Result.Columns.Add("误码率", typeof(double));
            for (int i = 0; i < 2; i++)
            {
                DataRow dr = dt_LVDS_Result.NewRow();
                dr["名称"] = Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_RecvChan_" + i.ToString(), "name");
                dr["总字节数"] = 0;
                dr["错误字节"] = 0;
                dr["误码率"] = 0;
                dt_LVDS_Result.Rows.Add(dr);
            }

        }


        public static void StartComPare()
        {
            MyLog.Info("开启LVDS接收实时比对");
            _StartCompare = true;
            new Thread(() => { ComPareFunc(); }).Start();
        }


        public static void ComPareFunc()
        {
            ErrorNums1 = 0;
            TotalNums1 = 0;
            ErrorNums2 = 0;
            TotalNums2 = 0;
            ErPert1 = 0;
            ErPert2 = 0;

            while (_StartCompare)
            {
                if(DataQueue1.Count()>=896)
                {
                    try
                    {
                        Lock1.EnterReadLock();
                        byte[] CADU = new byte[896];
                        for (int i = 0; i < 896; i++) CADU[i] = DataQueue1.Dequeue();
                        Lock1.ExitReadLock();

                        TotalNums1 += 896;

                        for(int i=0;i<896;i++)
                        {
                            if(CADU[i]!=ComPareBuf[i])
                            {
                                ErrorNums1 += 1;                                
                            }
                        }

                        ErPert1 = (double)ErrorNums1 / (double)TotalNums1;

                    }
                    catch(Exception ex1)
                    {
                        MyLog.Error("Error From Func_LVDS ComPareFunc()_1:" + ex1.Message);
                    }
                }


                if (DataQueue2.Count() >= 896)
                {
                    try
                    {
                        Lock2.EnterReadLock();
                        byte[] CADU = new byte[896];
                        for (int i = 0; i < 896; i++) CADU[i] = DataQueue2.Dequeue();
                        Lock2.ExitReadLock();

                        TotalNums2 += 896;

                        for (int i = 0; i < 896; i++)
                        {
                            if (CADU[i] != ComPareBuf[i])
                            {
                                ErrorNums2 += 1;
                            }
                        }

                        ErPert2 = (double)ErrorNums2 / (double)TotalNums2;

                    }
                    catch (Exception ex2)
                    {
                        MyLog.Error("Error From Func_LVDS ComPareFunc()_2:" + ex2.Message);
                    }
                }
            }


        }



    }
}

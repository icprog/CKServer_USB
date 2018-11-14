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
        public static string vcid1 = "000000";
        public static int startFrameCt1 = 0;
        public static int endFramCt1 = 0;
        public static int FrameCtNums1 = 0;
        public static int ErrFramCtNums1 = 0;

        public static string Recvd_Orign_Str1 = null;
        public static string cpstr1 = "11111111111111100000000000000100";//FFFE
        public static bool findhead1 = false;


        public static Queue<byte> DataQueue2 = new Queue<byte>();   //处理FF02数传第2通道LVDS数据
        public static ReaderWriterLockSlim Lock2 = new ReaderWriterLockSlim();
        public static int ErrorNums2 = 0;
        public static int TotalNums2 = 0;
        public static double ErPert2 = 0;
        public static string vcid2 = "000000";
        public static int startFrameCt2 = 0;
        public static int endFramCt2 = 0;
        public static int FrameCtNums2 = 0;
        public static int ErrFramCtNums2 = 0;

        public static string Recvd_Orign_Str2 = null;
        public static string cpstr2 = "11111111111111100000000000000100";//FFFE
        public static bool findhead2 = false;

        public static DataTable dt_LVDS_Result = new DataTable();

        public static bool _StartCompare = false;
        public static void Init_Table()
        {
            for (int i = 0; i < 896; i++) ComPareBuf[i] = 0;

            dt_LVDS_CP.Columns.Add("名称", typeof(String));
            dt_LVDS_CP.Columns.Add("设定值", typeof(String));

            for (int i = 0; i < 8; i++)
            {
                DataRow dr = dt_LVDS_CP.NewRow();

                dr["名称"] = Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_Config_" + i.ToString(), "name");
                dr["设定值"] = Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_Config_" + i.ToString(), "value");
                dt_LVDS_CP.Rows.Add(dr);
            }

            dt_LVDS_Result.Columns.Add("名称", typeof(String));
            dt_LVDS_Result.Columns.Add("VCID", typeof(String));
            dt_LVDS_Result.Columns.Add("起始帧计数", typeof(int));
            dt_LVDS_Result.Columns.Add("实时帧计数", typeof(int));
            dt_LVDS_Result.Columns.Add("VCDU计数", typeof(int));
            dt_LVDS_Result.Columns.Add("错误帧数", typeof(int));
            dt_LVDS_Result.Columns.Add("总字节数", typeof(int));
            dt_LVDS_Result.Columns.Add("错误字节", typeof(int));
            dt_LVDS_Result.Columns.Add("误码率", typeof(double));
            for (int i = 0; i < 2; i++)
            {
                DataRow dr = dt_LVDS_Result.NewRow();
                dr["名称"] = Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_RecvChan_" + i.ToString(), "name");

                dr["VCID"] = "000000";
                dr["起始帧计数"] = 0;
                dr["实时帧计数"] = 0;
                dr["VCDU计数"] = 0;
                dr["错误帧数"] = 0;
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


        public static void CompareChan1(ref bool FirstInFlag1,ref int LastFrameCt1)
        {
            if (DataQueue1.Count() >= 896)
            {
                try
                {
                    Lock1.EnterReadLock();
                    byte[] CADU = new byte[896];
                    for (int i = 0; i < 896; i++) CADU[i] = DataQueue1.Dequeue();
                    Lock1.ExitReadLock();

                    vcid1 = Convert.ToString(CADU[5] & 0x3f, 2).PadLeft(6, '0');
                    FrameCtNums1 += 1;//计数+1

                    endFramCt1 = CADU[6] * 65536 + CADU[7] * 256 + CADU[8];

                    if (FirstInFlag1)
                    {
                        FirstInFlag1 = false;
                        startFrameCt1 = endFramCt1;

                    }
                    else
                    {
                        if (endFramCt1 - LastFrameCt1 != 1 && endFramCt1 != 0)
                        {
                            ErrFramCtNums1 += 1;
                        }
                    }
                    LastFrameCt1 = endFramCt1;

                    TotalNums1 += 896 * 8;//换算成bit数

                    //处理0--23一共24Bytes的帧头
                    for (int i = 0; i < 24; i++)
                    {
                        if (i != 6 && i != 7 && i != 8)
                        {
                            if (CADU[i] != ComPareBuf[i])//假设Byte不同，再进一步判断多少bit不同，若相同则无需判断bit
                            {
                                string cadu = Convert.ToString(CADU[i], 2).PadLeft(8, '0');
                                string cpbuf = Convert.ToString(ComPareBuf[i], 2).PadLeft(8, '0');
                                for (int j = 0; j < 8; j++)
                                {
                                    if (cadu[j] != cpbuf[j])
                                    {
                                        ErrorNums1 += 1;//增加一个bit计数
                                    }
                                }
                                //String loginfo = "出错位置----VCDU计数：" + endFramCt1.ToString() + "字节所在位置：" + i.ToString();
                                //SaveFile.Lock_asyn_1.EnterWriteLock();
                                //SaveFile.DataQueue_asyn_1.Enqueue(loginfo);
                                //SaveFile.Lock_asyn_1.ExitWriteLock();
                            }
                        }
                    }

                    //处理24--895共872Byte的内容
                    for(int i=24;i<896;i++)
                    {
                        Recvd_Orign_Str1 += Convert.ToString(CADU[i], 2).PadLeft(8, '0');
                    }

                    if(Recvd_Orign_Str1.Length>32767)
                    {
                        if (findhead1 != true)
                        {
                            int headplace = Recvd_Orign_Str1.IndexOf(cpstr1);
                            if (headplace < 0)
                            {// 如果未在此实例中找到该字符或字符串，则此方法返回 -1
                                MyLog.Error("未匹配到设置的比对帧中的任何一处");
                                findhead1 = false;
                            }
                            else
                            {
                                MyLog.Error("跳过"+headplace.ToString()+"bits找到了同步头FFFE0004");
                                Recvd_Orign_Str1.Substring(headplace);
                                findhead1 = true;
                            }
                        }
                        else
                        {
                            string RecvPNFrame = Recvd_Orign_Str1.Substring(0, 32767);//取出32767个bit对应的string来比较
                            Recvd_Orign_Str1 = Recvd_Orign_Str1.Substring(32767);//删除取出的数据
                            
                            //32767加个bit转换成byte再来算
                            for(int i=0;i<32767;i++)
                            {
                                if(RecvPNFrame[i]!=Data.PRS15STR[i])
                                {
                                    ErrorNums1 += 1;//增加一个bit计数
                                }
                            }
                        }                     
                    }

                    ErPert1 = (double)ErrorNums1 / (double)TotalNums1;
                }
                catch (Exception ex1)
                {
                    MyLog.Error(ex1.ToString());
                }
            }
        }

        public static void CompareChan2(ref bool FirstInFlag2, ref int LastFrameCt2)
        {
            if (DataQueue2.Count() >= 896)
            {
                try
                {
                    Lock2.EnterReadLock();
                    byte[] CADU = new byte[896];
                    for (int i = 0; i < 896; i++) CADU[i] = DataQueue2.Dequeue();
                    Lock2.ExitReadLock();

                    vcid2 = Convert.ToString(CADU[5] & 0x3f, 2).PadLeft(6, '0');
                    FrameCtNums2 += 1;//计数+1

                    endFramCt2 = CADU[6] * 65536 + CADU[7] * 256 + CADU[8];

                    if (FirstInFlag2)
                    {
                        FirstInFlag2 = false;
                        startFrameCt2 = endFramCt2;
                    }
                    else
                    {
                        if (endFramCt2 - LastFrameCt2 != 1 && endFramCt2 != 0)
                        {
                            ErrFramCtNums2 += 1;
                        }
                    }

                    LastFrameCt2 = endFramCt2;

                    TotalNums2 += 896 * 8;

                    for (int i = 0; i < 24; i++)
                    {
                        if (i != 6 && i != 7 && i != 8)
                        {
                            if (CADU[i] != ComPareBuf[i])
                            {
                                string cadu = Convert.ToString(CADU[i], 2).PadLeft(8, '0');
                                string cpbuf = Convert.ToString(ComPareBuf[i], 2).PadLeft(8, '0');
                                for (int j = 0; j < 8; j++)
                                {
                                    if (cadu[j] != cpbuf[j])
                                    {
                                        ErrorNums2 += 1;//增加一个bit计数
                                    }
                                }
                            }
                        }
                    }

                    //处理24--895共872Byte的内容
                    for (int i = 24; i < 896; i++)
                    {
                        Recvd_Orign_Str2 += Convert.ToString(CADU[i], 2).PadLeft(8, '0');
                    }

                    if (Recvd_Orign_Str2.Length > 32767)
                    {
                        if (findhead2 != true)
                        {
                            int headplace = Recvd_Orign_Str2.IndexOf(cpstr1);
                            if (headplace < 0)
                            {// 如果未在此实例中找到该字符或字符串，则此方法返回 -1
                                MyLog.Error("Chan2未匹配到设置的比对帧中的任何一处");
                                findhead2 = false;
                            }
                            else
                            {
                                MyLog.Error("Chan2跳过" + headplace.ToString() + "bits找到了同步头FFFE0004");
                                Recvd_Orign_Str2.Substring(headplace);
                                findhead2 = true;
                            }
                        }
                        else
                        {
                            string RecvPNFrame = Recvd_Orign_Str2.Substring(0, 32767);//取出32767个bit对应的string来比较
                            Recvd_Orign_Str2 = Recvd_Orign_Str2.Substring(32767);//删除取出的数据

                            //32767加个bit转换成byte再来算
                            for (int i = 0; i < 32767; i++)
                            {
                                if (RecvPNFrame[i] != Data.PRS15STR[i])
                                {
                                    ErrorNums2 += 1;//增加一个bit计数
                                }
                            }
                        }
                    }
                    ErPert2 = (double)ErrorNums2 / (double)TotalNums2;
                }
                catch (Exception ex2)
                {
                    MyLog.Error(ex2.ToString());
                }
            }
        }

            public static void ComPareFunc()
        {
            ErrorNums1 = 0;
            TotalNums1 = 0;
            ErrorNums2 = 0;
            TotalNums2 = 0;
            ErPert1 = 0;
            ErPert2 = 0;

            ErrFramCtNums1 = 0;
            ErrFramCtNums2 = 0;

            vcid1 = "000000";
            startFrameCt1 = 0;
            endFramCt1 = 0;
            FrameCtNums1 = 0;

            vcid2 = "000000";
            startFrameCt2 = 0;
            endFramCt2 = 0;
            FrameCtNums2 = 0;

            bool FirstInFlag1 = true;
            bool FirstInFlag2 = true;//获取第一帧的计数
            int LastFrameCt1 = 0;
            int LastFrameCt2 = 0;


            while (_StartCompare)
            {
                CompareChan1(ref FirstInFlag1,ref LastFrameCt1);
                CompareChan2(ref FirstInFlag2, ref LastFrameCt2);
              
            }


        }



    }
}

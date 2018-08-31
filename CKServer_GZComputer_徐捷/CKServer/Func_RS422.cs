using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CKServer
{
    class Func_RS422
    {
        public static byte Reg_8CH = 0x0;
        public static byte Reg_8DH = 0x0;

        public static bool ShowTB1 = true;//同步422第一通道单次发送的时候显示
        public static bool ShowTB2 = true;//同步422第二通道单次发送的时候显示

        public static TextBox textBox_Show422Reslt;

        public static DataTable dt_RS422_Send = new DataTable();
        public static int RS422Nums = 48;
        public static void Init_Table()
        {
            dt_RS422_Send.Columns.Add("序号", typeof(Int32));
            dt_RS422_Send.Columns.Add("选中发送", typeof(Boolean));
            dt_RS422_Send.Columns.Add("名称", typeof(String));
            dt_RS422_Send.Columns.Add("码本路径", typeof(String));
            dt_RS422_Send.Columns.Add("发送", typeof(String));
            for (int i = 0; i < RS422Nums; i++)
            {
                DataRow dr = dt_RS422_Send.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = Function.GetConfigStr(Data.RS422configPath, "add", "RS422_Channel_" + i.ToString(), "name");
                string check = Function.GetConfigStr(Data.RS422configPath, "add", "RS422_Channel_" + i.ToString(), "check");
                dr["选中发送"] = Convert.ToBoolean(check);
                dr["码本路径"] = Function.GetConfigStr(Data.RS422configPath, "add", "RS422_Channel_" + i.ToString(), "path");
                dr["发送"] = "发送";
                dt_RS422_Send.Rows.Add(dr);
            }
        }

        public static void SaveToFile(ref ReaderWriterLockSlim rwlock, ref Queue<byte[]> queue, byte[] bufsav, int ChanNo)
        {
            rwlock.EnterWriteLock();
            queue.Enqueue(bufsav);
            rwlock.ExitWriteLock();

            //string tempShow = "";
            //for (int i = 0; i < bufsav.Length; i++)
            //{
            //    tempShow += bufsav[i].ToString("x2") + " ";
            //}

            //if (ChanNo < 48)
            //{
            //    textBox_Show422Reslt.BeginInvoke(new Action(() =>
            //    {
            //        if (textBox_Show422Reslt.Lines.Count() > 50)
            //        {
            //            textBox_Show422Reslt.Clear();
            //        }
            //        textBox_Show422Reslt.AppendText(tempShow);
            //    }
            //    ));
            //}
            //if (ChanNo == 48 && ShowTB1)
            //{
            //    textBox_Show422Reslt.BeginInvoke(new Action(() =>
            //    {
            //        if (textBox_Show422Reslt.Lines.Count() > 50)
            //        {
            //            textBox_Show422Reslt.Clear();
            //        }
            //        textBox_Show422Reslt.AppendText(tempShow);
            //    }
            //    ));
            //}
            //if (ChanNo == 49 && ShowTB2)
            //{
            //    textBox_Show422Reslt.BeginInvoke(new Action(() =>
            //    {
            //        if (textBox_Show422Reslt.Lines.Count() > 50)
            //        {
            //            textBox_Show422Reslt.Clear();
            //        }
            //        textBox_Show422Reslt.AppendText(tempShow);
            //    }
            //    ));
            //}


        }


    }
}

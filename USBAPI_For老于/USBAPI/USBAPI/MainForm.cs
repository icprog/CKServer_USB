using CyUSB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace USBSpeedTest
{
    public partial class MainForm : Form
    {
        SaveFile FileThread = null;
        public byte[] TempStoreBuf = new byte[8192];
        public int TempStoreBufTag = 0;




        int ThisCount = 0;
        int LastCount = 0;

        public MainForm()
        {
            InitializeComponent();

            //启动日志
            MyLog.richTextBox1 = richTextBox1;
            MyLog.path = Program.GetStartupPath() + @"LogData\";
            MyLog.lines = 50;
            MyLog.start();


            // Create the list of USB devices attached to the CyUSB3.sys driver.
            USB.usbDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);

            //Assign event handlers for device attachment and device removal.
            USB.usbDevices.DeviceAttached += new EventHandler(UsbDevices_DeviceAttached);
            USB.usbDevices.DeviceRemoved += new EventHandler(UsbDevices_DeviceRemoved);

            USB.Init();
        }

        void UsbDevices_DeviceAttached(object sender, EventArgs e)
        {
            SetDevice(false);
        }

        /*Summary
        This is the event handler for device removal. This method resets the device count and searches for the device with VID-PID 04b4-1003
        */
        void UsbDevices_DeviceRemoved(object sender, EventArgs e)
        {
            USBEventArgs evt = (USBEventArgs)e;
            USBDevice RemovedDevice = evt.Device;

            string RemovedDeviceName = evt.FriendlyName;
            MyLog.Error(RemovedDeviceName + "板卡断开");

            int key = int.Parse(evt.ProductID.ToString("x4").Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            USB.MyDeviceList[key] = null;

        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            SetDevice(false);

            Data.dt_AD01.Columns.Add("序号", typeof(Int32));
            Data.dt_AD01.Columns.Add("名称", typeof(String));
            Data.dt_AD01.Columns.Add("测量值", typeof(double));
            for (int i = 0; i < 8; i++)
            {
                DataRow dr = Data.dt_AD01.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = "通道" + (i + 1).ToString();
                dr["测量值"] = 0;
                Data.dt_AD01.Rows.Add(dr);
            }


            Data.dt_AD02.Columns.Add("序号", typeof(Int32));
            Data.dt_AD02.Columns.Add("名称", typeof(String));
            Data.dt_AD02.Columns.Add("测量值", typeof(double));
            for (int i = 8; i < 16; i++)
            {
                DataRow dr = Data.dt_AD02.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = "通道" + (i + 1).ToString();
                dr["测量值"] = 0;
                Data.dt_AD02.Rows.Add(dr);
            }

            Data.dt_AD03.Columns.Add("序号", typeof(Int32));
            Data.dt_AD03.Columns.Add("名称", typeof(String));
            Data.dt_AD03.Columns.Add("测量值", typeof(double));
            for (int i = 16; i < 24; i++)
            {
                DataRow dr = Data.dt_AD03.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = "通道" + (i + 1).ToString();
                dr["测量值"] = 0;
                Data.dt_AD03.Rows.Add(dr);
            }

            Data.dt_AD04.Columns.Add("序号", typeof(Int32));
            Data.dt_AD04.Columns.Add("名称", typeof(String));
            Data.dt_AD04.Columns.Add("测量值", typeof(double));
            for (int i = 24; i < 32; i++)
            {
                DataRow dr = Data.dt_AD04.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = "通道" + (i + 1).ToString();
                dr["测量值"] = 0;
                Data.dt_AD04.Rows.Add(dr);
            }

            dataGridView1.DataSource = Data.dt_AD01;
            dataGridView1.AllowUserToAddRows = false;

            dataGridView2.DataSource = Data.dt_AD02;
            dataGridView2.AllowUserToAddRows = false;

            dataGridView3.DataSource = Data.dt_AD03;
            dataGridView3.AllowUserToAddRows = false;

            dataGridView4.DataSource = Data.dt_AD04;
            dataGridView4.AllowUserToAddRows = false;
        }

        /*Summary
Search the device with VID-PID 04b4-00F1 and if found, select the end point
*/
        private void SetDevice(bool bPreserveSelectedDevice)
        {
            int nDeviceList = USB.usbDevices.Count;
            for (int nCount = 0; nCount < nDeviceList; nCount++)
            {
                USBDevice fxDevice = USB.usbDevices[nCount];
                String strmsg;
                strmsg = "(0x" + fxDevice.VendorID.ToString("X4") + " - 0x" + fxDevice.ProductID.ToString("X4") + ") " + fxDevice.FriendlyName;

                int key = int.Parse(fxDevice.ProductID.ToString("x4").Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                if (USB.MyDeviceList[key] == null)
                {
                    USB.MyDeviceList[key] = (CyUSBDevice)fxDevice;

                    MyLog.Info(USB.MyDeviceList[key].FriendlyName + ConfigurationManager.AppSettings[USB.MyDeviceList[key].FriendlyName] + "连接");

                    Data.OnlyId = key;

                    USB.SendCMD(Data.OnlyId, 0x81, 0x7f);
                    Thread.Sleep(100);
                    USB.SendCMD(Data.OnlyId, 0x81, 0x00);
                    Thread.Sleep(100);
                    USB.SendCMD(Data.OnlyId, 0x82, 0x7f);
                    Thread.Sleep(100);
                    USB.SendCMD(Data.OnlyId, 0x82, 0x00);
                    Thread.Sleep(100);
                    USB.SendCMD(Data.OnlyId, 0x83, 0x7f);
                    Thread.Sleep(100);
                    USB.SendCMD(Data.OnlyId, 0x83, 0x00);
                    Thread.Sleep(100);
                    USB.SendCMD(Data.OnlyId, 0x84, 0x7f);
                    Thread.Sleep(100);
                    USB.SendCMD(Data.OnlyId, 0x84, 0x00);
                    Thread.Sleep(100);
                    USB.SendCMD(Data.OnlyId, 0x85, 0x7f);
                    Thread.Sleep(100);
                    USB.SendCMD(Data.OnlyId, 0x85, 0x00);
                    Thread.Sleep(100);

                }
            }

        }

        bool RecvTag = false;
        private void button1_Click(object sender, EventArgs e)
        {
            if (this.button1.Text == "开始读取")
            {
                this.button1.Text = "停止读取";

                if (USB.MyDeviceList[Data.OnlyId] != null)
                {

                    CyControlEndPoint CtrlEndPt = null;
                    CtrlEndPt = USB.MyDeviceList[Data.OnlyId].ControlEndPt;

                    if (CtrlEndPt != null)
                    {
                        USB.SendCMD(Data.OnlyId, 0x80, 0x01);
                        USB.SendCMD(Data.OnlyId, 0x80, 0x00); 

                        USB.MyDeviceList[Data.OnlyId].Reset();
                        Register.Byte80H = (byte)(Register.Byte80H | 0x04);
                        USB.SendCMD(Data.OnlyId, 0x80, Register.Byte80H);
                    }

                    USB.SendCMD(Data.OnlyId, 0x81, 0x00);
                    USB.SendCMD(Data.OnlyId, 0x82, 0x00);

                    FileThread = new SaveFile();
                    FileThread.FileInit();
                    FileThread.FileSaveStart();

                    MyLog.Info("开始读取");
                    RecvTag = true;

                    ThisCount = 0;
                    LastCount = 0;

                    new Thread(() => { RecvAllUSB(); }).Start();
                    new Thread(() => { DealWithADFun(); }).Start();

                }
                else
                {
                    MyLog.Error("设备未连接！");
                }
            }
            else
            {
                this.button1.Text = "开始读取";
                ThisCount = 0;
                LastCount = 0;
                RecvTag = false;
                Thread.Sleep(500);
                if (FileThread != null)
                    FileThread.FileClose();
            }
        }

        int Recv4KCounts = 0;
        private void RecvAllUSB()
        {
            CyUSBDevice MyDevice01 = USB.MyDeviceList[Data.OnlyId];

            TempStoreBufTag = 0;
            while (RecvTag)
            {
                if (MyDevice01.BulkInEndPt != null)
                {
                    byte[] buf = new byte[4096];
                    int buflen = 4096;

                    MyDevice01.BulkInEndPt.XferData(ref buf, ref buflen);

                    if (buflen > 0)
                    {
                        Trace.WriteLine("收到数据包长度为：" + buflen.ToString());
                        Array.Copy(buf, 0, TempStoreBuf, TempStoreBufTag, buflen);
                        TempStoreBufTag += buflen;

                        byte[] Svbuf = new byte[buflen];
                        Array.Copy(buf, Svbuf, buflen);

                        SaveFile.Lock_1.EnterWriteLock();
                        SaveFile.DataQueue_SC1.Enqueue(Svbuf);
                        SaveFile.Lock_1.ExitWriteLock();

                        while (TempStoreBufTag >= 4096)
                        {
                            if (TempStoreBuf[0] == 0xff && (0x0 <= TempStoreBuf[1]) && (TempStoreBuf[1] < 0x11))
                            {
                                DealWithLongFrame(ref TempStoreBuf, ref TempStoreBufTag);
                            }
                            else
                            {
                                MyLog.Error("收到异常帧！");
                                Trace.WriteLine("收到异常帧" + TempStoreBufTag.ToString());
                                Array.Clear(TempStoreBuf, 0, TempStoreBufTag);
                                TempStoreBufTag = 0;
                            }
                        }
                    }
                    else if (buflen == 0)
                    {
                        //   Trace.WriteLine("数传422机箱 收到0包-----0000000000");
                    }
                    else
                    {
                        Trace.WriteLine("收到buflen <0");
                    }


                }
            }


        }

        void DealWithLongFrame(ref byte[] TempBuf, ref int TempTag)
        {
            ThisCount = TempStoreBuf[2] * 256 + TempStoreBuf[3];
            if (LastCount != 0 && ThisCount != 0 && (ThisCount - LastCount != 1))
            {
                MyLog.Error("出现漏帧情况！！");
                Trace.WriteLine("出现漏帧情况:" + LastCount.ToString("x4") + "--" + ThisCount.ToString("x4"));
            }
            LastCount = ThisCount;

            byte[] buf_LongFrame = new byte[4096];
            Array.Copy(TempStoreBuf, 0, buf_LongFrame, 0, 4096);

            Array.Copy(TempStoreBuf, 4096, TempStoreBuf, 0, TempStoreBufTag - 4096);
            TempStoreBufTag -= 4096;


            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x01)
            {
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_3.EnterWriteLock();
                SaveFile.DataQueue_SC3.Enqueue(bufsav);
                SaveFile.Lock_3.ExitWriteLock();
            }
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x02)
            {
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_4.EnterWriteLock();
                SaveFile.DataQueue_SC4.Enqueue(bufsav);
                SaveFile.Lock_4.ExitWriteLock();
            }
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x03)
            {
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_5.EnterWriteLock();
                SaveFile.DataQueue_SC5.Enqueue(bufsav);
                SaveFile.Lock_5.ExitWriteLock();
            }
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x04)
            {
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_6.EnterWriteLock();
                SaveFile.DataQueue_SC6.Enqueue(bufsav);
                SaveFile.Lock_6.ExitWriteLock();
            }

            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x08)
            {
                //FF08为短帧通道
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_2.EnterWriteLock();
                SaveFile.DataQueue_SC2.Enqueue(bufsav);
                SaveFile.Lock_2.ExitWriteLock();

                for (int i = 0; i < 6; i++)
                {
                    if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x00)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_7.EnterWriteLock();
                        SaveFile.DataQueue_SC7.Enqueue(buf1D0x);
                        SaveFile.Lock_7.ExitWriteLock();
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x01)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_8.EnterWriteLock();
                        SaveFile.DataQueue_SC8.Enqueue(buf1D0x);
                        SaveFile.Lock_8.ExitWriteLock();
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x02)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_9.EnterWriteLock();
                        SaveFile.DataQueue_SC9.Enqueue(buf1D0x);
                        SaveFile.Lock_9.ExitWriteLock();
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x03)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_10.EnterWriteLock();
                        SaveFile.DataQueue_SC10.Enqueue(buf1D0x);
                        SaveFile.Lock_10.ExitWriteLock();
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x04)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_11.EnterWriteLock();
                        SaveFile.DataQueue_SC11.Enqueue(buf1D0x);
                        SaveFile.Lock_11.ExitWriteLock();
                        lock (Data.ADList01)
                        {
                            for (int j = 0; j < num; j++)
                                Data.ADList01.Add(buf1D0x[j]);
                        }
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x05)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_12.EnterWriteLock();
                        SaveFile.DataQueue_SC12.Enqueue(buf1D0x);
                        SaveFile.Lock_12.ExitWriteLock();
                        lock (Data.ADList02)
                        {
                            for (int j = 0; j < num; j++)
                                Data.ADList02.Add(buf1D0x[j]);
                        }
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x06)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_13.EnterWriteLock();
                        SaveFile.DataQueue_SC13.Enqueue(buf1D0x);
                        SaveFile.Lock_13.ExitWriteLock();
                        lock (Data.ADList03)
                        {
                            for (int j = 0; j < num; j++)
                                Data.ADList03.Add(buf1D0x[j]);
                        }

                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x07)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_14.EnterWriteLock();
                        SaveFile.DataQueue_SC14.Enqueue(buf1D0x);
                        SaveFile.Lock_14.ExitWriteLock();
                        lock (Data.ADList04)
                        {
                            for (int j = 0; j < num; j++)
                                Data.ADList04.Add(buf1D0x[j]);
                        }
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x0f)
                    {
                        //空闲帧
                    }
                    else
                    {
                        Trace.WriteLine("FF08通道出错!");
                    }
                }
            }
        }

        private void DealWithADFun()
        {
            while (RecvTag)
            {
                bool Tag1 = false;
                bool Tag2 = false;
                bool Tag3 = false;
                bool Tag4 = false;

                lock (Data.ADList01)
                {
                    if (Data.ADList01.Count > 16)
                    {
                        Tag1 = true;

                        byte[] buf = new byte[16];
                        for (int t = 0; t < 16; t++)
                        {
                            buf[t] = Data.ADList01[t];
                        }
                        for (int k = 0; k < 8; k++)
                        {
                            int temp = (buf[2 * k] & 0x7f) * 256 + buf[2 * k + 1];

                            if ((buf[2 * k] & 0x80) == 0x80)
                            {
                                temp = 0x8000 - temp;
                            }
                            double value = temp;
                            value = 10 * (value / 32767);
                            if ((buf[2 * k] & 0x80) == 0x80)
                                Data.daRe_AD01[k] = -value;
                            else
                                Data.daRe_AD01[k] = value;
                        }
                        Data.ADList01.RemoveRange(0, 16);
                    }
                    else
                    {
                        Tag1 = false;
                    }

                }


                lock (Data.ADList02)
                {

                    if (Data.ADList02.Count > 16)
                    {
                        Tag2 = true;
                        byte[] buf = new byte[16];
                        for (int t = 0; t < 16; t++)
                        {
                            buf[t] = Data.ADList02[t];
                        }
                        for (int k = 0; k < 8; k++)
                        {
                            int temp = (buf[2 * k] & 0x7f) * 256 + buf[2 * k + 1];

                            if ((buf[2 * k] & 0x80) == 0x80)
                            {
                                temp = 0x8000 - temp;
                            }

                            double value = temp;
                            value = 10 * (value / 32767);
                            if ((buf[2 * k] & 0x80) == 0x80)
                                Data.daRe_AD02[k] = -value;
                            else
                                Data.daRe_AD02[k] = value;
                        }
                        Data.ADList02.RemoveRange(0, 16);
                    }
                    else
                    {
                        Tag2 = false;
                    }
                }

                lock (Data.ADList03)
                {

                    if (Data.ADList03.Count > 16)
                    {
                        Tag3 = true;
                        byte[] buf = new byte[16];
                        for (int t = 0; t < 16; t++)
                        {
                            buf[t] = Data.ADList03[t];
                        }
                        for (int k = 0; k < 8; k++)
                        {
                            int temp = (buf[2 * k] & 0x7f) * 256 + buf[2 * k + 1];

                            if ((buf[2 * k] & 0x80) == 0x80)
                            {
                                temp = 0x8000 - temp;
                            }

                            double value = temp;
                            value = 10 * (value / 32767);
                            if ((buf[2 * k] & 0x80) == 0x80)
                                Data.daRe_AD03[k] = -value;
                            else
                                Data.daRe_AD03[k] = value;
                        }
                        Data.ADList03.RemoveRange(0, 16);
                    }
                    else
                    {
                        Tag3 = false;
                    }
                }

                lock (Data.ADList04)
                {

                    if (Data.ADList04.Count > 16)
                    {
                        Tag4 = true;
                        byte[] buf = new byte[16];
                        for (int t = 0; t < 16; t++)
                        {
                            buf[t] = Data.ADList04[t];
                        }
                        for (int k = 0; k < 8; k++)
                        {
                            int temp = (buf[2 * k] & 0x7f) * 256 + buf[2 * k + 1];

                            if ((buf[2 * k] & 0x80) == 0x80)
                            {
                                temp = 0x8000 - temp;
                            }

                            double value = temp;
                            value = 10 * (value / 32767);
                            if ((buf[2 * k] & 0x80) == 0x80)
                                Data.daRe_AD04[k] = -value;
                            else
                                Data.daRe_AD04[k] = value;
                        }
                        Data.ADList04.RemoveRange(0, 16);
                    }
                    else
                    {
                        Tag4 = false;
                    }
                }

                if (Tag1 == false && Tag2 == false && Tag3 == false && Tag4 == false)
                {
                    Thread.Sleep(500);
                }


            }
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            RecvTag = false;

            if (USB.usbDevices != null)
            {
                USB.usbDevices.DeviceRemoved -= UsbDevices_DeviceRemoved;
                USB.usbDevices.DeviceAttached -= UsbDevices_DeviceAttached;
                USB.usbDevices.Dispose();
            }


            Thread.Sleep(200);
            if (FileThread != null)
                FileThread.FileClose();

            this.Dispose();

        }


        private static byte[] StrToHexByte(string hexString)
        {

            hexString = hexString.Replace(" ", "").Replace("\r", "").Replace("\n", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";

            byte[] returnBytes = new byte[hexString.Length / 2];

            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;

        }

        private void button7_Click(object sender, EventArgs e)
        {
            USB.SendCMD(Data.OnlyId, 0x82, 0x01);
            Thread.Sleep(100);
            USB.SendCMD(Data.OnlyId, 0x82, 0x00);

            String Str_Content = textBox7.Text.Replace(" ", "");
            int lenth = (Str_Content.Length) / 2;
            if (lenth >= 0)
            {
                int AddToFour = lenth % 4;
                if (AddToFour != 0)
                {
                    for (int i = 0; i < (4 - AddToFour); i++) Str_Content += "00";
                }

                byte[] temp = StrToHexByte("1D00" + lenth.ToString("x4") + Str_Content + "C0DEC0DEC0DEC0DEC0DEC0DEC0DEC0DE");

                USB.SendData(Data.OnlyId, temp);
            }
            else
            {
                MyLog.Error("请至少输入4个Byte的数据");
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            this.richTextBox1.Clear();
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            double str_len = textBox7.Text.Length;
            double byte_len = str_len / 2;
            textBox10.Text = byte_len.ToString();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Data.AdFrmIsAlive)
            {
                for (int i = 0; i < 8; i++)
                {
                    Data.dt_AD01.Rows[i]["测量值"] = Data.daRe_AD01[i];
                    Data.dt_AD02.Rows[i]["测量值"] = Data.daRe_AD02[i];
                    Data.dt_AD03.Rows[i]["测量值"] = Data.daRe_AD03[i];
                    Data.dt_AD04.Rows[i]["测量值"] = Data.daRe_AD04[i];
                }
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (button3.Text == "AD采集开始")
            {
                USB.SendCMD(Data.OnlyId, 0x81, 0x01);
                button3.Text = "AD采集关闭";
                Data.AdFrmIsAlive = true;
            }
            else
            {
                USB.SendCMD(Data.OnlyId, 0x81, 0x00);
                button3.Text = "AD采集开始";
                Data.AdFrmIsAlive = false;
            }
        }
    }

}

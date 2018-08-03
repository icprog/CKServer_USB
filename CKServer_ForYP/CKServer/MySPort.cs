using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

/// <summary>
/// 485串口控制类
/// </summary>
namespace CKServer
{
    class MySPort
    {
        public static SerialPort Port1;//转台控制端口
        public static double Hangle = 0;//水平角度
        public static double Vangle = 0;//垂直角度

        public static bool ShowAngleTag = true;

        public static double AddOnAngle = 0;

        public static string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        public static int Open(string PortName, string Baudrate, string DataBits, string Stopbit, string ParityStr)
        {
            int ret = 0;
            try
            {
                Port1 = new SerialPort();
                Port1.BaudRate = Convert.ToInt32(Baudrate);
                Port1.PortName = PortName;
                Port1.DataBits = Convert.ToInt32(DataBits);

                switch (Stopbit)
                {
                    case "1":
                        Port1.StopBits = StopBits.One;
                        break;
                    case "1.5":
                        Port1.StopBits = StopBits.OnePointFive;
                        break;
                    case "2":
                        Port1.StopBits = StopBits.Two;
                        break;
                    default:
                        MyLog.Error("Error:停止位参数设置不正确");
                        break;
                }

                switch (ParityStr)
                {
                    case "无校验":
                        Port1.Parity = Parity.None;
                        break;
                    case "偶校验":
                        Port1.Parity = Parity.Even;
                        break;
                    case "奇校验":
                        Port1.Parity = Parity.Odd;
                        break;
                    default:
                        MyLog.Error("Error:校验位参数设置不正确");
                        break;
                }

                Port1.Open();
                MyLog.Info("串口打开成功");
                ShowAngleTag = true;
                if (Port1.IsOpen)
                {
                    Port1.DataReceived += Port1_DataReceived;
                    ret = 1;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                ret = 0;
            }

            return ret;


        }

        private static void Port1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // throw new NotImplementedException();

            //从串口收数
            byte[] byteRead = new byte[Port1.BytesToRead];
            lock(Port1)
            Port1.Read(byteRead, 0, byteRead.Length);

            string printstr = null;
            for (int i = 0; i < byteRead.Length; i++)
            {
                printstr += byteRead[i].ToString("x2");
            }
            Trace.WriteLine("收到回传:" + printstr);

            //解析数据
            if (byteRead.Length >= 7 && byteRead[0] == 0xff)
            {
                if (byteRead[2] == 0x00 && byteRead[3] == 0x59)//水平回传
                {
                    double angle = byteRead[4] * 256 + byteRead[5];
                    Hangle = angle / 100;

                }
                if (byteRead[2] == 0x00 && byteRead[3] == 0x5B)//垂直回传
                {
                    double angle = byteRead[4] * 256 + byteRead[5];
                    if (angle > 18000)
                    {
                        Vangle = (36000 - angle) / 100;
                    }
                    else
                    {
                        Vangle = -(angle / 100);
                    }
                }
            }
        }

        public static void close()
        {
            if (Port1 != null)
            {
                Port1.Close();
                Port1 = null;
                MyLog.Info("串口关闭成功");
            }
            else
            {
                MyLog.Info("串口不存在或未打开");
            }
        }

        /// <summary>
        /// 设置水平绝对方位角
        /// </summary>
        /// <param name="CompassAgree"></param>
        public static void SetPanPosition(double CompassAgree)
        {
            CompassAgree = CompassAgree + AddOnAngle;
            if (CompassAgree >= 360) CompassAgree = CompassAgree - 360;
            byte[] SendData = new byte[7];
            short compassDeg = 0;
            if (CompassAgree >= 0)
            {
                compassDeg = (short)((CompassAgree) * 100);
            }
            else
            {
                compassDeg = (short)(36000 - (short)((-CompassAgree) * 100));
            }
            try
            {
                byte[] temp = BitConverter.GetBytes(compassDeg);
                SendData[0] = 0xff;
                SendData[1] = 0x01;
                SendData[2] = 0x00;
                SendData[3] = 0x4B;
                SendData[4] = temp[1];
                SendData[5] = temp[0];
                SendData[6] = (byte)(SendData[1] + SendData[2] + SendData[3] + SendData[4] + SendData[5]);

                WritePort(SendData);
            }
            catch (Exception ex)
            {
                MyLog.Error(ex.Message);
            }
        }

        /// <summary>
        /// 设置俯仰角
        /// </summary>
        /// <param name="InstantAgree"></param>
        public static void SetTiltPosition(double InstantAgree)
        {
            byte[] SendData = new byte[7];
            short InstantDeg = 0;
            if (InstantAgree >= 0)
            {
                if (InstantAgree > 40) InstantAgree = 40;

                InstantDeg = (short)(36000 - (short)(InstantAgree * 100));
            }
            else
            {
                if (InstantAgree < -75) InstantAgree = -75;
                InstantDeg = (short)((-InstantAgree) * 100);
            }

            byte[] temp = BitConverter.GetBytes(InstantDeg);
            SendData[0] = 0xff;
            SendData[1] = 0x01;
            SendData[2] = 0x00;
            SendData[3] = 0x4D;
            SendData[4] = temp[1];
            SendData[5] = temp[0];
            SendData[6] = (byte)(SendData[1] + SendData[2] + SendData[3] + SendData[4] + SendData[5]);

            WritePort(SendData);
        }

        /// <summary>
        /// 设置巡航
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="speed"></param>
        public static void SetCruise(string direction,byte speed)
        {
            byte[] SendData = new byte[7];
            short compassDeg = 0;

            try
            {
                SendData[0] = 0xff;
                SendData[1] = 0x01;
                SendData[2] = 0x00;
                if (direction == "左") SendData[3] = 0x02;
                else if(direction == "右")
                {
                    SendData[3] = 0x04;
                }
                else
                {
                    SendData[3] = 0x00;
                }

                SendData[4] = speed;
                SendData[5] =00;
                SendData[6] = (byte)(SendData[1] + SendData[2] + SendData[3] + SendData[4] + SendData[5]);

                WritePort(SendData);

            }
            catch (Exception ex)
            {
                MyLog.Error(ex.Message);
            }
        }

        public static void WritePort(byte[] SendData)
        {
            if (Port1.IsOpen)
            {
                lock(Port1)
                Port1.Write(SendData, 0, 7);
            }else
            {
                MyLog.Error("端口未打开，无法发送指令");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace CKServer
{
    class MySPort2
    {
        public static SerialPort Port1;//422通信端口1
        public static SerialPort Port2;//422通信端口2

        public static string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        public static int Open(SerialPort Port,string PortName, string Baudrate, string DataBits, string Stopbit, string ParityStr)
        {
            int ret = 0;
            try
            {
                Port = new SerialPort();
                Port.BaudRate = Convert.ToInt32(Baudrate);
                Port.PortName = PortName;
                Port.DataBits = Convert.ToInt32(DataBits);

                switch (Stopbit)
                {
                    case "1":
                        Port.StopBits = StopBits.One;
                        break;
                    case "1.5":
                        Port.StopBits = StopBits.OnePointFive;
                        break;
                    case "2":
                        Port.StopBits = StopBits.Two;
                        break;
                    default:
                        MyLog.Error("Error:停止位参数设置不正确");
                        break;
                }

                switch (ParityStr)
                {
                    case "无校验":
                        Port.Parity = Parity.None;
                        break;
                    case "偶校验":
                        Port.Parity = Parity.Even;
                        break;
                    case "奇校验":
                        Port.Parity = Parity.Odd;
                        break;
                    default:
                        MyLog.Error("Error:校验位参数设置不正确");
                        break;
                }

                Port.Open();
                MyLog.Info("串口打开成功");

                if (Port.IsOpen)////////////////////////////////////////////此处有疑问，会重复添加Event
                {
                    Port1.DataReceived += Port1_DataReceived;
                    Port2.DataReceived += Port2_DataReceived;
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

        private static void Port2_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //  throw new NotImplementedException();
            //从串口收数
            byte[] byteRead = new byte[Port2.BytesToRead];
            Port2.Read(byteRead, 0, byteRead.Length);

            string printstr = null;
            for (int i = 0; i < byteRead.Length; i++)
            {
                printstr += byteRead[i].ToString("x2");
            }
            Trace.WriteLine("收到Port2回传:" + printstr);

            //解析数据
        }

        private static void Port1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // throw new NotImplementedException();

            //从串口收数
            byte[] byteRead = new byte[Port1.BytesToRead];
            Port1.Read(byteRead, 0, byteRead.Length);

            string printstr = null;
            for (int i = 0; i < byteRead.Length; i++)
            {
                printstr += byteRead[i].ToString("x2");
            }
            Trace.WriteLine("收到Port1回传:" + printstr);

            //解析数据
            
        }

        public static void close(SerialPort Port)
        {
            if (Port != null)
            {
                Port.Close();
                Port = null;
                MyLog.Info("串口关闭成功");
            }
            else
            {
                MyLog.Info("串口不存在或未打开");
            }
        }


        public static void WritePort(SerialPort Port,byte[] SendData)
        {
            if (Port.IsOpen)
            {
                Port.Write(SendData, 0, 7);
            }
        }
    }
}

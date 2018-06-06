using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CKServer
{
    public partial class LoadFPGAForm : Form
    {
        public LoadFPGAForm()
        {
            InitializeComponent();
        }

        private void LoadFPGAForm_Load(object sender, EventArgs e)
        {
            buttonEdit1.Text = ConfigurationManager.AppSettings["PATH_FPGA_01"];
            buttonEdit2.Text = ConfigurationManager.AppSettings["PATH_FPGA_02"];
            buttonEdit3.Text = ConfigurationManager.AppSettings["PATH_FPGA_03"];
            buttonEdit4.Text = ConfigurationManager.AppSettings["PATH_FPGA_04"];
        }

        private void buttonEdit1_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            ButtonEdit editor = (ButtonEdit)sender;
            EditorButton Button = e.Button;
            String msgstr = "未选择FPGA";
            String ConfigPath = "PATH_FPGA_01";
            byte FrameHeadLastByte = 0xF1;
            switch (editor.Name)
            {
                case "buttonEdit1":
                    msgstr = "FPGA1";
                    ConfigPath = "PATH_FPGA_01";
                    FrameHeadLastByte = 0xF1;
                    break;
                case "buttonEdit2":
                    msgstr = "FPGA2";
                    ConfigPath = "PATH_FPGA_02";
                    FrameHeadLastByte = 0xF2;
                    break;
                case "buttonEdit3":
                    msgstr = "FPGA3";
                    ConfigPath = "PATH_FPGA_03";
                    FrameHeadLastByte = 0xF3;
                    break;
                case "buttonEdit4":
                    msgstr = "FPGA4";
                    ConfigPath = "PATH_FPGA_04";
                    FrameHeadLastByte = 0xF4;
                    break;
                default:
                    msgstr = "FPGA1";
                    ConfigPath = "PATH_FPGA_01";
                    FrameHeadLastByte = 0xF1;
                    break;
            }

            if (Button.Caption == "SelectBin")
            {
                openFileDialog1.InitialDirectory = Data.Path;
                string tmpFilter = openFileDialog1.Filter;
                string title = openFileDialog1.Title;
                openFileDialog1.Title = "选择要下载的FPGA文件";
                openFileDialog1.Filter = "bin files (*.bin)|*.bin|All files (*.*) | *.*";

                if (openFileDialog1.ShowDialog() == DialogResult.OK) //selecting bitstream
                {
                    editor.Text = openFileDialog1.FileName;
                    Refresh();
                    MyLog.Info("选取bin"+msgstr+"文件成功");
                    Function.SetConfigValue(ConfigPath, editor.Text);
                }
                else
                {
                    openFileDialog1.Filter = tmpFilter;
                    openFileDialog1.Title = title;
                    return;
                }
            }
            else
            {
                FileStream file = new FileStream(editor.Text, FileMode.Open, FileAccess.Read);

                int FPGABIN_filebytes = (int)file.Length + 8;//为何要+8??
                byte[] FPGABIN_filebuf =new byte[FPGABIN_filebytes];
                for (int i = 0; i < FPGABIN_filebytes; i++) FPGABIN_filebuf[i] = 0xff;
                file.Read(FPGABIN_filebuf, 0, FPGABIN_filebytes);

                //CF90CF90CF901DF1 + 数据 + (填写FF填满32位) + FFFFCF90 + N * CF90CF90
                byte[] FinalSendBytes = new byte[FPGABIN_filebytes + 40+4096];
                byte[] head = new byte[8] { 0xCF, 0x90, 0xCF, 0x90, 0xCF, 0x90, 0x1D, FrameHeadLastByte };
                byte[] end = new byte[32] { 0xCF, 0x90, 0xCF, 0x90, 0xCF, 0x90, 0xCF, 0x90, 0xCF, 0x90, 0xCF, 0x90, 0xCF, 0x90, 0xCF, 0x90, 0xCF, 0x90, 0xCF, 0x90, 0xCF, 0x90, 0xCF, 0x90, 0xCF, 0x90, 0xCF, 0x90, 0xCF, 0x90, 0xCF, 0x90, };
                byte[] addon = new byte[4096];
                for (int i = 0; i < 4096; i++) addon[i] = 0xff;

                head.CopyTo(FinalSendBytes, 0);
                FPGABIN_filebuf.CopyTo(FinalSendBytes, 8);
                end.CopyTo(FinalSendBytes, FPGABIN_filebytes + 8);
                addon.CopyTo(FinalSendBytes, FPGABIN_filebytes + 40);

                file.Close();

                if (USB.MyDeviceList[Data.Cardid] != null)
                {
                    USB.MyDeviceList[Data.Cardid].Reset();
                    USB.SendCMD(Data.Cardid, 0x80, 0x01);
                    Thread.Sleep(20);
                    USB.SendCMD(Data.Cardid, 0x80, 0x00);

                    int Send_Times = FinalSendBytes.Length / 16384;
                    int Last_SendNums = FinalSendBytes.Length % 16384;

                    for (int i = 0; i < Send_Times; i++)
                    {
                        byte[] SendBytes = new byte[16384];
                        Array.Copy(FinalSendBytes, i * 16384, SendBytes, 0, 16384);
                        USB.SendData(Data.Cardid, SendBytes);
                        Trace.WriteLine("Freq:" + ((100 * i) / Send_Times).ToString() + "%");
                        progressBarControl1.Position = (100 * i) / Send_Times;
                        progressBarControl1.Update();
                    }
                    if (Last_SendNums > 0)
                    {
                        byte[] Laset_SendBuf = new byte[Last_SendNums];
                        Array.Copy(FinalSendBytes, Send_Times * 16384, Laset_SendBuf, 0, Last_SendNums);
                        USB.SendData(Data.Cardid, Laset_SendBuf);
                        progressBarControl1.Increment(100);
                        MyLog.Info(msgstr + ".bin文件成功");
                    }

                    USB.SendCMD(Data.Cardid, 0x81, 0x00);
                    USB.SendCMD(Data.Cardid, 0x81, 0x0F);

                }


            }
        }

        private void buttonEdit2_ButtonClick(object sender, ButtonPressedEventArgs e)
        {

        }
    }
}

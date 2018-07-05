using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CKServer
{
    public partial class RS422FrameProduceForm : Form
    {
        public MainForm mform;
        public RS422FrameProduceForm(CKServer.MainForm parent)
        {
            InitializeComponent();
            mform = parent;

            dataGridView1.Rows.Add(1);
            dataGridView1.AllowUserToAddRows = false;

            dataGridView2.Rows.Add(1);
            dataGridView2.AllowUserToAddRows = false;


        }

        private void RS422FrameProduceForm_Load(object sender, EventArgs e)
        {
            this.dataGridView1.Rows[0].Cells[1].Value = "02";

            dataGridView2.Rows[0].Cells[0].Value = "000";
            dataGridView2.Rows[0].Cells[1].Value = "1";
            dataGridView2.Rows[0].Cells[2].Value = "0-无副导头";
            dataGridView2.Rows[0].Cells[3].Value = "63C";
            dataGridView2.Rows[0].Cells[4].Value = "11-独立包";
            dataGridView2.Rows[0].Cells[5].Value = "00";
            dataGridView2.Rows[0].Cells[6].Value = "00";
            dataGridView2.Rows[0].Cells[7].Value = "00";
            dataGridView2.Rows[0].Cells[8].Value = "00";


            dataGridView3.Rows.Add();
            dataGridView3.Rows[0].Cells[0].Value = "0000";
            dataGridView3.AllowUserToAddRows = false;
        }

        String ZhenStr;
        private void button1_Click(object sender, EventArgs e)
        {
            int temp = 0;
            if (this.textBox9.Text != null)
            {
                string str = this.textBox9.Text.Replace(" ", "");
                temp = str.Length;
            }

            if (temp < 4 || temp % 4 != 0 || temp > 243 * 2)
            {
                DialogResult dr = MessageBox.Show("请输入偶数个有效数据，数据区必须不超过242字节", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                String DataStr = this.textBox9.Text.Replace(" ", "");

                //GuoChengTag = 11bits应用过程标识符
                int temp1 = Convert.ToInt32(dataGridView2.Rows[0].Cells[3].FormattedValue.ToString(), 16);
                String GuoChengTag = Convert.ToString(temp1, 2).PadLeft(11, '0');
                GuoChengTag = GuoChengTag.Substring(GuoChengTag.Length - 11);//防止超出11bit，取最低11bit

                //XuLieCount = 14bits包序列计数
                int temp2 = int.Parse(dataGridView2.Rows[0].Cells[5].FormattedValue.ToString());
                String XuLieCount = Convert.ToString(int.Parse(dataGridView2.Rows[0].Cells[5].FormattedValue.ToString()), 2).PadLeft(14, '0');

                //包长
                int tempbao = int.Parse(dataGridView2.Rows[0].Cells[6].FormattedValue.ToString());
                string baoLen = tempbao.ToString("x4");

                //包长
                int tempfdt = int.Parse(dataGridView2.Rows[0].Cells[7].FormattedValue.ToString());
                string fudaot = tempfdt.ToString("x4");

                String StrBin = dataGridView2.Rows[0].Cells[0].FormattedValue.ToString() +
               dataGridView2.Rows[0].Cells[1].FormattedValue.ToString() +
               dataGridView2.Rows[0].Cells[2].FormattedValue.ToString()[0] +
               GuoChengTag +
               dataGridView2.Rows[0].Cells[4].FormattedValue.ToString().Substring(0, 2) +
               XuLieCount;// +            
                          //dataGridView2.Rows[0].Cells[6].FormattedValue.ToString();

                String temp3 = string.Format("{0:X}", Convert.ToInt32(StrBin, 2)).PadLeft(8, '0');

                String BagStr = temp3                                                   //包识别+包顺序控制
                                                                                        //+ dataGridView2.Rows[0].Cells[6].FormattedValue.ToString()          //包长
               + baoLen + fudaot
               + DataStr                                                           //数据域
               + dataGridView3.Rows[0].Cells[0].FormattedValue.ToString();         //和校验
                             

                //起始字+遥测包
                ZhenStr = dataGridView1.Rows[0].Cells[0].FormattedValue.ToString() 
                    + dataGridView1.Rows[0].Cells[1].FormattedValue.ToString()
                    + (int.Parse(dataGridView1.Rows[0].Cells[2].FormattedValue.ToString())).ToString("x2")
                    + BagStr;
            }

            switch (mform.Rs422_Channel_Name)
            {
                case "textBox_Send422_A":
                    mform.textBox_Send422_A.Text = ZhenStr;
                    break;
                default:
                    break;

            }

            this.Close();
        }

        int DataCount = 0;
        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            int crc = 0;
            String DataStr = this.textBox9.Text.Replace(" ", "");
            int count = DataStr.Length / 4;
            if (DataStr.Length % 4 == 0)
            {
                DataCount = DataStr.Length / 2;
                //this.dataGridView2.Rows[0].Cells[6].Value = (DataCount - 1).ToString("x4");
                this.dataGridView2.Rows[0].Cells[6].Value = DataCount + 3;

                this.dataGridView1.Rows[0].Cells[2].Value = DataCount + 10;

                for (int m = 0; m < DataStr.Length / 4; m++)
                {
                    int temp = Convert.ToInt32(DataStr.Substring(m * 4, 4), 16);
                    crc ^= temp;
                }
                this.dataGridView3.Rows[0].Cells[0].Value = crc.ToString("x4");

            }
            else
            {

            }
        }

        private void RS422FrameProduceForm_Paint(object sender, PaintEventArgs e)
        {
            Console.WriteLine("here is MainForm_Paint!!!");
            Pen mypen = new Pen(Color.Black);
            mypen.Width = 1;
            mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

            e.Graphics.DrawLine(mypen, label5.Location.X, label5.Location.Y, dataGridView2.Location.X, dataGridView2.Location.Y);
            e.Graphics.DrawLine(mypen, label5.Location.X + label5.Width, label5.Location.Y, dataGridView2.Location.X + dataGridView2.Width, dataGridView2.Location.Y);

            e.Graphics.DrawLine(mypen, label6.Location.X, label6.Location.Y, textBox9.Location.X, textBox9.Location.Y);
            e.Graphics.DrawLine(mypen, label6.Location.X + label6.Width, label6.Location.Y, dataGridView3.Location.X + dataGridView3.Width, dataGridView3.Location.Y);

        }
    }
}

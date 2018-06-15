using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CKServer
{
    public partial class DAmodify : Form
    {
        public DAmodify()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String Path = Program.GetStartupPath() + @"参数修正码本\";
            openFileDialog1.InitialDirectory = Path;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                MyLog.Info("载入DA修正码本成功！");

                string[] content = File.ReadAllLines(openFileDialog1.FileName);
                string[] temp = new string[3];

                if (content.Length >= 128)
                {
                    for (int i = 0; i < 128; i++)
                    {
                        temp = content[i].Split(',');
                        //    Data.value_a[i] = double.Parse(temp[1].Trim());
                        //    Data.value_b[i] = double.Parse(temp[2].Trim());

                        dataGridView1.Rows[i].Cells[1].Value = double.Parse(temp[1].Trim());
                        dataGridView1.Rows[i].Cells[2].Value = double.Parse(temp[2].Trim());
                        Data.SaveConfig(Data.DAconfigPath, "DAModifyA" + (i).ToString(), dataGridView1.Rows[i].Cells[1].FormattedValue.ToString());
                        Data.SaveConfig(Data.DAconfigPath, "DAModifyB" + (i).ToString(), dataGridView1.Rows[i].Cells[2].FormattedValue.ToString());

                    }

                    //for (int i = 0; i < 128; i++)
                    //{
                    //    SetConfigValue("DAModifyA" + (i).ToString(), dataGridView_M1.Rows[i].Cells[1].FormattedValue.ToString());
                    //    SetConfigValue("DAModifyB" + (i).ToString(), dataGridView_M1.Rows[i].Cells[2].FormattedValue.ToString());
                    //}
                }
            }
        }
    }
}

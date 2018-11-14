using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CKServer
{
    class TestUnit
    {
        public static void test1()
        {

            string firstRead = File.ReadAllText(Program.GetStartupPath() + @"PRBS-15.txt");
            int firstLen = firstRead.Count();

            string transtr = firstRead.Replace(",", "").Trim(); ;
            int transLen = transtr.Count();

            string fullstr = transtr + transtr + transtr + transtr + transtr + transtr + transtr + transtr;
            int fullLen = fullstr.Count();

            byte[] temp = new byte[32767];
            for (int i = 0; i < 32767; i++)
            {
                temp[i] = Convert.ToByte(fullstr.Substring(0, 8), 2);
                fullstr = fullstr.Substring(8);
            }

            byte[] FinalByte = temp;

            string filename = Program.GetStartupPath() + "Final1.dat";
            FileStream file = new FileStream(filename, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(file);

            bw.Write(FinalByte);
            bw.Flush();

            bw.Close();

        }
    }
}

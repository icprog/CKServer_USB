using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CKServer
{
    class CKCommon
    {
        /// <summary>
        /// 修改AppSettings中配置
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="value">相应值</param>
        public static bool SetConfigValue(string key, string value)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings[key] != null)
                    config.AppSettings.Settings[key].Value = value;
                else
                    config.AppSettings.Settings.Add(key, value);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                return true;
            }
            catch
            {
                return false;
            }
        }


        public static void clcDAValue(ref byte[] DASend, ref int[] DAValue,int DACardid)
        {
            double[] value_a;
            double[] value_b;
            switch(DACardid)
            {
                case 1:
                    value_a = Data.DA1_value_a;
                    value_b = Data.DA1_value_b;
                    break;
                case 2:
                    value_a = Data.DA2_value_a;
                    value_b = Data.DA2_value_b;
                    break;
                default:
                    value_a = Data.DA1_value_a;
                    value_b = Data.DA1_value_b;
                    break;
            }

            byte[] DAByteA = new byte[128];//1D00 1D04
            byte[] DAByteB = new byte[128];//1D01 1D05
            byte[] DAByteC = new byte[128];//1D02 1D06
            byte[] DAByteD = new byte[128];//1D03 1D07
            for (int i = 0; i < 128; i++)
            {
                double SendValue = value_a[i] + (value_b[i] * DAValue[i]) / 5.00;
                Int16 temp = Convert.ToInt16(SendValue);

                if (i>=0 && i<32) {
                    DAByteA[0 + 4 * i] = 0x00;
                    DAByteA[1 + 4 * i] = (byte)(0x40 + (i / 4));
                    byte a = (byte)((temp & 0x3f00) >> 8);
                    byte b = (byte)(((i % 4) & 0x03) << 6);
                    DAByteA[2 + 4 * i] = (byte)(b + a);
                    DAByteA[3 + 4 * i] = (byte)(temp & 0xff);
                };
                if (i >= 32 && i < 64)
                {
                    DAByteB[0 + 4 * (i-32)] = 0x00;
                    DAByteB[1 + 4 * (i-32)] = (byte)(0x40 + ((i-32) / 4));
                    byte a2 = (byte)((temp & 0x3f00) >> 8);
                    byte b2 = (byte)((((i-32) % 4) & 0x03) << 6);
                    DAByteB[2 + 4 * (i-32)] = (byte)(b2 + a2);
                    DAByteB[3 + 4 * (i-32)] = (byte)(temp & 0xff);
                }
                if (i >= 64 && i < 96)
                {
                    DAByteC[0 + 4 * (i-64)] = 0x00;
                    DAByteC[1 + 4 * (i-64)] = (byte)(0x40 + ((i-64) / 4));
                    byte a3 = (byte)((temp & 0x3f00) >> 8);
                    byte b3 = (byte)((((i-64) % 4) & 0x03) << 6);
                    DAByteC[2 + 4 * (i-64)] = (byte)(b3 + a3);
                    DAByteC[3 + 4 * (i-64)] = (byte)(temp & 0xff);
                }
                if (i >= 96 && i < 128)
                {
                    DAByteD[0 + 4 * (i-96)] = 0x00;
                    DAByteD[1 + 4 * (i-96)] = (byte)(0x40 + ((i-96) / 4));
                    byte a4 = (byte)((temp & 0x3f00) >> 8);
                    byte b4 = (byte)((((i-96) % 4) & 0x03) << 6);
                    DAByteD[2 + 4 * (i-96)] = (byte)(b4 + a4);
                    DAByteD[3 + 4 * (i-96)] = (byte)(temp & 0xff);
                }
            }
            Array.Copy(DAByteA, 0, DASend, 0, 128);
            Array.Copy(DAByteB, 0, DASend, 128, 128);
            Array.Copy(DAByteC, 0, DASend, 256, 128);
            Array.Copy(DAByteD, 0, DASend, 384, 128);
        }
    }

    class DiskInfo
    {
        // 获取指定驱动器的空间总大小(单位为MB)  
        public static long GetDiskSpace(string str_HardDiskName)
        {
            long totalSize = new long();
            str_HardDiskName = str_HardDiskName + ":\\";
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == str_HardDiskName)
                {
                    totalSize = drive.TotalSize / (1024 * 1024);
                }
            }
            return totalSize;
        }

        // 获取指定驱动器的剩余空间总大小(单位为MB)  

        public static long GetFreeSpace(string str_HardDiskName)
        {
            long freeSpace = new long();
            str_HardDiskName = str_HardDiskName + ":\\";
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == str_HardDiskName)
                {
                    freeSpace = drive.TotalFreeSpace / (1024 * 1024);
                }
            }
            return freeSpace;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Configuration;

namespace CKServer
{
    class Data
    {
        public static int CountV = 0;
        public static int CountA = 0;

        public static int OnlyID = 0;

        public static string Path = null;                       //程序运行目录

        public static int Cardid = int.Parse(ConfigurationManager.AppSettings["CardID"]);

        public static int[] DA_Card1 = new int[128];      //根据用户输入转化为对应的int值
        public static int[] DA_Card2 = new int[128];      //根据用户输入转化为对应的int值

        public static byte[] DA_Send1 = new byte[512];      //根据DA_Card1的实际DA值转化为对应的码字
        public static byte[] DA_Send2 = new byte[512];      //根据DA_Card2的实际DA值转化为对应的码字

        public static double[] DA1_value_a = new double[128];                         //DA板卡1修正参数a
        public static double[] DA1_value_b = new double[128];                         //DA板卡1修正参数b

        public static double[] DA2_value_a = new double[128];                         //DA板卡2修正参数a
        public static double[] DA2_value_b = new double[128];                         //DA板卡2修正参数b

        //   public static object synObj_dq0 = new object();
        public static object synObj_sc1 = new object();


        public static Dictionary<string, Queue<byte[]>> Apid_EPDU_Dictionary = new Dictionary<string, Queue<byte[]>>();
        public static Dictionary<string, Queue<byte[]>> Apid_EPDU_Dictionary2 = new Dictionary<string, Queue<byte[]>>();
        public static Dictionary<string, Queue<byte[]>> Apid_EPDU_Dictionary3 = new Dictionary<string, Queue<byte[]>>();

        public static List<string> AlreadyOnApid = new List<string>();
        public static List<string> AlreadyOnApid2 = new List<string>();
        public static List<string> AlreadyOnApid3 = new List<string>();

        public static bool TBChan1Used = false;
        public static bool TBChan2Used = false;
        public static bool MOXAChanUsed = false;

        public static int SCRecvCounts = 0;


        public static string DAconfigPath = Program.GetStartupPath() + @"配置文件\DAconfig.xml";
        public static string ADconfigPath = Program.GetStartupPath() + @"配置文件\ADconfig.xml";
        public static string RconfigPath = Program.GetStartupPath() + @"配置文件\Rconfig.xml";
        public static string OCconfigPath = Program.GetStartupPath() + @"配置文件\OCconfig.xml";
        public static string RegconfigPath = Program.GetStartupPath() + @"配置文件\Regconfig.xml";
        public static string YCconfigPath = Program.GetStartupPath() + @"配置文件\YCconfig.xml";
        public static string LVDSconfigPath = Program.GetStartupPath() + @"配置文件\LVDSconfig.xml";
        public static void SaveConfig(string Path,string key ,string value)
        {
            XDocument xDoc = XDocument.Load(Path);
            XmlReader reader = xDoc.CreateReader();

            bool Matched = false;//是否已在XML中

            foreach (var p in xDoc.Root.Elements("add"))
            {
                if (p.Attribute("key").Value == key)
                {
                    p.Attribute("value").Value = value;
                    Matched = true;
                }
            }
            if(Matched == false)
            {
                XElement element = new XElement("add", new XAttribute("key", key), new XAttribute("value", value));
                xDoc.Root.Add(element);
            }

            xDoc.Save(Path);
            //var query = from p in xDoc.Root.Elements("add")
            //            where p.Attribute("key").Value == "DAModifyA1"
            //            orderby p.Value
            //            select p.Value;

            //foreach (string s in query)
            //{
            //    Console.WriteLine(s);
            //}

        }

        public static string GetConfig(string Path,string key)
        {
            XDocument xDoc = XDocument.Load(Path);
            XmlReader reader = xDoc.CreateReader();
            string value = "Error";

            var query = from p in xDoc.Root.Elements("add")
                        where p.Attribute("key").Value == key
                        select p.Attribute("value").Value;

            foreach(string s in query)
            {
                value = s;
            }

            //foreach (var p in xDoc.Root.Elements("add"))
            //{
            //    if (p.Attribute("key").Value == key)
            //    {
            //        value = p.Attribute("value").Value;
            //    }
            //}
            return value;

        }

     
        public static void SaveConfigStr(string Path, string key, string name ,string value)
        {
            XDocument xDoc = XDocument.Load(Path);
            XmlReader reader = xDoc.CreateReader();

            bool Matched = false;//是否已在XML中

            foreach (var p in xDoc.Root.Elements("add"))
            {
                if (p.Attribute("key").Value == key)
                {
                    p.Attribute(name).Value = value;
                    Matched = true;
                }
            }
            if (Matched == false)
            {
                XElement element = new XElement("add", new XAttribute("key", key), new XAttribute("name", value));
                xDoc.Root.Add(element);
            }

            xDoc.Save(Path);
            //var query = from p in xDoc.Root.Elements("add")
            //            where p.Attribute("key").Value == "DAModifyA1"
            //            orderby p.Value
            //            select p.Value;

            //foreach (string s in query)
            //{
            //    Console.WriteLine(s);
            //}

        }


        public static string PRS15STR = "1111111111111110000000000000010000000000000110000000000001010000000000011110000000000100010000000001100110000000010101010000000111111110000001000000010000011000000110000101000001010001111000011110010001000100010110011001100111010101010101001111111111111010000000000001110000000000010010000000000110110000000001011010000000011101110000000100110010000001101010110000010111111010000111000001110001001000010010011011000110110101101001011011110111011101100011001100110100101010101011101111111111100110000000000101010000000001111110000000010000010000000110000110000001010001010000011110011110000100010100010001100111100110010101000101010111111001111111000001010000001000011110000011000100010000101001100110001111010101010010001111111110110010000000011010110000000101111010000001110001110000010010010010000110110110110001011011011010011101101101110100110110110011101011011010100111101101111101000110110000111001011010001001011101110011011100110010101100101010111110101111111000011110000001000100010000011001100110000101010101010001111111111110010000000000010110000000000111010000000001001110000000011010010000000101110110000001110011010000010010101110000110111110010001011000010110011101000111010100111001001111101001011010000111011101110001001100110010011010101010110101111111111011110000000001100010000000010100110000000111101010000001000111110000011001000010000101011000110001111101001010010000111011110110001001100011010011010100101110101111101110011110000110010100010001010111100110011111000101010100001001111111100011010000000100101110000001101110010000010110010110000111010111010001001111001110011010001010010101110011110111110010100011000010111100101000111000101111001001001110001011011010010011101101110110100110110011011101011010101100111101111110101000110000011111001010000100001011110001100011100010010100100100110111101101101011000110110111101001011011000111011101101001001100110111011010101011001101111111101010110000000111111010000001000001110000011000010010000101000110110001111001011010010001011101110110011100110011010100101010101111101111111110000110000000010001010000000110011110000001010100010000011111100110000100000101010001100001111110010100010000010111100110000111000101010001001001111110011011010000010101101110000111110110010001000011010110011000101111010101001110001111111010010010000001110110110000010011011010000110101101110001011110110010011100011010110100100101111011101101110001100110110010010101011010110111111101111011000000110001101000001010010111000011110111001000100011001011001100101011101010101111100111111110000101000000010001111000000110010001000001010110011000011111010101000100001111111001100010000001010100110000011111101010000100000111110001100001000010010100011000110111100101001011000101111011101001110001100111010010010101001110110111111010011011000001110101101000010011110111000110100011001001011100101011011100101111101100101110000110101110010001011110010110011100010111010100100111001111101101001010000110111011110001011001100010011101010100110100111111101011101000000111100111000001000101001000011001111011000101010001101001111110010111010000010111001110000111001010010001001011110110011011100011010101100100101111110101101110000011110110010000100011010110001100101111010010101110001110111110010010011000010110110101000111011011111001001101100001011010110100011101111011100100110001100101101010010101110111110111110011000011000010101000101000111111001111001000001010001011000011110011101000100010100111001100111101001010101000111011111111001001100000001011010100000011101111100000100110000100001101010001100010111110010100111000010111101001000111000111011001001001001101011011011010111101101101111000110110110001001011011010011011101101110101100110110011110101011010100011111101111100100000110000101100001010001110100011110010011100100010110100101100111011101110101001100110011111010101010100001111111111100010000000000100110000000001101010000000010111110000000111000010000001001000110000011011001010000101101011110001110111100010010011000100110110101001101011011111010111101100001111000110100010001001011100110011011100101010101100101111111110101110000000011110010000000100010110000001100111010000010101001110000111111010010001000001110110011000010011010101000110101111111001011110000001011100010000011100100110000100101101010001101110111110010110011000010111010101000111001111111001001010000001011011110000011101100010000100110100110001101011101010010111100111110111000101000011001001111000101011010001001111101110011010000110010101110001010111110010011111000010110100001000111011100011001001100100101011010101101111101111110110000110000011010001010000101110011110001110010100010010010111100110110111000101011011001001111101101011010000110111101110001011000110010011101001010110100111011111011101001100001100111010100010101001111100111111010000101000001110001111000010010010001000110110110011001011011010101011101101111111100110110000000101011010000001111101110000010000110010000110001010110001010011111010011110100001110100011100010011100100100110100101101101011101110110111100110011011000101010101101001111111110111010000000011001110000000101010010000001111110110000010000011010000110000101110001010001110010011110010010110100010110111011100111011001100101001101010101111010111111110001111000000010010001000000110110011000001011010101000011101111111000100110000001001101010000011010111110000101111000010001110001000110010010011001010110110101011111011011111100001101100000100010110100001100111011100010101001100100111111010101101000001111110111000010000011001000110000101011001010001111101011110010000111100010110001000100111010011001101001110101010111010011111111001110100000001010011100000011110100100000100011101100001100100110100010101101011100111110111100101000011000101111000101001110001001111010010011010001110110101110010011011110010110101100010111011110100111001100011101001010100100111011111101101001100000110111010100001011001111100011101010000100100111110001101101000010010110111000110111011001001011001101011011101010111101100111111000110101000001001011111000011011100001000101100100011001110101100101010011110101111110100011110000011100100010000100101100110001101110101010010110011111110111010100000011001111100000101010000100001111110001100010000010010100110000110111101010001011000111110011101001000010100111011000111101001101001000111010111011001001111001101011010001010111101110011111000110010100001001010111100011011111000100101100001001101110100011010110011100101111010100101110001111101110010010000110010110110001010111011010011111001101110100001010110011100011111010100100100001111101101100010000110110100110001011011101010011101100111110100110101000011101011111000100111100001001101000100011010111001100101111001010101110001011111110010011100000010110100100000111011101100001001100110100011010101011100101111111100101110000000101110010000001110010110000010010111010000110111001110001011001010010011101011110110100111100011011101000100101100111001101110101001010110011111011111010100001100001111100010100010000100111100110001101000101010010111001111110111001010000011001011110000101011100010001111100100110010000101101010110001110111111010010011000001110110101000010011011111000110101100001001011110100011011100011100101100100100101110101101101110011110110110010100011011010111100101101111000101110110001001110011010011010010101110101110111110011110011000010100010101000111100111111001000101000001011001111000011101010001000100111110011001101000010101010111000111111111001001000000001011011000000011101101000000100110111000001101011001000010111101011000111000111101001001001000111011011011001001101101101011010110110111101111011011000110001101101001010010110111011110111011001100011001101010100101010111111101111111000000110000001000001010000011000011110000101000100010001111001100110010001010101010110011111111111010100000000001111100000000010000100000000110001100000001010010100000011110111100000100011000100001100101001100010101111010100111110001111101000010010000111000110110001001001011010011011011101110101101100110011110110101010100011011111111100101100000000101110100000001110011100000010010100100000110111101100001011000110100011101001011100100111011100101101001100101110111010101110011001111110010101010000010111111110000111000000010001001000000110011011000001010101101000011111110111000100000011001001100000101011010100001111101111100010000110000100110001010001101010011110010111110100010111000011100111001000100101001011001101111011101010110001100111111010010101000001110111111000010011000001000110101000011001011111000101011100001001111100100011010000101100101110001110101110010010011110010110110100010111011011100111001101100101001010110101111011111011110001100001100010010100010100110111100111101011000101000111101001111001000111010001011001001110011101011010010100111101110111101000110011000111001010101001001011111111011011100000001101100100000010110101100000111011110100001001100011100011010100100100101111101101101110000110110110010001011011010110011101101111010100110110001111101011010010000111101110110001000110011010011001010101110101011111110011111100000010100000100000111100001100001000100010100011001100111100101010101000101111111111001110000000001010010000000011110110000000100011010000001100101110000010101110010000111110010110001000010111010011000111001110101001001010011111011011110100001101100011100010110100100100111011101101101001100110110111010101011011001111111101101010000000110111110000001011000010000011101000110000100111001010001101001011110010111011100010111001100100111001010101101001011111110111011100000011001100100000101010101100001111111110100010000000011100110000000100101010000001101111110000010110000010000111010000110001001110001010011010010011110101110110100011110011011100100010101100101100111110101110101000011110011111000100010100001001100111100011010101000100101111111001101110000001010110010000011111010110000100001111010001100010001110010100110010010111101010110111000111111011001001000001101011011000010111101101000111000110111001001001011001011011011101011101101100111100110110101000101011011111001111101100001010000110100011110001011100100010011100101100110100101110101011101110011111100110010100000101010111100001111111000100010000001001100110000011010101010000101111111110001110000000010010010000000110110110000001011011010000011101101110000100110110010001101011010110010111101111010111000110001111001001010010001011011110110011101100011010100110100101111101011101110000111100110010001000101010110011001111111010101010000001111111110000010000000010000110000000110001010000001010011110000011110100010000100011100110001100100101010010101101111110111110110000011000011010000101000101110001111001110010010001010010110110011110111011010100011001101111100101010110000101111111010001110000001110010010000010010110110000110111011010001011001101110011101010110010100111111010111101000001111000111000010001001001000110011011011001010101101101011111110110111100000011011000100000101101001100001110111010100010011001111100110101010000101011111110001111100000010010000100000110110001100001011010010100011101110111100100110011000101101010101001110111111111010011000000001110101000000010011111000000110100001000001011100011000011100100101000100101101111001101110110001010110011010011111010101110100001111110011100010000010100100110000111101101010001000110111110011001011000010101011101000111111100111001000000101001011000001111011101000010001100111000110010101001001010111111011011111000001101100001000010110100011000111011100101001001100101111011010101110001101111110010010110000010110111010000111011001110001001101010010011010111110110101111000011011110001000101100010011001110100110101010011101011111110100111100000011101000100000100111001100001101001010100010111011111100111001100000101001010100001111011111100010001100000100110010100001101010111100010111111000100111000001001101001000011010111011000101111001101001110001010111010010011111001110110100001010011011100011110101100100100011110101101100100011110110101100100011011110101100101100011110101110100100011110011101100100010100110101100111101011110101000111100011111001000100100001011001101100011101010110100100111111011101101000001100110111000010101011001000111111101011001000000111101011000001000111101000011001000111000101011001001001111101011011010000111101101110001000110110010011001011010110101011101111011111100110001100000101010010100001111110111100010000011000100110000101001101010001111010111110010001111000010110010001000111010110011001001111010101011010001111111101110010000000110010110000001010111010000011111001110000100001010010001100011110110010100100011010111101100101111000110101110001001011110010011011100010110101100100111011110101101001100011110111010100100011001111101100101010000110101111110001011110000010011100010000110100100110001011101101010011100110111110100101011000011101111101000100110000111001101010001001010111110011011111000010101100001000111110100011001000011100101011000100101111101001101110000111010110010001001111010110011010001111010101110010001111110010110010000010111010110000111001111010001001010001110011011110010010101100010110111110100111011000011101001101000100111010111001101001111001010111010001011111001110011100001010010100100011110111101100100011000110101100101001011110101111011100011110001100100100010010101101100110111110110101011000011011111101000101100000111001110100001001010011100011011110100100101100011101101110100100110110011101101011010100110111101111101011000110000111101001010001000111011110011001001100010101011010100111111101111101000000110000111000001010001001000011110011011000100010101101001100111110111010101000011001111111000101010000001001111110000011010000010000101110000110001110010001010010010110011110110111010100011011001111100101101010000101110111110001110011000010010010101000110110111111001011011000001011101101000011100110111000100101011001001101111101011010110000111101111010001000110001110011001010010010101011110110111111100011011000000100101101000001101110111000010110011001000111010101011001001111111101011010000000111101110000001000110010000011001010110000101011111010001111100001110010000100010010110001100110111010010101011001110111111101010011000000111110101000001000011111000011000100001000101001100011001111010100101010001111101111110010000110000010110001010000111010011110001001110100010011010011100110101110100101011110011101111100010100110000100111101010001101000111110010111001000010111001011000111001011101001001011100111011011100101001101100101111010110101110001111011110010010001100010110110010100111011010111101001101111000111010110001001001111010011011010001110101101110010011110110010110100011010111011100101111001100101110001010101110010011111110010110100000010111011100000111001100100001001010101100011011111110100101100000011101110100000100110011100001101010100100010111111101100111000000110101001000001011111011000011100001101000100100010111001101100111001010110101001011111011111011100001100001100100010100010101100111100111110101000101000011111001111000100001010001001100011110011010100100010101111101100111110000110101000010001011111000110011100001001010100100011011111101100101100000110101110100001011110011100011100010100100100100111101101101101000110110110111001011011011001011101101101011100110110111100101011011000101111101101001110000110111010010001011001110110011101010011010100111110101111101000011110000111000100010001001001100110011011010101010101101111111111110110000000000011010000000000101110000000001110010000000010010110000000110111010000001011001110000011101010010000100111110110001101000011010010111000101110111001001110011001011010010101011101110111111100110011000000101010101000001111111111000010000000001000110000000011001010000000101011110000001111100010000010000100110000110001101010001010010111110011110111000010100011001000111100101011001000101111101011001110000111101010010001000111110110011001000011010101011000101111111101001110000000111010010000001001110110000011010011010000101110101110001110011110010010010100010110110111100111011011000101001101101001111010110111010001111011001110010001101010010110010111110111010111000011001111001000101010001011001111110011101010000010100111110000111101000010001000111000110011001001001010101011011011111111101101100000000110110100000001011011100000011101100100000100110101100001101011110100010111100011100111000100100101001001101101111011010110110001101111011010010110001101110111010010110011001110111010101010011001111111110101010000000011111110000000100000010000001100000110000010100001010000111100011110001000100100010011001101100110101010110101011111111011111100000001100000100000010100001100000111100010100001000100111100011001101000100101010111001101111111001010110000001011111010000011100001110000100100010010001101100110110010110101011010111011111101111001100000110001010100001010011111100011110100000100100011100001101100100100010110101101100111011110110101001100011011111010100101100001111101110100010000110011100110001010100101010011111101111110100000110000011100001010000100100011110001101100100010010110101100110111011110101011001100011111101010100100000111111101100001000000110100011000001011100101000011100101111000100101110001001101110010011010110010110101111010111011110001111001100010010001010100110110011111101011010100000111101111100001000110000100011001010001100101011110010101111100010111110000100111000010001101001000110010111011001010111001101011111001010111100001011111000100011100001001100100100011010101101100101111110110101110000011011110010000101100010110001110100111010010011101001110110100111010011011101001110101100111010011110101001110100011111010011100100001110100101100010011101110100110100110011101011101010100111100111111101000101000000111001111000001001010001000011011110011000101100010101001110100111111010011101000001110100111000010011101001000110100111011001011101001101011100111010111100101001111000101111010001001110001110011010010010010101110110110111110011011011000010101101101000111110110111001000011011001011000101101011101001110111100111010011000101001110101001111010011111010001110100001110010011100010010110100100110111011101101011001100110111101010101011000111111111101001000000000111011000000001001101000000011010111000000101111001000001110001011000010010011101000110110100111001011011101001011101100111011100110101001100101011111010101111100001111110000100010000010001100110000110010101010001010111111110011111000000010100001000000111100011000001000100101000011001101111000101010110001001111111010011010000001110101110000010011110010000110100010110001011100111010011100101001110100101111010011101110001110100110010010011101010110110100111111011011101000001101100111000010110101001000111011111011001001100001101011010100010111101111100111000110000101001001010001111011011110010001101100010110010110100111010111011101001111001100111010001010101001110011111111010010100000001110111100000010011000100000110101001100001011111010100011100001111100100100010000101101100110001110110101010010011011111110110101100000011011110100000101100011100001110100100100010011101101100110100110110101011101011011111100111101100000101000110100001111001011100010001011100100110011100101101010100101110111111101110011000000110010101000001010111111000011111000001000100001000011001100011000101010100101001111111101111010000000110001110000001010010010000011110110110000100011011010001100101101110010101110110010111110011010111000010101111001000111110001011001000010011101011000110100111101001011101000111011100111001001100101001011010101111011101111110001100110000010010101010000110111111110001011000000010011101000000110100111000001011101001000011100111011000100101001101001101111010111010110001111001111010010001010001110110011110010011010100010110101111100111011110000101001100010001111010100110010001111101010110010000111111010110001000001111010011000010001110101000110010011111001010110100001011111011100011100001100100100100010101101101100111110110110101000011011011111000101101100001001110110100011010011011100101110101100101110011110101110010100011110010111100100010111000101100111001001110101001011010011111011101110100001100110011100010101010100100111111111101101000000000110111000000001011001000000011101011000000100111101000001101000111000010111001001000111001011011001001011101101011011100110111101100101011000110101111101001011110000111011100010001001100100110011010101101010101111110111111110000011000000010000101000000110001111000001010010001000011110110011000100011010101001100101111111010101110000001111110010000010000010110000110000111010001010001001110011110011010010100010101110111100111110011000101000010101001111000111111010001001000001110011011000010010101101000110111110111001011000011001011101000101011100111001111100101001010000101111011110001110001100010010010010100110110110111101011011011000111101101101001000110110111011001011011001101011101101010111100110111111000101011000001001111101000011010000111000101110001001001110010011011010010110101101110111011110110011001100011010101010100101111111111101110000000000110010000000001010110000000011111010000000100001110000001100010010000010100110110000111101011010001000111101110011001000110010101011001010111111101011111000000111100001000001000100011000011001100101000101010101111001111111110001010000000010011110000000110100010000001011100110000011100101010000100101111110001101110000010010110010000110111010110001011001111010011101010001110100111110010011101000010110100111000111011101001001001100111011011010101001101101111111010110110000001111011010000010001101110000110010110010001010111010110011111001111010100001010001111100011110010000100100010110001101100111010010110101001110111011111010011001100001110101010100010011111111100110100000000101011100000001111100100000010000101100000110001110100001010010011100011110110100100100011011101101100101100110110101110101011011110011111101100010100000110100111100001011101000100011100111001100100101001010101101111011111110110001100000011010010100000101110111100001110011000100010010101001100110111111010101011000001111111101000010000000111000110000001001001010000011011011110000101101100010001110110100110010011011101010110101100111111011110101000001100011111000010100100001000111101100011001000110100101011001011101111101011100110000111100101010001000101111110011001110000010101010010000111111110110001000000011010011000000101110101000001110011111000010010100001000110111100011001011000100101011101001101111100111010110000101001111010001111010001110010001110010010110010010110111010110111011001111011001101010001101010111110010111111000010111000001000111001000011001001011000101011011101001111101100111010000110101001110001011111010010011100001110110100100010011011101100110101100110101011110101011111100011111100000100100000100001101100001100010110100010100111011100111101001100101000111010101111001001111110001011010000010011101110000110100110010001011101010110011100111111010100101000001111101111000010000110001000110001010011001010011110101011110100011111100011100100000100100101100001101101110100010110110011100111011010100101001101111101111010110000110001111010001010010001110011110110010010100011010110111100101111011000101110001101001110010010111010010110111001110111011001010011001101011110101010111100011111111000100100000001001101100000011010110100000101111011100001110001100100010010010101100110110111110101011011000011111101101000100000110111001100001011001010100011101011111100100111100000101101000100001110111001100010011001010100110101011111101011111100000111100000100001000100001100011001100010100101010100111101111111101000110000000111001010000001001011110000011011100010000101100100110001110101101010010011110111110110100011000011011100101000101100101111001110101110001010011110010011110100010110100011100111011100100101001100101101111010101110110001111110011010010000010101110110000111110011010001000010101110011000111110010101001000010111111011000111000001101001001000010111011011000111001101101001001010110111011011111011001101100001101010110100010111111011100111000001100101001000010101111011000111110001101001000010010111011000110111001101001011001010111011101011111001100111100001010101000100011111111001100100000001010101100000011111110100000100000011100001100000100100010100001101100111100010110101000100111011111001101001100001010111010100011111001111100100001010000101100011110001110100100010010011101100110110100110101011011101011111101100111100000110101000100001011111001100011100001010100100100011111101101100100000110110101100001011011110100011101100011100100110100100101101011101101110111100110110011000101011010101001111101111111010000110000001110001010000010010011110000110110100010001011011100110011101100101010100110101111111101011110000000111100010000001000100110000011001101010000101010111110001111111000010010000001000110110000011001011010000101011101110001111100110010010000101010110110001111111011010010000001101110110000010110011010000111010101110001001111110010011010000010110101110000111011110010001001100010110011010100111010101111101001111110000111010000010001001110000110011010010001010101110110011111110011010100000010101111100000111110000100001000010001100011000110010100101001010111101111011111000110001100001001010010100011011110111100101100011000101110100101001110011101111010010100110001110111101010010011000111110110101001000011011111011000101100001101001110100010111010011100111001110100101001010011101111011110100110001100011101010010100100111110111101101000011000110111000101001011001001111011101011010001100111101110010101000110010111111001010111000001011111001000011100001011000100100011101001101100100111010110101101001111011110111010001100011001110010100101010010111101111110111000110000011001001010000101011011110001111101100010010000110100110110001011101011010011100111101110100101000110011101111001010100110001011111101010011100000111110100100001000011101100011000100110100101001101011101111010111100110001111000101010010001001111110110011010000011010101110000101111110010001110000010110010010000111010110110001001111011010011010001101110101110010110011110010111010100010111001111100111001010000101001011110001111011100010010001100100110110010101101011010111110111101111000011000110001000101001010011001111011110101010001100011111110010100100000010111101100000111000110100001001001011100011011011100100101101100101101110110101110110011011110011010101100010101111110100111110000011101000010000100111000110001101001001010010111011011110111001101100011001010110100101011111011101111100001100110000100010101010001100111111110010101000000010111111000000111000001000001001000011000011011000101000101101001111001110111010001010011001110011110101010010100011111110111100100000011000101100000101001110100001111010011100010001110100100110010011101101010110100110111111011101011000001100111101000010101000111000111111001001001000001011011011000011101101101000100110110111001101011011001010111101101011111000110111100001001011000100011011101001100101100111010101110101001111110011111010000010100001110000111100010010001000100110110011001101011010101010111101111111111000110000000001001010000000011011110000000101100010000001110100110000010011101010000110100111110001011101000010011100111000110100101001001011101111011011100110001101100101010010110101111110111011110000011001100010000101010100110001111111101010010000000111110110000001000011010000011000101110000101001110010001111010010110010001110111010110010011001111010110101010001111011111110010001100000010110010100000111010111100001001111000100011010001001100101110011010101110010101111110010111110000010111000010000111001000110001001011001010011011101011110101100111100011110101000100100011111001101100100001010110101100011111011110100100001100011101100010100100110100111101101011101000110111100111001011000101001011101001111011100111010001100101001110010101111010010111110001110111000010010011001000110110101011001011011111101011101100000111100110100001000101011100011001111100100101010000101101111110001110110000010010011010000110110101110001011011110010011101100010110100110100111011101011101001100111100111010101000101001111111001111010000001010001110000011110010010000100010110110001100111011010010101001101110111111010110011000001111010101000010001111111000110010000001001010110000011011111010000101100001110001110100010010010011100110110110100101011011011101111101101100110000110110101010001011011111110011101100000010100110100000111101011100001000111100100011001000101100101011001110101111101010011110000111110100010001000011100110011000100101010101001101111111111010110000000001111010000000010001110000000110010010000001010110110000011111011010000100001101110001100010110010010100111010110111101001111011000111010001101001001110010111011010010111001101110111001010110011001011111010101011100001111111100100010000000101100110000001110101010000010011111110000110100000010001011100000110011100100001010100101100011111101110100100000110011101100001010100110100011111101011100100000111100101100001000101110100011001110011100101010010100101111110111101110000011000110010000101001010110001111011111010010001100001110110010100010011010111100110101111000101011110001001111100010011010000100110101110001101011110010010111100010110111000100111011001001101001101011010111010111101111001111000110001010001001010011110011011110100010101100011100111110100100101000011101101111000100110110001001101011010011010111101110101111000110011110001001010100010011011111100110101100000101011110100001111100011100010000100100100110001101101101010010110110111110111011011000011001101101000101010110111001111111011001010000001101011110000010111100010000111000100110001001001101010011011010111110101101111000011110110001000100011010011001100101110101010101110011111111110010100000000010111100000000111000100000001001001100000011011010100000101101111100001110110000100010011010001100110101110010101011110010111111100010111000000100111001000001101001011000010111011101000111001100111001001010101001011011111111011101100000001100110100000010101011100000111111100100001000000101100011000001110100101000010011101111000110100110001001011101010011011100111110101100101000011110101111000100011110001001100100010011010101100110101111110101011110000011111100010000100000100110001100001101010010100010111110111100111000011000101001000101001111011001111010001101010001110010111110010010111000010110111001000111011001011001001101011101011010111100111101111000101000110001001111001010011010001011110101110011100011110010100100100010111101101100111000110110101001001011011111011011101100001101100110100010110101011100111011111100101001100000101111010100001110001111100010010010000100110110110001101011011010010111101101110111000110110011001001011010101011011101111111101100110000000110101010000001011111110000011100000010000100100000110001101100001010010110100011110111011100100011001100101100101010101110101111111110011110000000010100010000000111100110000001000101010000011001111110000101010000010001111110000110010000010001010110000110011111010001010100001110011111100010010100000100110111100001101011000100010111101001100111000111010101001001001111111011011010000001101101110000010110110010000111011010110001001101111010011010110001110101111010010011110001110110100010010011011100110110101100101011011110101111101100011110000110100100010001011101100110011100110101010100101011111111101111100000000110000100000001010001100000011110010100000100010111100001100111000100010101001001100111111011010101000001101111111000010110000001000111010000011001001110000101011010010001111101110110010000110011010110001010101111010011111110001110100000010010011100000110110100100001011011101100011101100110100100110101011101101011111100110111100000101011000100001111101001100010000111010100110001001111101010011010000111110101110001000011110010011000100010110101001100111011111010101001100001111111010100010000001111100110000010000101010000110001111110001010010000010011110110000110100011010001011100101110011100101110010100101110010111101110010111000110010111001001010111001011011111001011101100001011100110100011100101011100100101111100101101110000101110110010001110011010110010010101111010110111110001111011000010010001101000110110010111001011010111001011101111001011100110001011100101010011100101111110100101110000011101110010000100110010110001101010111010010111111001110111000001010011001000011110101011000100011111101001100100000111010101100001001111110100011010000011100101110000100101110010001101110010110010110010111010111010111001111001111001010001010001011110011110011100010100010100100111100111101101000101000110111001111001011001010001011101011110011100111100010100101000100111101111001101000110001010111001010011111001011110100001011100011100011100100100100100101101101101101110110110110110011011011011010101101101101111110110110110000011011011010000101101101110001110110110010010011011010110110101101111011011110110001101100011010010110100101110111011101110011001100110010101010101010";



    }
}

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
        public static string Path = null;                       //程序运行目录

        public static int Cardid1 = int.Parse(ConfigurationManager.AppSettings["Cardid1"]);
        public static int Cardid2 = int.Parse(ConfigurationManager.AppSettings["Cardid2"]);

        public static int[] DA_Card1 = new int[128];      //根据用户输入转化为对应的int值
        public static int[] DA_Card2 = new int[128];      //根据用户输入转化为对应的int值

        public static int[] DA_Card3 = new int[128];      //根据用户输入转化为对应的int值
        public static int[] DA_Card4 = new int[128];      //根据用户输入转化为对应的int值

        public static byte[] DA_Send1 = new byte[512];      //根据DA_Card1的实际DA值转化为对应的码字
        public static byte[] DA_Send2 = new byte[512];      //根据DA_Card2的实际DA值转化为对应的码字
 
        public static byte[] DA_Send3 = new byte[512];      //根据DA_Card1的实际DA值转化为对应的码字
        public static byte[] DA_Send4 = new byte[512];      //根据DA_Card2的实际DA值转化为对应的码字

        public static double[] DA1_value_a = new double[128];                         //DA板卡1修正参数a
        public static double[] DA1_value_b = new double[128];                         //DA板卡1修正参数b

        public static double[] DA2_value_a = new double[128];                         //DA板卡2修正参数a
        public static double[] DA2_value_b = new double[128];                         //DA板卡2修正参数b

        //   public static object synObj_dq0 = new object();
        public static object synObj_sc1 = new object();


        public static Queue<byte[]> DataQueue1 = new Queue<byte[]>();   //For FrameEPDU deal with 同步下行1
        public static Queue<byte[]> DataQueue2 = new Queue<byte[]>();   //For FrameEPDU deal with 同步下行2
        public static Queue<byte[]> DataQueue3 = new Queue<byte[]>();   //For FrameEPDU deal with 串口下行
        public static int Channel = 1;                                      //表明从哪里调用的FrameEPDU

        public static Dictionary<string, BinaryWriter> myDictionary1 = new Dictionary<string, BinaryWriter>();
        public static Dictionary<string, BinaryWriter> myDictionary2 = new Dictionary<string, BinaryWriter>();
        public static Dictionary<string, BinaryWriter> myDictionary3 = new Dictionary<string, BinaryWriter>();

        public static List<string> APIDList = new List<string>();
        public static List<string> APIDList2 = new List<string>();
        public static List<string> APIDList3 = new List<string>();

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
        public static void init(int channel)
        {
            switch (channel) {
                case 1:
                    AlreadyOnApid.Clear();
                    APIDList.Clear();

                    foreach (var item in Apid_EPDU_Dictionary)
                    {
                        item.Value.Clear();
                    }
                    Apid_EPDU_Dictionary.Clear();
                    break;
                case 2:
                    AlreadyOnApid2.Clear();
                    APIDList2.Clear();

                    foreach (var item in Apid_EPDU_Dictionary2)
                    {
                        item.Value.Clear();
                    }
                    Apid_EPDU_Dictionary2.Clear();
                    break;
                case 3:
                    AlreadyOnApid3.Clear();
                    APIDList3.Clear();

                    foreach (var item in Apid_EPDU_Dictionary3)
                    {
                        item.Value.Clear();
                    }
                    Apid_EPDU_Dictionary3.Clear();
                    break;
            }

        }


        public static string DAconfigPath = Program.GetStartupPath() + @"配置文件\DAconfig.xml";
        public static string RconfigPath = Program.GetStartupPath() + @"配置文件\Rconfig.xml";
        public static string OCconfigPath = Program.GetStartupPath() + @"配置文件\OCconfig.xml";

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

        public static string GetConfigStr(string Path, string key, string name)
        {
            XDocument xDoc = XDocument.Load(Path);
            XmlReader reader = xDoc.CreateReader();
            string value = "Error";
            var query = from p in xDoc.Root.Elements("add")
                        where p.Attribute("key").Value == key
                        select p.Attribute(name).Value;

            foreach (string s in query)
            {
                value = s;
            }

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



    }
}

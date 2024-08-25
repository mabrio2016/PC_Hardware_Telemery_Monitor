using LibreHardwareMonitor.Hardware;
using Mono.Unix.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using HardwareMonitor;
using MongoDB.Bson.Serialization.Conventions;
using LibreHardwareMonitor.Hardware.Cpu;
using System.Text.RegularExpressions;
using LibreHardwareMonitor.Hardware.Storage;

namespace Hardware_Monitor
{ 
    class Program
    {
        string Manufacturer_Model;
        string CPU_Model;
        string CPU_Temperatura;
        string CPU_Temperatura_value;
        string CPU_Load;
        string CPU_Load_value;
        string Memory_Load;
        string Memory_Load_Value;
        //DateTime localDateTime = DateTime.Now;
        //DateTime utcDateDateTime = DateTime.UtcNow;

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        static extern int SendARP(int destIp, int srcIP, byte[] macAddr, ref uint physicalAddrLen);

        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }

            //Computer computer = new Computer
            //{
            //    IsCpuEnabled = true,
            //    IsGpuEnabled = false,
            //    IsMemoryEnabled = true,
            //    IsMotherboardEnabled = true,
            //    IsControllerEnabled = true,
            //    IsNetworkEnabled = false,
            //    IsStorageEnabled = true
            //};
        }

        public void Monitor()
        {
            
            Computer computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = false,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsNetworkEnabled = false,
                IsStorageEnabled = true
            };

            computer.Open();
            computer.Accept(new UpdateVisitor());


            foreach (IHardware hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Motherboard )
                {
                    //Console.WriteLine("Hardware: {0}, {1}", hardware.HardwareType, hardware.Name);
                    Manufacturer_Model = hardware.Name.ToString();
                }
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    //Console.WriteLine("Hardware: {0}, {1}", hardware.HardwareType, hardware.Name);
                    CPU_Model = hardware.Name.ToString();
                }

                foreach (IHardware subhardware in hardware.SubHardware)
                {
                    //Console.WriteLine("\tSubhardware: {0}", subhardware.Name);

                    foreach (ISensor sensor in subhardware.Sensors)
                    {
                        //Console.WriteLine("\t{0} {1}, value:  {2}", sensor.SensorType, sensor.Name, sensor.Value);
                    }
                }
                foreach (ISensor sensor in hardware.Sensors)
                {
                    //Console.WriteLine("\t{0} {1}, value:  {2}", sensor.SensorType, sensor.Name, sensor.Value);

                    if (sensor.SensorType == SensorType.Temperature && sensor.Name == "Core Average")
                    {
                        CPU_Temperatura = sensor.Name.ToString() + "Temperature in Celsius";
                        CPU_Temperatura_value = sensor.Value.ToString();
                        //Console.WriteLine("\t{0} {1}, value:  {2}", sensor.SensorType, sensor.Name, sensor.Value);
                    }
                    //Console.WriteLine("\t{0} {1}, value:  {2}", sensor.SensorType, sensor.Name, sensor.Value);
                    if (sensor.SensorType == SensorType.Load && sensor.Name == "CPU Total")
                    {
                        CPU_Load = sensor.Name.ToString();
                        CPU_Load_value = sensor.Value.ToString();
                        //Console.WriteLine("\t{0} {1}, value:  {2}", sensor.SensorType, sensor.Name, sensor.Value);
                    }
                    if (sensor.SensorType == SensorType.Load && sensor.Name == "Memory")
                    {
                        Memory_Load = sensor.Name.ToString();
                        Memory_Load_Value = sensor.Value.ToString();
                        //Console.WriteLine("\t{0} {1}, value:  {2}", sensor.SensorType, Memory_Load, Memory_Load_Value);
                    }
                }
            }

            computer.Close();
        }
        static String getMAC_Address(string ipAddr)
        {
            IPAddress dst = IPAddress.Parse(ipAddr);

            byte[] macAddr = new byte[6];
            uint macAddrLen = (uint)macAddr.Length;

            if (SendARP(BitConverter.ToInt32(dst.GetAddressBytes(), 0), 0, macAddr, ref macAddrLen) != 0) {                 //throw new InvalidOperationException("SendARP failed.");
                Console.WriteLine("SendARP failed.");
            }

            string[] str = new string[(int)macAddrLen];
            for (int i = 0; i < macAddrLen; i++)
                str[i] = macAddr[i].ToString("x2");

            String MAC_Address = string.Join("-", str);
            return MAC_Address;
        }

        static void Main(string[] args)
        {
            int Count = 0;
            float CPU_Temprature_Avg = 0;
            float CPU_Load_AVG = 0;
            float CPU_Temprature_Sum = 0;
            float CPU_Load_Sum = 0;
            float Momory_Load_Average = 0;
            float Momory_Load_Sum = 0;

            Program Monitoring = new Program();
            string host = Dns.GetHostName();
            IPHostEntry ip = Dns.GetHostEntry(host);                            // Getting ip address using host name
            string IP_Address = null;                                           // used to pmake the IP_Address visible outside the for loop.
            for (int i = 0; i < 5; i++)
            {
                //Console.WriteLine(ip.AddressList[i].ToString());
                Match match = Regex.Match(ip.AddressList[i].ToString(), @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"); //This is to fing the IPV4 address inside the ip.AddressList[]
                if (match.Success)
                {
                    //Console.WriteLine("IP Address = " + match.Value);
                    IP_Address = ip.AddressList[i].ToString();
                }
            }            
            string mac_Address = getMAC_Address(IP_Address);

            // Getting ip address using host name 
            Console.WriteLine("\t Host name: {0}", host);
            Console.WriteLine("\t IP Address: {0}", IP_Address);
            Console.WriteLine("\t MAC Address: {0}", mac_Address);            

            var uri = "mongodb+srv://MABRIO:Toco2273@cluster0.ne2gv.mongodb.net/";
            var pack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("elementNameConvention", pack, x => true);

            while (true)
            {
                Monitoring.Monitor();

                Count++;
                float CPU_Temprature = float.Parse(Monitoring.CPU_Temperatura_value);
                CPU_Temprature_Sum = CPU_Temprature_Sum + CPU_Temprature;
                float CPU_Load = float.Parse(Monitoring.CPU_Load_value);
                CPU_Load_Sum = CPU_Load_Sum + CPU_Load;
                float Mem_Load = float.Parse(Monitoring.Memory_Load_Value);
                Momory_Load_Sum = Momory_Load_Sum + Mem_Load;


                if (Count == 10)
                {
                    CPU_Load_AVG = CPU_Load_Sum / 10;
                    CPU_Temprature_Avg = CPU_Temprature_Sum / 10;
                    Momory_Load_Average = Momory_Load_Sum / 10;
                    // Seting the trashold for CPU utilization and temperatura
                    if (CPU_Temprature_Avg > 65 || CPU_Load_AVG > 57 || Momory_Load_Average > 90)
                    {
                        Console.WriteLine("\t{0}, value:  {1}", "CPU Utilization", CPU_Load_AVG);
                        Console.WriteLine("\t{0}, value:  {1}", "CPU Temperatura", CPU_Temprature_Avg);
                        Console.WriteLine("\t{0}, value:  {1}", "CPU Temperatura", Momory_Load_Average);
                        //Console.WriteLine("CPU Temperatura = " + CPU_Temprature_Avg);
                        var client = new MongoClient(uri);
                        var db = client.GetDatabase("System_Events_Monitor");
                        var coll = db.GetCollection<Events>("events");
                        var events = new[]
                        {
                            new Events 
                            {
                                HostName = host,
                                IP_Address = IP_Address,
                                MAC_Address = mac_Address,
                                Manufacturer_Model =  Monitoring.Manufacturer_Model,
                                CPU_Model = Monitoring.CPU_Model,
                                EventName1 = Monitoring.CPU_Temperatura,
                                Value1 = Monitoring.CPU_Temperatura_value,
                                EventName2 = Monitoring.CPU_Load,
                                Value2 = Monitoring.CPU_Load_value,
                                EventName3 = Monitoring.Memory_Load,
                                Value3 = Monitoring.Memory_Load_Value,
                                UTC_TimeStamp = DateTime.UtcNow,
                            }
                        };
                        coll.InsertMany(events);
                        // To confirm if the record was savd in the DB.
                        foreach (var evt in events)
                        {
                            Console.WriteLine("Events sent to Cloud DB, event ID = " + evt.Id);
                        }
                    }
                    Count = 0;
                    CPU_Temprature_Sum = 0;
                    CPU_Load_Sum = 0;
                }
            }


        }
    }
}
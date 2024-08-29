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
using System.Net.NetworkInformation;
using System.Collections;
using Microsoft.Extensions.Configuration;


namespace Hardware_Monitor
{ 
    class Program
    {
        string Manufacturer_Model;
        string CPU_Model;
        string CPU_Temperatura;
        int CPU_Temperatura_value;
        string CPU_Load;
        int CPU_Load_value;
        string Memory_Load;
        int Memory_Load_Value;
        string Disk_Hardware;
        int Disk_Load_Value;
        string[,] Disks = { { "", "" }, { "", "" }, { "", "" } };
        int disk_Nbr = 0;
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
                //Console.WriteLine("Hardware: {0}, {1}", hardware.HardwareType, hardware.Name);  //Storage,
                if (hardware.HardwareType == HardwareType.Motherboard )
                {
                    Manufacturer_Model = hardware.Name.ToString();
                }
                if (hardware.HardwareType == HardwareType.Cpu)
                {
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
                        CPU_Temperatura_value = (int)sensor.Value; //(float)sensor.Value;
                    }
                    if (sensor.SensorType == SensorType.Load && sensor.Name == "CPU Total")
                    {
                        CPU_Load = sensor.Name.ToString();
                        CPU_Load_value = (int)sensor.Value;
                    }
                    if (sensor.SensorType == SensorType.Load && sensor.Name == "Memory")
                    {
                        Memory_Load = sensor.Name.ToString();
                        Memory_Load_Value = (int)sensor.Value;
                    }
                    if (hardware.HardwareType == HardwareType.Storage && sensor.SensorType == SensorType.Load && sensor.Name == "Used Space")
                    {
                        string temp_Disk_Hardware = "";
                        string temp_Disk_Load_Value = "";
                        foreach (IHardware DSK_hardware in computer.Hardware)
                        {
                            if (DSK_hardware != null && DSK_hardware.HardwareType == HardwareType.Storage)
                            {
                                Disk_Hardware = hardware.Name.ToString();
                                Disk_Load_Value = (int)sensor.Value;
                                if (Disk_Hardware != temp_Disk_Hardware && disk_Nbr == 0)
                                {
                                    Disks[0, 0] = Disk_Hardware;
                                    Disks[0, 1] = Disk_Load_Value.ToString();
                                    temp_Disk_Hardware = Disk_Hardware;
                                    temp_Disk_Load_Value = Disk_Load_Value.ToString();
                                    disk_Nbr++;
                                    //Console.WriteLine("\t{0} {1}, {2} : {3}", sensor.SensorType, hardware.Name.ToString(), sensor.Name, sensor.Value.ToString());
                                }
                                if (Disk_Hardware != temp_Disk_Hardware)
                                {
                                    Disks[disk_Nbr, 0] = Disk_Hardware;
                                    Disks[disk_Nbr, 1] = Disk_Load_Value.ToString();
                                    temp_Disk_Hardware = Disk_Hardware;
                                    temp_Disk_Load_Value = Disk_Load_Value.ToString();
                                    disk_Nbr++;
                                    //Console.WriteLine("\t{0} {1}, {2} : {3}", sensor.SensorType, hardware.Name.ToString(), sensor.Name, sensor.Value.ToString());
                                }
                            }
                        }
                        if (disk_Nbr > 2) disk_Nbr = 0;
                    }
                }
            }
            try
            {
                int Highest_Disk_Usage = int.Parse(Disks[0, 1]);
                Disk_Hardware = Disks[0, 0];
                for (int i = 1; i < Disks.GetLength(1); i++)
                {
                    if (int.Parse(Disks[1, 1]) > Highest_Disk_Usage)
                    {
                        Highest_Disk_Usage = int.Parse(Disks[1, 1]);
                        Disk_Hardware = Disks[1, 0];
                    }
                }
                Disk_Load_Value = Highest_Disk_Usage;
            }
            catch (Exception)
            {

                throw;
            }
            computer.Close();
        }
        public static IPAddress GetIPAddress(string hostName)
        {
            Ping ping = new Ping();
            var replay = ping.Send(hostName);

            if (replay.Status == IPStatus.Success)
            {
                return replay.Address;
            }
            return null;
        }
        public static string GetIP4Address(string hostName)
        {
            string IP4Address = String.Empty;

            foreach (IPAddress IPA in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                //Console.WriteLine("\t{0}", IPA.ToString());
                if (IPA.AddressFamily == AddressFamily.InterNetwork)
                {
                    IP4Address = IPA.ToString();
                    break;
                }
            }
            return IP4Address;
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
            Decimal CPU_Temprature_Avg = 0;
            Decimal CPU_Load_AVG = 0;
            Decimal CPU_Temprature_Sum = 0;
            Decimal CPU_Load_Sum = 0;
            Decimal Memory_Load_Average = 0;
            Decimal Memory_Load_Sum = 0;

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("secrets.json");
            IConfiguration configuration = configurationBuilder.Build();
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            var url = connectionString;

            Program Monitoring = new Program();
            string host = System.Net.Dns.GetHostName();                                       // used to make the IP_Address visible outside the for loop.
            string IP_Address = GetIP4Address(host);
            string mac_Address = getMAC_Address(IP_Address);

            Console.WriteLine("\t Host name: {0}", host);
            Console.WriteLine("\t IP Address: {0}", IP_Address);
            Console.WriteLine("\t MAC Address: {0}", mac_Address);

            var pack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("elementNameConvention", pack, x => true);

            while (true)
            {
                Monitoring.Monitor();

                Count++;
                int CPU_Temprature = Monitoring.CPU_Temperatura_value;
                CPU_Temprature_Sum = CPU_Temprature_Sum + CPU_Temprature;
                decimal CPU_Load = Monitoring.CPU_Load_value;
                CPU_Load_Sum = CPU_Load_Sum + CPU_Load;
                decimal Mem_Load = Monitoring.Memory_Load_Value;
                Memory_Load_Sum = Memory_Load_Sum + Mem_Load;

                if (Count == 20)
                {
                    CPU_Load_AVG = CPU_Load_Sum / 20;
                    CPU_Temprature_Avg = CPU_Temprature_Sum / 20;
                    Memory_Load_Average = Memory_Load_Sum / 20;
                    // Seting the trashold for CPU utilization, temperatura and memory
                    if (CPU_Temprature_Avg > 65 || CPU_Load_AVG > 68 || Memory_Load_Average > 89 || Monitoring.Disk_Load_Value > 80)
                    {
                        Console.WriteLine("\t{0}, value:  {1}", "CPU Utilization", CPU_Load_AVG);
                        Console.WriteLine("\t{0}, value:  {1}", "CPU Temperatura", CPU_Temprature_Avg);
                        Console.WriteLine("\t{0}, value:  {1}", "Memory Utilization", Memory_Load_Average);
                        Console.WriteLine("\t{0} {1}, {2}", "Disk Utilization", Monitoring.Disk_Hardware, Monitoring.Disk_Load_Value);

                        var client = new MongoClient(url);
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
                                CPU_Temp_Event = Monitoring.CPU_Temperatura,
                                CPU_Temp_Value = Monitoring.CPU_Temperatura_value,
                                CPU_Load_Event = Monitoring.CPU_Load,
                                CPU_Load_Value = Monitoring.CPU_Load_value,
                                Memory_Load_Event = Monitoring.Memory_Load,
                                Memory_Load_Value = Monitoring.Memory_Load_Value,
                                Disk_Hardware = Monitoring.Disk_Hardware,
                                Disk_Load_Value = Monitoring.Disk_Load_Value,
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
                    Memory_Load_Sum = 0;
                }
            }


        }
    }
}
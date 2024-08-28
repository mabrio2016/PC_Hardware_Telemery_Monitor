using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareMonitor
{
    public class Events
    {
        public ObjectId Id { get; set; }
        public string HostName { get; set; }
        public string IP_Address { get; set; }
        public string MAC_Address { get; set; }
        public string Manufacturer_Model { get; set; }
        public string CPU_Model { get; set; }
        public string CPU_Temp_Event { get; set; }
        public string CPU_Temp_Value { get; set; }
        public string CPU_Load_Event { get; set; }
        public string CPU_Load_Value { get; set; }
        public string Memory_Load_Event { get; set; }
        public string Memory_Load_Value { get; set; }
        public string Disk_Hardware { get; set; }
        public string Disk_Load_Value { get; set; }
        //public DateTime Local_TimeStamp { get; set; }
        public DateTime UTC_TimeStamp { get; set; }

    }
}

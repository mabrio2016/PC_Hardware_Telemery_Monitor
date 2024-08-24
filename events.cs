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
        public string EventName1 { get; set; }
        public string Value1 { get; set; }
        public string EventName2 { get; set; }
        public string Value2 { get; set; }
        //public DateTime Local_TimeStamp { get; set; }
        public DateTime UTC_TimeStamp { get; set; }

    }
}

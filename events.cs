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
        public string hostName { get; set; }
        public string iP_Address { get; set; }
        public string mAC_Address { get; set; }
        public string manufacturer_Model { get; set; }
        public string cPU_Model { get; set; }
        public string cPU_Temp_Event { get; set; }
        public int cPU_Temp_Value { get; set; }
        public string cPU_Load_Event { get; set; }
        public int cPU_Load_Value { get; set; }
        public string memory_Load_Event { get; set; }
        public int memory_Load_Value { get; set; }
        public string disk_Hardware { get; set; }
        public int disk_Load_Value { get; set; }
        //public DateTime Local_TimeStamp { get; set; }
        public DateTime uTC_TimeStamp { get; set; }

    }
}

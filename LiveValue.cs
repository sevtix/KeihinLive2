using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeihinLive
{
    internal class LiveValue
    {
        public int Id { get; set; }     // The ID (hidden value)
        public string Name { get; set; } // The Name (displayed value)
        public ReadIdentifier ReadIdentifier;
        public LiveValue() { 

        }
    }
}

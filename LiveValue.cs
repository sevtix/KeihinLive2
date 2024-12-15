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
        public string Unit { get; set; }
        public ReadIdentifier ReadIdentifier;
        public Func<double, double> Formula { get; set; }
        public double ApplyFormula(double inputValue)
        {
            return Formula != null ? Formula(inputValue) : inputValue;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarcraftBotLib
{
    public class UnitArgs : EventArgs
    {
        public BW.Unit Unit { get; set; }
        public UnitArgs(BW.Unit unit)
        {
            this.Unit = unit;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarcraftBotLib
{
    public interface IAIModule
    {        
        void Run();
        void ReceiveUnit(BW.Unit unit);
        BW.Unit YieldUnit();
    }
}

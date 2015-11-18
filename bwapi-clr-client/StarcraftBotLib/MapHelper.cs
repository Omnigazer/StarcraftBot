using SWIG.BWAPI;
using SWIG.BWTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarcraftBotLib
{
    class MapHelper
    {
        public static BaseLocation getMainBase()
        {
            return bwta.getStartLocation(bwapi.Broodwar.self());
        }

        public static Chokepoint getMainChokePoint()
        {
            return SWIG.BWTA.bwta.getNearestChokepoint(getMainBase().getPosition());
        }

        public static Position getMainToChokePosition()
        {            
            var chokepoint = getMainChokePoint().getCenter();
            var main = getMainBase().getPosition();            
            if (chokepoint != null)
                return new Position((chokepoint.xConst() + main.xConst()) / 2, (chokepoint.yConst() + main.yConst()) / 2);
            return null;
        }
    }
}

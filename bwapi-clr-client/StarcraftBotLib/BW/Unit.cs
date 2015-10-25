using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarcraftBotLib.BW
{
    public class Unit
    {
        public SWIG.BWAPI.Unit theUnit;
        public SWIG.BWAPI.Unit theTarget;
        public string Type { get; set; }
        public int Id { get; set; }        

        public Unit(SWIG.BWAPI.Unit u)
        {
            theUnit = u;
            Type = u.getType().getName();
            Id = u.getID();
            theTarget = null;
        }

        public bool HasTarget()
        {
            return (theTarget != null);
        }

        public void Attack(SWIG.BWAPI.Unit t)
        {
            if (t == null && theTarget != null)
            {
                theUnit.stop();
                theTarget = null;
            }
            else if (t != null && t != theTarget)
            {
                theTarget = t;
                theUnit.attack(theTarget);
            }
        }

        public void Gather(SWIG.BWAPI.Unit t)
        {
            if (t == null && theTarget != null)
            {
                theUnit.stop();
                theTarget = null;
            }
            else if (t != null)
            {
                theTarget = t;
                theUnit.gather(theTarget);                
            }
        }

        public override string ToString()
        {
            if (Type != null)
            {
                return Type;
            }
            else 
            {
                return base.ToString();
            }
        }

        public bool IsIdle()
        {
            return theUnit.getOrder().getName() == "PlayerGuard" || theUnit.getOrder().getName() == "Nothing";
        }
    }
}

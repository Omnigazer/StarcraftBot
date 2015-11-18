using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarcraftBotLib.CustomTypes
{
    public class NullableDictionary : Dictionary<string, int>
    {        
        public new int this[string key]
        {
            get
            {
                if (this.ContainsKey(key))
                    return base[key];
                else
                    return 0;
            }
            set
            {
                if (this.ContainsKey(key))
                    base[key] = value;
                else
                    base.Add(key, value);
            }
        }
    }
}

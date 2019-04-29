using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poktogone.Pokemon
{
    class Item
    {
        public readonly String name;
        public readonly bool oneUse;

        public Item(String name, bool isOneUse)
        {
            this.name = name;
            this.oneUse = isOneUse;
        }

        /**
         * "name+isOneUse"
         */
        public static Item Parse(String arg, char sep = '+')
        {
            String[] data = arg.Split(sep);
            return new Item(data[0], int.Parse(data[1]) == 1);
        }
    }
}

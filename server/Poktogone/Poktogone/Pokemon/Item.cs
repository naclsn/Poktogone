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

        public override string ToString()
        {
            return this.name;
        }
    }
}

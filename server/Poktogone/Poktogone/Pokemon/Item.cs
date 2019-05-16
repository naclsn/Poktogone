using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poktogone.Pokemon
{
    class Item
    {
        public int id;
        public String name;
        public readonly bool oneUse;
        public bool isUsed;

        public Item(int id, String name, bool isOneUse)
        {
            this.id = id;
            this.name = name;
            this.oneUse = isOneUse;
            this.isUsed = false;
        }

        public void Remove(bool onUsage = false)
        {
            this.id = 0;
            this.name = "No Item";
            this.isUsed |= onUsage;
        }

        public override string ToString()
        {
            return this.name;
        }
    }

    struct Ability
    {
        public int id;
        public String name;

        public Ability(int id, String name)
        {
            this.id = id;
            this.name = name;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poktogone.Pokemon
{
    public enum Sps
    {
        Stat,
        Physic,
        Special,
        Oof
    }

    public static class SpsExtensions
    {
        public static Sps Parse(String c)
        {
            switch (int.Parse(c))
            {
                case 0: return Sps.Stat;
                case 1: return Sps.Physic;
                case 2: return Sps.Special;
                default: return Sps.Oof;
            }
        }
    }

    class Move
    {
        public readonly String name;
        public readonly Type type;
        public readonly Sps sps;
        public readonly int power;
        public readonly int accuracy;

        public readonly int basePP;
        private int pp;

        public Move(String name, Type type, Sps sps, int power, int accuracy, int basePP)
        {
            this.name = name;
            this.type = type;
            this.sps = sps;
            this.power = power;
            this.accuracy = accuracy;
            this.basePP = basePP;
        }

        /**
         * "name+type+sps+power+accuracy+basePP"
         */
        public static Move Parse(String arg, char sep = '+')
        {
            String[] data = arg.Split(sep);
            return new Move(data[0], TypeExtensions.Parse(data[1]), SpsExtensions.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]), int.Parse(data[5]));
        }
    }
}

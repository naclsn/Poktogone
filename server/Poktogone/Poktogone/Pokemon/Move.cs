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

    struct Effect
    {
        public int id;
        public String desc;
        public int percent;
        public int value;

        public Effect(int id, String desc, int precent, int value)
        {
            this.id = id;
            this.desc = desc;
            this.percent = precent;
            this.value = value;
        }

        public Effect(int value) : this(0, "", 100, value) { }
    }

    class Move
    {
        public readonly int id;
        public readonly String name;
        public readonly Type type;
        public readonly Sps sps;
        public readonly int power;
        public readonly int accuracy;

        private readonly Effect[] _effects;
        public Effect? this[int effectId]
        {
            get
            {
                foreach (Effect e in this._effects)
                    if (e.id == effectId)
                        return e;
                return null;
            }
        }
        
        public readonly int basePP;
        private int pp;

        public Move(int id, String name, Type type, Sps sps, int power, int accuracy, int basePP, Effect[] effects)
        {
            this.id = id;
            this.name = name;
            this.type = type;
            this.sps = sps;
            this.power = power;
            this.accuracy = accuracy;
            this.basePP = basePP;
            this.pp = basePP;
            this._effects = effects;
        }
        
        public override string ToString()
        {
            return this.name;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poktogone.Pokemon
{
    public enum Type
    {
        Fire,
        Grass,
        Water,
        Oof
    }

    public static class TypeExtensions
    {
        public static Type Parse(String c)
        {
            switch (int.Parse(c))
            {
                case 01: return Type.Fire;
                case 02: return Type.Grass;
                case 03: return Type.Water;
                case 04: return Type.Oof;
                case 05: return Type.Oof;
                case 06: return Type.Oof;
                case 07: return Type.Oof;
                case 08: return Type.Oof;
                case 09: return Type.Oof;
                case 10: return Type.Oof;
                case 11: return Type.Oof;
                case 12: return Type.Oof;
                case 13: return Type.Oof;
                case 14: return Type.Oof;
                case 15: return Type.Oof;
                case 16: return Type.Oof;
                case 17: return Type.Oof;
                case 18: return Type.Oof;
                default: return Type.Oof;
            }
        }
    }

    class Base
    {
        public readonly String name;
        public readonly int[] _stats = new int[5];

        public readonly Type type1;
        public readonly Type type2;

        public int this[StatTarget stat]
        {
            private set { this._stats[(int)stat] = value; }
            get { return this._stats[(int)stat]; }
        }

        public Base(String baseName, Type type1, Type type2, int[] baseStat)
        {
            name = baseName;

            this.type1 = type1;
            this.type2 = type2;
            
            foreach (var stat in Enum.GetValues(typeof(StatTarget)))
                this._stats[(int)stat] = baseStat[(int)stat];
        }
    }
}

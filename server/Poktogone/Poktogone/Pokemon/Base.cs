using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poktogone.Pokemon
{
    public enum Type
    {
        None,
        Eau,
        Feu,
        Plante,
        Electrik,
        Normal,
        Combat,
        Glace,
        Roche,
        Sol,
        Acier,
        Tenèbres,
        Psy,
        Insecte,
        Poison,
        Vol,
        Spectre,
        Dragon,
        Fée,
        Oof
    }

    public static class TypeExtensions
    {
        public static Type Parse(String c)
        {
            switch (int.Parse(c))
            {
                case 00: return Type.None;
                case 01: return Type.Eau;
                case 02: return Type.Feu;
                case 03: return Type.Plante;
                case 04: return Type.Electrik;
                case 05: return Type.Normal;
                case 06: return Type.Combat;
                case 07: return Type.Glace;
                case 08: return Type.Roche;
                case 09: return Type.Sol;
                case 10: return Type.Acier;
                case 11: return Type.Tenèbres;
                case 12: return Type.Psy;
                case 13: return Type.Insecte;
                case 14: return Type.Poison;
                case 15: return Type.Vol;
                case 16: return Type.Spectre;
                case 17: return Type.Dragon;
                case 18: return Type.Fée;
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

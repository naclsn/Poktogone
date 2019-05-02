using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

using Poktogone.Main;

namespace Poktogone.Pokemon
{
    public enum StatTarget
    {
        Attack,
        Defence,
        AttackSpecial,
        DefenceSpecial,
        Speed,
        Oof
    }

    public static class StatTargetExtensions
    {
        public static StatTarget Parse(String c)
        {
            switch (c)
            {
                case "atk": return StatTarget.Attack;
                case "def": return StatTarget.Defence;
                case "spa": return StatTarget.AttackSpecial;
                case "spd": return StatTarget.DefenceSpecial;
                case "spe": return StatTarget.Speed;
                default: return StatTarget.Oof;
            }
        }
        public static bool IsPhysical(this StatTarget st)
        {
            return st == StatTarget.Attack || st == StatTarget.Defence;
        }
        public static bool IsSpecial(this StatTarget st)
        {
            return st == StatTarget.AttackSpecial || st == StatTarget.DefenceSpecial;
        }
        public static Sps GetSps(this StatTarget st)
        {
            return st.IsSpecial() ? Sps.Special : st.IsPhysical() ? Sps.Physic : Sps.Stat;
        }
    }

    enum Tags
    {
        Substitute,
        Fly,
        Dig,
        Flinch,
        Confusion
    }

    enum Status
    {
        Burn,
        Freeze,
        Paralysis,
        Poison,
        BadlyPoisoned,
        Sleep
    }

    struct EVDist
    {
        public StatTarget ev1, ev2;

        public EVDist(String ev1, String ev2)
        {
            this.ev1 = StatTargetExtensions.Parse(ev1);
            this.ev2 = StatTargetExtensions.Parse(ev2);
        }
    }

    struct Nature
    {
        public StatTarget up, down;

        public Nature(String up, String down)
        {
            this.up = StatTargetExtensions.Parse(up);
            this.down = StatTargetExtensions.Parse(down);
        }
    }

    class Set 
    {
        readonly String customName;

        Base baseStat;

        Move[] moves;
        Item item;

        EVDist _evDist;
        Nature _nature;

        int[] _mod = new int[5];

        public readonly int maxHp;
        private int _hp;
        public int Hp
        {
            get { return this._hp; }
            private set
            {
                if (value < 0)
                    this._hp = 0;
                else if (this.maxHp < value)
                    this._hp = this.maxHp;
                else
                    this._hp = value;
            }
        }

        public int this[StatTarget stat]
        {
            get
            {
                int mod = this._mod[(int)stat];
                int baz = this.baseStat[stat]; // base stats
                double r = baz + (stat == this._evDist.ev1 || stat == this._evDist.ev2 ? 63 : 0); // EVs
                r = (int)(r * (mod < 0 ? 1/(1 - .5 * mod) : 1 + .5 * mod)); // modif
                return (int)(r * (this._nature.up == stat ? 1.1 : this._nature.down == stat ? .9 : 1)); // nature
            }
            set // e.g.: shif gears -> pok[StatTarget] = 2 --> actualy does +2
            {
                if (-7 < this._mod[(int)stat] + value && this._mod[(int)stat] + value < 7)
                    this._mod[(int)stat] += value;
            }
        }

        public Set(String customName, int baseHp, Base baseStat, Move[] moves, Item item, EVDist evDist, Nature nature)
        {
            this.customName = customName;

            this.maxHp = baseHp;
            this.Hp = baseHp;
            this.baseStat = baseStat;

            this.moves = moves;
            this.item = item;

            this._evDist = evDist;
            this._nature = nature;
        }

        /**
         * Data order:
         * "baseName,[type1],[type2],baseHp,baseAttack,baseDefence,baseAttackSpecial,baseDefenceSpecial,baseSpeed,[moves1],[moves2],[moves3],[moves4],[item],[ev],[nature]"
         */
        public static Set Parse(String arg, char sep = ',')
        {
            String[] data = arg.Split(sep);
            String baseName = data[0];

            Type type1 = TypeExtensions.Parse(data[1]);
            Type type2 = TypeExtensions.Parse(data[2]);

            int baseHp = int.Parse(data[3]);
            int[] baseStat = new int[5];
            foreach (var stat in Enum.GetValues(typeof(StatTarget)))
                baseStat[(int)stat] = int.Parse(data[(int)stat + 4]);

            Move[] moves = new Move[4]; // 10 + 4 * 5
            for (int k = 0; k < 4; k++)
                moves[k] = Move.Parse(data[k + 9]);

            Item item = Item.Parse(data[13]);

            EVDist evDist = new EVDist(data[14], data[15]);
            Nature nature = new Nature(data[16], data[17]);

            return new Set(baseName, baseHp, new Base(baseName, type1, type2, baseStat), moves, item, evDist, nature);
        }

        public static Set FromDB(SqlHelper dbo, int id)
        {
            SqlDataReader r = dbo.Select(new Table("sets", id)
                                        .Join("pokemons", "sets::poke")
                                        .Join("moves", "sets::move1")
                                        .Join("moves", "sets::move2")
                                        .Join("moves", "sets::move3")
                                        .Join("moves", "sets::move4")
                                        /*...*/);
            return null; // new Set();
        }
    }
}

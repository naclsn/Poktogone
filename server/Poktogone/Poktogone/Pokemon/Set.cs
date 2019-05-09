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
        HP
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
                case "pdv": return StatTarget.HP; // TODO: rename in DB "pdv" to "hp"
                default: throw new FormatException($"'{c}' is not a valid StatTarget");
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
        public static String ShortString(this StatTarget st)
        {
            switch (st)
            {
                case StatTarget.Attack: return "atk";
                case StatTarget.Defence: return "def";
                case StatTarget.AttackSpecial: return "spa";
                case StatTarget.DefenceSpecial: return "spd";
                case StatTarget.Speed: return "spe";
                case StatTarget.HP: return "pdv"; // TODO: rename in DB "pdv" to "hp"
                default: return "oof";
            }
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

        private int _indexNextMove;
        public Move NextMove
        {
            get
            {
                if (-1 < this._indexNextMove)
                    return this.moves[this._indexNextMove];
                return null;
            }
            set
            {
                if (value != null)
                    for (int k = 0; k < this.moves.Length; k++)
                        if (this.moves[k].name == value.name)
                        {
                            this._indexNextMove = k;
                            return;
                        }
                this._indexNextMove = -1;
            }
        }

        public int this[StatTarget stat]
        {
            get
            {
                int mod = this._mod[(int)stat];
                int baz = this.baseStat[stat]; // base stats
                double r = baz + (stat == this._evDist.ev1 || stat == this._evDist.ev2 ? 63 : 0); // EVs
                r = (int)(r * (this._nature.up == stat ? 1.1 : this._nature.down == stat ? .9 : 1)); // nature
                return (int)(r * (mod < 0 ? 1 / (1 - .5 * mod) : 1 + .5 * mod)); // modif
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

            this._indexNextMove = -1;
        }

        public static Set FromDB(SqlHelper dbo, int id)
        {
            var r = dbo.Select(
                new Table("sets", id)
                    .Join("pokemons", "sets.pokemon")
                    .Join("moves AS move1", "sets.move1")
                    .Join("moves AS move2", "sets.move2")
                    .Join("moves AS move3", "sets.move3")
                    .Join("moves AS move4", "sets.move4")
                    .Join("abilities", "sets.ability")
                    .Join("items", "sets.item"),
                "sets.name", "pokemons.hp", "pokemons.type1", "pokemons.type2",
                "pokemons.name", "pokemons.hp", "pokemons.atk", "pokemons.def", "pokemons.spa", "pokemons.spd", "pokemons.spe", // baseStat
                "move1.id", "move1.name", "move1.type", "move1.sps", "move1.power", "move1.accuracy", "move1.pp", // move1
                "move2.id", "move2.name", "move2.type", "move2.sps", "move2.power", "move2.accuracy", "move2.pp", // move2
                "move3.id", "move3.name", "move3.type", "move3.sps", "move3.power", "move3.accuracy", "move3.pp", // move3
                "move4.id", "move4.name", "move4.type", "move4.sps", "move4.power", "move4.accuracy", "move4.pp", // move4
                "items.name", "items.uniq", // item
                "sets.EV1", "sets.EV2", "sets.nature+", "sets.nature-" // evDist, nature
            )[0];

            int[] baseStat = new int[5];
            for (int k = 0; k < 5; k++)
                baseStat[k] = int.Parse(r["pokemons." + ((StatTarget)k).ShortString()]) + 31; // 31: IVs

            Base thisBase = new Base(r["pokemons.name"], TypeExtensions.Parse(r["pokemons.type1"]), TypeExtensions.Parse(r["pokemons.type2"]), baseStat);

            Move[] thisMoves = new Move[4];
            for (int k = 1; k < 5; k++)
            {
                List<Effect> moveEffects = new List<Effect>();
                var r_ = dbo.Select(
                    new Table("moves", int.Parse(r[$"move{k}.id"]))
                        .Join("movesxeffects", "move", "moves.id")
                        .Join("effects", "movesxeffects.effect"),
                    "effects.id", "effects.desc", "movesxeffects.percent", "movesxeffects.value"
                );
                foreach (var effect in r_)
                {
                    moveEffects.Add(new Effect(int.Parse(effect["effects.id"]), effect["effects.desc"], int.Parse(effect["movesxeffects.percent"]), int.Parse(effect["movesxeffects.value"])));
                }

                thisMoves[k - 1] = new Move(r[$"move{k}.name"], TypeExtensions.Parse(r[$"move{k}.type"]), SpsExtensions.Parse(r[$"move{k}.sps"]), int.Parse(r[$"move{k}.power"]), int.Parse(r[$"move{k}.accuracy"]), int.Parse(r[$"move{k}.pp"]), moveEffects.ToArray());
            }

            Item thisItem = new Item(r["items.name"], r["items.uniq"] == "1");

            EVDist thisEV = new EVDist(r["sets.EV1"], r["sets.EV2"]);
            Nature thisNature = new Nature(r["sets.nature+"], r["sets.nature-"]);

            return new Set(r["sets.name"], int.Parse(r["pokemons.hp"]), thisBase, thisMoves, thisItem, thisEV, thisNature);
        }

        public String GetName()
        {
            return $"{this.baseStat.name} '{this.customName}'";
        }

        public override String ToString()
        {
            String moves = "";
            String potentialNext = "";

            foreach (var p in this.moves)
                moves += $"\n\t\t- {p}";

            Move nextMove = this.NextMove;
            if (nextMove != null)
                potentialNext = $"\n\t\the will use {nextMove}";

            return $"{this.GetName()} @{this.item}{moves}{potentialNext}";
        }
    }
}

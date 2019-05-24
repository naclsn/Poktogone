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
                case StatTarget.HP: return "pdv";
                default: return "oof";
            }
        }
    }

    enum Flags
    {
        None = 0,
        Substitute = 1,
        Flinch = 2,
        Confusion = 4,//pas utilisée
        LeechSeed = 8,
        Protect = 16,
        MagmaStorm = 32,
        Locked = 64,
        Charge = 128,
        Recharge = 256,
        Taunt = 512,
        Roost = 1024,
    }

    enum Status
    {
        None,
        Burn,
        Freeze,
        Paralysis,
        Poison,
        BadlyPoisoned,
        Sleep,
        Dead
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
        public Item item { get; set; }
        public Ability ability { get; set; }

        private Flags flags;
        private Status _status;
        public Status Status
        {
            get { return _status; }
            set
            {
                if(value == Status.None)
                {
                    _status = value;
                }
                else if(this._status == Status.None)
                {
                    _status = value;
                }
            }
        }

        EVDist _evDist;
        Nature _nature;

        int nbTurns = 1;
        int magmaStormCount;
        int tauntCounter;
        int sleepCounter;

        public int GetNbTurns()
        {
            return this.nbTurns;
        }
        public void IncNbTurn()
        {
            this.nbTurns++;
            this._indexLastMove = this._indexNextMove;

            if (this.HasFlags(Flags.MagmaStorm) && 3 < this.magmaStormCount++)
            {
                this.RemoveFlags(Flags.MagmaStorm);
                this.magmaStormCount = 0;
            }

            if (this.HasFlags(Flags.Taunt) && 3 < this.tauntCounter++)
            {
                this.RemoveFlags(Flags.Taunt);
                this.tauntCounter = 0;
            }

            if (this.Status == Status.Sleep && 2 < this.sleepCounter++)
            {
                this.Status = Status.None;
                this.sleepCounter = 0;
            }
        }
        public void RstNbTurn()
        {
            this.nbTurns = 1;
        }

        int[] _mod = new int[5];
        
        private int _hp;
        public int Hp
        {
            get { return this._hp; }
            set
            {
                if (value < 0)
                    this._hp = 0;
                else if (this[StatTarget.HP] < value)
                    this._hp = this[StatTarget.HP];
                else
                    this._hp = value;

                if (this._hp == 0)
                    this._status = Status.Dead;
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
        }

        public void SetNextMove(int i)
        {
            this._indexNextMove = i;
        }
        private int _indexLastMove;
        public Move LastMove
        {
            get
            {
                if (-1 < this._indexLastMove)
                    return this.moves[this._indexLastMove];
                return null;
            }
        }
        public int this[StatTarget stat]
        {
            get
            {
                if (stat == StatTarget.HP)
                    return this.baseStat[stat];

                int mod = this._mod[(int)stat];
                int baz = this.baseStat[stat]; // base stats
                double r = baz + (stat == this._evDist.ev1 || stat == this._evDist.ev2 ? 63 : 0); // EVs
                r = (int)(r * (this._nature.up == stat ? 1.1 : this._nature.down == stat ? .9 : 1)); // nature

                if (this.Status == Status.Paralysis && stat == StatTarget.Speed)//Paralysis
                    r = (int)(r * 0.5);

                return (int)(r * (mod < 0 ? 1 / (1 - .5 * mod) : 1 + .5 * mod)); // modif
            }
            set // e.g.: shif gears -> pok[StatTarget] = 2 --> actualy does +2
            {
                if (this.ability.id == 48)//ContraryExeption
                {
                    if (value == 0)
                        this._mod[(int)stat] = 0;
                    else if (-7 < this._mod[(int)stat] + value && this._mod[(int)stat] + value < 7)
                        this._mod[(int)stat] -= value;
                }
                else
                {
                    if (value == 0)
                        this._mod[(int)stat] = 0;
                    else if (-7 < this._mod[(int)stat] + value && this._mod[(int)stat] + value < 7)
                        this._mod[(int)stat] += value;
                }
            }
        }

        public Set(String customName, Base baseStat, Move[] moves, Item item, Ability ability, EVDist evDist, Nature nature)
        {
            this.customName = customName;

            this.baseStat = baseStat;
            this.Hp = this[StatTarget.HP];

            this.moves = moves;
            this.item = item;
            this.ability = ability;

            this._evDist = evDist;
            this._nature = nature;

            this._indexNextMove = -1;

            this.flags = Flags.None;
            this._status = Status.None;
        }

        public Set Copy()
        {
            return new Set(this.customName, this.baseStat, this.moves, this.item.Copy(), this.ability.Copy(), this._evDist, this._nature);
        }

        public static Set FromDB(SqlHelper dbo, int id)
        {
            Program.Log("dbo", "Selecting set from database : no " + id);
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
                "items.id", "items.name", "items.uniq", "abilities.id", "abilities.name", // item & ability
                "sets.EV1", "sets.EV2", "sets.nature+", "sets.nature-" // evDist, nature
            )[0];
            Program.Log("dbo", "\tSelected!");

            EVDist thisEV = new EVDist(r["sets.EV1"], r["sets.EV2"]);
            Nature thisNature = new Nature(r["sets.nature+"], r["sets.nature-"]);

            int[] baseStat = new int[6];
            for (int k = 0; k < 5; k++)
                baseStat[k] = 2 * int.Parse(r["pokemons." + ((StatTarget)k).ShortString()]) + 5 + 31; // 31: IVs
            baseStat[5] = 2 * int.Parse(r["pokemons.hp"]) + 110 + 31;
            if (thisEV.ev1 == StatTarget.HP || thisEV.ev2 == StatTarget.HP)
                baseStat[5] += 63;

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

                thisMoves[k - 1] = new Move(int.Parse(r[$"move{k}.id"]), r[$"move{k}.name"], TypeExtensions.Parse(r[$"move{k}.type"]), SpsExtensions.Parse(r[$"move{k}.sps"]), int.Parse(r[$"move{k}.power"]), int.Parse(r[$"move{k}.accuracy"]), int.Parse(r[$"move{k}.pp"]), moveEffects.ToArray());
            }

            Item thisItem = new Item(int.Parse(r["items.id"]), r["items.name"], r["items.uniq"] == "1");
            Ability thisAbility = new Ability(int.Parse(r["abilities.id"]), r["abilities.name"]);

            return new Set(r["sets.name"], thisBase, thisMoves, thisItem, thisAbility, thisEV, thisNature);
        }

        public void SetFlags(params Flags[] flags)
        {
            this.flags = flags.Aggregate((Flags s, Flags c) => s | c);
        }

        public void AddFlags(params Flags[] flags)
        {
            this.flags |= flags.Aggregate((Flags s, Flags c) => s | c);
        }

        public bool HasFlags(params Flags[] flags)
        {
            return (this.flags & flags.Aggregate((Flags s, Flags c) => s | c)) != Flags.None;
        }

        public void RemoveFlags(params Flags[] flags)
        {
            this.flags &= flags.Aggregate((Flags s, Flags c) => s | ~c);
        }

        public void RemoveFlags()
        {
            this.flags = Flags.None;
        }

        public int GetMaxHp()
        {
            return this.baseStat[StatTarget.HP];
        }

        public Type Type1
        {
            get { return this.baseStat.type1; }
            set { this.baseStat.type1 = value;  }
        }
        public Type Type2
        {
            get { return this.baseStat.type2; }
            set { this.baseStat.type2 = value; }
        }
        
        public bool IsStab(Type type)
        {
            return (type == this.baseStat.type1 || type == this.baseStat.type2);
        }

        public int GetMod(StatTarget mods)
        {
            return this._mod[(int)mods];
        }

        public int[] GetAllMods()
        {
            return this._mod;
        }

        public int GetIndexLastMove()
        {
            return _indexLastMove;
        }

        public String GetName()
        {
            return $"{this.baseStat.name}";
        }

        public String GetFullName()
        {
            return $"{this.baseStat.name} '{this.customName}'";
        }

        public String Repr()
        {
            String r = "";

            String moves = "";
            String potentialNext = null;

            int k = 0;
            foreach (var m in this.moves)
                moves += $"\t{k++}. {m}\n";

            Move nextMove = this.NextMove;
            if (nextMove != null)
                potentialNext = $"will use {nextMove}";

            String flagsDesc = "", sep = "";
            foreach (Flags f in Enum.GetValues(typeof(Flags)))
            {
                if (this.HasFlags(f))
                {
                    flagsDesc += $"{sep}{f}";
                    sep = ",";
                }
            }
            if (flagsDesc == "")
                flagsDesc = "None";

            r += $"{this.GetFullName()} @{this.item}\n";
            r += new String('-', r.Length - 1) + "\n\n";
            r += $"({potentialNext ?? "waiting"}) hp: {this.Hp} / {this.GetMaxHp()} | status: {this.Status} | flags: {flagsDesc}\n";
            r += $"{moves}";

            return r;
        }

        public override String ToString()
        {
            String r = "";

            String status = "";
            switch (this.Status)
            {
                case Status.Burn:
                    status = " (brulé)";
                    break;
                case Status.Dead:
                    status = " (mort)";
                    break;
                case Status.Paralysis:
                    status = " (paralisé)";
                    break;
                case Status.Poison:
                    status = " (empoisonné)";
                    break;
                case Status.BadlyPoisoned:
                    status = " (gravement empoisonné)";
                    break;
                case Status.Freeze:
                    status = " (gelé)";
                    break;
                case Status.Sleep:
                    status = " (endormi)";
                    break;
            }

            String flagsDesc = "", sep = "";
            foreach (Flags f in Enum.GetValues(typeof(Flags)))
            {
                if (this.HasFlags(f))
                {
                    flagsDesc += $"{sep}{f}";
                    sep = ",";
                }
            }

            double ratio = (double)this.Hp / this.GetMaxHp();
            
            r += $"{this.GetName()}{status}\n";
            r += "$2" + new String('█', (int)(ratio * 60)) + "$4" + new String('█', (int)((1 - ratio) * 60)) + $"$F ({(int)(ratio * 100)}%)";
            r += $"{flagsDesc}\n";

            return r;
        }

        public String ToStringPlayer()
        {
            String r = "";

            String status = "";
            switch (this.Status)
            {
                case Status.Burn:
                    status = " (brulé)";
                    break;
                case Status.Dead:
                    status = " (mort)";
                    break;
                case Status.Paralysis:
                    status = " (paralisé)";
                    break;
                case Status.Poison:
                    status = " (empoisonné)";
                    break;
                case Status.BadlyPoisoned:
                    status = " (gravement empoisonné)";
                    break;
                case Status.Freeze:
                    status = " (gelé)";
                    break;
                case Status.Sleep:
                    status = " (endormi)";
                    break;
            }

            String flagsDesc = "", sep = "";
            foreach (Flags f in Enum.GetValues(typeof(Flags)))
            {
                if (this.HasFlags(f))
                {
                    flagsDesc += $"{sep}{f}";
                    sep = ",";
                }
            }

            double ratio = (double) this.Hp / this.GetMaxHp();

            r += $"{this.GetFullName()} [{this.Type1}{(this.Type2 == Type.None ? "" : $" | {this.Type2}")}]{status}\n";
            r += "$2" + new String('█', (int)(ratio * 60)) + "$4" + new String('█', (int)((1 - ratio) * 60)) + $"$F ({(int)(ratio * 100)}%)\n";
            r += $"Objet équipé : {this.item}\n";
            r += $"{flagsDesc}\n";
            r += "\n";
            r += "Attaques :\n";

            int k = 0;
            foreach (Move m in this.moves)
                r += $"\t{k++} : {m}\n";

            return r;
        }

        public StatTarget GetBestStat()
        {
            List<int> stats = new List<int>() { this.baseStat[StatTarget.Attack], this.baseStat[StatTarget.Defence], this.baseStat[StatTarget.AttackSpecial], this.baseStat[StatTarget.DefenceSpecial], this.baseStat[StatTarget.Speed] };
            int indBest = 0;
            for(int i = 0; i<5; i++)
            {
                if (stats[i] > stats[indBest]) { indBest = i; }
            }
            return (StatTarget)indBest;
        }

        public void RemoveItem()
        {
            this.item.Remove();
            if (this.ability.id == 26) { this[StatTarget.Speed] = 2; }
        }
    }
}

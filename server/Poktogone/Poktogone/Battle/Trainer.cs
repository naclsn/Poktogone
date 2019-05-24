using Poktogone.Pokemon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poktogone.Battle
{
    enum Hazards
    {
        None = 0,
        StealthRock = 1,
        Spikes = 2,
        Spikes2 = 4,
        Spikes3 = 8,
        StickyWeb = 16,
        ToxicSpikes = 32,
        ToxicSpikes2 = 64,

        Reflect = 128,
        LightScreen = 256,
        HealingWish = 512
    }

    class Trainer
    {
        private String name;
        private Set[] pokemons;
        private Hazards hazards;
        public int playerNumber;

        private int reflectCounter;
        private int lightScreenCounter;

        private int _indexPokemonOut;
        public Set Pokemon
        {
            get { return this.pokemons[_indexPokemonOut]; }
        }
        private String _nextCommand;
        public String NextAction
        {
            get { return this._nextCommand; }
            set
            {
                this._nextCommand = value;
                
                if (value == "...")
                    this.Pokemon.SetNextMove(-1);
                else if (value.StartsWith("attack"))
                    this.Pokemon.SetNextMove(int.Parse(value.Replace("attack", "").Trim()));
            }
        }

        public Trainer(String name, int playerNumber, Set[] pokemons)
        {
            this.name = name;
            this.pokemons = pokemons;
            this._indexPokemonOut = 0;
            this.NextAction = "...";
            this.hazards = Hazards.None;
            this.playerNumber = playerNumber;
        }

        public Trainer Copy()
        {
            Set[] copySet = new Set[this.pokemons.Length];

            for (int k = 0; k < copySet.Length; k++)
                copySet[k] = this.pokemons[k].Copy();

            return new Trainer(this.name, this.playerNumber, copySet);
        }

        public void SwitchTo(int i)
        {
            this.Pokemon.RstNbTurn();
            this._indexPokemonOut = i;
        }

        public void IncNbTurn()
        {
            if (this.HasHazards(Hazards.Reflect) && 4 < this.reflectCounter++)
            {
                this.RemoveHazards(Hazards.Reflect);
                this.reflectCounter = 0;
            }

            if (this.HasHazards(Hazards.LightScreen) && 4 < this.lightScreenCounter++)
            {
                this.RemoveHazards(Hazards.LightScreen);
                this.lightScreenCounter = 0;
            }
        }

        public void SetHazards(params Hazards[] hazards)
        {
            this.hazards = hazards.Aggregate((Hazards s, Hazards c) => s | c);
        }

        public void AddHazards(params Hazards[] hazards)
        {
            this.hazards |= hazards.Aggregate((Hazards s, Hazards c) => s | c);
        }

        public bool HasHazards(params Hazards[] hazards)
        {
            return (this.hazards & hazards.Aggregate((Hazards s, Hazards c) => s | c)) != Hazards.None;
        }

        public void RemoveHazards(params Hazards[] hazards)
        {
            this.hazards &= hazards.Aggregate((Hazards s, Hazards c) => s | ~c);
        }

        public void RemoveHazards()
        {
            this.hazards = Hazards.None;
        }

        public Set GetAPokemon(int i)
        {
            return this.pokemons[i];
        }

        public bool HasPokemon()
        {
            //return this.pokemons.Aggregate(false, (bool a, Set p) => a || p.Status != Status.Dead);
            return this.pokemons[0].Status != Status.Dead || this.pokemons[1].Status != Status.Dead || this.pokemons[2].Status != Status.Dead;
        }

        public bool HasPokemonLeft()
        {
            bool r = false;
            for (int i = 0; i < 3 && r==false; i++)
            {
                if (i != this._indexPokemonOut) 
                    r = this.pokemons[i].Status != Status.Dead;
            }
            return r;
        }

        public String GetName()
        {
            return this.name;
        }

        public String Repr()
        {
            String r = "";

            String pokemons = "";
            int k = 0;
            foreach (var p in this.pokemons)
                pokemons += $"\t{k++}: {p.GetName()} (hp: {p.Hp} / {p.GetMaxHp()} | status: {p.Status})\n";
            
            String hazardsDesc = "", sep = "";
            foreach (Hazards h in Enum.GetValues(typeof(Hazards)))
            {
                if (this.HasHazards(h))
                {
                    hazardsDesc += $"{sep}{h}";
                    sep = ",";
                }
            }
            if (hazardsDesc == "")
                hazardsDesc = "None";

            r += $"{this.name}\n";
            r += new String('=', r.Length - 1) + "\n\n";
            r += $"(next action: {this.NextAction})\n";
            r += $"{this.Pokemon.Repr()}\n";
            r += $"team: {pokemons}";
            r += $"hazards: {hazardsDesc}";

            return r;
        }

        public override String ToString()
        {
            String r = "";

            r += $"Equipe de {this.name} : ";
            foreach (var p in this.pokemons)
            {
                switch (p.Status)
                {
                    case Status.None:
                        r += "o";
                        break;
                    case Status.Burn:
                        r += "$Co$F";
                        break;
                    case Status.Dead:
                        r += "$8o$F";
                        break;
                    case Status.Paralysis:
                        r += "$Eo$F";
                        break;
                    case Status.Poison:
                        r += "$Do$F";
                        break;
                    case Status.BadlyPoisoned:
                        r += "$5o$F";
                        break;
                    case Status.Freeze:
                        r += "$9o$F";
                        break;
                    case Status.Sleep:
                        r += "z";
                        break;
                }
                r += " ";
            }

            r += "\n";
            r += $"{this.Pokemon}\n";

            return r;
        }

        public String ToStringPlayer()
        {
            String r = "";

            r += $"Ton équipe, {this.name} : ";
            foreach (var p in this.pokemons)
            {
                switch (p.Status)
                {
                    case Status.None:
                        r += "o";
                        break;
                    case Status.Burn:
                        r += "$Co$F";
                        break;
                    case Status.Dead:
                        r += "$8o$F";
                        break;
                    case Status.Paralysis:
                        r += "$Eo$F";
                        break;
                    case Status.Poison:
                        r += "$Do$F";
                        break;
                    case Status.BadlyPoisoned:
                        r += "$5o$F";
                        break;
                    case Status.Freeze:
                        r += "$9o$F";
                        break;
                    case Status.Sleep:
                        r += "z";
                        break;
                }
                r += " ";
            }

            r += "\n\n";
            r += $"{this.Pokemon.ToStringPlayer()}\n";

            r += "\n";
            r += "Pokémons :\n";

            int k = 0;
            foreach (Set p in this.pokemons)
                r += $"\t{k++} : {p.GetFullName()}\n";

            r += "\n";

            return r;
        }
    }
}

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
        private Pokemon.Set[] pokemons;
        private Hazards hazards;

        private int _indexPokemonOut;
        public Pokemon.Set Pokemon
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

        public Trainer(String name, Pokemon.Set[] pokemons)
        {
            this.name = name;
            this.pokemons = pokemons;
            this._indexPokemonOut = 0;
            this.NextAction = "...";
            this.hazards = Hazards.None;
        }

        public void SwitchTo(int i)
        {
            this.Pokemon.RstNbTurn();
            this._indexPokemonOut = i;
        }

        public void IncNbTurn()
        {
            
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
            this.hazards ^= hazards.Aggregate((Hazards s, Hazards c) => s | c);
        }

        public void RemoveHazards()
        {
            this.hazards = Hazards.None;
        }


        public String GetName()
        {
            return this.name;
        }

        public override string ToString()
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
            r += $"{this.Pokemon}\n";
            r += $"team: {pokemons}";
            r += $"hazards: {hazardsDesc}";

            return r;
        }
    }
}

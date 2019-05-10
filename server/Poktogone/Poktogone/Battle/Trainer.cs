using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poktogone.Battle
{
    class Trainer
    {
        private String name;
        private Pokemon.Set[] pokemons;

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
                    this.Pokemon.NextMove = null;
                else if (value.StartsWith("attack"))
                    this.Pokemon.NextMove = Poktogone.Pokemon.Move.TmpFromName(value.Replace("attack", "").Trim());
            }
        }

        public Trainer(String name, Pokemon.Set[] pokemons)
        {
            this.name = name;
            this.pokemons = pokemons;
            this._indexPokemonOut = 0;
            this.NextAction = "...";
        }

        public void SwitchTo(int i)
        {
            this._indexPokemonOut = i;
        }

        public String GetName()
        {
            return this.name;
        }

        public override string ToString()
        {
            String pokemons = "";

            foreach (var p in this.pokemons)
                pokemons += $"\n\t{p.GetName()}";

            return $"{this.name} with: {this.Pokemon}\n\tnext action: {this.NextAction}\n  team: {pokemons}";
        }
    }
}

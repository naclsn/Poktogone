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

        public Trainer(String name, Pokemon.Set[] pokemons)
        {
            this.name = name;
            this.pokemons = pokemons;
        }

        public String GetName()
        {
            return this.name;
        }

        public override string ToString()
        {
            String pokemons = "";

            foreach (var p in this.pokemons)
                pokemons += $"\n\t{p}";

            return $"{this.name} with: {pokemons}";
        }
    }
}

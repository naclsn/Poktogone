using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poktogone.Battle
{
    enum WeatherType
    {
        ClearSky,
        HarshSunlight,
        Rain,
        Sandstorm,
        Hail
    }

    enum TerrainType
    {
        Normal,
        Eletric,
        Grassy,
        Misty,
        Psychic
    }

    class Stage
    {
        public WeatherType weather { get; set; }
        public TerrainType terrain { get; set; }

        public Stage()
        {
            this.weather = WeatherType.ClearSky;
            this.terrain = TerrainType.Normal;
        }

        public override string ToString()
        {
            return $"\n\tweather: {this.weather}\n\tterrain: {this.terrain}";
        }
    }
}

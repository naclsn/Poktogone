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
        private WeatherType _weather;
        private int _weatherNbTurn;
        public WeatherType Weather
        {
            get
            {
                return this._weather;
            }
            set
            {
                this._weather = value;
                this._weatherNbTurn = 0;
            }
        }

        private TerrainType _terrain;
        private int _terrainNbTurn;
        public TerrainType Terrain
        {
            get
            {
                return this._terrain;
            }
            set
            {
                this._terrain = value;
                this._terrainNbTurn = 0;
            }
        }

        public Stage()
        {
            this.Weather = WeatherType.ClearSky;
            this.Terrain = TerrainType.Normal;
        }

        public void IncNbTurn()
        {
            if (4 < this._weatherNbTurn++)
                this.Weather = WeatherType.ClearSky;
            if (4 < this._terrainNbTurn++)
                this.Terrain = TerrainType.Normal;
        }

        public override string ToString()
        {
            
            return $"\tweather: {this.Weather}\n\tterrain: {this.Terrain}\n";
        }
    }
}

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
            if (this.Weather != WeatherType.ClearSky && 4 < this._weatherNbTurn++)
                this.Weather = WeatherType.ClearSky;
            if (this.Terrain != TerrainType.Normal && 4 < this._terrainNbTurn++)
                this.Terrain = TerrainType.Normal;
        }

        public String Repr()
        {
            return $"\tweather: {this.Weather}\n\tterrain: {this.Terrain}\n";
        }

        public override String ToString()
        {
            String r = "";

            String t = "";
            switch (this.Terrain)
            {
                case TerrainType.Eletric:
                    t += " électrique";
                    break;
                case TerrainType.Grassy:
                    t += " herbeux";
                    break;
                case TerrainType.Misty:
                    t += " mystique";
                    break;
                case TerrainType.Psychic:
                    t += " psychique";
                    break;
            }

            switch (this.Weather)
            {
                case WeatherType.Hail:
                    r += $"La grêle s'abbat sur le terrain{t} ! (tours {this._weatherNbTurn + 1} / 5)";
                    break;
                case WeatherType.HarshSunlight:
                    r += $"le soleil brille fort sur le terrain{t} ! (tours {this._weatherNbTurn + 1} / 5)";
                    break;
                case WeatherType.Rain:
                    r += $"La pluie s'abbat sur le terrain{t} ! (tours {this._weatherNbTurn + 1} / 5)";
                    break;
                case WeatherType.Sandstorm:
                    r += $"La tempête de sable fait rage sur le terrain{t} ! (tours {this._weatherNbTurn + 1} / 5)";
                    break;
            }

            return r;
        }
    }
}

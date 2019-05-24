using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Poktogone.Pokemon;
using Poktogone.Main;

namespace Poktogone.Battle
{
    enum BattleState
    {
        Starting,
        Waiting,
        WaitingP1,
        WaitingP2,
        VictoryP1,
        VictoryP2,
        Unknown
    }

    class Battle
    {
        private Trainer P1;
        private Trainer P2;

        private Stage stage;

        public BattleState State { get; private set; }

        public Battle(Trainer P1, Trainer P2)
        {
            this.P1 = P1;
            this.P2 = P2;

            this.stage = new Stage();

            this.State = BattleState.Starting;
        }

        public int InputCommand(int player, String c)
        {
            if (player == 1 || player == 2)
            {
                if (!c.StartsWith("attack") && !c.StartsWith("switch"))
                    return 0;
                
                player--;
                Trainer[] P = { this.P1, this.P2 };
                BattleState[] S = { BattleState.WaitingP2, BattleState.WaitingP1};

                if (c.StartsWith("switch") && !this.CanSwitch(P[player], int.Parse(c.Replace("switch", "").Trim())))
                    return 0;

                // switch si ded (ne compte pas comme jouer) <-- note : cette situation de devrait plus jamais arrivée...
                if (P[player].Pokemon.Status == Status.Dead)
                {
                    if (c.StartsWith("switch"))
                    {
                        //P[player].SwitchTo(int.Parse(c.Replace("switch", "").Trim()));
                        int choice = int.Parse(c.Replace("switch", "").Trim());
                        if (this.CanSwitch(P[player], choice))
                        {
                            this.DoSwitch(P[player], P[1 - player], choice);
                            Program.Println($"{P[player].GetName()} send out {P[player].Pokemon.GetName()}!");
                            return player + 1;
                        }
                        else return 0;
                    }
                    else return 0;
                }

                if (P[player].Pokemon.HasFlags(Flags.Locked) && int.Parse(c.Replace("attack", "")) != P[player].Pokemon.GetIndexLastMove())
                    return 0;
                
                Program.Println($"{P[player].GetName()}{(this.State == S[player] ? " changes its mind and" : "")} will do '{c}'.");
                P[player].NextAction = c;

                if (this.State != BattleState.Waiting && this.State != S[player])
                {
                    Program.Log("turn", "Playing turn in `DoTurn`");
                    this.DoTurn();
                    Program.Log("turn", "\tPlayed!");

                    this.State = BattleState.Waiting;
                    if (!this.P1.HasPokemon()) this.State = BattleState.VictoryP2;
                    if (!this.P2.HasPokemon()) this.State = BattleState.VictoryP1;
                }
                else
                {
                    this.State = S[player];
                }

                return player + 1;
            }
            return -1;
        }

        public void DoTurn()
        {
            // Ordre tour
            //
            // 1- Poursuite si switch
            // 2- Switch (+Natural cure et regenerator)
            // 3- Hazards si switch
            // 4- Talents switch
            // 5- Mega-évo
            //
            // damageCalc
            // ----------
            // 6- Attaques poké 1
            // 7- Effet attaque poké 1 (effets → recul → baies → effet de kill)
            // 8- De même poké 2
            // ----------
            //
            // 9- Restes
            // 10- poison / toxic / burn
            // 11- Leech Seed
            // 12- Hail / Sand + Rain dish
            // 13- Décomptes tour

            bool isP1Attack = this.P1.NextAction.StartsWith("attack");
            bool isP2Attack = this.P2.NextAction.StartsWith("attack");
            bool isP1Switch = this.P1.NextAction.StartsWith("switch");
            bool isP2Switch = this.P2.NextAction.StartsWith("switch");

            // 1- Poursuite si switch
            if (isP2Switch && isP1Attack && this.P1.Pokemon.NextMove.id == 85/*Poursuite*/)
                Calc.DamageCalculator(this.stage, this.P1.Pokemon, this.P2.Pokemon, this.P1, this.P2); // p1 fait poursuite
            if (isP1Switch && isP2Attack && this.P2.Pokemon.NextMove.id == 85/*Poursuite*/)
                Calc.DamageCalculator(this.stage, this.P2.Pokemon, this.P1.Pokemon, this.P2, this.P1); // p2 fait poursuite

            // 2-, 3- et 4- Switch
            if (isP1Switch)
                this.DoSwitch(this.P1, this.P2, int.Parse(this.P1.NextAction.Replace("switch", "").Trim()));
            if (isP2Switch)
                this.DoSwitch(this.P2, this.P1, int.Parse(this.P2.NextAction.Replace("switch", "").Trim()));

            // 6-, 7- et 8-
            Trainer[] order = this.OrderPrioriry();
            if (order[0].NextAction.StartsWith("attack") && order[0].Pokemon.Status != Status.Dead)
            {
                Set poke1before = order[1].Pokemon;
                int damage = Calc.DamageCalculator(this.stage, order[0].Pokemon, order[1].Pokemon, order[0], order[1]);

                Program.Log("dmc", "\n\n");
                Program.Log("dmc", $"{damage} damages to {order[1].Pokemon.GetName()} ({order[1].Pokemon.Hp} / {order[1].Pokemon.GetMaxHp()})");
                Program.Log("dmc", $"(from {order[0].Pokemon.GetName()}'s {order[0].Pokemon.NextMove})");
                int HpBeforeDmg = order[1].Pokemon.Hp; //Utilisé pour le Recul

                Calc.InflictDamage(damage, order[0].Pokemon, order[1].Pokemon);

                if (order[0].Pokemon.NextMove[6] != null)//Recoil
                {
                    order[0].Pokemon.Hp -= (int)(order[0].Pokemon.NextMove[6].Value.value * (HpBeforeDmg - order[1].Pokemon.Hp) / 100f);
                }
                if(order[1].Pokemon.ability.id == 39)//Defiant
                {
                    if (Calc.DefiantActive(poke1before, order[1].Pokemon))
                    {
                        order[1].Pokemon[StatTarget.Attack] = 2;
                    }
                }

                if (order[1].Pokemon.ability.id == 1 && Program.RngNext(3) == 0 && order[0].Pokemon.NextMove.sps == Sps.Physic)//Static
                {
                    order[0].Pokemon.Status = Status.Paralysis;
                }

                if (order[1].Pokemon.ability.id == 52 && Program.RngNext(3) == 0 && order[0].Pokemon.NextMove.sps == Sps.Physic)//FlameBody
                {
                    order[0].Pokemon.Status = Status.Burn;
                }

                if (order[0].Pokemon.NextMove[22] != null)//Pivotage
                {
                    order[0].SwitchTo(Program.RequireSwitch(order[0]));
                }
            }
            if (order[1].NextAction.StartsWith("attack") && order[1].Pokemon.Status != Status.Dead)
            {
                Set poke2before = order[0].Pokemon;
                int damage = Calc.DamageCalculator(this.stage, order[1].Pokemon, order[0].Pokemon, order[1], order[0]);

                Program.Log("dmc", "\n\n");
                Program.Log("dmc", $"{damage} damages to {order[0].Pokemon.GetName()} ({order[0].Pokemon.Hp} / {order[0].Pokemon.GetMaxHp()})");
                Program.Log("dmc", $"(from {order[1].Pokemon.GetName()}'s {order[1].Pokemon.NextMove})");

                Calc.InflictDamage(damage, order[1].Pokemon, order[0].Pokemon);

                int HpBeforeDmg = order[0].Pokemon.Hp; //Utilisé pour le Recul
                if (order[1].Pokemon.NextMove[6] != null && P1.Pokemon.ability.id != 10)//Recoil
                {
                    order[1].Pokemon.Hp -= (int)(order[1].Pokemon.NextMove[6].Value.value * (HpBeforeDmg - order[0].Pokemon.Hp) / 100f);
                }
                if (order[0].Pokemon.ability.id == 39)//Defiant
                {
                    if (Calc.DefiantActive(poke2before, order[0].Pokemon))
                    {
                        order[0].Pokemon[StatTarget.Attack] = 2;
                    }
                }

                if(order[0].Pokemon.ability.id == 1 && Program.RngNext(3)==0 && order[1].Pokemon.NextMove.sps == Sps.Physic)//Static
                {
                    order[1].Pokemon.Status = Status.Paralysis;
                }

                if (order[0].Pokemon.ability.id == 52 && Program.RngNext(3) == 0 && order[1].Pokemon.NextMove.sps == Sps.Physic)//FlameBody
                {
                    order[1].Pokemon.Status = Status.Burn;
                }

                if (order[1].Pokemon.NextMove[22] != null)//Pivotage
                {
                    order[1].SwitchTo(Program.RequireSwitch(order[1]));
                }
            }

            // 9- Restes
            if (this.P1.Pokemon.item.id == 9/*Restes*/)
                this.P1.Pokemon.Hp += (int)(this.P1.Pokemon.GetMaxHp() / 16.0);
            if (this.P2.Pokemon.item.id == 9/*Restes*/)
                this.P2.Pokemon.Hp += (int)(this.P2.Pokemon.GetMaxHp() / 16.0);

            // 10- poison / toxic / burn
            if (this.P1.Pokemon.Status == Status.Poison && P1.Pokemon.ability.id != 10)
            {
                if (this.P1.Pokemon.ability.id == 31/*Soin poison*/)
                    this.P1.Pokemon.Hp += (int)(this.P1.Pokemon.GetMaxHp() / 8.0);
                else
                    this.P1.Pokemon.Hp -= (int)(this.P1.Pokemon.GetMaxHp() / 8.0);
            }
            if (this.P2.Pokemon.Status == Status.Poison && P2.Pokemon.ability.id != 10)
            {
                if (this.P2.Pokemon.ability.id == 31/*Soin poison*/)
                    this.P2.Pokemon.Hp += (int)(this.P2.Pokemon.GetMaxHp() / 8.0);
                else
                    this.P2.Pokemon.Hp -= (int)(this.P2.Pokemon.GetMaxHp() / 8.0);
            }
            if (this.P1.Pokemon.Status == Status.BadlyPoisoned && P1.Pokemon.ability.id != 10)
            {
                this.P1.Pokemon.Hp -= (int)(this.P1.Pokemon.GetMaxHp() * this.P1.Pokemon.GetNbTurns() / 16.0);
            }
            if (this.P1.Pokemon.Status == Status.BadlyPoisoned && P2.Pokemon.ability.id != 10)
            {
                this.P1.Pokemon.Hp -= (int)(this.P1.Pokemon.GetMaxHp() * this.P1.Pokemon.GetNbTurns() / 16.0);
            }
            if (this.P1.Pokemon.Status == Status.Burn && P1.Pokemon.ability.id != 10)
            {
                this.P1.Pokemon.Hp -= (int)(this.P1.Pokemon.GetMaxHp() / 16.0);
            }
            if (this.P2.Pokemon.Status == Status.Burn && P2.Pokemon.ability.id != 10)
            {
                this.P2.Pokemon.Hp -= (int)(this.P2.Pokemon.GetMaxHp() / 16.0);
            }

            // 11- Leech Seed
            if (this.P1.Pokemon.HasFlags(Flags.LeechSeed) && P1.Pokemon.ability.id != 10)
            {
                int tmp = (int)(this.P1.Pokemon.GetMaxHp() / 8.0);
                this.P1.Pokemon.Hp -= tmp;
                this.P2.Pokemon.Hp += tmp;
            }
            //Program.Log("turn:11-", "P2");
            if (this.P2.Pokemon.HasFlags(Flags.LeechSeed) && P2.Pokemon.ability.id != 10)
            {
                int tmp = (int)(this.P2.Pokemon.GetMaxHp() / 8.0);
                this.P2.Pokemon.Hp -= tmp;
                this.P1.Pokemon.Hp += tmp;
            }

            // 12- Hail / Sand + Rain dish
            if (this.stage.Weather == WeatherType.Hail)
            {
                if ((!this.P1.Pokemon.IsStab(Pokemon.Type.Glace)) && P1.Pokemon.ability.id != 10)
                    this.P1.Pokemon.Hp -= (int)(this.P1.Pokemon.GetMaxHp() / 16.0);
                if ((!this.P2.Pokemon.IsStab(Pokemon.Type.Glace)) && P2.Pokemon.ability.id != 10)
                    this.P2.Pokemon.Hp -= (int)(this.P2.Pokemon.GetMaxHp() / 16.0);
            }
            if (this.stage.Weather == WeatherType.Sandstorm)
            {
                if (!this.P1.Pokemon.IsStab(Pokemon.Type.Roche) && !this.P1.Pokemon.IsStab(Pokemon.Type.Sol) && !this.P1.Pokemon.IsStab(Pokemon.Type.Acier) && P1.Pokemon.ability.id != 10)
                    this.P1.Pokemon.Hp -= (int)(this.P1.Pokemon.GetMaxHp() / 16.0);
                if (!this.P2.Pokemon.IsStab(Pokemon.Type.Roche) && !this.P2.Pokemon.IsStab(Pokemon.Type.Sol) && !this.P2.Pokemon.IsStab(Pokemon.Type.Acier) && P1.Pokemon.ability.id != 10)
                    this.P2.Pokemon.Hp -= (int)(this.P2.Pokemon.GetMaxHp() / 16.0);
            }
            if (this.stage.Weather == WeatherType.Rain)
            {
                if (this.P1.Pokemon.ability.id == 8/*Rain dish*/)
                    this.P1.Pokemon.Hp += (int)(this.P1.Pokemon.GetMaxHp() / 16.0);
                if (this.P2.Pokemon.ability.id == 8/*Rain dish*/)
                    this.P2.Pokemon.Hp += (int)(this.P2.Pokemon.GetMaxHp() / 16.0);
            }

            // 13- Décomptes tour
            this.DoEndTurn();
        }
        
        public bool CanSwitch(Trainer self, int bla)
        {
            return self.GetAPokemon(bla).Status != Status.Dead && self.GetAPokemon(bla) != self.Pokemon && !self.Pokemon.HasFlags(Flags.MagmaStorm);
        }

        public void DoSwitch(Trainer self, Trainer mate, int bla)
        {
            // 1- Retirer les flags + protéen
            self.Pokemon.RemoveFlags();
            if (self.Pokemon.ability.id == 56)
            {
                self.Pokemon.Type1 = Pokemon.Type.Eau;
                self.Pokemon.Type2 = Pokemon.Type.Tenèbres;
            }

            self.SwitchTo(bla);

            // 2- Switch (+Natural cure, regenerator, Healing Wish)
            if (self.Pokemon.ability.id == 11/*Natural cure*/)
                self.Pokemon.Status = Status.None;
            else if (this.P1.Pokemon.ability.id == 67/*Regenerator*/)
                self.Pokemon.Hp = (int)(self.Pokemon.Hp * 1.3);
            if (self.HasHazards(Hazards.HealingWish))
            {
                self.Pokemon.Hp = self.Pokemon.GetMaxHp();
                self.Pokemon.Status = Status.None;
                self.RemoveHazards(Hazards.HealingWish);
            }

            // 3- Hazards si switch
            if (self.HasHazards(Hazards.StealthRock) && self.Pokemon.ability.id != 10)
                self.Pokemon.Hp -= (int)(self.Pokemon.Hp * 12.5 / 100 * Program.GetMatchup(Pokemon.Type.Roche, self.Pokemon.Type1, self.Pokemon.Type2));

            if (self.HasHazards(Hazards.Spikes) && self.Pokemon.ability.id != 10)
                self.Pokemon.Hp -= (int)(self.Pokemon.Hp * 12.5 / 100);
            else if (self.HasHazards(Hazards.Spikes2) && self.Pokemon.ability.id != 10)
                self.Pokemon.Hp -= (int)(self.Pokemon.Hp * 16.66 / 100);
            else if (self.HasHazards(Hazards.Spikes3) && self.Pokemon.ability.id != 10)
                self.Pokemon.Hp -= (int)(self.Pokemon.Hp * 25.0 / 100);

            if (self.HasHazards(Hazards.StickyWeb))
            {
                self.Pokemon[StatTarget.Speed] = -1;
                if (self.Pokemon.ability.id == 39)
                    self.Pokemon[StatTarget.Attack] = 2;
            }

            if (self.Pokemon.IsStab(Pokemon.Type.Poison))
            {
                self.RemoveHazards(Hazards.ToxicSpikes, Hazards.ToxicSpikes2);
            }
            else
            {
                if (self.HasHazards(Hazards.ToxicSpikes))
                    self.Pokemon.Status = Status.Poison;
                else if (self.HasHazards(Hazards.ToxicSpikes2))
                    self.Pokemon.Status = Status.BadlyPoisoned;
            }

            // 4- Talents switch
            //Sand Stream (25), Trace (30), Intimidate(34), Protean (56), Electric surge (69), Psychic surge (70), Drought (74)
            if (self.Pokemon.ability.id == 25)
                stage.Weather = WeatherType.Sandstorm;

            if (self.Pokemon.ability.id == 30)
            {
                self.Pokemon.ability = mate.Pokemon.ability;
            }

            if(self.Pokemon.ability.id == 34)
            {
                if (mate.Pokemon.ability.id != 35)
                    mate.Pokemon[StatTarget.Attack] = -1;
                if (mate.Pokemon.ability.id == 39)
                    mate.Pokemon[StatTarget.Attack] = 2;
            }

            if (self.Pokemon.ability.id == 69)
                stage.Terrain = TerrainType.Eletric;

            if (self.Pokemon.ability.id == 70)
                stage.Terrain = TerrainType.Psychic;

            if (self.Pokemon.ability.id == 74)
                stage.Weather = WeatherType.HarshSunlight;
        }

        /**
         * returns first at 0, last at 1
         */
        public Trainer[] OrderPrioriry()
        {
            Trainer[] r = new Trainer[] { null, null };

            int prio1 = this.P1.NextAction.StartsWith("attack") ? this.P1.Pokemon.NextMove[4/*effet prio*/].GetValueOrDefault(new Effect(0)).value : 6;
            int prio2 = this.P2.NextAction.StartsWith("attack") ? this.P2.Pokemon.NextMove[4/*effet prio*/].GetValueOrDefault(new Effect(0)).value : 6;

            if (prio1 < prio2)
            {
                r[0] = this.P1;
                r[1] = this.P2;
            }
            else if (prio2 < prio1)
            {
                r[1] = this.P1;
                r[0] = this.P2;
            }
            else
            {
                int speed1 = P1.Pokemon[StatTarget.Speed];
                int speed2 = P2.Pokemon[StatTarget.Speed];
                //sandrush & swiftswim & chlorophyll
                if((P1.Pokemon.ability.id == 50 && stage.Weather == WeatherType.Sandstorm) || (P1.Pokemon.ability.id == 77 && stage.Weather == WeatherType.Rain) || (P1.Pokemon.ability.id == 7 && stage.Weather == WeatherType.HarshSunlight))
                {
                    speed1 *= 2;
                }
                if ((P2.Pokemon.ability.id == 50 && stage.Weather == WeatherType.Sandstorm) || (P2.Pokemon.ability.id == 77 && stage.Weather == WeatherType.Rain) || (P2.Pokemon.ability.id == 7 && stage.Weather == WeatherType.HarshSunlight))
                {
                    speed2 *= 2;
                }
                if (speed2 < speed1)
                {
                    r[0] = this.P1;
                    r[1] = this.P2;
                }
                else if (speed1 < speed2)
                {
                    r[1] = this.P1;
                    r[0] = this.P2;
                }
                else
                {
                    int bla = Program.RngNext(2);
                    r[bla] = this.P1;
                    r[1 - bla] = this.P2;
                }
            }

            return r;
        }

        public void DoEndTurn()
        {
            // Speed Boost
            if (this.P1.Pokemon.ability.id == 27)
                P1.Pokemon[StatTarget.Speed] = 1;
            if (this.P2.Pokemon.ability.id == 27)
                P2.Pokemon[StatTarget.Speed] = 1;

            // counters:
            // end weather
            // end terrain
            this.stage.IncNbTurn();

            // TODO: end screens
            // TODO: end magma storm
            this.P1.IncNbTurn();
            this.P2.IncNbTurn();

            this.P1.Pokemon.RemoveFlags(Flags.Flinch);
            this.P2.Pokemon.RemoveFlags(Flags.Flinch);

            this.P1.Pokemon.RemoveFlags(Flags.Protect);
            this.P2.Pokemon.RemoveFlags(Flags.Protect);

            // TODO: end taunt
            // TODO: end sleep
            this.P1.Pokemon.IncNbTurn();
            this.P2.Pokemon.IncNbTurn();

            this.P1.NextAction = "...";
            this.P2.NextAction = "...";

            if (this.P1.Pokemon.Status == Status.Dead && this.P1.HasPokemon())
                this.DoSwitch(this.P1, this.P2, Program.RequireSwitch(this.P1));//this.P1.SwitchTo(Program.RequireSwitch(this.P1, 1));
            if (this.P2.Pokemon.Status == Status.Dead && this.P2.HasPokemon())
                this.DoSwitch(this.P2, this.P1, Program.RequireSwitch(this.P2));//this.P2.SwitchTo(Program.RequireSwitch(this.P2, 2));
        }

        public bool Start() // return false if battle settings invalids, in this case state will be `BattleState.Unknown`
        {
            // TODO: verify.. ?

            this.State = BattleState.Waiting;
            return true;
        }

        public override string ToString()
        {
            return $"Battle between thoses players (currently: {this.State}):\n\n\n{this.P1}\n\n\n{this.P2}\n\n\nstage: {this.stage}\n\n";
        }
    }
}

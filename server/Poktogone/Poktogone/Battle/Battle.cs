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
        static String[] COMMANDS = { "attack", "switch" };

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
                bool isValid = false;
                for (int k = 0; k < Battle.COMMANDS.Length && !isValid; isValid = c.StartsWith(Battle.COMMANDS[k++]))
                    ;
                if (!isValid)
                    return 0;
                
                player -= 1;
                Trainer[] P = { this.P1, this.P2 };
                BattleState[] S = { BattleState.WaitingP2, BattleState.WaitingP1};

                if (this.State == S[player])
                    Console.WriteLine($"The player {P[player].GetName()} changes its mind and will do '{c}'.");
                else
                    Console.WriteLine($"The player {P[player].GetName()} will do '{c}'.");
                P[player].NextAction = c;

                if (this.State != BattleState.Waiting && this.State != S[player])
                {
                    this.DoTurn();
                    this.State = BattleState.Waiting;
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
                Program.DamageCalculator(this.stage, this.P1.Pokemon, this.P2.Pokemon, this.P1, this.P2); // p1 fait poursuite
            if (isP1Switch && isP2Attack && this.P2.Pokemon.NextMove.id == 85/*Poursuite*/)
                Program.DamageCalculator(this.stage, this.P2.Pokemon, this.P1.Pokemon, this.P2, this.P1); // p2 fait poursuite

            // 2-, 3- et 4- Switch
            if (isP1Switch)
                this.DoSwitch(this.P1, this.P2);
            if (isP2Switch)
                this.DoSwitch(this.P2, this.P1);

            // 5- Mega-évo


            // 6-, 7- et 8-
            Trainer[] order = this.OrderPrioriry();
            if (order[0].NextAction.StartsWith("attack"))
            {
                int damage = Program.DamageCalculator(this.stage, order[0].Pokemon, order[1].Pokemon, order[0], order[1]);
                Program.InflictDamage(damage, order[0].Pokemon, order[1].Pokemon);
            }
            if (order[1].NextAction.StartsWith("attack"))
            { 
                int damage = Program.DamageCalculator(this.stage, order[1].Pokemon, order[0].Pokemon, order[1], order[0]);
                Program.InflictDamage(damage, order[1].Pokemon, order[0].Pokemon);
            }
            // 9- Restes
            if (this.P1.Pokemon.item.id == 9/*Restes*/)
                this.P1.Pokemon.Hp += (int)(this.P1.Pokemon.GetMaxHp() / 16.0);
            if (this.P2.Pokemon.item.id == 9/*Restes*/)
                this.P2.Pokemon.Hp += (int)(this.P2.Pokemon.GetMaxHp() / 16.0);

            // 10- poison / toxic / burn
            if (this.P1.Pokemon.Status == Status.Poison)
            {
                if (this.P1.Pokemon.ability.id == 31/*Soint poison*/)
                    this.P1.Pokemon.Hp += (int)(this.P1.Pokemon.GetMaxHp() / 8.0);
                else
                    this.P1.Pokemon.Hp -= (int)(this.P1.Pokemon.GetMaxHp() / 8.0);
            }
            if (this.P2.Pokemon.Status == Status.Poison)
            {
                if (this.P2.Pokemon.ability.id == 31/*Soint poison*/)
                    this.P2.Pokemon.Hp += (int)(this.P2.Pokemon.GetMaxHp() / 8.0);
                else
                    this.P2.Pokemon.Hp -= (int)(this.P2.Pokemon.GetMaxHp() / 8.0);
            }
            /*Status.BadlyPoisoned*/
            if (this.P1.Pokemon.Status == Status.Burn)
                this.P1.Pokemon.Hp += (int)(this.P1.Pokemon.GetMaxHp() / 16.0);
            if (this.P2.Pokemon.Status == Status.Burn)
                this.P2.Pokemon.Hp += (int)(this.P2.Pokemon.GetMaxHp() / 16.0);

            // 11- Leech Seed
            if (this.P1.Pokemon.HasFlags(Flags.LeechSeed))
            {
                int tmp = (int)(this.P1.Pokemon.GetMaxHp() / 8.0);
                this.P1.Pokemon.Hp -= tmp;
                this.P2.Pokemon.Hp += tmp;
            }
            if (this.P2.Pokemon.HasFlags(Flags.LeechSeed))
            {
                int tmp = (int)(this.P2.Pokemon.GetMaxHp() / 8.0);
                this.P2.Pokemon.Hp -= tmp;
                this.P1.Pokemon.Hp += tmp;
            }

            // 12- Hail / Sand + Rain dish
            if (this.stage.weather == WeatherType.Hail)
            {
                if (!this.P1.Pokemon.IsStab(Pokemon.Type.Glace))
                    this.P1.Pokemon.Hp -= (int)(this.P1.Pokemon.GetMaxHp() / 16.0);
                if (!this.P2.Pokemon.IsStab(Pokemon.Type.Glace))
                    this.P2.Pokemon.Hp -= (int)(this.P2.Pokemon.GetMaxHp() / 16.0);
            }
            if (this.stage.weather == WeatherType.Sandstorm)
            {
                if (!this.P1.Pokemon.IsStab(Pokemon.Type.Roche) && !this.P1.Pokemon.IsStab(Pokemon.Type.Sol) && !this.P1.Pokemon.IsStab(Pokemon.Type.Acier))
                    this.P1.Pokemon.Hp -= (int)(this.P1.Pokemon.GetMaxHp() / 16.0);
                if (!this.P2.Pokemon.IsStab(Pokemon.Type.Roche) && !this.P2.Pokemon.IsStab(Pokemon.Type.Sol) && !this.P2.Pokemon.IsStab(Pokemon.Type.Acier))
                    this.P2.Pokemon.Hp -= (int)(this.P2.Pokemon.GetMaxHp() / 16.0);
            }
            if (this.stage.weather == WeatherType.Rain)
            {
                if (this.P1.Pokemon.ability.id == 8/*Rain dish*/)
                    this.P1.Pokemon.Hp += (int)(this.P1.Pokemon.GetMaxHp() / 16.0);
                if (this.P2.Pokemon.ability.id == 8/*Rain dish*/)
                    this.P2.Pokemon.Hp += (int)(this.P2.Pokemon.GetMaxHp() / 16.0);
            }
            
            // 13- Décomptes tour
            this.DoEndTurn();
        }

        public void DoSwitch(Trainer self, Trainer mate)
        {
            // 2- Switch (+Natural cure et regenerator)
            self.SwitchTo(int.Parse(self.NextAction.Replace("switch", "").Trim()));

            if (self.Pokemon.ability.id == 11/*Natural cure*/)
                self.Pokemon.Status = Status.None;
            else if (this.P1.Pokemon.ability.id == 67/*Regenerator*/)
                self.Pokemon.Hp = (int)(self.Pokemon.Hp * 1.3);

            // 3- Hazards si switch
            if (self.HasHazards(Hazards.StealthRock))
                self.Pokemon.Hp -= (int)(self.Pokemon.Hp * 12.5 / 100 * Program.GetMatchup(Pokemon.Type.Roche, self.Pokemon.Type1, self.Pokemon.Type2));

            if (self.HasHazards(Hazards.Spikes))
                self.Pokemon.Hp -= (int)(self.Pokemon.Hp * 12.5 / 100);
            else if (self.HasHazards(Hazards.Spikes2))
                self.Pokemon.Hp -= (int)(self.Pokemon.Hp * 16.66 / 100);
            else if (self.HasHazards(Hazards.Spikes3))
                self.Pokemon.Hp -= (int)(self.Pokemon.Hp * 25.0 / 100);

            if (self.HasHazards(Hazards.StickyWeb))
                self.Pokemon[StatTarget.Speed] = -1;

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
            //Sand Stream (25), Trace (30), Protean (56), Electric surge (69), Psychic surge (70), Drought (74)
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
                int bla = Program.RngNext(2);
                r[bla] = this.P1;
                r[1 - bla] = this.P2;
            }

            return r;
        }

        public void DoEndTurn()
        {
            // coutners:
            // end weather
            // end terrain
            // end screens
            // end taunt
            // end sleep
            // end magma storm

            this.P1.NextAction = "...";
            this.P2.NextAction = "...";
        }

        public bool Start() // return false if battle settings invalids, in this case state will be `BattleState.Unknown`
        {
            // TODO: verify.. ?

            this.State = BattleState.Waiting;
            return true;
        }

        public override string ToString()
        {
            return $"Battle between thoses players (currently: {this.State}):\n\n* {this.P1}\n\n* {this.P2}\n\nstage: {this.stage}\n";
        }
    }
}

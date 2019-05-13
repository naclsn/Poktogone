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
        
        public void DoTurn(Main.SqlHelper dbo)
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

            Func<int, int, double> GetMatchup = (int idTypeAtk, int idTypeDef) => int.Parse(dbo.Select("matchups", new Where("type_atk", idTypeAtk).And("type_def", idTypeDef), "coef")[0]["coef"]);

            bool isP1Attack = this.P1.NextAction.StartsWith("attack");
            bool isP2Attack = this.P2.NextAction.StartsWith("attack");
            bool isP1Switch = this.P1.NextAction.StartsWith("switch");
            bool isP2Switch = this.P2.NextAction.StartsWith("switch");

            // 1- Poursuite si switch
            if (isP2Switch && isP1Attack && this.P1.Pokemon.NextMove.id == 85/*Poursuite*/)
                Main.Program.DamageCalculator(this.stage, this.P1.Pokemon, this.P2.Pokemon, this.P1, this.P2); // p1 fait poursuite
            if (isP1Switch && isP2Attack && this.P2.Pokemon.NextMove.id == 85/*Poursuite*/)
                Main.Program.DamageCalculator(this.stage, this.P2.Pokemon, this.P1.Pokemon, this.P2, this.P1); // p2 fait poursuite

            // 2- et 3- Switch
            if (isP1Switch)
                this.DoSwitch(this.P1);
            if (isP2Switch)
                this.DoSwitch(this.P2);

            Trainer[] order = this.OrderPrioriry();

            if (this.P1.NextAction.StartsWith("attack"))
                Program.DamageCalculator(this.stage, order[0].Pokemon, order[1].Pokemon, order[1]);

            if (this.P2.NextAction.StartsWith("attack"))
                Program.DamageCalculator(this.stage, order[1].Pokemon, order[0].Pokemon, order[0]);

            this.DoEndTurn();
        }

        public void DoSwitch(Func<int, int, double> GetMatchup, Trainer t)
        {
            // 2- Switch (+Natural cure et regenerator)
            t.SwitchTo(int.Parse(t.NextAction.Replace("switch", "").Trim()));

            if (t.Pokemon.ability.id == 11/*Natural cure*/)
                t.Pokemon.Status = Status.None;
            else if (this.P1.Pokemon.ability.id == 67/*Regenerator*/)
                t.Pokemon.Hp = (int)(t.Pokemon.Hp * 1.3);

            // 3- Hazards si switch
            if (t.HasHazards(Hazards.StealthRock))
                t.Pokemon.Hp -= (int)t.Pokemon.Hp * 12.5 / 100 * GetMatchup(Pokemon.Type.Roche, t.Pokemon.Type1, t.Pokemon.Type2));

            if (t.HasHazards(Hazards.Spikes))
                t.Pokemon.Hp -= (int)(t.Pokemon.Hp * 12.5 / 100);
            else if (t.HasHazards(Hazards.Spikes2))
                t.Pokemon.Hp -= (int)(t.Pokemon.Hp * 16.66 / 100);
            else if (t.HasHazards(Hazards.Spikes3))
                t.Pokemon.Hp -= (int)(t.Pokemon.Hp * 25.0 / 100);

            if (t.HasHazards(Hazards.StickyWeb))
                t.Pokemon[StatTarget.Speed] = -1;

            if (t.Pokemon.Type1 == Pokemon.Type.Poison || t.Pokemon.Type2 == Pokemon.Type.Poison)
            {
                t.RemoveHazards(Hazards.ToxicSpikes, Hazards.ToxicSpikes2);
            }
            else
            {
                if (t.HasHazards(Hazards.ToxicSpikes))
                    t.Pokemon.Status = Status.Poison;
                else if (t.HasHazards(Hazards.ToxicSpikes2))
                    t.Pokemon.Status = Status.BadlyPoisoned;
            }
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
                int bla = Main.Program.RngNext(2);
                r[bla] = this.P1;
                r[1 - bla] = this.P2;
            }

            return r;
        }

        public void DoEndTurn()
        {
            // damage pokemon from weather
            // damage pokemon from hazards
            // heal pokemon from berries

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

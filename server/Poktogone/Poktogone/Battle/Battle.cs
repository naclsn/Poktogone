using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Poktogone.Pokemon;

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
            // -----
            // 6- Attaques poké 1
            // 7- Effet attaque poké 1 (effets → recul → baies → effet de kill)
            // 8- De même poké 2
            // -----
            //
            // 9- Restes
            // 10- poison / toxic / burn
            // 11- Leech Seed
            // 12- Hail / Sand + Rain dish
            // 13- Décompte tour

            Trainer[] order = this.OrderPrioriry();

            if (this.P1.NextAction.StartsWith("attack"))
                Main.Program.DamageCalculator(this.stage, order[0].Pokemon, order[1].Pokemon, order[1]);
            else if (this.P1.NextAction.StartsWith("switch"))
                this.P1.SwitchTo(int.Parse(this.P1.NextAction.Replace("switch", "").Trim()));

            if (this.P2.NextAction.StartsWith("attack"))
                Main.Program.DamageCalculator(this.stage, order[1].Pokemon, order[0].Pokemon, order[0]);
            else if (this.P2.NextAction.StartsWith("switch"))
                this.P2.SwitchTo(int.Parse(this.P2.NextAction.Replace("switch", "").Trim()));

            this.DoEndTurn();
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

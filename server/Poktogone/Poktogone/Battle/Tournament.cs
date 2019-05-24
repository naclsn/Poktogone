using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Poktogone.Main;

namespace Poktogone.Battle
{
    struct Competitor
    {
        public int competitorNumber;
        public bool isPlayer;

        public String name;
        public String team;
        public Trainer asTrainer;

        public Competitor(int competitorNumber, String name, String team, bool isPlayer = false)
        {
            this.competitorNumber = competitorNumber;
            this.isPlayer = isPlayer;

            this.name = name;
            this.team = team;
            this.asTrainer = new Trainer(this.name, this.competitorNumber + 1, Program.ParseSets(this.team));//TODO

            Program.Log("tournament", $"{this.name} joins the battle!");
        }

        /*public void Heal()
        {
            //this.asTrainer = new Trainer(this.name, this.competitorNumber + 1, Program.ParseSets(this.team));
        }*/

        public Trainer GetTrainer()
        {
            return this.asTrainer.Copy();
        }
    }

    class Tournament
    {
        private Competitor[] competitors;

        public Tournament(uint nbTrainer, uint nbPlayer = 1)
        {
            this.competitors = new Competitor[nbTrainer];

            nbPlayer %= nbTrainer;
            for (int k = 0; k < nbTrainer; k++)
                this.competitors[k] = new Competitor(
                        k,
                        k < nbPlayer ? Program.Input($"Nom du joueur{(nbPlayer > 1 ? $" {k + 1}" : "")} : ") : Tournament.RandomName(),
                        $"{Program.RngNext(123) + 1};{Program.RngNext(123) + 1};{Program.RngNext(123) + 1}",
                        k < nbPlayer
                    );
        }        

        public Competitor DoTournament()
        {
            List<int> lefts = new List<int>();
            List<int> nexts = new List<int>();

            for (int k = 0; k < this.competitors.Length; lefts.Add(k++))
                ;

            int counter = 0;
            while (1 < lefts.Count)
            {
                int battles = lefts.Count / 2;

                Program.Println();

                String tmp = $"| Tour {++counter}, {battles} combats ! |";
                Program.Println("+" + new String('-', tmp.Length - 2) + "+");
                Program.Println(tmp);
                Program.Println("+" + new String('-', tmp.Length - 2) + "+");

                Program.Println();

                for (int k = 0; k < battles; k++)
                {
                    Competitor winner;

                    Competitor C1 = this.competitors[lefts[2 * k]];
                    Competitor C2 = this.competitors[lefts[2 * k + 1]];

                    Program.Println($"Début du combat entre {C1.name} et {C2.name}.");

                    // player exist
                    if (C1.isPlayer || C2.isPlayer)
                    {
                        //Battle battle = new Battle(C1.asTrainer, C2.asTrainer);
                        Battle battle = new Battle(C1.GetTrainer(), C2.GetTrainer());

                        battle.Start();
                        do
                        {
                            Program.Println(battle);

                            // eventual computer turn
                            if (!C1.isPlayer)
                                battle.InputCommand(1, Tournament.GetCpuCommand(battle));
                            if (!C2.isPlayer)
                                battle.InputCommand(2, Tournament.GetCpuCommand(battle));

                            // player turn
                            int code = -1;
                            while (code < 1)
                            {
                                try
                                {
                                    if (C1.isPlayer && C2.isPlayer)
                                        code = battle.InputCommand(int.Parse(Program.Input("Your player num: ")), Program.Input("Your command (attack / switch): "));
                                    else if (C1.isPlayer)
                                        code = battle.InputCommand(1, Program.Input("Your command (attack / switch): "));
                                    else if (C2.isPlayer)
                                        code = battle.InputCommand(2, Program.Input("Your command (attack / switch): "));
                                }
                                catch (FormatException)
                                {
                                    Program.Print("Not a number... ");
                                }

                                if (code < 0)
                                    Program.Println("Wrong player number! (nice try tho...)");
                                else if (code == 0)
                                    Program.Println("Invalid command! (or typo...)");
                            }
                            
                            Program.Input("Press Enter to continue... ");
                            Program.ConsoleClear();
                        }
                        while (battle.State != BattleState.VictoryP1 && battle.State != BattleState.VictoryP2);

                        winner = battle.State == BattleState.VictoryP1 ? C1 : C2;
                        //winner.Heal();
                    }
                    else winner = this.competitors[lefts[2 * k + Program.RngNext(2)]];
                    
                    Program.Println($"{winner.name} l'emporte !");
                    Program.Input("\n");

                    nexts.Add(winner.competitorNumber);
                }

                // if odd number of participants
                if (2 * battles < lefts.Count) nexts.Add(lefts.Last());

                lefts = nexts;
                nexts = new List<int>();
            }

            return this.competitors[lefts.DefaultIfEmpty(0).Last()];
        }

        private static String RandomName()
        {
            String r = "" + (char)Program.RngNext('A', 'Z' + 1);

            for (int k = Program.RngNext(5, 7); 0 < k; k--)
                r += (char)Program.RngNext('a', 'z' + 1);

            return r;
        }

        private static String GetCpuCommand(Battle b)
        {
            return "attack " + Program.RngNext(4);
        }
    }
}

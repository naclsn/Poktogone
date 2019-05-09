using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

using Poktogone.Pokemon;
using Poktogone.Battle;

namespace Poktogone.Main
{
    class Program
    {
        static private SqlHelper dbo;
        static private bool isFromCmd;
        static private Random rng;

        /**
         * cli args:
         *  Poktogone.exe nameP1 teamP1 nameP2 teamP2 [--dbo fileName] [--rng seed] [--dmc (damage calculator args)]
         */
        static int Main(String[] args)
        {
            Program.dbo = new SqlHelper();
            Program.isFromCmd = 0 < args.Length;
            Program.rng = new Random();

            // TODO: args parse (meh..)

            Program.Log("info", "Connecting to sqllocaldb");
            Program.dbo.Connect(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Database.mdf"));
            Program.Log("info", "\tConnected!");

            Trainer P1, P2;
            Battle.Battle battle;

            Program.Log("info", "Loading players' sets");
            if (Program.isFromCmd)
            {
                P1 = new Trainer(args[0], ParseSets(args[1]));
                P2 = new Trainer(args[2], ParseSets(args[3]));
            }
            else
            {
                P1 = new Trainer(Program.Input("Nom du joueur 1: "), ParseSets($"{Program.rng.Next(121) + 1};{Program.rng.Next(121) + 1};{Program.rng.Next(121) + 1}"));
                P2 = new Trainer(Program.Input("Nom du joueur 2: "), ParseSets($"{Program.rng.Next(121) + 1};{Program.rng.Next(121) + 1};{Program.rng.Next(121) + 1}"));
            }
            Program.Log("info", "\tLoaded!");

            battle = new Battle.Battle(P1, P2);
            Program.Println(battle);

            Program.Input("Press Enter to start... ");
            Program.ConsoleClear();

            if (battle.Start())
            {
                do
                {
                    Program.Println(battle);
                    int code = battle.InputCommand(int.Parse(Program.Input("Your player num: ")), Program.Input("Your commande: "));

                    if (code < 0)
                        Program.Println("Wrong player number! (nice try tho...)");
                    else if (code == 0)
                        Program.Println("Wrong command name! (or typo...)");

                    Program.Input("Press Enter to start... ");
                    Program.ConsoleClear();
                }
                while (battle.State != BattleState.VictoryP1 || battle.State != BattleState.VictoryP2);
            }
            else
            {
                Program.Println($"Couln't start battle... Last known state: {battle.State}");
                return (int)battle.State;
            }

            Console.WriteLine(battle.State);
            return 0;
        }

        public static void Print(Object o)
        {
            Console.Write(o.ToString());
        }

        public static void Println(Object o)
        {
            Console.WriteLine(o.ToString());
        }

        public static void Println()
        {
            Console.WriteLine();
        }

        public static String Input(Object o)
        {
            Program.Print(o);
            return Console.ReadLine();
        }

        public static String Input()
        {
            return Console.ReadLine();
        }

        public static void ConsoleClear()
        {
            if (!Program.isFromCmd)
                Console.Clear();
        }

        public static void Log(String tag, String c)
        {
            Console.WriteLine($"[log]{tag}: {c}");
        }

        public static int RngNext(int maxValue)
        {
            return Program.rng.Next(maxValue);
        }

        public static int RngNext(int minValue, int maxValue)
        {
            return Program.rng.Next(minValue, maxValue);
        }

        /**
         * Data arg:
         * "[setId1];[setId2];[setId3]"
         */
        static Set[] ParseSets(String arg, char sep = ';')
        {
            Set[] r = new Set[3];
            int k = 0;

            foreach (String id in arg.Split(sep))
                r[k++] = Set.FromDB(Program.dbo, int.Parse(id));

            return r;
        }

        public static void DamageCalculator(Stage stage, Set atk, Set def, Trainer defTrainer)
        {
            /*-*/
        }
    }
}

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
        static private SqlHelper dbo; // Ued to query to the local Pokemon DB (the .mdf file).
        static private bool isFromCmd; // Used to know weather called with or without arguments.
        static private Random rng; // Grlobal RNG, private; to get RNG, usethe `RngNext` familly of functions.

        static private bool verbose = false;

        static void Main(String[] args)
        {
            Program.dbo = new SqlHelper();
            Program.isFromCmd = 0 < args.Length;
            Program.rng = new Random();

            Program.ConsoleSize(80, 40);

            Program.Log("info", "Connecting to sqllocaldb");
            Program.dbo.Connect(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Database.mdf"));
            Program.Log("info", "\tConnected!");

            Program.Log("info", "Starting tournament");
            Tournament T = new Tournament(16, 1);
            Program.Log("info", "\tStarted!");

            Program.ConsoleClear();
            Competitor winner = T.DoTournament();

            Program.ConsoleClear();
            Program.Println($"Fin du tournoi, {winner.name} est le grand vainqueur");

            Program.dbo.Disconnect();
            Program.Input("Press blabla.. ");
        }

        /// <summary>
        /// Play a battle between the trainers defined by thir names and sets if given through args,
        /// else ask for the names through stdin and use randomly choosen sets from the database.
        /// If the battle succed to start, process to play a game: get players commands, play turn, resets status.
        /// </summary>
        /// <param name="args">`Poktogone.exe nameP1 teamP1 nameP2 teamP2 [--dbo fileName] [--rng seed] [--dmc (damage calculator args)] [--log [fileName]]`.</param>
        /// <returns>0 if success, else last known <see cref="BattleState"/> (as an int).</returns>
        /// <remarks>This function is to be used by the PHP script (see web-based).</remarks>
        static int PhpMain(String[] args)
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
                P1 = new Trainer(args[0], 1, ParseSets(args[1]));
                P2 = new Trainer(args[2], 2, ParseSets(args[3]));
            }
            else
            {
                P1 = new Trainer(Program.Input("Player 1 name: "), 1, ParseSets($"{Program.rng.Next(121) + 1};{Program.rng.Next(121) + 1};{Program.rng.Next(121) + 1}"));
                P2 = new Trainer(Program.Input("Player 2 name: "), 2, ParseSets($"{Program.rng.Next(121) + 1};{Program.rng.Next(121) + 1};{Program.rng.Next(121) + 1}"));
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
                    int code = -1;
                    try
                    {
                        code = battle.InputCommand(int.Parse(Program.Input("Your player num: ")), Program.Input("Your command (attack / switch): "));
                    }
                    catch (FormatException)
                    {
                        Program.Print("Not a number... ");
                    }

                    if (code < 0)
                        Program.Println("Wrong player number! (nice try tho...)");
                    else if (code == 0)
                        Program.Println("Invalid command! (or typo...)");

                    Program.Input("Press Enter to continue... ");
                    Program.ConsoleClear();
                }
                while (battle.State != BattleState.VictoryP1 && battle.State != BattleState.VictoryP2);
            }
            else
            {
                Program.Println($"Couln't start battle... Last known state: {battle.State}");
                return (int)battle.State;
            }

            Program.Println(battle.State);
            return 0;
        }

        /// <summary>
        /// Prints to stdout using the <seealso cref="Console"/>.
        /// </summary>
        /// <param name="o">Object to print.</param>
        public static void Print(Object o)
        {
            foreach (String c in ("F$_0" + o.ToString()).Split('$'))
            {
                if (0 < c.Length && c[0] == '_')
                {
                    Console.BackgroundColor = (ConsoleColor)int.Parse("" + c[1], System.Globalization.NumberStyles.HexNumber);
                    Console.Write(c.Substring(2));
                }
                else if (0 < c.Length)
                {
                    Console.ForegroundColor = (ConsoleColor)int.Parse("" + c[0], System.Globalization.NumberStyles.HexNumber);
                    Console.Write(c.Substring(1));
                }
                else Console.Write(c);
            }
        }

        /// <summary>
        /// Prints to stdout using the <seealso cref="Console"/>, then line feeds.
        /// </summary>
        /// <param name="o">Object to print.</param>
        public static void Println(Object o)
        {
            Program.Print(o.ToString() + "\n");
        }

        /// <summary>
        /// Line feeds.
        /// </summary>
        public static void Println()
        {
            Program.Println("");
        }

        /// <summary>
        /// Waits for an input from stdin using the <seealso cref="Console"/>.
        /// </summary>
        /// <returns>The String read.</returns>
        public static String Input()
        {
            Program.PhpMessage("int", "waiting for user input");
            return Console.ReadLine();
        }

        /// <summary>
        /// Prints to stdout then waits for an input from stdin using the <seealso cref="Console"/>.
        /// </summary>
        /// <param name="o">Object to print.</param>
        /// <returns>The String read.</returns>
        public static String Input(Object o)
        {
            Program.Print(o);
            return Program.Input();
        }

        /// <summary>
        /// Clear the <seealso cref="Console"/>.
        /// </summary>
        /// <remarks>Only if no args where proviede at start (<seealso cref="Program.isFromCmd"/>).</remarks>
        public static void ConsoleClear()
        {
            if (!Program.isFromCmd)
            {
                if (verbose)
                    Console.WriteLine(new String('\n', 12));
                else Console.Clear();
            }
            Program.PhpMessage("clc", "clearing buffer");
        }

        public static void ConsoleSize(int w, int h)
        {
            Console.SetWindowSize(w, h);
            Console.SetBufferSize(w, h);
        }

        /// <summary>
        /// Used to send signals to the PHP scripts (see web-based).
        /// </summary>
        /// <param name="signal">Signal identifier (3 chars).</param>
        /// <param name="description">Optionnal description of the signal.</param>
        /// <remarks>(I don't want to setup any more pipes.)</remarks>
        public static void PhpMessage(String signal, String description)
        {
            if (Program.isFromCmd)
                Console.Error.Write($"{signal}/{description} (this is not an error)\n");
        }

        /// <summary>
        /// Prints to the log (can be specified through the args, default to the <seealso cref="Console"/>).
        /// </summary>
        /// <param name="tag">Tag to display before the message</param>
        /// <param name="c">Text to display </param>
        public static void Log(String tag, String c)
        {
            // TOD: use the specified output (should be a file), if any.
            if (verbose)
                Program.Println($"$8$_0[{DateTime.Now.ToString("HH:mm")}] {tag}: {c}");
        }

        /// <summary>
        /// Return the next pseudo random int value, from 0 (included).
        /// </summary>
        /// <param name="maxValue">Max value (excluded).</param>
        /// <returns>A pseudo random, uniformly distributed, int.</returns>
        public static int RngNext(int maxValue)
        {
            return Program.rng.Next(maxValue);
        }

        /// <summary>
        /// Return the next pseudo random int value.
        /// </summary>
        /// <param name="minValue">Min value (included).</param>
        /// <param name="maxValue">Max value (excluded).</param>
        /// <returns>A pseudo random, uniformly distributed, int.</returns>
        public static int RngNext(int minValue, int maxValue)
        {
            return Program.rng.Next(minValue, maxValue);
        }

        /// <summary>
        /// Parse and load from database the 3 sets specified by the argument.
        /// </summary>
        /// <param name="arg">"[setId1];[setId2];[setId3]"</param>
        /// <param name="sep">Separator, use ';' by default</param>
        /// <returns>Return a list of 3 sets.</returns>
        public static Set[] ParseSets(String arg, char sep = ';')
        {
            Set[] r = new Set[3];
            int k = 0;

            foreach (String id in arg.Split(sep))
                r[k++] = Set.FromDB(Program.dbo, int.Parse(id));

            return r;
        }

        public static float GetMatchup(Pokemon.Type typeAtk, Pokemon.Type typeDef)
        {
            if (typeDef == Pokemon.Type.None) return 1;
            return float.Parse(dbo.Select("matchups", new Where("type_atk", (int)typeAtk).And("type_def", (int)typeDef), "coef")[0]["coef"]);
        }

        public static float GetMatchup(Pokemon.Type typeAtk, Pokemon.Type typeDef1, Pokemon.Type typeDef2)
        {
            return Program.GetMatchup(typeAtk, typeDef1) * (typeDef2 == Pokemon.Type.None ? 1 : Program.GetMatchup(typeAtk, typeDef2));
        }

        public static int RequireSwitch(Trainer t)
        {
            int r = 0;

            if (t.playerNumber != 1)
            {
                while (t.GetAPokemon(r = Program.RngNext(3)).Status == Status.Dead)
                    ;
                return r;
            }
            
            Program.PhpMessage($"sw{t.playerNumber}", "forced switch");
            try
            {
                while (t.GetAPokemon(r = int.Parse(Program.Input($"{t.GetName()}, qui envoyer ? "))).Status == Status.Dead)
                    Program.Println($"{t.GetAPokemon(r).GetName()} ne veut plus se battre !");
            }
            catch (FormatException)
            {
                Program.Println("Merci d'entrer un nombre valide (0, 1 ou 2) !");
                r = RequireSwitch(t);
            }

            return r;
        }
    }
}

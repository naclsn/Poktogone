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

        static int Main(String[] args)
        {
            Program.dbo = new SqlHelper();
            Program.dbo.Connect(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Database.mdf"));

            Random r = new Random();
            Trainer P1 = new Trainer("Jean", ParseSets($"{r.Next(42) + 1};{r.Next(42) + 1};{r.Next(42) + 1}"));
            Trainer P2 = new Trainer("Paul", ParseSets($"{r.Next(42) + 1};{r.Next(42) + 1};{r.Next(42) + 1}"));

            Battle.Battle battle = new Battle.Battle(P1, P2);
            Console.WriteLine(battle);
            Console.Write("Press Enter to start... ");
            Console.ReadLine();
            Console.Clear();

            if (battle.Start())
                do
                {
                    Console.WriteLine(battle);
                    Console.Write("What should who do ? (your player num, then your commande): ");
                    if (battle.InputCommand(int.Parse(Console.ReadLine()), Console.ReadLine()) < 0)
                        Console.WriteLine("Wrong player number! (nice try tho)");
                    Console.Write("Press Enter to continue... ");
                    Console.ReadLine();
                    Console.Clear();
                }
                while (battle.State != BattleState.VictoryP1 || battle.State != BattleState.VictoryP2);
            else
                return (int)battle.State; // Console.WriteLine("Couln't start battle.");

            Console.WriteLine(battle.State);
            return 0;
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
    }
}

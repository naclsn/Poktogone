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

            //String c = "SELECT * FROM [sets] JOIN [pokemons] WHERE [pokemons].id = poke ORDER BY [nom] DESC";
            String c = "SELECT nom, pdv, type1, type2 FROM sets, pokemons WHERE sets.id = 42 AND poke = pokemons.id";
            using (SqlDataReader reader = Program.dbo.Select(c)) if (reader != null)
            {
                while (reader.Read())
                    Console.WriteLine("{0}, {1}, {2}, {3}", reader[0], reader[1], reader[2], reader[3]);
                reader.Close();
            }

            return 0;

            Trainer P1 = new Trainer(args[1], ParseSets(args[2]));
            Trainer P2 = new Trainer(args[3], ParseSets(args[4]));

            Battle.Battle battle = new Battle.Battle(P1, P2);

            if (battle.Start())
                do battle.InputCommand(int.Parse(Console.ReadLine()), Console.ReadLine());
                while (battle.State != BattleState.VictoryP1 || battle.State != BattleState.VictoryP2);
            else
                return (int)battle.State; // Console.WriteLine("Couln't start battle.");

            Console.WriteLine(battle.State);
            return 0;
        }

        /**
         * Data arg:
         * "[pok1];[pok2];[pok3]"
         */
        static Set[] ParseTeam(String arg, char sep = ';')
        {
            Set[] r = new Set[3];
            int k = 0;

            foreach (String pok in arg.Split(sep))
                r[k++] = Set.Parse(pok);

            return r;
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

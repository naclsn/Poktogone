using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Poktogone.Pokemon;
using Poktogone.Battle;

namespace Poktogone.Main
{
    class Program
    {
        static int Main(String[] args)
        {
            Trainer P1 = new Trainer(args[1], ParseTeam(args[2]));
            Trainer P2 = new Trainer(args[3], ParseTeam(args[4]));

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
         * Data order:
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

    }
}

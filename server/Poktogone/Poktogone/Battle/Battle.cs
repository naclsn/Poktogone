using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public BattleState State
        {
            get;
            private set;
        }

        public Battle(Trainer P1, Trainer P2)
        {
            this.P1 = P1;
            this.P2 = P2;

            this.State = BattleState.Starting;
        }

        public void InputCommand(int player, String c)
        {
        }

        public bool Start() // return false if battle settings invalids, in this case state will be `BattleState.Unknown`
        {
            // TODO: verify mega
            // TODO: verify.. ?
            this.State = BattleState.Waiting;
            return true;
        }
    }
}

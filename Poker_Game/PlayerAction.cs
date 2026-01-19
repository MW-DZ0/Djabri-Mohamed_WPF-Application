using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker_Game
{
    [Serializable]
    public class PlayerAction
    {
        public string Type; // "Miser", "Suivre", "Passer", "Check"
        public int Montant;
    }
}

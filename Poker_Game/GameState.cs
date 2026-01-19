using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker_Game
{
    [Serializable]
    public class GameState
    {
        public List<Joueur> Joueurs { get; set; }
        public List<string> CartesCommunes { get; set; }
        public int Pot { get; set; }
        public int TourActuel { get; set; }
        public int JoueurActuel { get; set; }
        public int MiseActuelle { get; set; }
    }
}

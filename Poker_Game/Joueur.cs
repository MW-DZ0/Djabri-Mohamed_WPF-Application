using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker_Game
{
    public class Joueur
    {
        public string Nom { get; set; }
        public int Argent { get; set; }
        public List<string> Cartes { get; set; }
        public int MiseActuelle { get; set; }
        public bool APasse { get; set; }
        public bool EstHumain { get; set; }
        public int Position { get; set; }


        public Joueur(string nom, int argent, bool estHumain = true, int position = 0)
        {
            Nom = nom;
            Argent = argent;
            EstHumain = estHumain;
            Cartes = new List<string>();
            MiseActuelle = 0;
            APasse = false;
            Position = position;
        }
    }
}

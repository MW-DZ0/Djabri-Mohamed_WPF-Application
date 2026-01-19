using System.Collections.Generic;

namespace Poker_Game
{
    public class HandResult
    {
        public HandRank Rank { get; set; }
        public List<int> Valeurs { get; set; }

        public HandResult()
        {
            Valeurs = new List<int>();
        }

        public int CompareTo(HandResult other)
        {
            if (Rank != other.Rank)
                return Rank.CompareTo(other.Rank);

            for (int i = 0; i < Valeurs.Count; i++)
            {
                if (Valeurs[i] != other.Valeurs[i])
                    return Valeurs[i].CompareTo(other.Valeurs[i]);
            }

            return 0;
        }
    }
}

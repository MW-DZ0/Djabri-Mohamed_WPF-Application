using System;
using System.Collections.Generic;
using System.Linq;

namespace Poker_Game
{
    public static class HandEvaluator
    {
        private static readonly Dictionary<string, int> ValeurCarte =
            new Dictionary<string, int>
            {
                {"2",2},{"3",3},{"4",4},{"5",5},{"6",6},{"7",7},{"8",8},{"9",9},
                {"10",10},{"jack",11},{"queen",12},{"king",13},{"ace",14}
            };

        public static HandResult Evaluer(List<string> cartes)
        {
            List<int> valeurs = cartes
                .Select(c => ValeurCarte[c.Split('_')[0]])
                .ToList();

            List<string> couleurs = cartes
                .Select(c => c.Split('_')[2])
                .ToList();

            var groupesValeurs = valeurs
                .GroupBy(v => v)
                .OrderByDescending(g => g.Count())
                .ThenByDescending(g => g.Key)
                .ToList();

            var groupeCouleur = couleurs
                .GroupBy(c => c)
                .FirstOrDefault(g => g.Count() >= 5);

            /* ================= QUINTE FLUSH ================= */
            if (groupeCouleur != null)
            {
                List<int> valeursCouleur = cartes
                    .Where(c => c.EndsWith(groupeCouleur.Key))
                    .Select(c => ValeurCarte[c.Split('_')[0]])
                    .ToList();

                if (EstQuinte(valeursCouleur, out int hauteQF))
                {
                    return new HandResult
                    {
                        Rank = HandRank.QuinteFlush,
                        Valeurs = new List<int> { hauteQF }
                    };
                }
            }

            /* ================= CARRÉ ================= */
            if (groupesValeurs[0].Count() == 4)
            {
                return new HandResult
                {
                    Rank = HandRank.Carre,
                    Valeurs = new List<int>
                    {
                        groupesValeurs[0].Key,
                        groupesValeurs[1].Key
                    }
                };
            }

            /* ================= FULL HOUSE ================= */
            if (groupesValeurs[0].Count() == 3 && groupesValeurs[1].Count() >= 2)
            {
                return new HandResult
                {
                    Rank = HandRank.FullHouse,
                    Valeurs = new List<int>
                    {
                        groupesValeurs[0].Key,
                        groupesValeurs[1].Key
                    }
                };
            }

            /* ================= COULEUR ================= */
            if (groupeCouleur != null)
            {
                List<int> topCouleur = valeurs
                    .Where((v, i) => couleurs[i] == groupeCouleur.Key)
                    .OrderByDescending(v => v)
                    .Take(5)
                    .ToList();

                return new HandResult
                {
                    Rank = HandRank.Couleur,
                    Valeurs = topCouleur
                };
            }

            /* ================= QUINTE ================= */
            if (EstQuinte(valeurs, out int hauteQuinte))
            {
                return new HandResult
                {
                    Rank = HandRank.Quinte,
                    Valeurs = new List<int> { hauteQuinte }
                };
            }

            /* ================= BRELAN ================= */
            if (groupesValeurs[0].Count() == 3)
            {
                return new HandResult
                {
                    Rank = HandRank.Brelan,
                    Valeurs = new List<int>
                    {
                        groupesValeurs[0].Key,
                        groupesValeurs[1].Key,
                        groupesValeurs[2].Key
                    }
                };
            }

            /* ================= DEUX PAIRES ================= */
            if (groupesValeurs[0].Count() == 2 && groupesValeurs[1].Count() == 2)
            {
                return new HandResult
                {
                    Rank = HandRank.DeuxPaires,
                    Valeurs = new List<int>
                    {
                        groupesValeurs[0].Key,
                        groupesValeurs[1].Key,
                        groupesValeurs[2].Key
                    }
                };
            }

            /* ================= PAIRE ================= */
            if (groupesValeurs[0].Count() == 2)
            {
                return new HandResult
                {
                    Rank = HandRank.Paire,
                    Valeurs = new List<int>
                    {
                        groupesValeurs[0].Key,
                        groupesValeurs[1].Key,
                        groupesValeurs[2].Key,
                        groupesValeurs[3].Key
                    }
                };
            }

            /* ================= CARTE HAUTE ================= */
            return new HandResult
            {
                Rank = HandRank.CarteHaute,
                Valeurs = valeurs
                    .OrderByDescending(v => v)
                    .Take(5)
                    .ToList()
            };
        }

        private static bool EstQuinte(List<int> valeurs, out int haute)
        {
            List<int> distincts = valeurs
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            if (distincts.Contains(14))
                distincts.Insert(0, 1);

            for (int i = 0; i <= distincts.Count - 5; i++)
            {
                if (distincts[i + 4] - distincts[i] == 4)
                {
                    haute = distincts[i + 4];
                    return true;
                }
            }

            haute = 0;
            return false;
        }
    }
}

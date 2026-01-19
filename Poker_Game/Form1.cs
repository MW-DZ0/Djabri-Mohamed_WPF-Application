using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Poker_Game
{
    public partial class MenuPrincipal : Form
    {
        public MenuPrincipal()
        {
            InitializeComponent();
            StyliserBouton(btnQuitter);
            StyliserBouton(btnCredits);
            StyliserBouton(btnHTP);
            StyliserBouton(btnStartGame);

            // ✅ NOUVEAU: Ajouter le bouton Charger Partie
            CreerBoutonCharger();
        }

        private void StyliserBouton(Button btn)
        {
            btn.BackColor = Color.Transparent;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
        }

        // ✅ NOUVEAU: Créer le bouton pour charger une partie sauvegardée
        private void CreerBoutonCharger()
        {
            Button btnChargerPartie = new Button();
            btnChargerPartie.Text = "📂 Charger Partie";
            btnChargerPartie.Size = new Size(217, 42);
            btnChargerPartie.Font = new Font("Agency FB", 16, FontStyle.Bold);
            btnChargerPartie.BackColor = Color.FromArgb(255, 152, 0);
            btnChargerPartie.ForeColor = Color.White;
            btnChargerPartie.FlatStyle = FlatStyle.Flat;
            btnChargerPartie.FlatAppearance.BorderSize = 2;
            btnChargerPartie.FlatAppearance.BorderColor = Color.White;
            btnChargerPartie.Cursor = Cursors.Hand;

            // Positionner le bouton (ajuste selon ton layout)
            // Option 1: Position fixe
            btnChargerPartie.Location = new Point(
                (this.ClientSize.Width - btnChargerPartie.Width) / 100, 250  // Ajuste cette valeur selon ton interface
            );

            // Effet hover
            btnChargerPartie.MouseEnter += (s, e) => btnChargerPartie.BackColor = Color.FromArgb(255, 180, 50);
            btnChargerPartie.MouseLeave += (s, e) => btnChargerPartie.BackColor = Color.FromArgb(255, 152, 0);

            btnChargerPartie.Click += BtnChargerPartie_Click;

            this.Controls.Add(btnChargerPartie);
        }

        // ✅ NOUVEAU: Gestionnaire du bouton Charger Partie
        private void BtnChargerPartie_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog
                {
                    Filter = "Fichier de sauvegarde Poker (*.json)|*.json",
                    Title = "Charger une partie sauvegardée",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    // Lire le fichier pour vérifier s'il est valide et s'il est en mode réseau
                    string json = File.ReadAllText(openDialog.FileName);
                    var state = JsonConvert.DeserializeObject<GameStateSauvegarde>(json);

                    if (state == null || state.Joueurs == null || state.Joueurs.Count == 0)
                    {
                        MessageBox.Show("Le fichier de sauvegarde est corrompu ou invalide.",
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Afficher les infos de la sauvegarde
                    string modeInfo = state.ModeReseau ? "Réseau (Multijoueur)" : "Local";
                    string joueursInfo = string.Join(", ", state.Joueurs.ConvertAll(j => j.Nom));

                    DialogResult confirm = MessageBox.Show(
                        $"📁 Fichier: {Path.GetFileName(openDialog.FileName)}\n" +
                        $"📅 Sauvegardé le: {state.DateSauvegarde:dd/MM/yyyy HH:mm}\n" +
                        $"🎮 Mode: {modeInfo}\n" +
                        $"👥 Joueurs: {joueursInfo}\n" +
                        $"💰 Pot: {state.Pot} €\n" +
                        $"🎴 Tour: {GetNomTour(state.TourActuel)}\n\n" +
                        "Voulez-vous charger cette partie ?",
                        "Charger la partie",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirm != DialogResult.Yes)
                        return;

                    // Créer le jeu avec le fichier de sauvegarde
                    JeuPoker jeu = new JeuPoker(openDialog.FileName);
                    jeu.Show();
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement:\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ✅ NOUVEAU: Obtenir le nom du tour
        private string GetNomTour(int tour)
        {
            switch (tour)
            {
                case 0: return "Pré-flop";
                case 1: return "Flop";
                case 2: return "Turn";
                case 3: return "River";
                default: return "Fin";
            }
        }

        // ✅ NOUVEAU: Classe pour désérialiser la sauvegarde (doit correspondre à celle dans JeuPoker)
        [Serializable]
        public class GameStateSauvegarde
        {
            public System.Collections.Generic.List<Joueur> Joueurs { get; set; }
            public System.Collections.Generic.List<string> CartesCommunes { get; set; }
            public int Pot { get; set; }
            public int TourActuel { get; set; }
            public int JoueurActuel { get; set; }
            public int MiseActuelle { get; set; }
            public System.Collections.Generic.List<string> Deck { get; set; }
            public int MiseMinimum { get; set; }
            public int MiseDuTourActuel { get; set; }
            public System.Collections.Generic.List<int> JoueursAyantJoue { get; set; }
            public bool AEuUneRelance { get; set; }
            public DateTime DateSauvegarde { get; set; }
            public bool ModeReseau { get; set; }
            public int NombreJoueurs { get; set; }
        }

        private void btnQuitter_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnHTP_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://www.joa.fr/casinos/jeux/texas-hold-em-poker",
                UseShellExecute = true
            });
        }

        private void btnCredits_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Jeu créé par Djabri Mohamed\nMention spéciale à ChatGPT",
                "Crédits");
        }

        // Mode local
        private void btnStartGame_Click(object sender, EventArgs e)
        {
            ConfigPartiecs config = new ConfigPartiecs();
            config.Show();
            this.Hide();
        }

        // Mode réseau unifié (Serveur OU Client)
        private void btnCreerReseau_Click(object sender, EventArgs e)
        {
            var joueurs = new System.Collections.Generic.List<string>
            {
                "Joueur 1",
                "Joueur 2"
            };

            JeuPoker jeu = new JeuPoker(joueurs, 1000, modeReseau: true);
            jeu.Show();
        }

        // Client rejoint via le même système
        private void btnRejoindre_Click(object sender, EventArgs e)
        {
            var joueurs = new System.Collections.Generic.List<string>
            {
                "Joueur 1",
                "Joueur 2"
            };

            JeuPoker jeu = new JeuPoker(joueurs, 1000, modeReseau: true);
            jeu.Show();
        }
    }
}
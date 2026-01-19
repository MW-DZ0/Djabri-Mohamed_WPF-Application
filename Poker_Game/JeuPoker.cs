using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;

namespace Poker_Game
{
    public partial class JeuPoker : Form
    {
        private int argentJoueur;
        private int nombreAdversaires;

        private PokerServer serveur;
        private PokerClient client;
        private bool isServeur;
        private bool modeReseau;
        private TextBox logBox;
        private int monJoueurIndex = 0;

        private List<Joueur> joueurs = new List<Joueur>();
        private List<string> cartesCommunes = new List<string>();
        private List<int> joueursAyantJoue = new List<int>();
        private bool aEuUneRelance = false;

        private List<string> deck = new List<string>();
        private Random rng = new Random();

        private int pot = 0;
        private int miseMinimum = 50;
        private int miseActuelle = 0;
        private int tourActuel = 0; // 0: préflop, 1: flop, 2: turn, 3: river
        private int joueurActuel = 0;
        private int miseDuTourActuel = 0;

        // Contrôles d'interface
        private ListBox listBoxJoueurs = new ListBox();
        private Label labelPot;
        private Label labelTour;
        private Label labelStatut;
        private Button boutonSuivre;
        private Button boutonRelancer;
        private Button boutonPasser;
        private TextBox miseInput;
        private Button boutonMiser;
        private Button boutonNouvellePartie;
        private Button boutonCheck;
        private Label labelJoueurActuel;
        private Button boutonMasquerCartes;
        private bool cartesVisibles = true;

        // ✅ Bouton sauvegarde (le chargement est dans le menu principal)
        private Button boutonSauvegarder;

        private List<PictureBox> pictureBoxesCartes = new List<PictureBox>();

        // ✅ Chemin du fichier de sauvegarde
        private const string FICHIER_SAUVEGARDE = "sauvegarde_poker.json";

        public JeuPoker(List<string> nomsJoueurs, int argentInitial, bool modeReseau = false)
        {
            this.argentJoueur = argentInitial;
            this.nombreAdversaires = nomsJoueurs.Count - 1;

            InitializeComponent();
            InitialiserInterface();

            if (modeReseau)
            {
                InitialiserReseau();
            }

            InitialiserJoueursHumains(nomsJoueurs, argentInitial);
            NouvellePartie();
        }

        // ✅ CONSTRUCTEUR pour charger une partie sauvegardée (appelé depuis le menu principal)
        public JeuPoker(string fichierSauvegarde)
        {
            InitializeComponent();
            InitialiserInterface();

            // Charger la partie depuis le fichier
            if (!ChargerPartieDepuisFichier(fichierSauvegarde))
            {
                MessageBox.Show("Erreur lors du chargement de la sauvegarde. Démarrage d'une nouvelle partie.",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Fallback: créer une partie par défaut
                var joueursDéfaut = new List<string> { "Joueur 1", "Joueur 2" };
                InitialiserJoueursHumains(joueursDéfaut, 1000);
                NouvellePartie();
            }
        }

        private async void InitialiserReseau()
        {
            this.modeReseau = true;

            logBox = new TextBox
            {
                Location = new Point(20, 680),
                Size = new Size(760, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.LimeGreen,
                Font = new Font("Consolas", 9)
            };
            this.Controls.Add(logBox);

            Log("=== Initialisation du mode réseau ===");

            NetworkConfigForm configForm = new NetworkConfigForm();
            if (configForm.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show("Configuration annulée. Mode local activé.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                modeReseau = false;
                logBox.Visible = false;
                return;
            }

            isServeur = configForm.IsServer;

            if (isServeur)
            {
                Log("Mode SERVEUR sélectionné");
                monJoueurIndex = 0;

                serveur = new PokerServer();
                serveur.OnLog += Log;
                serveur.OnActionRecue += TraiterActionDistante;

                bool success = await serveur.StartServerAsync();
                if (success)
                {
                    Log("✓ Serveur démarré avec succès!");
                    Log("En attente de la connexion du client...");

                    int tentatives = 0;
                    while (!serveur.IsConnected && tentatives < 100)
                    {
                        await Task.Delay(100);
                        tentatives++;
                    }

                    if (serveur.IsConnected)
                    {
                        await serveur.EnvoyerEtatAsync(CreerEtat());
                        Log("État initial envoyé au client");
                        Log($"Vous contrôlez : {joueurs[0].Nom}");
                    }
                }
                else
                {
                    Log("✗ Échec du démarrage du serveur");
                    MessageBox.Show("Impossible de démarrer le serveur.",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    modeReseau = false;
                }
            }
            else
            {
                Log("Mode CLIENT sélectionné");
                monJoueurIndex = 1;

                Log($"Tentative de connexion à {configForm.ServerIP}...");

                client = new PokerClient();

                Log("🔗 Attachement des événements...");
                client.OnLog += Log;
                client.OnEtatRecu += ChargerEtat;
                Log("✓ Événements attachés (OnLog et OnEtatRecu)");

                bool success = await client.ConnectAsync(configForm.ServerIP);
                if (!success)
                {
                    Log("✗ Connexion échouée");
                    MessageBox.Show("Impossible de se connecter au serveur.\n\n" +
                        "Vérifiez que :\n" +
                        "- Le serveur est démarré\n" +
                        "- L'adresse IP est correcte\n" +
                        "- Le pare-feu autorise le port 5000",
                        "Erreur de connexion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    modeReseau = false;
                    logBox.Visible = false;
                }
                else
                {
                    Log("✓ Connecté au serveur avec succès!");
                    Log($"Vous contrôlez : {joueurs[1].Nom}");
                    Log("👂 En écoute des messages du serveur...");
                }
            }

            VerifierTourJoueur();
        }

        private void VerifierTourJoueur()
        {
            if (!modeReseau)
            {
                ActiverBoutonsAction(true);
                return;
            }

            bool monTour = (joueurActuel == monJoueurIndex);

            ActiverBoutonsAction(monTour);

            if (monTour)
            {
                Log($"C'est votre tour !");
                labelStatut.Text = "À votre tour de jouer";
                labelStatut.ForeColor = Color.LimeGreen;
            }
            else
            {
                Log($"En attente du joueur {joueurs[joueurActuel].Nom}...");
                labelStatut.Text = $"Tour de {joueurs[joueurActuel].Nom} - Veuillez patienter";
                labelStatut.ForeColor = Color.Orange;
            }
        }

        private void AfficherMesCartes()
        {
            if (!modeReseau)
            {
                AfficherCartesDuJoueur();
                return;
            }

            if (!cartesVisibles) return;

            var joueur = joueurs[monJoueurIndex];
            int x = 20;
            int y = 350;

            labelStatut.Text = $"Vos cartes ({joueur.Nom}):";

            for (int i = 0; i < joueur.Cartes.Count; i++)
            {
                try
                {
                    var carteNom = joueur.Cartes[i];
                    var image = Properties.Resources.ResourceManager.GetObject(carteNom) as byte[];
                    if (image == null) continue;

                    PictureBox pb = new PictureBox();
                    pb.Image = ByteArrayToImage(image);
                    pb.SizeMode = PictureBoxSizeMode.StretchImage;
                    pb.Location = new Point(x, y);
                    pb.Size = new Size(80, 120);
                    pb.Tag = i;

                    pb.Click += CarteJoueur_Click;
                    pb.Cursor = Cursors.Hand;

                    this.Controls.Add(pb);
                    pictureBoxesCartes.Add(pb);
                    x += 90;
                }
                catch { continue; }
            }
        }

        private void Log(string message)
        {
            // ✅ Vérifier si logBox existe (mode local = pas de logBox)
            if (logBox == null) return;

            if (logBox.InvokeRequired)
            {
                logBox.Invoke(new Action<string>(Log), message);
                return;
            }

            logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
        }

        private void TraiterActionDistante(PlayerAction action)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<PlayerAction>(TraiterActionDistante), action);
                return;
            }

            Log($"Action reçue : {action.Type} (Montant: {action.Montant})");

            int joueurDistant = isServeur ? 1 : 0;

            if (joueurDistant >= joueurs.Count)
            {
                Log("Erreur: Joueur distant introuvable");
                return;
            }

            Joueur j = joueurs[joueurDistant];

            switch (action.Type)
            {
                case "Miser":
                    j.Argent -= action.Montant;
                    j.MiseActuelle += action.Montant;
                    pot += action.Montant;
                    miseActuelle = Math.Max(miseActuelle, j.MiseActuelle);
                    Log($"{j.Nom} mise {action.Montant} €");
                    break;

                case "Suivre":
                    int montantSuivre = miseActuelle - j.MiseActuelle;
                    j.Argent -= montantSuivre;
                    j.MiseActuelle += montantSuivre;
                    pot += montantSuivre;
                    Log($"{j.Nom} suit avec {montantSuivre} €");
                    break;

                case "Passer":
                    j.APasse = true;
                    Log($"{j.Nom} se couche");
                    break;

                case "Check":
                    Log($"{j.Nom} checke");
                    break;

                case "Relancer":
                    int montantRelance = action.Montant;
                    j.Argent -= montantRelance;
                    j.MiseActuelle += montantRelance;
                    pot += montantRelance;
                    miseActuelle = j.MiseActuelle;
                    Log($"{j.Nom} relance à {montantRelance} €");
                    break;
            }

            AfficherJoueurs();
            labelPot.Text = $"Pot: {pot} €";

            if (!joueursAyantJoue.Contains(joueurDistant))
            {
                joueursAyantJoue.Add(joueurDistant);
            }

            PasserAuJoueurSuivant();
            VerifierTourJoueur();
        }

        private async void EnvoyerActionReseau(string type, int montant)
        {
            if (!modeReseau) return;

            PlayerAction action = new PlayerAction
            {
                Type = type,
                Montant = montant
            };

            try
            {
                if (isServeur && serveur != null && serveur.IsConnected)
                {
                    Log($"📤 Envoi état après action: {type}");
                    await serveur.EnvoyerEtatAsync(CreerEtat());
                    Log($"✓ État envoyé (Joueur actuel: {joueurActuel})");
                }
                else if (!isServeur && client != null && client.IsConnected)
                {
                    Log($"📤 Envoi action au serveur: {type} ({montant} €)");
                    await client.EnvoyerActionAsync(action);
                }
                else
                {
                    Log("⚠ Impossible d'envoyer l'action: non connecté");
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Erreur envoi action: {ex.Message}");
            }
        }

        private void InitialiserInterface()
        {
            this.BackColor = Color.FromArgb(0, 100, 0);
            this.Size = new Size(1200, 800);

            // ==================== ZONE D'INFORMATIONS (Haut) ====================

            labelPot = new Label();
            labelPot.Text = "Pot: 0 €";
            labelPot.Location = new Point(450, 30);
            labelPot.Size = new Size(300, 50);
            labelPot.TextAlign = ContentAlignment.MiddleCenter;
            labelPot.Font = new Font("Arial", 24, FontStyle.Bold);
            labelPot.ForeColor = Color.Gold;
            labelPot.BackColor = Color.FromArgb(0, 50, 0);
            labelPot.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(labelPot);

            labelTour = new Label();
            labelTour.Text = "Tour: Pré-flop";
            labelTour.Location = new Point(450, 90);
            labelTour.Size = new Size(300, 35);
            labelTour.TextAlign = ContentAlignment.MiddleCenter;
            labelTour.Font = new Font("Arial", 14, FontStyle.Bold);
            labelTour.ForeColor = Color.White;
            labelTour.BackColor = Color.FromArgb(0, 70, 0);
            this.Controls.Add(labelTour);

            labelJoueurActuel = new Label();
            labelJoueurActuel.Text = "Tour de: ";
            labelJoueurActuel.Location = new Point(450, 135);
            labelJoueurActuel.Size = new Size(300, 40);
            labelJoueurActuel.TextAlign = ContentAlignment.MiddleCenter;
            labelJoueurActuel.Font = new Font("Arial", 16, FontStyle.Bold);
            labelJoueurActuel.ForeColor = Color.Cyan;
            labelJoueurActuel.BackColor = Color.FromArgb(0, 90, 0);
            this.Controls.Add(labelJoueurActuel);

            labelStatut = new Label();
            labelStatut.Text = "À votre tour";
            labelStatut.Location = new Point(400, 185);
            labelStatut.Size = new Size(400, 30);
            labelStatut.TextAlign = ContentAlignment.MiddleCenter;
            labelStatut.Font = new Font("Arial", 14, FontStyle.Italic);
            labelStatut.ForeColor = Color.Yellow;
            this.Controls.Add(labelStatut);

            // ==================== ZONE JOUEURS (Droite) ====================

            Panel panelJoueurs = new Panel();
            panelJoueurs.Location = new Point(950, 30);
            panelJoueurs.Size = new Size(230, 350);
            panelJoueurs.BackColor = Color.FromArgb(20, 20, 20);
            panelJoueurs.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(panelJoueurs);

            Label labelTitreJoueurs = new Label();
            labelTitreJoueurs.Text = "JOUEURS";
            labelTitreJoueurs.Location = new Point(5, 5);
            labelTitreJoueurs.Size = new Size(220, 30);
            labelTitreJoueurs.TextAlign = ContentAlignment.MiddleCenter;
            labelTitreJoueurs.Font = new Font("Arial", 14, FontStyle.Bold);
            labelTitreJoueurs.ForeColor = Color.Gold;
            panelJoueurs.Controls.Add(labelTitreJoueurs);

            listBoxJoueurs.Size = new Size(220, 305);
            listBoxJoueurs.Location = new Point(5, 40);
            listBoxJoueurs.BackColor = Color.FromArgb(40, 40, 40);
            listBoxJoueurs.ForeColor = Color.White;
            listBoxJoueurs.Font = new Font("Consolas", 10);
            listBoxJoueurs.BorderStyle = BorderStyle.None;
            panelJoueurs.Controls.Add(listBoxJoueurs);

            // ==================== ZONE CONTRÔLES (Bas) ====================

            Panel panelControles = new Panel();
            panelControles.Location = new Point(520, 520);
            panelControles.Size = new Size(760, 120);
            panelControles.BackColor = Color.FromArgb(30, 30, 30);
            panelControles.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(panelControles);

            Label labelMise = new Label();
            labelMise.Text = "Votre mise:";
            labelMise.Location = new Point(20, 25);
            labelMise.AutoSize = true;
            labelMise.Font = new Font("Arial", 12, FontStyle.Bold);
            labelMise.ForeColor = Color.White;
            panelControles.Controls.Add(labelMise);

            miseInput = new TextBox();
            miseInput.Location = new Point(130, 22);
            miseInput.Width = 100;
            miseInput.Height = 30;
            miseInput.Font = new Font("Arial", 14);
            miseInput.TextAlign = HorizontalAlignment.Center;
            miseInput.Text = miseMinimum.ToString();
            panelControles.Controls.Add(miseInput);

            int btnY = 70;
            int btnWidth = 100;
            int btnHeight = 40;

            boutonMiser = new Button();
            boutonMiser.Text = "MISER";
            boutonMiser.Location = new Point(20, btnY);
            boutonMiser.Size = new Size(btnWidth, btnHeight);
            boutonMiser.BackColor = Color.FromArgb(200, 0, 0);
            boutonMiser.ForeColor = Color.White;
            boutonMiser.Font = new Font("Arial", 11, FontStyle.Bold);
            boutonMiser.FlatStyle = FlatStyle.Flat;
            boutonMiser.FlatAppearance.BorderSize = 0;
            boutonMiser.Cursor = Cursors.Hand;
            boutonMiser.Click += BoutonMiser_Click;
            panelControles.Controls.Add(boutonMiser);

            boutonCheck = new Button();
            boutonCheck.Text = "CHECK";
            boutonCheck.Location = new Point(130, btnY);
            boutonCheck.Size = new Size(btnWidth, btnHeight);
            boutonCheck.BackColor = Color.FromArgb(0, 120, 215);
            boutonCheck.ForeColor = Color.White;
            boutonCheck.Font = new Font("Arial", 11, FontStyle.Bold);
            boutonCheck.FlatStyle = FlatStyle.Flat;
            boutonCheck.FlatAppearance.BorderSize = 0;
            boutonCheck.Cursor = Cursors.Hand;
            boutonCheck.Click += BoutonCheck_Click;
            panelControles.Controls.Add(boutonCheck);

            boutonSuivre = new Button();
            boutonSuivre.Text = "SUIVRE";
            boutonSuivre.Location = new Point(240, btnY);
            boutonSuivre.Size = new Size(btnWidth, btnHeight);
            boutonSuivre.BackColor = Color.FromArgb(255, 193, 7);
            boutonSuivre.ForeColor = Color.Black;
            boutonSuivre.Font = new Font("Arial", 11, FontStyle.Bold);
            boutonSuivre.FlatStyle = FlatStyle.Flat;
            boutonSuivre.FlatAppearance.BorderSize = 0;
            boutonSuivre.Cursor = Cursors.Hand;
            boutonSuivre.Click += BoutonSuivre_Click;
            panelControles.Controls.Add(boutonSuivre);

            boutonRelancer = new Button();
            boutonRelancer.Text = "RELANCER";
            boutonRelancer.Location = new Point(350, btnY);
            boutonRelancer.Size = new Size(btnWidth, btnHeight);
            boutonRelancer.BackColor = Color.FromArgb(156, 39, 176);
            boutonRelancer.ForeColor = Color.White;
            boutonRelancer.Font = new Font("Arial", 11, FontStyle.Bold);
            boutonRelancer.FlatStyle = FlatStyle.Flat;
            boutonRelancer.FlatAppearance.BorderSize = 0;
            boutonRelancer.Cursor = Cursors.Hand;
            boutonRelancer.Click += BoutonRelancer_Click;
            panelControles.Controls.Add(boutonRelancer);

            boutonPasser = new Button();
            boutonPasser.Text = "PASSER";
            boutonPasser.Location = new Point(460, btnY);
            boutonPasser.Size = new Size(btnWidth, btnHeight);
            boutonPasser.BackColor = Color.FromArgb(96, 96, 96);
            boutonPasser.ForeColor = Color.White;
            boutonPasser.Font = new Font("Arial", 11, FontStyle.Bold);
            boutonPasser.FlatStyle = FlatStyle.Flat;
            boutonPasser.FlatAppearance.BorderSize = 0;
            boutonPasser.Cursor = Cursors.Hand;
            boutonPasser.Click += BoutonPasser_Click;
            panelControles.Controls.Add(boutonPasser);

            boutonMasquerCartes = new Button();
            boutonMasquerCartes.Text = "👁 Masquer";
            boutonMasquerCartes.Location = new Point(570, btnY);
            boutonMasquerCartes.Size = new Size(btnWidth + 20, btnHeight);
            boutonMasquerCartes.BackColor = Color.FromArgb(103, 58, 183);
            boutonMasquerCartes.ForeColor = Color.White;
            boutonMasquerCartes.Font = new Font("Arial", 10, FontStyle.Bold);
            boutonMasquerCartes.FlatStyle = FlatStyle.Flat;
            boutonMasquerCartes.FlatAppearance.BorderSize = 0;
            boutonMasquerCartes.Cursor = Cursors.Hand;
            boutonMasquerCartes.Click += BoutonMasquerCartes_Click;
            panelControles.Controls.Add(boutonMasquerCartes);

            // ==================== BOUTONS NOUVELLE PARTIE / SAUVEGARDE / CHARGEMENT ====================

            boutonNouvellePartie = new Button();
            boutonNouvellePartie.Text = "🔄 NOUVELLE PARTIE";
            boutonNouvellePartie.Location = new Point(800, 450);
            boutonNouvellePartie.Size = new Size(180, 50);
            boutonNouvellePartie.BackColor = Color.FromArgb(0, 150, 0);
            boutonNouvellePartie.ForeColor = Color.White;
            boutonNouvellePartie.Font = new Font("Arial", 12, FontStyle.Bold);
            boutonNouvellePartie.FlatStyle = FlatStyle.Flat;
            boutonNouvellePartie.FlatAppearance.BorderSize = 2;
            boutonNouvellePartie.FlatAppearance.BorderColor = Color.Gold;
            boutonNouvellePartie.Cursor = Cursors.Hand;
            boutonNouvellePartie.Click += BoutonNouvellePartie_Click;
            boutonNouvellePartie.Visible = false;
            this.Controls.Add(boutonNouvellePartie);

            // ✅ NOUVEAU: Bouton Sauvegarder (le chargement est dans le menu principal)
            boutonSauvegarder = new Button();
            boutonSauvegarder.Text = "💾 Sauvegarder";
            boutonSauvegarder.Location = new Point(20, 570);
            boutonSauvegarder.Size = new Size(150, 45);
            boutonSauvegarder.BackColor = Color.FromArgb(33, 150, 243);
            boutonSauvegarder.ForeColor = Color.White;
            boutonSauvegarder.Font = new Font("Arial", 11, FontStyle.Bold);
            boutonSauvegarder.FlatStyle = FlatStyle.Flat;
            boutonSauvegarder.FlatAppearance.BorderSize = 0;
            boutonSauvegarder.Cursor = Cursors.Hand;
            boutonSauvegarder.Click += BoutonSauvegarder_Click;
            this.Controls.Add(boutonSauvegarder);

            // Ajouter des effets hover sur les boutons
            AjouterEffetsHover(boutonMiser, Color.FromArgb(220, 20, 20));
            AjouterEffetsHover(boutonCheck, Color.FromArgb(20, 140, 235));
            AjouterEffetsHover(boutonSuivre, Color.FromArgb(255, 213, 27));
            AjouterEffetsHover(boutonRelancer, Color.FromArgb(176, 59, 196));
            AjouterEffetsHover(boutonPasser, Color.FromArgb(116, 116, 116));
            AjouterEffetsHover(boutonMasquerCartes, Color.FromArgb(123, 78, 203));
            AjouterEffetsHover(boutonNouvellePartie, Color.FromArgb(20, 170, 20));
            AjouterEffetsHover(boutonSauvegarder, Color.FromArgb(53, 170, 255));
        }

        private void AjouterEffetsHover(Button btn, Color hoverColor)
        {
            Color originalColor = btn.BackColor;

            btn.MouseEnter += (s, e) => {
                btn.BackColor = hoverColor;
            };

            btn.MouseLeave += (s, e) => {
                btn.BackColor = originalColor;
            };
        }

        GameState CreerEtat()
        {
            return new GameState
            {
                Joueurs = joueurs,
                CartesCommunes = cartesCommunes,
                Pot = pot,
                TourActuel = tourActuel,
                JoueurActuel = joueurActuel,
                MiseActuelle = miseActuelle
            };
        }

        // ✅ NOUVEAU: Classe étendue pour la sauvegarde complète
        [Serializable]
        public class GameStateSauvegarde : GameState
        {
            public List<string> Deck { get; set; }
            public int MiseMinimum { get; set; }
            public int MiseDuTourActuel { get; set; }
            public List<int> JoueursAyantJoue { get; set; }
            public bool AEuUneRelance { get; set; }
            public DateTime DateSauvegarde { get; set; }

            // ✅ NOUVEAU: Informations réseau
            public bool ModeReseau { get; set; }
            public int NombreJoueurs { get; set; }
        }

        // ✅ NOUVEAU: Créer un état complet pour la sauvegarde
        private GameStateSauvegarde CreerEtatComplet()
        {
            return new GameStateSauvegarde
            {
                Joueurs = joueurs,
                CartesCommunes = cartesCommunes,
                Pot = pot,
                TourActuel = tourActuel,
                JoueurActuel = joueurActuel,
                MiseActuelle = miseActuelle,
                Deck = deck,
                MiseMinimum = miseMinimum,
                MiseDuTourActuel = miseDuTourActuel,
                JoueursAyantJoue = joueursAyantJoue.ToList(),
                AEuUneRelance = aEuUneRelance,
                DateSauvegarde = DateTime.Now,
                // ✅ NOUVEAU: Sauvegarder les infos réseau
                ModeReseau = modeReseau,
                NombreJoueurs = joueurs.Count
            };
        }

        void ChargerEtat(GameState state)
        {
            Log("📥 ChargerEtat() appelé !");

            if (this.InvokeRequired)
            {
                Log("⚡ Invocation sur thread UI...");
                this.Invoke(new Action<GameState>(ChargerEtat), state);
                return;
            }

            Log($"📊 Réception de l'état - Joueur actuel: {state.JoueurActuel}, Pot: {state.Pot}");

            int ancienJoueurActuel = joueurActuel;

            joueurs = state.Joueurs;
            cartesCommunes = state.CartesCommunes;
            pot = state.Pot;
            tourActuel = state.TourActuel;
            joueurActuel = state.JoueurActuel;
            miseActuelle = state.MiseActuelle;

            Log($"✓ État chargé - Ancien joueur: {ancienJoueurActuel}, Nouveau: {joueurActuel}");

            AfficherJoueurs();

            var cartesCommunes_pb = pictureBoxesCartes.Where(pb => pb.Location.Y == 200).ToList();
            foreach (var pb in cartesCommunes_pb)
            {
                this.Controls.Remove(pb);
                pb.Dispose();
                pictureBoxesCartes.Remove(pb);
            }

            AfficherCartesCommunes();

            SupprimerCartesJoueur();
            if (modeReseau)
            {
                Log($"🃏 Affichage des cartes du joueur {monJoueurIndex}");
                AfficherMesCartes();
            }
            else
            {
                AfficherCartesDuJoueur();
            }

            labelPot.Text = $"Pot: {pot} €";

            string[] tours = { "Pré-flop", "Flop", "Turn", "River" };
            if (tourActuel >= 0 && tourActuel < tours.Length)
            {
                labelTour.Text = $"Tour: {tours[tourActuel]}";
            }

            if (joueurActuel < joueurs.Count)
            {
                labelJoueurActuel.Text = $"Tour de: {joueurs[joueurActuel].Nom}";
            }

            Log($"🎮 Vérification du tour - Mon index: {monJoueurIndex}, Joueur actuel: {joueurActuel}");
            VerifierTourJoueur();

            Log($"✅ État synchronisé - C'est {(joueurActuel == monJoueurIndex ? "VOTRE" : "le")} tour");
        }

        private void InitialiserJoueursHumains(List<string> noms, int argent)
        {
            joueurs.Clear();

            for (int i = 0; i < noms.Count; i++)
            {
                joueurs.Add(new Joueur(noms[i], argent, true));
            }

            joueurActuel = 0;
        }

        private void NouvellePartie()
        {
            SupprimerCartesAffichees();

            pot = 0;
            miseActuelle = miseMinimum;
            miseDuTourActuel = 0;
            tourActuel = 0;
            joueurActuel = 0;
            cartesCommunes.Clear();

            labelPot.Text = "Pot: 0 €";
            labelTour.Text = "Tour: Pré-flop";
            labelStatut.Text = "À votre tour";

            boutonNouvellePartie.Visible = false;
            ActiverBoutonsAction(true);

            InitialiserDeck();
            DistribuerCartes();
            AfficherCartesDuJoueur();
            AfficherJoueurs();
            MisesObligatoires();

            if (modeReseau)
            {
                SupprimerCartesJoueur();
                AfficherMesCartes();
                VerifierTourJoueur();
            }
        }

        private void SupprimerCartesAffichees()
        {
            foreach (var pb in pictureBoxesCartes.ToList())
            {
                if (pb != null && this.Controls.Contains(pb))
                {
                    this.Controls.Remove(pb);
                    pb.Dispose();
                }
            }
            pictureBoxesCartes.Clear();
        }

        private void MisesObligatoires()
        {
            if (joueurs.Count >= 2)
            {
                int smallBlindIndex = 0;
                int smallBlind = miseMinimum / 2;
                FaireUneMise(joueurs[smallBlindIndex], smallBlind);
                listBoxJoueurs.Items.Add($"{joueurs[smallBlindIndex].Nom} (Small Blind): {smallBlind} €");

                int bigBlindIndex = 1;
                FaireUneMise(joueurs[bigBlindIndex], miseMinimum);
                listBoxJoueurs.Items.Add($"{joueurs[bigBlindIndex].Nom} (Big Blind): {miseMinimum} €");

                miseActuelle = miseMinimum;
                labelPot.Text = $"Pot: {pot} €";
            }
        }

        private void AfficherJoueurs()
        {
            listBoxJoueurs.Items.Clear();
            foreach (var joueur in joueurs)
            {
                string statut = joueur.APasse ? " (Passé)" : "";
                listBoxJoueurs.Items.Add($"{joueur.Nom} - {joueur.Argent}€ - Mise: {joueur.MiseActuelle}€{statut}");
            }
        }

        private void AfficherCartesDuJoueur()
        {
            if (!cartesVisibles) return;

            var joueur = joueurs[joueurActuel];
            int x = 20;
            int y = 350;

            labelStatut.Text = $"Cartes de {joueur.Nom}:";

            for (int i = 0; i < joueur.Cartes.Count; i++)
            {
                try
                {
                    var carteNom = joueur.Cartes[i];
                    var image = Properties.Resources.ResourceManager.GetObject(carteNom) as byte[];
                    if (image == null) continue;

                    PictureBox pb = new PictureBox();
                    pb.Image = ByteArrayToImage(image);
                    pb.SizeMode = PictureBoxSizeMode.StretchImage;
                    pb.Location = new Point(x, y);
                    pb.Size = new Size(80, 120);

                    pb.Tag = i;

                    if (joueurActuel == 0)
                    {
                        pb.Click += CarteJoueur_Click;
                        pb.Cursor = Cursors.Hand;
                    }

                    this.Controls.Add(pb);
                    pictureBoxesCartes.Add(pb);
                    x += 90;
                }
                catch { continue; }
            }
        }

        private void AfficherCartesCommunes()
        {
            int x = 250;
            int y = 200;

            foreach (var carteNom in cartesCommunes)
            {
                try
                {
                    var image = Properties.Resources.ResourceManager.GetObject(carteNom) as byte[];
                    if (image == null) continue;

                    PictureBox pb = new PictureBox();
                    pb.Image = ByteArrayToImage(image);
                    pb.SizeMode = PictureBoxSizeMode.StretchImage;
                    pb.Location = new Point(x, y);
                    pb.Size = new Size(80, 120);
                    this.Controls.Add(pb);
                    pictureBoxesCartes.Add(pb);
                    x += 90;
                }
                catch { continue; }
            }
        }

        private void InitialiserDeck()
        {
            deck.Clear();
            string[] valeurs = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "jack", "queen", "king", "ace" };
            string[] couleurs = { "hearts", "diamonds", "clubs", "spades" };

            foreach (var couleur in couleurs)
            {
                foreach (var valeur in valeurs)
                {
                    deck.Add($"{valeur}_of_{couleur}");
                }
            }

            deck = deck.OrderBy(x => rng.Next()).ToList();
        }

        private void DistribuerCartes()
        {
            foreach (var joueur in joueurs)
            {
                joueur.Cartes.Clear();
                joueur.Cartes.Add(deck[0]);
                deck.RemoveAt(0);
                joueur.Cartes.Add(deck[0]);
                deck.RemoveAt(0);
                joueur.MiseActuelle = 0;
                joueur.APasse = false;
            }
        }

        private void TirerCartesCommunes(int nombreDeCartes)
        {
            for (int i = 0; i < nombreDeCartes; i++)
            {
                cartesCommunes.Add(deck[0]);
                deck.RemoveAt(0);
            }
        }

        private void FaireUneMise(Joueur joueur, int montant)
        {
            if (montant <= 0) return;

            if (montant > joueur.Argent)
            {
                montant = joueur.Argent;
            }

            joueur.Argent -= montant;
            joueur.MiseActuelle += montant;
            pot += montant;

            labelPot.Text = $"Pot: {pot} €";
            AfficherJoueurs();

            if (joueur.Argent == 0)
            {
                MessageBox.Show($"{joueur.Nom} est All-in!");
                Log($"⚠ {joueur.Nom} est All-in (0€ restant)");
            }
        }

        private void PasserAuJoueurSuivant()
        {
            if (!joueursAyantJoue.Contains(joueurActuel))
            {
                joueursAyantJoue.Add(joueurActuel);
            }

            int joueursActifs = joueurs.Count(j => !j.APasse);
            if (joueursActifs <= 1)
            {
                TerminerPartie();
                return;
            }

            if (TourEncheresTermine())
            {
                PasserAuTourSuivant();
                return;
            }

            int joueurDepart = joueurActuel;
            int tentatives = 0;

            do
            {
                joueurActuel = (joueurActuel + 1) % joueurs.Count;
                tentatives++;

                if (tentatives >= joueurs.Count)
                {
                    PasserAuTourSuivant();
                    return;
                }
            }
            while (joueurs[joueurActuel].APasse);

            VerifierFinDeJeu();
            MettreAJourInterfaceJoueurActuel();
        }

        private bool TourEncheresTermine()
        {
            var joueursActifs = joueurs.Where(j => !j.APasse).ToList();

            if (joueursActifs.Count <= 1) return true;

            int miseReference = joueursActifs.First().MiseActuelle;
            bool tousEgaux = joueursActifs.All(j => j.MiseActuelle == miseReference);

            if (!tousEgaux) return false;

            var indicesJoueursActifs = joueurs
                .Select((joueur, index) => new { joueur, index })
                .Where(x => !x.joueur.APasse)
                .Select(x => x.index)
                .ToList();

            bool tousOntJoue = indicesJoueursActifs.All(index => joueursAyantJoue.Contains(index));

            if (tourActuel == 0 && tousEgaux && !tousOntJoue)
            {
                return false;
            }

            return tousOntJoue;
        }

        private void MettreAJourInterfaceJoueurActuel()
        {
            var joueur = joueurs[joueurActuel];
            labelJoueurActuel.Text = $"Tour de: {joueur.Nom} ({joueur.Argent}€)";

            SupprimerCartesJoueur();

            if (modeReseau)
            {
                if (cartesVisibles)
                {
                    AfficherMesCartes();
                }
                VerifierTourJoueur();
            }
            else
            {
                if (cartesVisibles)
                {
                    AfficherCartesDuJoueur();
                }
                ActiverBoutonsAction(true);
            }
        }

        private void SupprimerCartesJoueur()
        {
            var cartesJoueur = pictureBoxesCartes.Where(pb => pb.Location.Y == 350).ToList();
            foreach (var pb in cartesJoueur)
            {
                if (joueurActuel == 0)
                {
                    pb.Click -= CarteJoueur_Click;
                }

                this.Controls.Remove(pb);
                pb.Dispose();
                pictureBoxesCartes.Remove(pb);
            }
        }

        private void PasserAuTourSuivant()
        {
            tourActuel++;

            joueursAyantJoue.Clear();
            aEuUneRelance = false;

            switch (tourActuel)
            {
                case 1:
                    labelTour.Text = "Tour: Flop";
                    TirerCartesCommunes(3);
                    AfficherCartesCommunes();
                    break;
                case 2:
                    labelTour.Text = "Tour: Turn";
                    TirerCartesCommunes(1);
                    AfficherCartesCommunes();
                    break;
                case 3:
                    labelTour.Text = "Tour: River";
                    TirerCartesCommunes(1);
                    AfficherCartesCommunes();
                    break;
                default:
                    TerminerPartie();
                    return;
            }

            foreach (var joueur in joueurs.Where(j => !j.APasse))
            {
                joueur.MiseActuelle = 0;
            }
            miseActuelle = 0;
            miseDuTourActuel = 0;

            joueurActuel = 0;
            int tentatives = 0;
            while (joueurs[joueurActuel].APasse && tentatives < joueurs.Count)
            {
                joueurActuel = (joueurActuel + 1) % joueurs.Count;
                tentatives++;
            }

            if (joueurs.Count(j => !j.APasse) <= 1)
            {
                TerminerPartie();
                return;
            }

            MettreAJourInterfaceJoueurActuel();
        }

        private void TerminerPartie()
        {
            ActiverBoutonsAction(false);

            if (joueurs.Count(j => !j.APasse) == 1)
            {
                var gagnant = joueurs.First(j => !j.APasse);
                gagnant.Argent += pot;

                labelStatut.Text = $"🏆 {gagnant.Nom} remporte {pot} €!";
                labelStatut.ForeColor = Color.Gold;

                MessageBox.Show(
                    $"🏆 {gagnant.Nom} remporte le pot de {pot} €!\n\n" +
                    $"Tous les autres joueurs se sont couchés.",
                    "Fin de la manche",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                while (cartesCommunes.Count < 5)
                {
                    TirerCartesCommunes(1);
                }

                SupprimerCartesAffichees();
                AfficherCartesCommunes();
                AfficherCartesTousJoueurs();
                DeterminerGagnant();
            }

            labelPot.Text = $"Pot: 0 € (distribué)";
            pot = 0;

            boutonNouvellePartie.Visible = true;
            AfficherJoueurs();
        }

        private void AfficherCartesTousJoueurs()
        {
            int yPosition = 480;
            int xStart = 20;

            foreach (var joueur in joueurs.Where(j => !j.APasse))
            {
                int x = xStart;

                Label lblNom = new Label
                {
                    Text = $"{joueur.Nom}:",
                    Location = new Point(x, yPosition),
                    Size = new Size(100, 20),
                    ForeColor = Color.White,
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    BackColor = Color.Transparent
                };
                this.Controls.Add(lblNom);

                x += 110;

                foreach (var carteNom in joueur.Cartes)
                {
                    try
                    {
                        var image = Properties.Resources.ResourceManager.GetObject(carteNom) as byte[];
                        if (image == null) continue;

                        PictureBox pb = new PictureBox
                        {
                            Image = ByteArrayToImage(image),
                            SizeMode = PictureBoxSizeMode.StretchImage,
                            Location = new Point(x, yPosition),
                            Size = new Size(60, 90)
                        };

                        this.Controls.Add(pb);
                        pictureBoxesCartes.Add(pb);
                        x += 70;
                    }
                    catch { continue; }
                }

                xStart += 250;

                if (xStart > 800)
                {
                    xStart = 20;
                    yPosition += 120;
                }
            }
        }

        private void VerifierFinDeJeu()
        {
            foreach (var joueur in joueurs)
            {
                if (joueur.Argent <= 0 && !joueur.APasse)
                {
                    var gagnant = joueurs.FirstOrDefault(j => j.Argent > 0);

                    if (gagnant != null)
                    {
                        Log($"🏆 {gagnant.Nom} remporte la partie ! {joueur.Nom} n'a plus d'argent.");

                        MessageBox.Show(
                            $"🏆 PARTIE TERMINÉE !\n\n" +
                            $"{gagnant.Nom} remporte la partie !\n" +
                            $"{joueur.Nom} n'a plus d'argent.\n\n" +
                            $"Argent final de {gagnant.Nom} : {gagnant.Argent}€",
                            "Fin de partie",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        ActiverBoutonsAction(false);
                        boutonNouvellePartie.Visible = true;
                        boutonNouvellePartie.Text = "Rejouer";

                        return;
                    }
                }
            }
        }

        private void DeterminerGagnant()
        {
            HandResult meilleureMain = null;
            List<Joueur> gagnants = new List<Joueur>();
            Dictionary<Joueur, HandResult> mainsJoueurs = new Dictionary<Joueur, HandResult>();

            foreach (Joueur joueur in joueurs.Where(j => !j.APasse))
            {
                List<string> toutesCartes = new List<string>();
                toutesCartes.AddRange(joueur.Cartes);
                toutesCartes.AddRange(cartesCommunes);

                HandResult resultat = HandEvaluator.Evaluer(toutesCartes);
                mainsJoueurs[joueur] = resultat;

                if (meilleureMain == null || resultat.CompareTo(meilleureMain) > 0)
                {
                    meilleureMain = resultat;
                    gagnants.Clear();
                    gagnants.Add(joueur);
                }
                else if (resultat.CompareTo(meilleureMain) == 0)
                {
                    gagnants.Add(joueur);
                }
            }

            int gain = pot / gagnants.Count;
            foreach (Joueur j in gagnants)
            {
                j.Argent += gain;
            }

            string nomGagnants = string.Join(", ", gagnants.Select(j => j.Nom));
            string mainGagnante = meilleureMain?.Rank.ToString() ?? "Main inconnue";

            string mainEnFrancais = ConvertirMainEnFrancais(meilleureMain?.Rank.ToString());

            labelStatut.Text = $"🏆 {nomGagnants} gagne avec {mainEnFrancais}!";
            labelStatut.ForeColor = Color.Gold;
            labelStatut.Font = new Font("Arial", 14, FontStyle.Bold);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════");
            sb.AppendLine("        🏆 SHOWDOWN 🏆");
            sb.AppendLine("═══════════════════════════════\n");

            foreach (var kvp in mainsJoueurs.OrderByDescending(x => x.Value.CompareTo(meilleureMain) == 0 ? 1 : 0))
            {
                Joueur j = kvp.Key;
                HandResult main = kvp.Value;
                string mainFr = ConvertirMainEnFrancais(main.Rank.ToString());
                string cartesStr = string.Join(" + ", j.Cartes.Select(c => ConvertirCarteEnFrancais(c)));

                bool estGagnant = gagnants.Contains(j);
                string prefix = estGagnant ? "🏆 " : "   ";

                sb.AppendLine($"{prefix}{j.Nom}");
                sb.AppendLine($"   Cartes: {cartesStr}");
                sb.AppendLine($"   Main: {mainFr}");
                if (estGagnant)
                {
                    sb.AppendLine($"   Gains: +{gain} €");
                }
                sb.AppendLine();
            }

            sb.AppendLine("═══════════════════════════════");
            sb.AppendLine($"Pot total: {pot} €");

            if (gagnants.Count > 1)
            {
                sb.AppendLine($"Split entre {gagnants.Count} joueurs ({gain} € chacun)");
            }

            MessageBox.Show(
                sb.ToString(),
                "Résultat de la manche",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private string ConvertirMainEnFrancais(string rankEnAnglais)
        {
            if (string.IsNullOrEmpty(rankEnAnglais)) return "Main inconnue";

            string rank = rankEnAnglais.ToLower().Replace(" ", "").Replace("_", "");

            switch (rank)
            {
                case "cartehaute": return "Carte Haute";
                case "paire": return "une Paire";
                case "deuxpaires": return "Deux Paires";
                case "brelan": return "un Brelan";
                case "quinte": return "une Suite";
                case "couleur": return "une Couleur";
                case "fullhouse": return "un Full";
                case "carre": return "un Carré";
                case "quinteflush": return "une Quinte Flush";

                case "royalflush": return "Quinte Flush Royale";
                case "straightflush": return "Quinte Flush";
                case "fourofakind":
                case "quads": return "un Carré";
                case "full": return "un Full";
                case "flush": return "une Couleur";
                case "straight": return "une Suite";
                case "threeofakind":
                case "trips": return "un Brelan";
                case "twopair":
                case "twopairs": return "Deux Paires";
                case "onepair":
                case "pair": return "une Paire";
                case "highcard":
                case "high": return "Carte Haute";

                default: return rankEnAnglais;
            }
        }

        private string ConvertirCarteEnFrancais(string carteAnglais)
        {
            if (string.IsNullOrEmpty(carteAnglais)) return "";

            string[] parts = carteAnglais.Split('_');
            if (parts.Length < 3) return carteAnglais;

            string valeur = parts[0].ToLower();
            string couleur = parts[2].ToLower();

            string valeurFr;
            switch (valeur)
            {
                case "ace": valeurFr = "As"; break;
                case "king": valeurFr = "Roi"; break;
                case "queen": valeurFr = "Dame"; break;
                case "jack": valeurFr = "Valet"; break;
                case "10": valeurFr = "10"; break;
                case "9": valeurFr = "9"; break;
                case "8": valeurFr = "8"; break;
                case "7": valeurFr = "7"; break;
                case "6": valeurFr = "6"; break;
                case "5": valeurFr = "5"; break;
                case "4": valeurFr = "4"; break;
                case "3": valeurFr = "3"; break;
                case "2": valeurFr = "2"; break;
                default: valeurFr = valeur; break;
            }

            string couleurFr;
            switch (couleur)
            {
                case "spades": couleurFr = "♠"; break;
                case "hearts": couleurFr = "♥"; break;
                case "diamonds": couleurFr = "♦"; break;
                case "clubs": couleurFr = "♣"; break;
                default: couleurFr = couleur; break;
            }

            return $"{valeurFr}{couleurFr}";
        }

        private void ActiverBoutonsAction(bool actif)
        {
            boutonMiser.Enabled = actif;
            boutonCheck.Enabled = actif;
            boutonSuivre.Enabled = actif;
            boutonRelancer.Enabled = actif;
            boutonPasser.Enabled = actif;
            miseInput.Enabled = actif;
        }

        private Image ByteArrayToImage(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return null;

            try
            {
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    return Image.FromStream(ms);
                }
            }
            catch
            {
                return null;
            }
        }

        // ==================== GESTIONNAIRES D'ÉVÉNEMENTS ====================

        private void BoutonMiser_Click(object sender, EventArgs e)
        {
            Log($"🎲 BoutonMiser_Click - Joueur actuel: {joueurActuel}, Mon index: {monJoueurIndex}");

            if (modeReseau && joueurActuel != monJoueurIndex)
            {
                Log($"⚠ Tentative de mise hors tour !");
                MessageBox.Show($"Ce n'est pas votre tour !\nC'est le tour de {joueurs[joueurActuel].Nom}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (int.TryParse(miseInput.Text, out int mise) && mise >= miseMinimum)
            {
                Log($"💰 Mise de {mise}€ validée");

                Joueur joueurCourant = joueurs[joueurActuel];
                int ancienneMise = joueurCourant.MiseActuelle;

                FaireUneMise(joueurCourant, mise);
                miseActuelle = Math.Max(miseActuelle, joueurCourant.MiseActuelle);
                miseDuTourActuel = Math.Max(miseDuTourActuel, mise);

                listBoxJoueurs.Items.Clear();
                listBoxJoueurs.Items.Add($"{joueurCourant.Nom} mise {mise} €");
                ActiverBoutonsAction(false);

                Log($"➡ Passage au joueur suivant...");
                PasserAuJoueurSuivant();

                Log($"✓ Nouveau joueur actuel: {joueurActuel}");

                if (modeReseau)
                {
                    Log($"📤 Envoi de l'action réseau...");
                    EnvoyerActionReseau("Miser", mise);
                }

                Log($"✓ Mise terminée");
            }
            else
            {
                MessageBox.Show($"Veuillez entrer un montant valide supérieur ou égal à {miseMinimum} €.");
            }
        }

        private void BoutonCheck_Click(object sender, EventArgs e)
        {
            if (modeReseau && joueurActuel != monJoueurIndex)
            {
                MessageBox.Show($"Ce n'est pas votre tour !\nC'est le tour de {joueurs[joueurActuel].Nom}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Joueur joueur = joueurs[joueurActuel];
            int montantASuivre = miseActuelle - joueur.MiseActuelle;

            if (montantASuivre > 0)
            {
                MessageBox.Show("Vous ne pouvez pas checker : il y a une mise à suivre. Utilisez 'Suivre' ou 'Passer'.");
                return;
            }

            AfficherJoueurs();
            ActiverBoutonsAction(false);

            PasserAuJoueurSuivant();

            if (modeReseau)
                EnvoyerActionReseau("Check", 0);
        }

        private void BoutonSuivre_Click(object sender, EventArgs e)
        {
            if (modeReseau && joueurActuel != monJoueurIndex)
            {
                MessageBox.Show($"Ce n'est pas votre tour !\nC'est le tour de {joueurs[joueurActuel].Nom}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Joueur joueurCourant = joueurs[joueurActuel];
            int montantASuivre = miseActuelle - joueurCourant.MiseActuelle;

            if (montantASuivre <= 0)
            {
                MessageBox.Show("Il n'y a pas de mise à suivre. Utilisez 'Check'.");
                return;
            }

            FaireUneMise(joueurCourant, montantASuivre);
            listBoxJoueurs.Items.Add($"{joueurCourant.Nom} suit ({montantASuivre} €)");
            ActiverBoutonsAction(false);

            PasserAuJoueurSuivant();

            if (modeReseau)
                EnvoyerActionReseau("Suivre", montantASuivre);
        }

        private void BoutonRelancer_Click(object sender, EventArgs e)
        {
            if (modeReseau && joueurActuel != monJoueurIndex)
            {
                MessageBox.Show($"Ce n'est pas votre tour !\nC'est le tour de {joueurs[joueurActuel].Nom}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (int.TryParse(miseInput.Text, out int mise))
            {
                Joueur joueurCourant = joueurs[joueurActuel];
                int montantASuivre = miseActuelle - joueurCourant.MiseActuelle;

                if (mise <= montantASuivre)
                {
                    MessageBox.Show($"Pour relancer, vous devez miser plus que {montantASuivre} €.");
                    return;
                }

                FaireUneMise(joueurCourant, mise);
                miseActuelle = joueurCourant.MiseActuelle;
                miseDuTourActuel = mise;

                listBoxJoueurs.Items.Clear();
                listBoxJoueurs.Items.Add($"{joueurCourant.Nom} relance à {miseActuelle} €");
                ActiverBoutonsAction(false);

                PasserAuJoueurSuivant();

                if (modeReseau)
                    EnvoyerActionReseau("Relancer", mise);
            }
        }

        private void BoutonPasser_Click(object sender, EventArgs e)
        {
            if (modeReseau && joueurActuel != monJoueurIndex)
            {
                MessageBox.Show($"Ce n'est pas votre tour !\nC'est le tour de {joueurs[joueurActuel].Nom}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Joueur joueurCourant = joueurs[joueurActuel];
            joueurCourant.APasse = true;
            listBoxJoueurs.Items.Add($"{joueurCourant.Nom} passe");
            ActiverBoutonsAction(false);

            PasserAuJoueurSuivant();

            if (modeReseau)
                EnvoyerActionReseau("Passer", 0);
        }

        private void BoutonMasquerCartes_Click(object sender, EventArgs e)
        {
            cartesVisibles = !cartesVisibles;
            boutonMasquerCartes.Text = cartesVisibles ? "Masquer Cartes" : "Afficher Cartes";

            if (cartesVisibles)
            {
                AfficherCartesDuJoueur();
            }
            else
            {
                SupprimerCartesJoueur();
                labelStatut.Text = "Cartes masquées - Passez le dispositif au joueur suivant";
            }
        }

        private void BoutonNouvellePartie_Click(object sender, EventArgs e)
        {
            SupprimerCartesAffichees();

            pot = 0;
            miseActuelle = miseMinimum;
            miseDuTourActuel = 0;
            tourActuel = 0;
            joueurActuel = 0;
            cartesCommunes.Clear();

            joueursAyantJoue.Clear();
            aEuUneRelance = false;

            labelPot.Text = "Pot: 0 €";
            labelTour.Text = "Tour: Pré-flop";
            labelStatut.Text = "À votre tour";

            boutonNouvellePartie.Visible = false;
            ActiverBoutonsAction(true);

            InitialiserDeck();
            DistribuerCartes();
            AfficherCartesDuJoueur();
            AfficherJoueurs();
            MisesObligatoires();
        }

        private void CarteJoueur_Click(object sender, EventArgs e)
        {
            if (joueurActuel != 0)
                return;

            if (sender is PictureBox pb && pb.Tag is int indexCarte)
            {
                var joueur = joueurs[joueurActuel];

                if (deck.Count == 0)
                {
                    MessageBox.Show("Le deck est vide !");
                    return;
                }

                string ancienneCarte = joueur.Cartes[indexCarte];
                string nouvelleCarte = deck[0];
                deck.RemoveAt(0);
                deck.Add(ancienneCarte);

                joueur.Cartes[indexCarte] = nouvelleCarte;

                SupprimerCartesJoueur();
                AfficherCartesDuJoueur();

                MessageBox.Show($"Carte remplacée: {ancienneCarte} → {nouvelleCarte}");
            }
        }

        private void listBoxJoueurs_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Ne rien faire
        }

        // ==================== SAUVEGARDE ET CHARGEMENT ====================

        /// <summary>
        /// ✅ NOUVEAU: Gestionnaire du bouton Sauvegarder
        /// </summary>
        private void BoutonSauvegarder_Click(object sender, EventArgs e)
        {
            try
            {
                // Ouvrir une boîte de dialogue pour choisir l'emplacement
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Fichier de sauvegarde Poker (*.json)|*.json",
                    Title = "Sauvegarder la partie",
                    FileName = $"poker_save_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    SauvegarderPartie(saveDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log($"✗ Erreur sauvegarde: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ Sauvegarder la partie dans un fichier
        /// </summary>
        private void SauvegarderPartie(string cheminFichier)
        {
            try
            {
                GameStateSauvegarde state = CreerEtatComplet();

                string json = JsonConvert.SerializeObject(state, Formatting.Indented);
                File.WriteAllText(cheminFichier, json);

                Log($"💾 Partie sauvegardée: {cheminFichier}");

                string modeInfo = modeReseau ? "Réseau (Multijoueur)" : "Local";

                MessageBox.Show(
                    $"Partie sauvegardée avec succès !\n\n" +
                    $"📁 Fichier: {Path.GetFileName(cheminFichier)}\n" +
                    $"📅 Date: {state.DateSauvegarde:dd/MM/yyyy HH:mm}\n" +
                    $"🎮 Mode: {modeInfo}\n" +
                    $"👥 Joueurs: {string.Join(", ", joueurs.Select(j => j.Nom))}\n" +
                    $"💰 Pot: {pot} €\n" +
                    $"🎴 Tour: {GetNomTour(tourActuel)}\n" +
                    $"👤 Au tour de: {joueurs[joueurActuel].Nom}\n\n" +
                    (modeReseau ? "⚠ Pour reprendre en réseau, les DEUX joueurs doivent\ncharger le MÊME fichier de sauvegarde." : ""),
                    "Sauvegarde réussie",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                throw new Exception($"Impossible de sauvegarder la partie: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ NOUVEAU: Charger une partie depuis un fichier
        /// </summary>
        private bool ChargerPartieDepuisFichier(string cheminFichier)
        {
            try
            {
                if (!File.Exists(cheminFichier))
                {
                    MessageBox.Show("Le fichier de sauvegarde n'existe pas.",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                string json = File.ReadAllText(cheminFichier);
                GameStateSauvegarde state = JsonConvert.DeserializeObject<GameStateSauvegarde>(json);

                if (state == null || state.Joueurs == null || state.Joueurs.Count == 0)
                {
                    MessageBox.Show("Le fichier de sauvegarde est corrompu ou invalide.",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // ✅ NOUVEAU: Si c'était une partie réseau, demander quel joueur on contrôle
                if (state.ModeReseau)
                {
                    // Créer un formulaire pour demander le rôle
                    DialogResult roleResult = MessageBox.Show(
                        "Cette partie était en mode réseau.\n\n" +
                        "Êtes-vous le SERVEUR (Joueur 1) ?\n\n" +
                        "• Cliquez OUI si vous êtes le Serveur (Joueur 1)\n" +
                        "• Cliquez NON si vous êtes le Client (Joueur 2)",
                        "Choisir votre rôle",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (roleResult == DialogResult.Cancel)
                    {
                        return false;
                    }

                    // Configurer le mode réseau
                    modeReseau = true;
                    isServeur = (roleResult == DialogResult.Yes);
                    monJoueurIndex = isServeur ? 0 : 1;

                    // Créer la zone de log pour le mode réseau
                    if (logBox == null)
                    {
                        logBox = new TextBox
                        {
                            Location = new Point(20, 680),
                            Size = new Size(760, 100),
                            Multiline = true,
                            ScrollBars = ScrollBars.Vertical,
                            ReadOnly = true,
                            BackColor = Color.Black,
                            ForeColor = Color.LimeGreen,
                            Font = new Font("Consolas", 9)
                        };
                        this.Controls.Add(logBox);
                    }

                    Log($"📂 Partie réseau chargée - Vous êtes {(isServeur ? "SERVEUR (Joueur 1)" : "CLIENT (Joueur 2)")}");
                    Log($"   Vous contrôlez: {state.Joueurs[monJoueurIndex].Nom}");

                    // ✅ IMPORTANT: Demander de reconnecter le réseau
                    DialogResult connectResult = MessageBox.Show(
                        $"Vous avez choisi d'être {(isServeur ? "SERVEUR" : "CLIENT")}.\n\n" +
                        "Voulez-vous établir la connexion réseau maintenant ?\n\n" +
                        "• Le SERVEUR doit démarrer en premier\n" +
                        "• Le CLIENT se connecte ensuite",
                        "Connexion réseau",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (connectResult == DialogResult.Yes)
                    {
                        // Reconnecter le réseau
                        ReconnecterReseau();
                    }
                    else
                    {
                        Log("⚠ Connexion réseau non établie - Mode hors-ligne");
                        MessageBox.Show(
                            "Attention: La connexion réseau n'est pas établie.\n" +
                            "Vous jouez en mode hors-ligne.\n\n" +
                            "Les actions ne seront pas synchronisées avec l'autre joueur.",
                            "Mode hors-ligne",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    // Mode local - pas besoin de configuration réseau
                    modeReseau = false;
                    monJoueurIndex = 0;
                }

                // Restaurer l'état complet
                joueurs = state.Joueurs;
                cartesCommunes = state.CartesCommunes ?? new List<string>();
                pot = state.Pot;
                tourActuel = state.TourActuel;
                joueurActuel = state.JoueurActuel;
                miseActuelle = state.MiseActuelle;

                // Restaurer les données supplémentaires si disponibles
                if (state.Deck != null)
                    deck = state.Deck;
                else
                    InitialiserDeck();

                if (state.MiseMinimum > 0)
                    miseMinimum = state.MiseMinimum;

                miseDuTourActuel = state.MiseDuTourActuel;

                if (state.JoueursAyantJoue != null)
                    joueursAyantJoue = state.JoueursAyantJoue;
                else
                    joueursAyantJoue = new List<int>();

                aEuUneRelance = state.AEuUneRelance;

                // Mettre à jour l'interface
                SupprimerCartesAffichees();
                AfficherCartesCommunes();

                // ✅ Afficher les bonnes cartes selon le mode
                if (modeReseau)
                {
                    AfficherMesCartes();
                }
                else
                {
                    AfficherCartesDuJoueur();
                }

                AfficherJoueurs();

                labelPot.Text = $"Pot: {pot} €";
                labelTour.Text = $"Tour: {GetNomTour(tourActuel)}";

                if (joueurActuel < joueurs.Count)
                {
                    labelJoueurActuel.Text = $"Tour de: {joueurs[joueurActuel].Nom}";
                }

                // ✅ Mettre à jour le statut selon le mode
                if (modeReseau)
                {
                    bool monTour = (joueurActuel == monJoueurIndex);
                    if (monTour)
                    {
                        labelStatut.Text = "Partie chargée - À votre tour !";
                        labelStatut.ForeColor = Color.LimeGreen;
                    }
                    else
                    {
                        labelStatut.Text = $"Partie chargée - Tour de {joueurs[joueurActuel].Nom}";
                        labelStatut.ForeColor = Color.Orange;
                    }
                    VerifierTourJoueur();
                }
                else
                {
                    labelStatut.Text = "Partie chargée - À vous de jouer !";
                    labelStatut.ForeColor = Color.LimeGreen;
                    ActiverBoutonsAction(true);
                }

                boutonNouvellePartie.Visible = false;

                Log($"📂 Partie chargée depuis: {cheminFichier}");
                Log($"   Date de sauvegarde: {state.DateSauvegarde:dd/MM/yyyy HH:mm}");
                Log($"   Tour actuel: {GetNomTour(tourActuel)}, Joueur: {joueurs[joueurActuel].Nom}");

                return true;
            }
            catch (Exception ex)
            {
                Log($"✗ Erreur lors du chargement: {ex.Message}");
                MessageBox.Show($"Erreur lors du chargement de la partie:\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// ✅ NOUVEAU: Reconnecter le réseau après chargement d'une sauvegarde
        /// </summary>
        private async void ReconnecterReseau()
        {
            try
            {
                if (isServeur)
                {
                    Log("🖥 Démarrage du serveur...");

                    serveur = new PokerServer();
                    serveur.OnLog += Log;
                    serveur.OnActionRecue += TraiterActionDistante;

                    bool success = await serveur.StartServerAsync();
                    if (success)
                    {
                        Log("✓ Serveur démarré! En attente du client...");

                        // Attendre la connexion du client
                        int tentatives = 0;
                        while (!serveur.IsConnected && tentatives < 300) // 30 secondes max
                        {
                            await Task.Delay(100);
                            tentatives++;
                        }

                        if (serveur.IsConnected)
                        {
                            Log("✓ Client connecté!");
                            // Envoyer l'état actuel au client
                            await serveur.EnvoyerEtatAsync(CreerEtat());
                            Log("📤 État synchronisé avec le client");
                        }
                        else
                        {
                            Log("⚠ Timeout: Le client ne s'est pas connecté");
                        }
                    }
                    else
                    {
                        Log("✗ Échec du démarrage du serveur");
                    }
                }
                else
                {
                    // Mode Client - demander l'IP du serveur
                    string ip = Prompt.ShowDialog(
                        "Entrez l'adresse IP du serveur:",
                        "Connexion au serveur");

                    if (string.IsNullOrWhiteSpace(ip))
                    {
                        Log("⚠ Connexion annulée");
                        return;
                    }

                    Log($"🔌 Connexion à {ip}...");

                    client = new PokerClient();
                    client.OnLog += Log;
                    client.OnEtatRecu += ChargerEtat;

                    bool success = await client.ConnectAsync(ip);
                    if (success)
                    {
                        Log("✓ Connecté au serveur!");
                        Log("👂 En écoute des messages...");
                    }
                    else
                    {
                        Log("✗ Connexion échouée");
                        MessageBox.Show("Impossible de se connecter au serveur.",
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                VerifierTourJoueur();
            }
            catch (Exception ex)
            {
                Log($"✗ Erreur de reconnexion: {ex.Message}");
                MessageBox.Show($"Erreur de connexion réseau:\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// ✅ NOUVEAU: Obtenir le nom du tour en français
        /// </summary>
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
    }
}
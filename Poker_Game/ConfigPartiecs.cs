using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Poker_Game
{
    public partial class ConfigPartiecs : Form
    {
        private TextBox textBoxNomJoueur1;
        private TextBox textBoxNomJoueur2;
        private TextBox textBoxNomJoueur3;
        private TextBox textBoxNomJoueur4;
        private TextBox textBoxArgentInitial;
        private NumericUpDown numericNombreJoueurs;
        private Button boutonCommencer;
        private Label labelTitre;
        private List<TextBox> textBoxesNoms;

        public ConfigPartiecs()
        {
            InitializeComponent();
            InitialiserInterface();
        }

        private void InitialiserInterface()
        {
            this.Text = "Poker Multijoueur Local";
            this.Size = new Size(500, 600);
            this.BackColor = Color.DarkGreen;
            this.StartPosition = FormStartPosition.CenterScreen;

            textBoxesNoms = new List<TextBox>();

            // Titre
            labelTitre = new Label();
            labelTitre.Text = "POKER MULTIJOUEUR LOCAL";
            labelTitre.Font = new Font("Arial", 18, FontStyle.Bold);
            labelTitre.ForeColor = Color.Gold;
            labelTitre.AutoSize = true;
            labelTitre.Location = new Point(80, 30);
            this.Controls.Add(labelTitre);

            // Nombre de joueurs
            Label labelNombreJoueurs = new Label();
            labelNombreJoueurs.Text = "Nombre de joueurs (2-4):";
            labelNombreJoueurs.ForeColor = Color.White;
            labelNombreJoueurs.Font = new Font("Arial", 12);
            labelNombreJoueurs.Location = new Point(50, 80);
            labelNombreJoueurs.AutoSize = true;
            this.Controls.Add(labelNombreJoueurs);

            numericNombreJoueurs = new NumericUpDown();
            numericNombreJoueurs.Minimum = 2;
            numericNombreJoueurs.Maximum = 4;
            numericNombreJoueurs.Value = 2;
            numericNombreJoueurs.Location = new Point(250, 78);
            numericNombreJoueurs.ValueChanged += NumericNombreJoueurs_ValueChanged;
            this.Controls.Add(numericNombreJoueurs);

            // Argent initial
            Label labelArgent = new Label();
            labelArgent.Text = "Argent initial par joueur:";
            labelArgent.ForeColor = Color.White;
            labelArgent.Font = new Font("Arial", 12);
            labelArgent.Location = new Point(50, 120);
            labelArgent.AutoSize = true;
            this.Controls.Add(labelArgent);

            textBoxArgentInitial = new TextBox();
            textBoxArgentInitial.Text = "1000";
            textBoxArgentInitial.Location = new Point(250, 118);
            textBoxArgentInitial.Width = 100;
            this.Controls.Add(textBoxArgentInitial);

            // Noms des joueurs
            Label labelNoms = new Label();
            labelNoms.Text = "Noms des joueurs:";
            labelNoms.ForeColor = Color.White;
            labelNoms.Font = new Font("Arial", 14, FontStyle.Bold);
            labelNoms.Location = new Point(50, 170);
            labelNoms.AutoSize = true;
            this.Controls.Add(labelNoms);

            // TextBox pour Joueur 1
            Label label1 = new Label();
            label1.Text = "Joueur 1:";
            label1.ForeColor = Color.White;
            label1.Location = new Point(70, 210);
            label1.AutoSize = true;
            this.Controls.Add(label1);

            textBoxNomJoueur1 = new TextBox();
            textBoxNomJoueur1.Text = "Joueur 1";
            textBoxNomJoueur1.Location = new Point(150, 208);
            textBoxNomJoueur1.Width = 200;
            this.Controls.Add(textBoxNomJoueur1);
            textBoxesNoms.Add(textBoxNomJoueur1);

            // TextBox pour Joueur 2
            Label label2 = new Label();
            label2.Text = "Joueur 2:";
            label2.ForeColor = Color.White;
            label2.Location = new Point(70, 250);
            label2.AutoSize = true;
            this.Controls.Add(label2);

            textBoxNomJoueur2 = new TextBox();
            textBoxNomJoueur2.Text = "Joueur 2";
            textBoxNomJoueur2.Location = new Point(150, 248);
            textBoxNomJoueur2.Width = 200;
            this.Controls.Add(textBoxNomJoueur2);
            textBoxesNoms.Add(textBoxNomJoueur2);

            // TextBox pour Joueur 3
            Label label3 = new Label();
            label3.Text = "Joueur 3:";
            label3.ForeColor = Color.White;
            label3.Location = new Point(70, 290);
            label3.AutoSize = true;
            this.Controls.Add(label3);

            textBoxNomJoueur3 = new TextBox();
            textBoxNomJoueur3.Text = "Joueur 3";
            textBoxNomJoueur3.Location = new Point(150, 288);
            textBoxNomJoueur3.Width = 200;
            textBoxNomJoueur3.Visible = false;
            label3.Visible = false;
            this.Controls.Add(textBoxNomJoueur3);
            this.Controls.Add(label3);
            textBoxesNoms.Add(textBoxNomJoueur3);

            // TextBox pour Joueur 4
            Label label4 = new Label();
            label4.Text = "Joueur 4:";
            label4.ForeColor = Color.White;
            label4.Location = new Point(70, 330);
            label4.AutoSize = true;
            this.Controls.Add(label4);

            textBoxNomJoueur4 = new TextBox();
            textBoxNomJoueur4.Text = "Joueur 4";
            textBoxNomJoueur4.Location = new Point(150, 328);
            textBoxNomJoueur4.Width = 200;
            textBoxNomJoueur4.Visible = false;
            label4.Visible = false;
            this.Controls.Add(textBoxNomJoueur4);
            this.Controls.Add(label4);
            textBoxesNoms.Add(textBoxNomJoueur4);

            // Bouton Commencer
            boutonCommencer = new Button();
            boutonCommencer.Text = "COMMENCER LA PARTIE";
            boutonCommencer.Font = new Font("Arial", 14, FontStyle.Bold);
            boutonCommencer.BackColor = Color.Gold;
            boutonCommencer.ForeColor = Color.Black;
            boutonCommencer.Size = new Size(250, 50);
            boutonCommencer.Location = new Point(125, 450);
            boutonCommencer.Click += BoutonCommencer_Click;
            this.Controls.Add(boutonCommencer);

            // Instructions
            Label labelInstructions = new Label();
            labelInstructions.Text = "Les joueurs joueront chacun leur tour sur le même écran.\n" +
                                     "Utilisez le bouton 'Masquer Cartes' pour passer\n" +
                                     "l'ordinateur d'un joueur à l'autre.";
            labelInstructions.ForeColor = Color.LightYellow;
            labelInstructions.Font = new Font("Arial", 10);
            labelInstructions.Location = new Point(50, 380);
            labelInstructions.Size = new Size(400, 60);
            this.Controls.Add(labelInstructions);
        }

        private void NumericNombreJoueurs_ValueChanged(object sender, EventArgs e)
        {
            int nombreJoueurs = (int)numericNombreJoueurs.Value;

            // Afficher/masquer les champs selon le nombre de joueurs
            for (int i = 0; i < 4; i++)
            {
                bool visible = i < nombreJoueurs;
                textBoxesNoms[i].Visible = visible;
                this.Controls[this.Controls.IndexOf(textBoxesNoms[i]) - 1].Visible = visible; // Label correspondant
            }
        }

        private void BoutonCommencer_Click(object sender, EventArgs e)
        {
            // Validation
            if (!int.TryParse(textBoxArgentInitial.Text, out int argentInitial) || argentInitial < 100)
            {
                MessageBox.Show("Veuillez entrer un montant d'argent initial valide (minimum 100€).");
                return;
            }

            // Récupérer les noms des joueurs
            List<string> nomsJoueurs = new List<string>();
            int nombreJoueurs = (int)numericNombreJoueurs.Value;

            for (int i = 0; i < nombreJoueurs; i++)
            {
                string nom = textBoxesNoms[i].Text.Trim();
                if (string.IsNullOrEmpty(nom))
                {
                    nom = $"Joueur {i + 1}";
                }
                nomsJoueurs.Add(nom);
            }

            // Vérifier qu'il n'y a pas de doublons
            if (nomsJoueurs.Distinct().Count() != nomsJoueurs.Count)
            {
                MessageBox.Show("Tous les joueurs doivent avoir des noms différents.");
                return;
            }

            // Démarrer le jeu
            this.Hide();
            JeuPoker jeuPoker = new JeuPoker(nomsJoueurs, argentInitial);
            jeuPoker.FormClosed += (s, args) => this.Show();
            jeuPoker.Show();
        }
    }
}

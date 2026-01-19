using System;
using System.Drawing;
using System.Windows.Forms;

namespace Poker_Game
{
    public partial class NetworkConfigForm : Form
    {
        public bool IsServer { get; private set; }
        public string ServerIP { get; private set; }

        private RadioButton radioServeur;
        private RadioButton radioClient;
        private TextBox textBoxIP;
        private Label labelIP;
        private Button btnOK;
        private Button btnCancel;
        private Label labelInfo;

        public NetworkConfigForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Configuration Réseau - Poker";
            this.Size = new Size(450, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Label d'information
            labelInfo = new Label
            {
                Text = "Choisissez le mode de jeu réseau:",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            // Radio button Serveur
            radioServeur = new RadioButton
            {
                Text = "Héberger une partie (Serveur)",
                Location = new Point(30, 60),
                AutoSize = true,
                Checked = true
            };
            radioServeur.CheckedChanged += RadioServeur_CheckedChanged;

            // Radio button Client
            radioClient = new RadioButton
            {
                Text = "Rejoindre une partie (Client)",
                Location = new Point(30, 90),
                AutoSize = true
            };
            radioClient.CheckedChanged += RadioClient_CheckedChanged;

            // Label IP
            labelIP = new Label
            {
                Text = "Adresse IP du serveur:",
                Location = new Point(30, 130),
                AutoSize = true,
                Enabled = false
            };

            // TextBox IP
            textBoxIP = new TextBox
            {
                Location = new Point(30, 155),
                Width = 200,
                Text = "192.168.1.100",
                Enabled = false
            };

            // Bouton OK
            btnOK = new Button
            {
                Text = "OK",
                Location = new Point(250, 155),
                Width = 80,
                BackColor = Color.Green,
                ForeColor = Color.White
            };
            btnOK.Click += BtnOK_Click;

            // Bouton Annuler
            btnCancel = new Button
            {
                Text = "Annuler",
                Location = new Point(340, 155),
                Width = 80,
                BackColor = Color.Red,
                ForeColor = Color.White
            };
            btnCancel.Click += BtnCancel_Click;

            // Ajouter les contrôles
            this.Controls.Add(labelInfo);
            this.Controls.Add(radioServeur);
            this.Controls.Add(radioClient);
            this.Controls.Add(labelIP);
            this.Controls.Add(textBoxIP);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void RadioServeur_CheckedChanged(object sender, EventArgs e)
        {
            if (radioServeur.Checked)
            {
                labelIP.Enabled = false;
                textBoxIP.Enabled = false;
            }
        }

        private void RadioClient_CheckedChanged(object sender, EventArgs e)
        {
            if (radioClient.Checked)
            {
                labelIP.Enabled = true;
                textBoxIP.Enabled = true;
                textBoxIP.Focus();
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            IsServer = radioServeur.Checked;

            if (!IsServer)
            {
                // Valider l'adresse IP
                if (string.IsNullOrWhiteSpace(textBoxIP.Text))
                {
                    MessageBox.Show("Veuillez entrer une adresse IP valide.",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                ServerIP = textBoxIP.Text.Trim();
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

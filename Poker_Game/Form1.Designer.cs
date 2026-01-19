namespace Poker_Game
{
    partial class MenuPrincipal
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTitre = new System.Windows.Forms.Label();
            this.btnQuitter = new System.Windows.Forms.Button();
            this.btnCredits = new System.Windows.Forms.Button();
            this.btnHTP = new System.Windows.Forms.Button();
            this.btnStartGame = new System.Windows.Forms.Button();
            this.btnCreerReseau = new System.Windows.Forms.Button();
            this.btnRejoindre = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitre
            // 
            this.lblTitre.BackColor = System.Drawing.Color.Transparent;
            this.lblTitre.Font = new System.Drawing.Font("Agency FB", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitre.ForeColor = System.Drawing.Color.Red;
            this.lblTitre.Location = new System.Drawing.Point(238, 48);
            this.lblTitre.Name = "lblTitre";
            this.lblTitre.Size = new System.Drawing.Size(308, 75);
            this.lblTitre.TabIndex = 0;
            this.lblTitre.Text = "Poker Game";
            this.lblTitre.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnQuitter
            // 
            this.btnQuitter.BackColor = System.Drawing.Color.Transparent;
            this.btnQuitter.Font = new System.Drawing.Font("Agency FB", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuitter.ForeColor = System.Drawing.Color.Red;
            this.btnQuitter.Location = new System.Drawing.Point(297, 382);
            this.btnQuitter.Name = "btnQuitter";
            this.btnQuitter.Size = new System.Drawing.Size(174, 43);
            this.btnQuitter.TabIndex = 1;
            this.btnQuitter.Text = "Quitter";
            this.btnQuitter.UseVisualStyleBackColor = false;
            this.btnQuitter.Click += new System.EventHandler(this.btnQuitter_Click);
            // 
            // btnCredits
            // 
            this.btnCredits.BackColor = System.Drawing.Color.Transparent;
            this.btnCredits.Font = new System.Drawing.Font("Agency FB", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCredits.ForeColor = System.Drawing.Color.Red;
            this.btnCredits.Location = new System.Drawing.Point(297, 318);
            this.btnCredits.Name = "btnCredits";
            this.btnCredits.Size = new System.Drawing.Size(174, 43);
            this.btnCredits.TabIndex = 2;
            this.btnCredits.Text = "Credits";
            this.btnCredits.UseVisualStyleBackColor = false;
            this.btnCredits.Click += new System.EventHandler(this.btnCredits_Click);
            // 
            // btnHTP
            // 
            this.btnHTP.Font = new System.Drawing.Font("Agency FB", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHTP.ForeColor = System.Drawing.Color.Red;
            this.btnHTP.Location = new System.Drawing.Point(297, 253);
            this.btnHTP.Name = "btnHTP";
            this.btnHTP.Size = new System.Drawing.Size(174, 43);
            this.btnHTP.TabIndex = 3;
            this.btnHTP.Text = "Régles du jeu ";
            this.btnHTP.UseVisualStyleBackColor = true;
            this.btnHTP.Click += new System.EventHandler(this.btnHTP_Click);
            // 
            // btnStartGame
            // 
            this.btnStartGame.Font = new System.Drawing.Font("Agency FB", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartGame.ForeColor = System.Drawing.Color.Red;
            this.btnStartGame.Location = new System.Drawing.Point(297, 191);
            this.btnStartGame.Name = "btnStartGame";
            this.btnStartGame.Size = new System.Drawing.Size(174, 42);
            this.btnStartGame.TabIndex = 4;
            this.btnStartGame.Text = "Start Game";
            this.btnStartGame.UseVisualStyleBackColor = true;
            this.btnStartGame.Click += new System.EventHandler(this.btnStartGame_Click);
            // 
            // btnCreerReseau
            // 
            this.btnCreerReseau.Font = new System.Drawing.Font("Agency FB", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCreerReseau.ForeColor = System.Drawing.Color.Red;
            this.btnCreerReseau.Location = new System.Drawing.Point(36, 191);
            this.btnCreerReseau.Name = "btnCreerReseau";
            this.btnCreerReseau.Size = new System.Drawing.Size(217, 42);
            this.btnCreerReseau.TabIndex = 5;
            this.btnCreerReseau.Text = "Créer Partie Réseau";
            this.btnCreerReseau.UseVisualStyleBackColor = true;
            this.btnCreerReseau.Click += new System.EventHandler(this.btnCreerReseau_Click);
            // 
            // btnRejoindre
            // 
            this.btnRejoindre.Font = new System.Drawing.Font("Agency FB", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRejoindre.ForeColor = System.Drawing.Color.Red;
            this.btnRejoindre.Location = new System.Drawing.Point(36, 254);
            this.btnRejoindre.Name = "btnRejoindre";
            this.btnRejoindre.Size = new System.Drawing.Size(217, 42);
            this.btnRejoindre.TabIndex = 6;
            this.btnRejoindre.Text = "Rejoindre Partie Réseau";
            this.btnRejoindre.UseVisualStyleBackColor = true;
            this.btnRejoindre.Click += new System.EventHandler(this.btnRejoindre_Click);
            // 
            // MenuPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackgroundImage = global::Poker_Game.Properties.Resources._360_F_290521309_UpvOs8Dwp67xAppU7ebUsITQYJAtj0Z5;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnRejoindre);
            this.Controls.Add(this.btnCreerReseau);
            this.Controls.Add(this.btnStartGame);
            this.Controls.Add(this.btnHTP);
            this.Controls.Add(this.btnCredits);
            this.Controls.Add(this.btnQuitter);
            this.Controls.Add(this.lblTitre);
            this.Name = "MenuPrincipal";
            this.Text = "Menu Principal";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblTitre;
        private System.Windows.Forms.Button btnQuitter;
        private System.Windows.Forms.Button btnCredits;
        private System.Windows.Forms.Button btnHTP;
        private System.Windows.Forms.Button btnStartGame;
        private System.Windows.Forms.Button btnCreerReseau;
        private System.Windows.Forms.Button btnRejoindre;
    }
}


namespace AnalyseRDM6
{
    partial class FenetrePrincipale
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
            this.TxtCheminFichier = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BtValider = new System.Windows.Forms.Button();
            this.BtRechercherFichier = new System.Windows.Forms.Button();
            this.Resultat = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // TxtCheminFichier
            // 
            this.TxtCheminFichier.Location = new System.Drawing.Point(143, 28);
            this.TxtCheminFichier.Name = "TxtCheminFichier";
            this.TxtCheminFichier.ReadOnly = true;
            this.TxtCheminFichier.Size = new System.Drawing.Size(369, 20);
            this.TxtCheminFichier.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(141, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Selectionnez le fichier *.por :";
            // 
            // BtValider
            // 
            this.BtValider.Location = new System.Drawing.Point(15, 64);
            this.BtValider.Name = "BtValider";
            this.BtValider.Size = new System.Drawing.Size(75, 23);
            this.BtValider.TabIndex = 2;
            this.BtValider.Text = "Ok";
            this.BtValider.UseVisualStyleBackColor = true;
            this.BtValider.Click += new System.EventHandler(this.BtValider_Click);
            // 
            // BtRechercherFichier
            // 
            this.BtRechercherFichier.Location = new System.Drawing.Point(13, 26);
            this.BtRechercherFichier.Name = "BtRechercherFichier";
            this.BtRechercherFichier.Size = new System.Drawing.Size(124, 23);
            this.BtRechercherFichier.TabIndex = 3;
            this.BtRechercherFichier.Text = "Rechercher un fichier";
            this.BtRechercherFichier.UseVisualStyleBackColor = true;
            this.BtRechercherFichier.Click += new System.EventHandler(this.BtRechercherFichier_Click);
            // 
            // Resultat
            // 
            this.Resultat.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Resultat.Location = new System.Drawing.Point(96, 69);
            this.Resultat.Name = "Resultat";
            this.Resultat.ReadOnly = true;
            this.Resultat.Size = new System.Drawing.Size(286, 13);
            this.Resultat.TabIndex = 4;
            // 
            // FenetrePrincipale
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 101);
            this.Controls.Add(this.Resultat);
            this.Controls.Add(this.BtRechercherFichier);
            this.Controls.Add(this.BtValider);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TxtCheminFichier);
            this.Name = "FenetrePrincipale";
            this.Text = "Analyser un fichier RDM6 Ossature";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TxtCheminFichier;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BtValider;
        private System.Windows.Forms.Button BtRechercherFichier;
        private System.Windows.Forms.TextBox Resultat;
    }
}


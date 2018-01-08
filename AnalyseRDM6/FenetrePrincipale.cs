using System;
using System.Windows.Forms;

namespace AnalyseRDM6
{
    public partial class FenetrePrincipale : Form
    {
        private AnalyseRM6 AnalyseRDM6;

        public FenetrePrincipale()
        {
            InitializeComponent();

            AnalyseRDM6 = new AnalyseRM6();
        }

        private void BtRechercherFichier_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.CheckFileExists = true;
            OFD.CheckPathExists = true;
            OFD.Multiselect = false;
            OFD.RestoreDirectory = true;
            OFD.InitialDirectory = Environment.CurrentDirectory;
            OFD.Filter = "Fichier RDM6 ossature|*.por";
            OFD.Title = "Selectionnez un fichier .por";

            // Show the Dialog.
            // If the user clicked OK in the dialog and
            // a .por file was selected
            if (OFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TxtCheminFichier.Text = OFD.FileName;
                AnalyseRDM6.CheminFichierPOR = OFD.FileName;
            }
        }

        private void BtValider_Click(object sender, EventArgs e)
        {
            AnalyseRDM6.Data.Vider();
            AnalyseRDM6.Executer();
            Resultat.Text = "Terminé";
        }
    }
}

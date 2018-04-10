using System;
using System.Collections.Generic;
using System.IO;
using NPOI;

namespace AnalyseRDM6
{
    public class AnalyseRM6
    {
        public String CheminFichierPOR { get; set; }
        public DATA Data = new DATA();
        public int Precision = 0;

        public void Executer()
        {
            var dossier = Path.GetDirectoryName(CheminFichierPOR);

            var FichierPOR = new FichierPOR(CheminFichierPOR, Data);
            FichierPOR.Analyser();

            var CheminFichierRES = Path.Combine(dossier, Path.GetFileNameWithoutExtension(CheminFichierPOR) + ".res");
            var FichierRES = new FichierRES(CheminFichierRES, Data);
            FichierRES.Analyser();

            foreach (var c in Data.ListeCasDeCharge.Values)
            {
                var chemin = Path.Combine(dossier, c.Fichier);
                var FichierOX = new FichierOX(c, chemin, Data);
                FichierOX.Analyser();
            }

            foreach (var c in Data.ListeCombinaisons.Values)
            {
                var chemin = Path.Combine(dossier, c.Fichier);
                var FichierOX = new FichierOX(c, chemin, Data);
                FichierOX.Analyser();
            }

            ExportBarreMax(dossier);
            ExportBarre(dossier);
            ExportDeplacement(dossier);
            ExportReaction(dossier);
        }

        private void ExportBarreMax(String dossier)
        {
            // Creer le fichier CSV
            using (StreamWriter sw = File.CreateText(Path.Combine(dossier, "AnalyseBarreMax.csv")))
            {
                sw.WriteLine("No;Origine;Extremite;Section;Lg;NcMax;NcMax comb;NtMax;NtMax comb;TyMax;TyMax comb;TzMax;TzMax comb;MFyMax;MFyMax comb;MFzMax;MFzMax comb;MtMax;MtMax comb");

                Func<double, string, string> format = delegate (double val, string comb)
                {
                    val = Math.Round(val, Precision);
                    if (val != 0)
                        return val.ToString() + ";" + comb;

                    return ";";
                };

                foreach (var Poutre in Data.ListePoutres.Values)
                {
                    String NcCombinaisonMax = "";
                    double NcMax = 0;
                    String NtCombinaisonMax = "";
                    double NtMax = 0;
                    String TyCombinaisonMax = "";
                    double TyMax = 0;
                    String TzCombinaisonMax = "";
                    double TzMax = 0;
                    String MFyCombinaisonMax = "";
                    double MFyMax = 0;
                    String MFzCombinaisonMax = "";
                    double MFzMax = 0;
                    String MtCombinaisonMax = "";
                    double MtMax = 0;


                    foreach (var Comb in Data.ListeCombinaisons.Values)
                    {
                        var Efforts = Comb.ListeEffortsPoutre[Poutre.No];

                        if (Efforts.EffortsMax.Nc < NcMax)
                        {
                            NcMax = Efforts.EffortsMax.Nc;
                            NcCombinaisonMax = Comb.Nom;
                        }

                        if (Efforts.EffortsMax.Nt > NtMax)
                        {
                            NtMax = Efforts.EffortsMax.Nt;
                            NtCombinaisonMax = Comb.Nom;
                        }

                        if (Efforts.EffortsMax.TYmax > TyMax)
                        {
                            TyMax = Efforts.EffortsMax.TYmax;
                            TyCombinaisonMax = Comb.Nom;
                        }

                        if (Efforts.EffortsMax.TZmax > TzMax)
                        {
                            TzMax = Efforts.EffortsMax.TZmax;
                            TzCombinaisonMax = Comb.Nom;
                        }

                        if (Efforts.EffortsMax.MFYmax > MFyMax)
                        {
                            MFyMax = Efforts.EffortsMax.MFYmax;
                            MFyCombinaisonMax = Comb.Nom;
                        }

                        if (Efforts.EffortsMax.MFZmax > MFzMax)
                        {
                            MFzMax = Efforts.EffortsMax.MFZmax;
                            MFzCombinaisonMax = Comb.Nom;
                        }

                        if (Efforts.EffortsMax.MTmax > MtMax)
                        {
                            MtMax = Efforts.EffortsMax.MTmax;
                            MtCombinaisonMax = Comb.Nom;
                        }

                    }

                    var L = new List<String>();
                    L.Add(Poutre.No.ToString());
                    L.Add(Poutre.Origine.ToString());
                    L.Add(Poutre.Extremite.ToString());
                    L.Add(Data.ListeSections[Poutre.Section].Nom);
                    L.Add(Math.Round(Poutre.Longueur, 3).ToString());


                    // Compression pure
                    L.Add(format(NcMax, NcCombinaisonMax));

                    // Traction pure
                    L.Add(format(NtMax, NtCombinaisonMax));

                    // Tranchant Y
                    L.Add(format(TyMax, TyCombinaisonMax));

                    // Tranchant Z
                    L.Add(format(TzMax, TzCombinaisonMax));

                    // Moment flechissant Y
                    L.Add(format(MFyMax, MFyCombinaisonMax));

                    // Moment flechissant Z
                    L.Add(format(MFzMax, MFzCombinaisonMax));

                    // Moment de torsion
                    L.Add(format(MtMax, MtCombinaisonMax));

                    sw.WriteLine(String.Join(";", L));
                }
            }
        }

        private void ExportBarre(String dossier)
        {
            // Creer le fichier CSV
            using (StreamWriter sw = File.CreateText(Path.Combine(dossier, "AnalyseBarre.csv")))
            {
                sw.WriteLine("No;Section;Lg;Noeud O;oN;oTy;oTz;oMFt;oMFz;oMt;Noeud E;eN;eTy;eTz;eMFt;eMFz;eMt;Comb");

                foreach (var Comb in Data.ListeCasDeCharge.Values)
                {
                    foreach (var Poutre in Comb.ListeEffortsPoutre.Values)
                    {
                        var L = new List<String>();
                        L.Add(Poutre.Poutre.ToString());

                        var p = Data.ListePoutres[Poutre.Poutre];
                        L.Add(Data.ListeSections[p.Section].Nom);
                        L.Add(Math.Round(p.Longueur, 3).ToString());

                        L.Add(ValToString(Poutre.EffortsNorigine.Noeud));
                        L.Add(ValToString(Poutre.EffortsNorigine.N));
                        L.Add(ValToString(Poutre.EffortsNorigine.TY));
                        L.Add(ValToString(Poutre.EffortsNorigine.TZ));
                        L.Add(ValToString(Poutre.EffortsNorigine.MFY));
                        L.Add(ValToString(Poutre.EffortsNorigine.MFZ));
                        L.Add(ValToString(Poutre.EffortsNorigine.MT));

                        L.Add(ValToString(Poutre.EffortsNextremite.Noeud));
                        L.Add(ValToString(Poutre.EffortsNextremite.N));
                        L.Add(ValToString(Poutre.EffortsNextremite.TY));
                        L.Add(ValToString(Poutre.EffortsNextremite.TZ));
                        L.Add(ValToString(Poutre.EffortsNextremite.MFY));
                        L.Add(ValToString(Poutre.EffortsNextremite.MFZ));
                        L.Add(ValToString(Poutre.EffortsNextremite.MT));

                        L.Add(Comb.Nom);
                        sw.WriteLine(String.Join(";", L));
                    }
                }

                foreach (var Comb in Data.ListeCombinaisons.Values)
                {
                    foreach (var Poutre in Comb.ListeEffortsPoutre.Values)
                    {
                        var L = new List<String>();
                        L.Add(Poutre.Poutre.ToString());

                        var p = Data.ListePoutres[Poutre.Poutre];
                        L.Add(Data.ListeSections[p.Section].Nom);
                        L.Add(Math.Round(p.Longueur, 3).ToString());

                        L.Add(ValToString(Poutre.EffortsNorigine.Noeud));
                        L.Add(ValToString(Poutre.EffortsNorigine.N));
                        L.Add(ValToString(Poutre.EffortsNorigine.TY));
                        L.Add(ValToString(Poutre.EffortsNorigine.TZ));
                        L.Add(ValToString(Poutre.EffortsNorigine.MFY));
                        L.Add(ValToString(Poutre.EffortsNorigine.MFZ));
                        L.Add(ValToString(Poutre.EffortsNorigine.MT));

                        L.Add(ValToString(Poutre.EffortsNextremite.Noeud));
                        L.Add(ValToString(Poutre.EffortsNextremite.N));
                        L.Add(ValToString(Poutre.EffortsNextremite.TY));
                        L.Add(ValToString(Poutre.EffortsNextremite.TZ));
                        L.Add(ValToString(Poutre.EffortsNextremite.MFY));
                        L.Add(ValToString(Poutre.EffortsNextremite.MFZ));
                        L.Add(ValToString(Poutre.EffortsNextremite.MT));

                        L.Add(Comb.Nom);
                        sw.WriteLine(String.Join(";", L));
                    }
                }
            }
        }

        private void ExportDeplacement(String dossier)
        {
            // Creer le fichier CSV
            using (StreamWriter sw = File.CreateText(Path.Combine(dossier, "AnalyseDeplacement.csv")))
            {
                sw.WriteLine("No;Dx;Dy;Dz;Rx;Ry;Rz;Comb");

                foreach (var Comb in Data.ListeCasDeCharge.Values)
                {
                    foreach (var Noeud in Comb.ListeDeplacementNoeud.Values)
                    {
                        var L = new List<String>();
                        L.Add(Noeud.Noeud.ToString());
                        L.Add(ValToString(Noeud.DX, 4));
                        L.Add(ValToString(Noeud.DY, 4));
                        L.Add(ValToString(Noeud.DZ, 4));
                        L.Add(ValToString(Noeud.RX, 4));
                        L.Add(ValToString(Noeud.RY, 4));
                        L.Add(ValToString(Noeud.RZ, 4));
                        L.Add(Comb.Nom);
                        sw.WriteLine(String.Join(";", L));
                    }
                }

                foreach (var Comb in Data.ListeCombinaisons.Values)
                {
                    foreach (var Noeud in Comb.ListeDeplacementNoeud.Values)
                    {
                        var L = new List<String>();
                        L.Add(Noeud.Noeud.ToString());
                        L.Add(ValToString(Noeud.DX, 4));
                        L.Add(ValToString(Noeud.DY, 4));
                        L.Add(ValToString(Noeud.DZ, 4));
                        L.Add(ValToString(Noeud.RX, 4));
                        L.Add(ValToString(Noeud.RY, 4));
                        L.Add(ValToString(Noeud.RZ, 4));
                        L.Add(Comb.Nom);
                        sw.WriteLine(String.Join(";", L));
                    }
                }
            }
        }

        private void ExportReaction(String dossier)
        {
            // Creer le fichier CSV
            using (StreamWriter sw = File.CreateText(Path.Combine(dossier, "AnalyseReaction.csv")))
            {
                sw.WriteLine("No;Fx;Fy;Fz;Mx;My;Mz;Comb");

                foreach (var Comb in Data.ListeCasDeCharge.Values)
                {
                    foreach (var Noeud in Comb.ListeReactionNoeud.Values)
                    {
                        var L = new List<String>();
                        L.Add(Noeud.Noeud.ToString());
                        L.Add(ValToString(Noeud.FX));
                        L.Add(ValToString(Noeud.FY));
                        L.Add(ValToString(Noeud.FZ));
                        L.Add(ValToString(Noeud.MX));
                        L.Add(ValToString(Noeud.MY));
                        L.Add(ValToString(Noeud.MZ));
                        L.Add(Comb.Nom);
                        sw.WriteLine(String.Join(";", L));
                    }
                }

                foreach (var Comb in Data.ListeCombinaisons.Values)
                {
                    foreach (var Noeud in Comb.ListeReactionNoeud.Values)
                    {
                        var L = new List<String>();
                        L.Add(Noeud.Noeud.ToString());
                        L.Add(ValToString(Noeud.FX));
                        L.Add(ValToString(Noeud.FY));
                        L.Add(ValToString(Noeud.FZ));
                        L.Add(ValToString(Noeud.MX));
                        L.Add(ValToString(Noeud.MY));
                        L.Add(ValToString(Noeud.MZ));
                        L.Add(Comb.Nom);
                        sw.WriteLine(String.Join(";", L));
                    }
                }
            }
        }

        private String ValToString(double val)
        {
            val = Math.Round(val, Precision);
            if (val != 0)
                return val.ToString();

            return "";
        }

        private String ValToString(double val, int precision)
        {
            val = Math.Round(val, precision);
            if (val != 0)
                return val.ToString();

            return "";
        }

        private class FichierPOR
        {
            private String _CheminFichier;

            private Dictionary<String, Action<StreamReader>> _Fonctions;

            private DATA _Data;

            public FichierPOR(String CheminFichier, DATA data)
            {
                _CheminFichier = CheminFichier;
                _Data = data;

                _Fonctions = new Dictionary<string, Action<StreamReader>>();
                //fonctions.Add("debut du fichier", null);
                //fonctions.Add("version", null);
                //fonctions.Add("SI", null);
                //fonctions.Add("nom du fichier", null);
                //fonctions.Add("date", null);
                //fonctions.Add("heure", null);
                //fonctions.Add("ossature", null);
                _Fonctions.Add("noeuds", PNoeud);
                _Fonctions.Add("poutres", PPoutre);
                _Fonctions.Add("sections", PSection);
                _Fonctions.Add("materiaux", PMateriau);
                //fonctions.Add("liaisons", null);
                //fonctions.Add("gpesanteur", null);
                _Fonctions.Add("cas de charges", PCasDeCharge);
                _Fonctions.Add("combinaisons", PCombinaison);
                //fonctions.Add("modes propres", null);
                //fonctions.Add("maillage", null);
                //fonctions.Add("fin du fichier", null);
            }

            public void Analyser()
            {
                var Lecteur = new Lecteur();
                Lecteur.CheminFichier = _CheminFichier;
                Lecteur.Separateur = '$';
                Lecteur.Fonctions = _Fonctions;
                Lecteur.Analyser();
                Lecteur = null;
            }

            private void PNoeud(StreamReader sr)
            {
                var l = sr.ReadLine();
                var t = l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (t.Length == 4)
                {
                    var n = new DATA.Noeud();
                    n.No = Int32.Parse(t[0]);
                    n.X = Double.Parse(t[1]);
                    n.Y = Double.Parse(t[2]);
                    n.Z = Double.Parse(t[3]);
                    _Data.ListeNoeuds.Add(n.No, n);
                }
            }

            private double LgPoutre(int A, int B)
            {
                var NA = _Data.ListeNoeuds[A];
                var NB = _Data.ListeNoeuds[B];
                return Math.Sqrt(Math.Pow(NB.X - NA.X, 2) + Math.Pow(NB.Y - NA.Y, 2) + Math.Pow(NB.Z - NA.Z, 2));
            }

            private void PPoutre(StreamReader sr)
            {
                var l = sr.ReadLine();
                var t = l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (t.Length == 9)
                {
                    var p = new DATA.Poutre();
                    p.No = Int32.Parse(t[0]);
                    p.Relaxation = t[1];
                    p.Origine = Int32.Parse(t[2]);
                    p.Extremite = Int32.Parse(t[3]);
                    p.Section = Int32.Parse(t[7]);
                    p.Materiau = Int32.Parse(t[8]);
                    p.Longueur = LgPoutre(p.Origine, p.Extremite);
                    _Data.ListePoutres.Add(p.No, p);
                }
            }

            private void PSection(StreamReader sr)
            {
                try
                {
                    /*
                    Structure du fichier :
                    13
                    TYPE BIBLIOTHEQUE
                    NOM *UPE
                    DESIGNATION *200
                     */
                    var no = Int32.Parse(sr.ReadLine().Trim());
                    var s = new DATA.Section();
                    s.No = no;
                    // On echappe le type de la section
                    sr.ReadLine();

                    // On lit le nom
                    var r = sr.ReadLine().Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                    if (r.Length == 2)
                        s.Nom = r[1].Trim();

                    // On ajoute la description au nom
                    r = sr.ReadLine().Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                    if (r.Length == 2)
                        s.Nom = s.Nom + " " + r[1].Trim();

                    _Data.ListeSections.Add(s.No, s);
                }
                catch { }
            }

            private void PMateriau(StreamReader sr)
            {
                try
                {
                    var no = Int32.Parse(sr.ReadLine().Trim());
                    var m = new DATA.Materiau();
                    m.No = no;


                    var r = sr.ReadLine();
                    if (r.StartsWith("NOM "))
                    {
                        r = r.Replace("NOM", "").Trim();
                        m.Nom = r;
                    }

                    _Data.ListeMateriaux.Add(m.No, m);
                }
                catch { }
            }

            private void PCasDeCharge(StreamReader sr)
            {
                try
                {
                    var no = Int32.Parse(sr.ReadLine().Trim());
                    var c = new DATA.CasDeCharge();
                    c.No = no;


                    var r = sr.ReadLine();
                    if (r.StartsWith("nom "))
                    {
                        r = r.Replace("nom", "").Trim();
                        c.Nom = r;
                    }

                    _Data.ListeCasDeCharge.Add(c.No, c);
                }
                catch { }
            }

            private void PCombinaison(StreamReader sr)
            {
                var l = sr.ReadLine();
                var t = l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (t.Length > 1)
                {
                    var c = new DATA.Combinaison();
                    c.No = _Data.ListeCombinaisons.Count + 1;
                    var n = (Int32.Parse(t[0]) * 2) + 1;

                    var Nom = new List<String>();
                    for (int i = 1; i < n; i += 2)
                    {
                        var T = new DATA.Terme(Int32.Parse(t[i]), Double.Parse(t[i + 1]));
                        var Cas = _Data.ListeCasDeCharge[T.Cas];
                        Nom.Add(T.Facteur + " x " + Cas.Nom);
                        c.Liste.Add(T);
                    }
                    c.Nom = String.Join(" + ", Nom);


                    _Data.ListeCombinaisons.Add(c.No, c);

                }
            }
        }

        private class FichierRES
        {
            private String _CheminFichier;

            private Dictionary<String, Action<StreamReader>> _Fonctions;

            private DATA _Data;

            public FichierRES(String CheminFichier, DATA data)
            {
                _CheminFichier = CheminFichier;
                _Data = data;

                _Fonctions = new Dictionary<string, Action<StreamReader>>();
                _Fonctions.Add("Analyse statique", PAnalyseStatique);
            }

            public void Analyser()
            {
                var Lecteur = new Lecteur();
                Lecteur.CheminFichier = _CheminFichier;
                Lecteur.Separateur = '|';
                Lecteur.Fonctions = _Fonctions;
                Lecteur.Analyser();
                Lecteur = null;
            }

            private void PAnalyseStatique(StreamReader sr)
            {
                var l = sr.ReadLine();
                if (l.StartsWith("Cas de charges"))
                {
                    int no = Int32.Parse(l.Replace("Cas de charges", "").Trim());
                    if (_Data.ListeCasDeCharge.ContainsKey(no))
                    {
                        var c = _Data.ListeCasDeCharge[no];
                        sr.ReadLine(); sr.ReadLine();
                        l = sr.ReadLine().Split(new char[] { ':' })[1].Trim();
                        c.Fichier = l;
                    }
                }
                else if (l.StartsWith("Combinaison") && !l.StartsWith("Combinaison  :"))
                {
                    int no = Int32.Parse(l.Split(new char[] { '=' })[0].Replace("Combinaison", "").Trim());
                    if (_Data.ListeCombinaisons.ContainsKey(no))
                    {
                        var c = _Data.ListeCombinaisons[no];
                        sr.ReadLine(); sr.ReadLine();
                        l = sr.ReadLine().Split(new char[] { ':' })[1].Trim();
                        c.Fichier = l;
                    }
                }
            }
        }

        private class FichierOX
        {
            private DATA.CasDeCharge _Cas;
            private String _CheminFichier;

            private Dictionary<String, Action<StreamReader>> _Fonctions;

            private DATA _Data;

            public FichierOX(DATA.CasDeCharge Cas, String CheminFichier, DATA data)
            {
                _Cas = Cas;
                _CheminFichier = CheminFichier;
                _Data = data;

                _Fonctions = new Dictionary<string, Action<StreamReader>>();
                _Fonctions.Add("noeuds", PNoeud);
                _Fonctions.Add("elements", PElement);
                _Fonctions.Add("reactions", PReaction);
            }

            public void Analyser()
            {
                var Lecteur = new Lecteur();
                Lecteur.CheminFichier = _CheminFichier;
                Lecteur.Separateur = '$';
                Lecteur.Fonctions = _Fonctions;
                Lecteur.Analyser();
                Lecteur = null;
            }

            private int _Poutre = 1;

            private void PElement(StreamReader sr)
            {
                if (_Data.ListePoutres.ContainsKey(_Poutre))
                {
                    var Poutre = _Data.ListePoutres[_Poutre];
                    var Efforts = new DATA.EffortsPoutre();
                    Efforts.Poutre = Poutre.No;
                    Efforts.Cas = _Cas.No;

                    _Cas.ListeEffortsPoutre.Add(Efforts.Poutre, Efforts);

                    // Efforts Noeud Origine
                    var l = sr.ReadLine();
                    var t = l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var No = Efforts.EffortsNorigine;
                    No.Poutre = Poutre.No;
                    No.Noeud = Poutre.Origine;
                    No.Cas = _Cas.No;
                    No.N = -1 * Double.Parse(t[0]) * 0.1;
                    No.TY = Double.Parse(t[1]) * 0.1;
                    No.TZ = Double.Parse(t[2]) * 0.1;
                    No.MT = Double.Parse(t[3]) * 0.1;
                    No.MFY = Double.Parse(t[4]) * 0.1;
                    No.MFZ = Double.Parse(t[5]) * 0.1;

                    // Efforts Noeud Extremite
                    l = sr.ReadLine();
                    t = l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var Ne = Efforts.EffortsNextremite;
                    Ne.Poutre = Poutre.No;
                    Ne.Noeud = Poutre.Extremite;
                    Ne.Cas = _Cas.No;
                    Ne.N = Double.Parse(t[0]) * 0.1;
                    Ne.TY = Double.Parse(t[1]) * 0.1;
                    Ne.TZ = Double.Parse(t[2]) * 0.1;
                    Ne.MT = Double.Parse(t[3]) * 0.1;
                    Ne.MFY = Double.Parse(t[4]) * 0.1;
                    Ne.MFZ = Double.Parse(t[5]) * 0.1;

                    // Efforts Maximum
                    l = sr.ReadLine();
                    t = l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var M = Efforts.EffortsMax;
                    M.Poutre = Poutre.No;
                    M.Cas = _Cas.No;
                    M.Nc = Math.Min(0, Math.Min(Ne.N, No.N));
                    M.Nt = Math.Max(0, Math.Max(Ne.N, No.N));
                    M.TYmax = Double.Parse(t[0]) * 0.1;
                    M.TZmax = Double.Parse(t[1]) * 0.1;
                    M.MTmax = Math.Abs(No.MT);
                    M.MFYmax = Double.Parse(t[2]) * 0.1;
                    M.MFZmax = Double.Parse(t[3]) * 0.1;

                    _Poutre++;
                }
            }

            private int _Noeud = 1;

            private void PNoeud(StreamReader sr)
            {
                if (_Data.ListeNoeuds.ContainsKey(_Noeud))
                {
                    var Noeud = _Data.ListeNoeuds[_Noeud];
                    var Dep = new DATA.DeplacementNoeud();
                    Dep.Noeud = Noeud.No;
                    Dep.Cas = _Cas.No;

                    _Cas.ListeDeplacementNoeud.Add(Dep.Noeud, Dep);

                    // Déplacement Noeud
                    var l = sr.ReadLine();
                    var t = l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Dep.DX = Double.Parse(t[0]);
                    Dep.DY = Double.Parse(t[1]);
                    Dep.DZ = Double.Parse(t[2]);
                    Dep.RX = Double.Parse(t[3]);
                    Dep.RY = Double.Parse(t[4]);
                    Dep.RZ = Double.Parse(t[5]);

                    _Noeud++;
                }
            }

            private void PReaction(StreamReader sr)
            {
                var Reac = new DATA.ReactionNoeud();
                Reac.Cas = _Cas.No;

                // Efforts Noeud Origine
                var l = sr.ReadLine();
                var t = l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (t.GetLength(0) == 7)
                {
                    Reac.Noeud = Int32.Parse(t[0]);
                    Reac.FX = Double.Parse(t[1]) * 0.1;
                    Reac.FY = Double.Parse(t[2]) * 0.1;
                    Reac.FZ = Double.Parse(t[3]) * 0.1;
                    Reac.MX = Double.Parse(t[4]) * 0.1;
                    Reac.MY = Double.Parse(t[5]) * 0.1;
                    Reac.MZ = Double.Parse(t[6]) * 0.1;

                    _Cas.ListeReactionNoeud.Add(Reac.Noeud, Reac);
                }
            }
        }

        private class Lecteur
        {
            public String CheminFichier { get; set; }
            public char Separateur { get; set; }
            public Dictionary<String, Action<StreamReader>> Fonctions { get; set; }

            private Boolean NextLineIsSection(StreamReader s)
            {
                int n = s.Peek();
                if ((n > -1) && ((Char)n == Separateur))
                    return true;

                return false;
            }

            public void Analyser()
            {
                using (var fileStream = File.OpenRead(CheminFichier))
                {
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        while (streamReader.EndOfStream == false)
                        {
                            if (NextLineIsSection(streamReader))
                            {
                                String section = streamReader.ReadLine().Substring(1).Split(new char[] { '(', '|' })[0].Trim();

                                if (!Fonctions.ContainsKey(section)) continue;

                                Action<StreamReader> F = Fonctions[section];

                                if (F != null)
                                    while (!NextLineIsSection(streamReader) && (streamReader.EndOfStream == false))
                                        F(streamReader);
                            }
                            else { streamReader.ReadLine(); }
                        }
                    }
                }
            }
        }

        public class DATA
        {
            public Dictionary<int, Noeud> ListeNoeuds = new Dictionary<int, Noeud>();
            public Dictionary<int, Poutre> ListePoutres = new Dictionary<int, Poutre>();
            public Dictionary<int, Section> ListeSections = new Dictionary<int, Section>();
            public Dictionary<int, Materiau> ListeMateriaux = new Dictionary<int, Materiau>();
            public Dictionary<int, CasDeCharge> ListeCasDeCharge = new Dictionary<int, CasDeCharge>();
            public Dictionary<int, Combinaison> ListeCombinaisons = new Dictionary<int, Combinaison>();

            public class Noeud
            {
                public int No { get; set; }
                public double X { get; set; }
                public double Y { get; set; }
                public double Z { get; set; }
            }

            public class Poutre
            {
                public int No { get; set; }
                public string Relaxation { get; set; }
                public int Origine { get; set; }
                public int Extremite { get; set; }
                public int Section { get; set; }
                public int Materiau { get; set; }
                public double Longueur { get; set; }
            }

            public class EffortsPoutre
            {
                public int Poutre { get; set; }
                public int Cas { get; set; }

                private EffortsNoeud _EffortsNorigine = new EffortsNoeud();
                public EffortsNoeud EffortsNorigine { get { return _EffortsNorigine; } }

                private EffortsNoeud _EffortsNextremite = new EffortsNoeud();
                public EffortsNoeud EffortsNextremite { get { return _EffortsNextremite; } }

                private EffortsMax _EffortsMax = new EffortsMax();
                public EffortsMax EffortsMax { get { return _EffortsMax; } }

            }

            public class Section
            {
                private string _Nom;
                public int No { get; set; }
                public string Nom
                {
                    get
                    {
                        if (String.IsNullOrWhiteSpace(_Nom))
                            return No.ToString();

                        return _Nom;
                    }
                    set { _Nom = value; }
                }
            }

            public class Materiau
            {
                public int No { get; set; }
                public string Nom { get; set; }
            }

            public class CasDeCharge
            {
                private string _Nom = "";
                public int No { get; set; }
                public string Nom
                {
                    get
                    {
                        if (String.IsNullOrWhiteSpace(_Nom))
                            return "Cas " + No;

                        return _Nom;
                    }
                    set { _Nom = value; }
                }
                public string Fichier { get; set; }
                public Dictionary<int, EffortsPoutre> ListeEffortsPoutre = new Dictionary<int, EffortsPoutre>();
                public Dictionary<int, DeplacementNoeud> ListeDeplacementNoeud = new Dictionary<int, DeplacementNoeud>();
                public Dictionary<int, ReactionNoeud> ListeReactionNoeud = new Dictionary<int, ReactionNoeud>();
            }

            public class Combinaison : CasDeCharge
            {
                private List<Terme> _Lst = new List<Terme>();
                public List<Terme> Liste { get { return _Lst; } }
            }

            public class Terme
            {
                public Terme() { }
                public Terme(int cas, double facteur)
                {
                    Cas = cas;
                    Facteur = facteur;
                }
                public int Cas { get; set; }
                public double Facteur { get; set; }
            }

            public class DeplacementNoeud
            {
                public int Noeud { get; set; }
                public int Cas { get; set; }
                public double DX { get; set; }
                public double DY { get; set; }
                public double DZ { get; set; }
                public double RX { get; set; }
                public double RY { get; set; }
                public double RZ { get; set; }
            }

            public class EffortsNoeud
            {
                public int Poutre { get; set; }
                public int Noeud { get; set; }
                public int Cas { get; set; }
                public double N { get; set; }
                public double TY { get; set; }
                public double TZ { get; set; }
                public double MT { get; set; }
                public double MFY { get; set; }
                public double MFZ { get; set; }
            }

            public class ReactionNoeud
            {
                public int Noeud { get; set; }
                public int Cas { get; set; }
                public double FX { get; set; }
                public double FY { get; set; }
                public double FZ { get; set; }
                public double MX { get; set; }
                public double MY { get; set; }
                public double MZ { get; set; }
            }

            public class EffortsMax
            {
                public int Poutre { get; set; }
                public int Cas { get; set; }
                public double Nc { get; set; }
                public double Nt { get; set; }
                public double TYmax { get; set; }
                public double TZmax { get; set; }
                public double MTmax { get; set; }
                public double MFYmax { get; set; }
                public double MFZmax { get; set; }
            }

            public void Vider()
            {
                ListeNoeuds.Clear();
                ListePoutres.Clear();
                ListeSections.Clear();
                ListeMateriaux.Clear();
                ListeCasDeCharge.Clear();
                ListeCombinaisons.Clear();
            }
        }
    }
}

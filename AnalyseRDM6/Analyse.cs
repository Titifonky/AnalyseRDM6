using System;
using System.Collections.Generic;
using System.IO;

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

            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(Path.Combine(dossier, "Analyse.csv")))
            {
                sw.WriteLine("No;Origine;Extremite;Section;Lg;Nc;Nc comb;Nt;Nt comb;Ty;Ty comb;Tz;Tz comb;MFy;MFy comb;MFz;MFz comb;Mt;Mt comb");

                foreach (var Poutre in Data.ListePoutres.Values)
                {
                    String NcCombinaisonMax = "";
                    double Nc = 0;
                    String NtCombinaisonMax = "";
                    double Nt = 0;
                    String TyCombinaisonMax = "";
                    double Ty = 0;
                    String TzCombinaisonMax = "";
                    double Tz = 0;
                    String MFyCombinaisonMax = "";
                    double MFy = 0;
                    String MFzCombinaisonMax = "";
                    double MFz = 0;
                    String MtCombinaisonMax = "";
                    double Mt = 0;


                    foreach (var Comb in Data.ListeCombinaisons.Values)
                    {
                        var Efforts = Comb.ListeEffortsPoutre[Poutre.No];

                        if(Efforts.EffortsMax.Nc < Nc)
                        {
                            Nc = Efforts.EffortsMax.Nc;
                            NcCombinaisonMax = Comb.Nom;
                        }

                        if (Efforts.EffortsMax.Nt > Nt)
                        {
                            Nt = Efforts.EffortsMax.Nt;
                            NtCombinaisonMax = Comb.Nom;
                        }

                        if (Efforts.EffortsMax.TYmax > Ty)
                        {
                            Ty = Efforts.EffortsMax.TYmax;
                            TyCombinaisonMax = Comb.Nom;
                        }

                        if (Efforts.EffortsMax.TZmax > Tz)
                        {
                            Tz = Efforts.EffortsMax.TZmax;
                            TzCombinaisonMax = Comb.Nom;
                        }

                        if (Efforts.EffortsMax.MFYmax > MFy)
                        {
                            MFy = Efforts.EffortsMax.MFYmax;
                            MFyCombinaisonMax = Comb.Nom;
                        }

                        if (Efforts.EffortsMax.MFZmax > MFz)
                        {
                            MFz = Efforts.EffortsMax.MFZmax;
                            MFzCombinaisonMax = Comb.Nom;
                        }

                        if (Efforts.EffortsMax.MTmax > Mt)
                        {
                            Mt = Efforts.EffortsMax.MTmax;
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
                    L.Add(ValToString(Nc, NcCombinaisonMax));

                    // Traction pure
                    L.Add(ValToString(Nt, NtCombinaisonMax));

                    // Tranchant Y
                    L.Add(ValToString(Ty, TyCombinaisonMax));

                    // Tranchant Z
                    L.Add(ValToString(Tz, TzCombinaisonMax));

                    // Moment flechissant Y
                    L.Add(ValToString(MFy, MFyCombinaisonMax));

                    // Moment flechissant Z
                    L.Add(ValToString(MFz, MFzCombinaisonMax));

                    // Moment de torsion
                    L.Add(ValToString(Mt, MtCombinaisonMax));

                    sw.WriteLine(String.Join(";", L));
                }
            }
        }

        private String ValToString(double val, String Comb)
        {
            val = Math.Round(val, Precision);
            if (val != 0)
                return val.ToString() + ";" + Comb;

            return ";";
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
                    var no = Int32.Parse(sr.ReadLine().Trim());
                    var s = new DATA.Section();
                    s.No = no;
                    sr.ReadLine();
                    var r = sr.ReadLine().Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                    if (r.Length == 2)
                        s.Nom = r[1].Trim();

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
                _Fonctions.Add("elements", PElement);
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
            }

            public class Combinaison : CasDeCharge
            {
                private List<Terme> _Lst = new List<Terme>();
                public List<Terme> Liste { get { return _Lst; }}
            }

            public class Terme
            {
                public Terme() { }
                public Terme(int cas, double facteur)
                {
                    Cas = cas;
                    Facteur = facteur;
                }
                public int Cas { get; set;}
                public double Facteur { get; set;}
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

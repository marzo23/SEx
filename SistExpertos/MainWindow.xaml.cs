using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SistExpertos
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string AtomoFile = @"C:\Users\L440\source\repos\SistExpertos\SistExpertos\DataTables\Atoms.csv";
        private string ProposicionesFile = @"C:\Users\L440\source\repos\SistExpertos\SistExpertos\DataTables\Propositions.csv";
        private string RelacionesFile = @"C:\Users\L440\source\repos\SistExpertos\SistExpertos\DataTables\Relations.csv";

        class Proposicion
        {
            public string Id { get; set; }
            public string Descripcion { get; set; }

            public Proposicion(string[] array, string[] relations)
            {
                Id = array[0];
                if (array.Length == 3)
                {
                    string tmp = string.Empty;
                    foreach (string rel in relations)
                    {
                        string[] r = rel.Split(',');
                        if (r.Length == 3)
                        {
                            if (r[0].Equals(array[0]))
                            {
                                if (!tmp.Equals(string.Empty))
                                    tmp += "^";
                                tmp += (bool.Parse(r[2]) == false ? "!" : "")+ r[1];
                            }
                        }
                    }
                    if (!tmp.Equals(string.Empty))
                    {
                        Descripcion = tmp + "->" + (bool.Parse(array[2]) == false ? "!" : "") + array[1];
                    }
                }
            }
        }

        class Atomo
        {
            public string Id { get; set; }
            public string Descripcion { get; set; }
        }

        ObservableCollection<Proposicion> ProposicionList { get; set; }
        ObservableCollection<Atomo> AtomoList { get; set; }

        Proposicion SelectedProposicion { get; set; }
        Atomo SelectedAtom { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ProposicionList = new ObservableCollection<Proposicion>();
            AtomoList = new ObservableCollection<Atomo>();
            this.DtGrid_Atomos.ItemsSource = AtomoList;
            this.DtGrid_Proposiciones.ItemsSource = ProposicionList;
            LoadFileData();
        }
        

        private void VerProposicion(object sender, RoutedEventArgs e)
        {
            string tmp = "";
            SelectedProposicion = (Proposicion)this.DtGrid_Proposiciones.SelectedItem;
            List<string> relaciones = File.ReadAllLines(RelacionesFile).ToList();
            List<string> proposiciones = File.ReadAllLines(ProposicionesFile).ToList();
            foreach (string item in relaciones)
            {
                string[] rel = item.Split(',');
                if(rel[0].Equals(SelectedProposicion.Id))
                    tmp+=AtomoList.ToList().Find(a=>a.Id.Equals(rel[1])).Descripcion+" and ";
            }
            tmp+=" then ";
            string end = proposiciones.ToList().Find(p => p.Split(',')[0].Equals(SelectedProposicion.Id));
            tmp += AtomoList.ToList().Find(a => a.Id.Equals(end.Split(',')[1])).Descripcion;
            MessageBox.Show(tmp);
        }

            public void LoadFileData()
        {
            ProposicionList.Clear();
            AtomoList.Clear();
            string[] atomsTmp = File.ReadAllLines(AtomoFile);
            foreach (string atomRow in atomsTmp)
            {
                string[] tmp = atomRow.Split(',');
                AtomoList.Add(new Atomo() { Id = tmp[0], Descripcion = tmp[1] });
            }

            ParseProposiciones();
        }

        private void EliminarAtomo(object sender, RoutedEventArgs e)
        {
            SelectedAtom = (Atomo) this.DtGrid_Atomos.SelectedItem;
            List<string> relaciones = File.ReadAllLines(RelacionesFile).ToList();
            List<string> proposiciones = File.ReadAllLines(ProposicionesFile).ToList();
            List<string> atomos = File.ReadAllLines(AtomoFile).ToList();
            atomos.RemoveAll(r => r.Split(',')[0].Equals(SelectedAtom.Id));

            for (int i = 0; i < relaciones.Count; i++)
            {
                string[] tmp = relaciones[i].Split(',');
                if (tmp[1].Equals(SelectedAtom.Id))
                {
                    relaciones.RemoveAll(r => r.Split(',')[0].Equals(tmp[0]));
                    proposiciones.RemoveAll(r => r.Split(',')[0].Equals(tmp[0]));
                    i = 0;
                }
            }

            for (int i = 0; i < proposiciones.Count; i++)
            {
                string[] tmp = proposiciones[i].Split(',');
                if (tmp[1].Equals(SelectedAtom.Id))
                {
                    relaciones.RemoveAll(r => r.Split(',')[0].Equals(tmp[0]));
                    proposiciones.RemoveAll(r => r.Split(',')[0].Equals(tmp[0]));
                    i = 0;
                }
            }
            
            File.WriteAllLines(RelacionesFile, relaciones);
            File.WriteAllLines(AtomoFile, atomos);
            File.WriteAllLines(ProposicionesFile, proposiciones);
            LoadFileData();
        }

        private void EliminarProposicion(object sender, RoutedEventArgs e)
        {
            SelectedProposicion = (Proposicion)this.DtGrid_Proposiciones.SelectedItem;
            List<string> relaciones = File.ReadAllLines(RelacionesFile).ToList();
            List<string> proposiciones = File.ReadAllLines(ProposicionesFile).ToList();


            for (int i = 0; i < relaciones.Count; i++)
            {
                string[] tmp = relaciones[i].Split(',');
                if (tmp[0].Equals(SelectedProposicion.Id))
                {
                    relaciones.RemoveAll(r => r.Split(',')[0].Equals(tmp[0]));
                    proposiciones.RemoveAll(r => r.Split(',')[0].Equals(tmp[0]));
                    i = 0;
                }
            }

            for (int i = 0; i < proposiciones.Count; i++)
            {
                string[] tmp = proposiciones[i].Split(',');
                if (tmp[0].Equals(SelectedProposicion.Id))
                {
                    relaciones.RemoveAll(r => r.Split(',')[0].Equals(tmp[0]));
                    proposiciones.RemoveAll(r => r.Split(',')[0].Equals(tmp[0]));
                    i = 0;
                }
            }

            File.WriteAllLines(RelacionesFile, relaciones);
            File.WriteAllLines(ProposicionesFile, proposiciones);
            LoadFileData();
        }

        private void GuardarProposicion(object sender, RoutedEventArgs e)
        {
            BasicValidarProposicion(this.TxtBox_Proposicion.Text);
            /*
            string[] atomsTmp = File.ReadAllLines(AtomoFile);
            string lastRow = null;
            if (atomsTmp.Length > 0)
                lastRow = (int.Parse(atomsTmp[atomsTmp.Length - 1].Split(',')[0]) + 1).ToString();
            else
                lastRow = "1";

            File.AppendAllText(AtomoFile, lastRow + "," + this.TxtBox_Atomo.Text.Replace(",", "") + "\n");
            AtomoList.Add(new Atomo() { Id = lastRow, Description = this.TxtBox_Atomo.Text.Replace(",", "") });*/
        }

        private bool BasicValidarProposicion(string proposicion)
        {
            proposicion.Replace(" ", "");
            string[] atomos = Regex.Split(proposicion, @"(?<=[&>])");
            Dictionary<string, bool> antecedenteDic = new Dictionary<string, bool>();
            string consecuente = null;
            bool consecuenteMod = true;
            List<string> operandosL = new List<string>();
            int isConsecuente = 0;
            foreach (string atomo in atomos)
            {
                if (atomo.EndsWith(">"))
                {
                    isConsecuente++;
                }

                if (isConsecuente==0 || atomo.EndsWith(">"))
                {
                    string atomoTmp = atomo.Substring(0, atomo.Length - 1).Replace("!", "");
                    if (ValidaAtomo(atomoTmp))
                    {
                        if (!antecedenteDic.ContainsKey(atomoTmp))
                        {
                            if (atomo.Contains("!"))
                            {
                                antecedenteDic.Add(atomoTmp, false);
                            }
                            else
                            {
                                antecedenteDic.Add(atomoTmp, true);
                            }
                        }
                    }
                    else
                        return false;
                }
                else if (isConsecuente == 1)
                {
                    if (ValidaAtomo(atomo.Replace("!", "")))
                    {
                        consecuente = atomo.Replace("!", "");
                        if (atomo.Contains("!"))
                            consecuenteMod = false;
                    }
                    else
                        return false;
                    isConsecuente++;
                }
                else
                    this.Lbl_MensajeError.Content = "Error formato consecuente.";
            }

            string lastPropId = GetLastId(ProposicionesFile);

            if (consecuente != null)
            {
                foreach (KeyValuePair<string, bool> entry in antecedenteDic)
                {
                    GuardarRelacion(lastPropId, entry.Key, entry.Value);
                }

                GuardarProposicion(lastPropId, consecuente, consecuenteMod);
            }

            return true;
        }

        public void GuardarProposicion(string id, string consecuente, bool esFalso)
        {
            File.AppendAllText(ProposicionesFile, id + "," + consecuente + "," + esFalso + "\n");
            ParseProposiciones();
        }

        public void ParseProposiciones()
        {
            ProposicionList.Clear();
            string[] relaciones = File.ReadAllLines(RelacionesFile);
            string[] proposiciones = File.ReadAllLines(ProposicionesFile);
            foreach (string prop in proposiciones)
            {
                if (prop.Split(',').Length == 3)
                    ProposicionList.Add(new Proposicion(prop.Split(','), relaciones));
            }
        }

        private void GuardarRelacion(string proposicionId, string atomoId,bool esFalso)
        {
            File.AppendAllText(RelacionesFile, proposicionId + "," + atomoId + "," + esFalso + "\n");
        }

        private bool ValidaAtomo(string id)
        {
            if (AtomoList.ToList().Find(a => a.Id.Equals(id)) != null)
                return true;
            this.Lbl_MensajeError.Content = "Atomo no encontrado: \n" + id;
            return false;
        }

        private string GetLastId(string file)
        {
            string[] atomsTmp = File.ReadAllLines(file);
            string lastRow = null;
            if (atomsTmp.Length > 0)
                lastRow = (int.Parse(atomsTmp[atomsTmp.Length - 1].Split(',')[0]) + 1).ToString();
            else
                lastRow = "1";
            return lastRow;
        }

        private void GuardarAtomo(object sender, RoutedEventArgs e)
        {
            string lastRow = GetLastId(AtomoFile);

            File.AppendAllText(AtomoFile, lastRow + "," + this.TxtBox_Atomo.Text.Replace(",", "") + "\n");
            AtomoList.Add(new Atomo() { Id = lastRow, Descripcion = this.TxtBox_Atomo.Text.Replace(",", "") });
        }
    }
}

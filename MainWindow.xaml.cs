using System.IO;
using System.Windows;
using System.Xml.Xsl;
using System.Xml;
using Path = System.IO.Path;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using Microsoft.Win32;
using System.Security.Cryptography.Pkcs;
using iText.Kernel.Utils;
using iText.Kernel.Pdf;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using FatEle2PDF.Properties;


namespace XMLtoPDF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> files, listaDel, listaMerge;
        string multiHTML;
        PdfOptions options;
        string percorsoFileDest;
        int errCounter;
        struct XML
        {
            private string _filePath;
            private string _RSoc;
            private double _totImp;
            private string _pIVA;

            public string filePath => _filePath;
            public string RSoc => _RSoc;
            public double totImp => _totImp;
            public string pIVA => _pIVA;
            public XML(string filePath, string rsoc, double totImp, string pIva)
            {
                _filePath = filePath;
                _pIVA = pIva;
                _RSoc = rsoc;
                _totImp = totImp;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            multiHTML = string.Empty;
            listaDel = new List<string>();
            listaMerge = new List<string>();
            errCounter = 0;

            options = new PdfOptions();
            options.Format = PaperFormat.A4;
            options.MarginOptions = new MarginOptions
            {
                Left = "2cm",
                Top = "2.5cm",
                Right = "2cm",
                Bottom = "2cm",
            };
        }


        private async void StartButton(object sender, RoutedEventArgs e)
        {
            if (files != null && percorsoFileDest != null)
            {
                mWindow.IsEnabled = false;
                ConvertiButton.Content = $"Elaborazione di {files.Count} XML";
                bool combina = RadioCombine.IsChecked ?? false;
                int ordine = Ordine.SelectedIndex;
                int creDecr = 1;
                if (DecrRadio.IsChecked == true) creDecr = -1;

                await Task.Run(() => Convert2Pdf(files, combina, ordine, creDecr));
                ConvertiButton.Content = "Converti";
                mWindow.IsEnabled = true;
                if (errCounter > 0)
                {
                    LabelErrori.Visibility = Visibility.Visible;
                }
                else
                {
                    LabelErrori.Visibility= Visibility.Hidden;
                }
            }
        }

        public async Task Convert2Pdf(List<string> files, bool combina, int ordine, int creDecr)
        {
            List<XML> listaXML = new List<XML>();
            StringBuilder errorBuilder = new StringBuilder();
            string nomeFilePdf;
            string fileDest = Path.GetFileName(percorsoFileDest);
            string percorsoDest = Path.GetDirectoryName(percorsoFileDest);

            XslCompiledTransform xslTransform = new XslCompiledTransform();

            using (var reader = new StringReader(FatEle2PDF.Properties.Resources.FoglioStile))
            {
                using (XmlReader xmlReader = XmlReader.Create(reader))
                {
                    xslTransform.Load(xmlReader);
                }
            }

            foreach (string file in files)
            {
                string fileXML = DecodeBase64String(file, listaDel);

                if (Path.GetExtension(file) == ".p7m")
                {
                    fileXML = EstraiFirma(fileXML);
                    listaDel.Add(fileXML);
                }
                
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(fileXML);

                    string rSoc = string.Empty;
                    double totImp = 0;
                    string pIva = string.Empty;

                    if (doc.DocumentElement.SelectSingleNode("//FatturaElettronicaHeader/CedentePrestatore/DatiAnagrafici/Anagrafica/Denominazione") != null)
                    {
                        rSoc = doc.DocumentElement.SelectSingleNode("//FatturaElettronicaHeader/CedentePrestatore/DatiAnagrafici/Anagrafica/Denominazione").InnerText;
                    }
                    else if (doc.DocumentElement.SelectSingleNode("//FatturaElettronicaHeader/CedentePrestatore/DatiAnagrafici/Anagrafica/Cognome") != null
                        || doc.SelectSingleNode("//FatturaElettronicaHeader/CedentePrestatore/DatiAnagrafici/Anagrafica/Nome") != null)
                    {
                        rSoc = doc.DocumentElement.SelectSingleNode("//FatturaElettronicaHeader/CedentePrestatore/DatiAnagrafici/Anagrafica/Cognome").InnerText + doc.SelectSingleNode("//FatturaElettronicaHeader/CedentePrestatore/DatiAnagrafici/Anagrafica/Nome").InnerText;
                    }

                    if (doc.DocumentElement.SelectSingleNode("//FatturaElettronicaBody/DatiGenerali/DatiGeneraliDocumento/ImportoTotaleDocumento") != null)
                    {
                        totImp = Convert.ToDouble(doc.DocumentElement.SelectSingleNode("//FatturaElettronicaBody/DatiGenerali/DatiGeneraliDocumento/ImportoTotaleDocumento").InnerText);
                    }

                    if (doc.DocumentElement.SelectSingleNode("//FatturaElettronicaHeader/CedentePrestatore/DatiAnagrafici/IdFiscaleIVA/IdCodice") != null)
                    {
                        pIva = doc.DocumentElement.SelectSingleNode("//FatturaElettronicaHeader/CedentePrestatore/DatiAnagrafici/IdFiscaleIVA/IdCodice").InnerText;
                    }

                    XML xml = new XML(fileXML, rSoc, totImp, pIva);

                    listaXML.Add(xml);
                }
                catch (Exception ex)
                {
                    errCounter++;
                    errorBuilder.Append($"Errore su file {file}: {ex.Message}");
                }
                
                
            }
            if (combina == true)
            {

                switch (ordine)
                {
                    case 0:
                        listaXML.Sort((x, y) => creDecr * x.RSoc.CompareTo(y.RSoc));
                        break;
                    case 1:
                        listaXML.Sort((x, y) => creDecr * x.totImp.CompareTo(y.totImp));
                        break;
                    case 2:
                        listaXML.Sort((x, y) => creDecr * x.pIVA.CompareTo(y.pIVA));
                        break;
                }
            }

            //string percorsoFilePdf = Path.Combine(cartellaInput, "totale.pdf");
            await new BrowserFetcher().DownloadAsync();
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            using var page = await browser.NewPageAsync();
            // Converti ogni file XML in PDF

            int c = 0;
            
            foreach (XML file in listaXML)
            {
                c++;

                if (combina == false)
                {
                    nomeFilePdf = Path.GetFileNameWithoutExtension(file.filePath.Replace("DECODED_", "")) + ".pdf";
                }
                else
                {
                    nomeFilePdf = fileDest;
                }

                StringWriter results = new StringWriter();

                using (XmlReader reader = XmlReader.Create(file.filePath))
                {
                   xslTransform.Transform(reader, null, results);
                }
                multiHTML = multiHTML + results.ToString() + "<div style='break-after: page' ></div>";

                if (file.filePath != listaXML.Last().filePath && c % 250 != 0 && combina == true)
                {
                    continue;
                }

                await page.SetContentAsync(multiHTML);

                if (combina == true)
                {
                    nomeFilePdf = Path.GetFileNameWithoutExtension(fileDest) + c + ".pdf";
                    await page.PdfAsync(Path.Combine(percorsoDest, nomeFilePdf), options);
                    listaMerge.Add(Path.Combine(percorsoDest, nomeFilePdf));
                }
                else
                {
                    await page.PdfAsync(Path.Combine(percorsoDest, nomeFilePdf), options);
                }


                multiHTML = string.Empty;
                
            }

            if (combina == true)
            {
                PdfDocument pdf = new PdfDocument(new PdfWriter(Path.Combine(percorsoDest, fileDest)));
                PdfMerger merger = new PdfMerger(pdf);
                foreach (string file in listaMerge)
                {
                    using PdfDocument SourcePdf = new PdfDocument(new PdfReader(file));
                    merger.Merge(SourcePdf, 1, SourcePdf.GetNumberOfPages());
                }
                pdf.Close();
            }
            await browser.DisposeAsync();
            CancellaFiles(listaMerge);
            //File.AppendAllText(Path.Combine(Path.GetDirectoryName(GetConfigPath()), $"log {DateTime.Now.ToString("dd.MM.yyyy HH-mm-ss")}.txt"), errorBuilder.ToString());
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(GetConfigPath()), "log.txt"), errorBuilder.ToString());
            errorBuilder.Clear();
        }

        private void BrowseButtonSel(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "File XML|*.xml;*.p7m";
            if (openFileDialog.ShowDialog() == true)
            {
                files = new List<string>(openFileDialog.FileNames);
                if (files.Count > 1)
                {
                    SelectedFiles.Text = "XML multipli";
                    LabelSelezione.Content = $"{files.Count} XML selezionati";
                }
                else
                {
                    LabelSelezione.Content = string.Empty;
                    SelectedFiles.Text = files[0];
                }
                
            }
                
        }

        public string EstraiFirma(string file)
        {
            byte[] FileFirmatoP7m = File.ReadAllBytes(file);
            if (FileFirmatoP7m == null)
                throw new ArgumentNullException("Errore nel file firmato selezionato.");
            //Uso la classe per estrapolare il fle
            SignedCms cmsFirmato = new();
            cmsFirmato.Decode(FileFirmatoP7m);

            if (cmsFirmato.Detached)
                throw new InvalidOperationException("Errore nella fase di estrapolazione del contenuto dal file firmato.");
            //Estrapola l'array byte del file firmato
            byte[] FileRilevato = cmsFirmato.ContentInfo.Content;
            //Nome del file che viene generato
            string NomeFileDaCreare = new FileInfo(file).Name.Replace(new FileInfo(file).Extension, "");
            string percorsoFileDaCreare = Path.GetDirectoryName(file) + "\\" + NomeFileDaCreare;
            File.WriteAllBytes(percorsoFileDaCreare, FileRilevato);
            return percorsoFileDaCreare;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            FatEle2PDF.Properties.Settings.Default.Save();
            CancellaFiles(listaDel);
            CancellaFiles(listaMerge);
        }

        private void BrowseButtonDest(object sender, RoutedEventArgs e)
        {
            if (RadioCombine.IsChecked == true)
            {
                SaveFileDialog saveFileDialog = new();
                saveFileDialog.Filter = "File PDF|*.pdf";
                if (saveFileDialog.ShowDialog() == true)
                {
                    SelectedFileDest.Text = saveFileDialog.FileName;
                    percorsoFileDest = saveFileDialog.FileName;
                }
            }
            else
            {
                OpenFolderDialog folderFileDialog = new();
                if (folderFileDialog.ShowDialog() == true)
                {
                    SelectedFileDest.Text = folderFileDialog.FolderName;
                    percorsoFileDest = folderFileDialog.FolderName;
                }
            }
            
        }
        public string DecodeBase64String(string file, List<string> listaDel)
        {
            string base64 = File.ReadAllText(file);
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            if (Convert.TryFromBase64String(base64, buffer, out int bytesParsed))
            {
                string newFilePath = Path.GetDirectoryName(file) + "\\DECODED_" + Path.GetFileName(file);
                File.WriteAllBytes(newFilePath, Convert.FromBase64String(base64));
                listaDel.Add(newFilePath);
                return newFilePath;
            }
            return file;
        }

        public void CancellaFiles(List<string> listaDel)
        {
            if (listaDel.Count > 0)
            {
                foreach (string fileDel in listaDel)
                {
                    File.Delete(fileDel);
                }
            }
            listaDel.Clear();
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(GetConfigPath()))
            {
                //Existing user config does not exist, so load settings from previous assembly
                Settings.Default.Upgrade();
                Settings.Default.Reload();
                Settings.Default.Save();
            }
        }

        public string GetConfigPath()
        {
            return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
        }

        private void ApriLog(object sender, RoutedEventArgs e)
        {
            string logPath = Path.Combine(Path.GetDirectoryName(GetConfigPath()), "log.txt");
            if (File.Exists(logPath))
            {
                ProcessStartInfo psi = new ProcessStartInfo(logPath);
                psi.Verb = "open";
                psi.UseShellExecute = true;
                Process.Start(psi);

            }
        }

        private void Checked(object sender, RoutedEventArgs e)
        {
            SelectedFileDest.Text = null;
            percorsoFileDest = null;
        }

    }
}
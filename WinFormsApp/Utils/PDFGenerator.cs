using iText.Forms;
using iText.Kernel.Pdf;
using System;
using System.Collections.Generic; 
using System.IO;
using System.Threading.Tasks;

namespace FormulariRif_G.Utils
{
    internal class PDFGenerator
    {
        private readonly string _templatePath;
        private readonly string _outputPath;

        /// <summary>
        /// Inizializza il filler PDF per la fattura.
        /// </summary>
        /// <param name="templatePdfPath">Percorso del file PDF di template con i campi compilabili.</param>
        /// <param name="outputPdfPath">Percorso dove salvare il PDF compilato.</param>
        public PDFGenerator(string templatePdfPath, string outputPdfPath)
        {
            _templatePath = templatePdfPath;
            _outputPath = outputPdfPath;

            // Assicurati che la directory di output esista
            var outputDir = Path.GetDirectoryName(_outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
        }

        /// <summary>
        /// Popola i campi del PDF con i dati forniti.
        /// </summary>
        /// <param name="datiFattura">Un dizionario o un oggetto con i dati da inserire nei campi.</param>
        /// <returns>True se l'operazione è riuscita, false altrimenti.</returns>
        public bool FillFattura(Dictionary<string, string> datiFattura)
        {
            //try
            //{

            if (!File.Exists(_templatePath))
            {
                // Rilanciare un'eccezione specifica per "file non trovato"
                throw new FileNotFoundException($"Il file template PDF non è stato trovato al percorso: {_templatePath}", _templatePath);
            }
            // Crea un PdfReader per leggere il template esistente
            using (var reader = new PdfReader(_templatePath))
                {
                    // Crea un PdfWriter per scrivere nel nuovo file di output
                    using (var writer = new PdfWriter(_outputPath))
                    {
                        // Crea un PdfDocument da un lettore e uno scrittore per modificare il PDF
                        using (var pdfDocument = new PdfDocument(reader, writer))
                        {
                            // Ottieni il modulo del PDF (AcroForm)
                            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDocument, true);

                            // Ottieni la mappa dei campi del modulo
                            // MODIFICA QUI: Usa GetAllFormFields() per le versioni recenti di iText7
                            var fields = form.GetAllFormFields();

                            foreach (var entry in datiFattura)
                            {
                                if (fields.ContainsKey(entry.Key))
                                {
                                    // Imposta il valore del campo
                                    fields[entry.Key].SetValue(entry.Value);
                                }
                                else
                                {
                                    Console.WriteLine($"Attenzione: Campo '{entry.Key}' non trovato nel PDF di template.");
                                }
                            }

                            // Facoltativo: "appiattisci" i campi. Questo li rende non modificabili e parte integrante del documento.
                            // Se non lo fai, i campi rimarranno editabili in un lettore PDF.
                            form.FlattenFields();
                        } // pdfDocument.Close() viene chiamato qui grazie all'uso di 'using'
                    } // writer.Close() viene chiamato qui
                } // reader.Close() viene chiamato qui

                Console.WriteLine($"PDF compilato salvato in: {_outputPath}");
                return true;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Errore durante la compilazione del PDF: {ex.Message}");
            //    return false;
            //}
        }

        /// <summary>
        /// Metodo asincrono per popolare i campi del PDF.
        /// </summary>
        /// <param name="datiFattura">Un dizionario o un oggetto con i dati da inserire nei campi.</param>
        /// <returns>True se l'operazione è riuscita, false altrimenti.</returns>
        public async Task<bool> FillFatturaAsync(Dictionary<string, string> datiFattura)
        {
            // La maggior parte delle operazioni di I/O di iText7 non ha controparti async dirette per la manipolazione di campi.
            // Tuttavia, possiamo avvolgerla in un Task.Run per non bloccare il thread chiamante (UI).
            return await Task.Run(() => FillFattura(datiFattura));
        }
    }
}


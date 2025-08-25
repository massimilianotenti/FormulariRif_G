// File: Forms/FormulariRifiutiDetailForm.cs
// Questo form permette di inserire o modificare un singolo formulario rifiuti.
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using FormulariRif_G.Utils;
using Microsoft.EntityFrameworkCore; // Per l'include delle proprietà di navigazione
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
// using System.Drawing.Text; // Non sembra essere usato
using System.Linq;
using System.Globalization;
using System.IO; // Aggiunto per Path.Combine e IOException

namespace FormulariRif_G.Forms
{
    public partial class FormulariRifiutiDetailForm : Form
    {
        private readonly IGenericRepository<Configurazione> _configurazioneRepository;
        private readonly IGenericRepository<FormularioRifiuti> _formularioRifiutiRepository;
        private readonly IGenericRepository<Cliente> _clienteRepository;
        private readonly IGenericRepository<ClienteIndirizzo> _clienteIndirizzoRepository;
        private readonly IGenericRepository<Automezzo> _automezzoRepository;
        private FormularioRifiuti? _currentFormulario;
        // Flag per evitare che l'evento SelectedIndexChanged si attivi durante il caricamento iniziale dei dati
        private bool _isLoading = false;

        private bool _isFormularioSaved = false;

        public FormulariRifiutiDetailForm(IGenericRepository<FormularioRifiuti> formularioRifiutiRepository,
                                         IGenericRepository<Cliente> clienteRepository,
                                         IGenericRepository<ClienteIndirizzo> clienteIndirizzoRepository,
                                         IGenericRepository<Automezzo> automezzoRepository,
                                         IGenericRepository<Configurazione> configurazioneRepository)
        {
            InitializeComponent();
            _formularioRifiutiRepository = formularioRifiutiRepository;
            _clienteRepository = clienteRepository;
            _clienteIndirizzoRepository = clienteIndirizzoRepository;
            _automezzoRepository = automezzoRepository;
            _configurazioneRepository = configurazioneRepository;
            this.Load += FormulariRifiutiDetailForm_Load;
        }

        private async void FormulariRifiutiDetailForm_Load(object? sender, EventArgs e)
        {
            _isLoading = true;
            await LoadComboBoxes();
            await LoadFormularioData();
            _isLoading = false;
            cmbProduttore.SelectedIndexChanged += cmbProduttore_SelectedIndexChanged;
            cmbDestinatario.SelectedIndexChanged += cmbDestinatario_SelectedIndexChanged;
            cmbTrasportatore.SelectedIndexChanged += cmbTrasportatore_SelectedIndexChanged;
        }

        /// <summary>
        /// Imposta il formulario da visualizzare o modificare.
        /// </summary>
        /// <param name="formulario">L'oggetto FormularioRifiuti.</param>
        public void SetFormulario(FormularioRifiuti formulario)
        {
            // La data viene impostata nel LoadFormularioData dopo che le combobox sono caricate
            _currentFormulario = formulario;
            // La logica _isFormularioSaved ora riflette se il formulario esiste già nel DB (non è nuovo)
            _isFormularioSaved = (_currentFormulario != null && _currentFormulario.Id != 0);
        }

        /// <summary>
        /// Carica i dati del formulario nei controlli del form.
        /// </summary>
        private async Task LoadFormularioData()
        {
            if (_currentFormulario != null)
            {
                dtpData.Value = _currentFormulario.Data;
                txtNumeroFormulario.Text = _currentFormulario.NumeroFormulario;

                // Carica Produttore e relativo indirizzo
                cmbProduttore.SelectedValue = _currentFormulario.IdProduttore;
                await LoadIndirizziAsync(cmbProduttore, cmbProduttoreIndirizzo, _currentFormulario.IdProduttoreIndirizzo);

                // Carica Destinatario e relativo indirizzo
                cmbDestinatario.SelectedValue = _currentFormulario.IdDestinatario;
                await LoadIndirizziAsync(cmbDestinatario, cmbDestinatarioIndirizzo, _currentFormulario.IdDestinatarioIndirizzo);

                // Carica Trasportatore e relativo indirizzo
                cmbTrasportatore.SelectedValue = _currentFormulario.IdTrasportatore;
                await LoadIndirizziAsync(cmbTrasportatore, cmbTrasportatoreIndirizzo, _currentFormulario.IdTrasportatoreIndirizzo);

                cmbAutomezzo.SelectedValue = _currentFormulario.IdAutomezzo;

                // Caratteristiche del rifiuto
                txtCodiceEER.Text = _currentFormulario.CodiceEER ?? string.Empty;
                if (_currentFormulario.SatoFisico.HasValue)
                    txtStatoFisco.Text = _currentFormulario.SatoFisico.Value.ToString();
                else
                    txtStatoFisco.Text = string.Empty;

                // NOTA: La proprietà CaratteristicheChimiche è usata per due campi diversi.
                // Questo potrebbe essere un errore nel modello o nel design.
                // Per ora, la mappo su entrambi come nel codice originale.
                txtCarattPericolosità.Text = _currentFormulario.CaratteristicheChimiche ?? string.Empty;
                if (_currentFormulario.Provenienza.HasValue)
                {
                    if (_currentFormulario.Provenienza.Value == 1) // Urbano
                        rbProvUrb.Checked = true;
                    else if (_currentFormulario.Provenienza.Value == 2) // Speciale
                        rbProvSpec.Checked = true;
                }
                else
                {
                    rbProvUrb.Checked = false;
                    rbProvSpec.Checked = false;
                }
                txtDescr.Text = _currentFormulario.Descrizione ?? string.Empty;
                if (_currentFormulario.Quantita.HasValue)
                    txtQuantita.Text = _currentFormulario.Quantita.Value.ToString("F2"); // Formatta come decimale con 2 cifre
                else
                    txtQuantita.Text = string.Empty;
                if (_currentFormulario.Kg_Lt.HasValue)
                {
                    if (_currentFormulario.Kg_Lt.Value == 1) // Kg
                        rbKg.Checked = true;
                    else if (_currentFormulario.Kg_Lt.Value == 2) // Litri
                        rbLitri.Checked = true;
                }
                else
                {
                    rbKg.Checked = false;
                    rbLitri.Checked = false;
                }
                if (_currentFormulario.PesoVerificato.HasValue)
                    ckPesoVerificato.Checked = _currentFormulario.PesoVerificato.Value;
                else
                    ckPesoVerificato.Checked = false;
                if (_currentFormulario.NumeroColli.HasValue)
                    txtColli.Text = _currentFormulario.NumeroColli.Value.ToString();
                else
                    txtColli.Text = string.Empty;
                if (_currentFormulario.AllaRinfusa.HasValue)
                    ckAllaRinfusa.Checked = _currentFormulario.AllaRinfusa.Value;
                else
                    ckAllaRinfusa.Checked = false;
                txtChimicoFisiche.Text = _currentFormulario.CaratteristicheChimiche ?? string.Empty;

                _isFormularioSaved = (_currentFormulario.Id != 0);
            }
            else
            {
                dtpData.Value = DateTime.Now;
                txtNumeroFormulario.Text = string.Empty;
                cmbProduttore.SelectedIndex = -1;
                cmbProduttoreIndirizzo.SelectedIndex = -1;
                cmbDestinatario.SelectedIndex = -1;
                cmbDestinatarioIndirizzo.SelectedIndex = -1;
                cmbTrasportatore.SelectedIndex = -1;
                cmbTrasportatoreIndirizzo.SelectedIndex = -1;
                cmbAutomezzo.SelectedIndex = -1;

                // Pulisci campi caratteristiche rifiuto
                txtCodiceEER.Text = string.Empty;
                txtStatoFisco.Text = string.Empty;
                txtCarattPericolosità.Text = string.Empty;
                rbProvUrb.Checked = false;
                rbProvSpec.Checked = false;
                txtDescr.Text = string.Empty;
                txtQuantita.Text = string.Empty;
                rbKg.Checked = false;
                rbLitri.Checked = false;
                ckPesoVerificato.Checked = false;
                txtColli.Text = string.Empty;
                ckAllaRinfusa.Checked = false;
                txtChimicoFisiche.Text = string.Empty;

                _isFormularioSaved = false;
            }
            UpdatePrintButtonState();
        }


        private async void btnSalva_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            if (_currentFormulario == null)
            {
                _currentFormulario = new FormularioRifiuti();
            }

            _currentFormulario.Data = dtpData.Value;
            _currentFormulario.NumeroFormulario = txtNumeroFormulario.Text.Trim();

            // Assicurati che un cliente, indirizzo e automezzo siano selezionati
            if (!ValidateInput()) return;

            _currentFormulario.IdProduttore = (int)cmbProduttore.SelectedValue;
            _currentFormulario.IdProduttoreIndirizzo = (int)cmbProduttoreIndirizzo.SelectedValue;

            _currentFormulario.IdDestinatario = (int)cmbDestinatario.SelectedValue;
            _currentFormulario.IdDestinatarioIndirizzo = (int)cmbDestinatarioIndirizzo.SelectedValue;

            _currentFormulario.IdTrasportatore = (int)cmbTrasportatore.SelectedValue;
            _currentFormulario.IdTrasportatoreIndirizzo = (int)cmbTrasportatoreIndirizzo.SelectedValue;

            _currentFormulario.IdAutomezzo = (int)cmbAutomezzo.SelectedValue;

            // Caratteristiche del rifiuto
            _currentFormulario.CodiceEER = txtCodiceEER.Text.Trim();
            if (!string.IsNullOrWhiteSpace(txtStatoFisco.Text) && int.TryParse(txtStatoFisco.Text, out int st_fisico))
                _currentFormulario.SatoFisico = st_fisico;
            else
                _currentFormulario.SatoFisico = null;

            _currentFormulario.CaratteristicheChimiche = txtCarattPericolosità.Text.Trim();
            if (rbProvUrb.Checked)
                _currentFormulario.Provenienza = 1; // Urbano
            else if (rbProvSpec.Checked)
                _currentFormulario.Provenienza = 2; // Speciale
            else
                _currentFormulario.Provenienza = null; // Nessuna selezione
            _currentFormulario.Descrizione = txtDescr.Text.Trim();
            if (!string.IsNullOrWhiteSpace(txtQuantita.Text) && decimal.TryParse(txtQuantita.Text, out decimal quantita))
                _currentFormulario.Quantita = quantita;
            else
                _currentFormulario.Quantita = null;
            if (rbKg.Checked)
                _currentFormulario.Kg_Lt = 1; // Kg
            else if (rbLitri.Checked)
                _currentFormulario.Kg_Lt = 2; // Litri
            else
                _currentFormulario.Kg_Lt = null; // Nessuna selezione

            if (ckPesoVerificato.Checked)
                _currentFormulario.PesoVerificato = true;
            else
                _currentFormulario.PesoVerificato = false;

            if (!string.IsNullOrWhiteSpace(txtColli.Text) && decimal.TryParse(txtColli.Text, out decimal numColli))
                _currentFormulario.NumeroColli = (int)numColli;
            else
                _currentFormulario.NumeroColli = null;
            if (ckAllaRinfusa.Checked)
                _currentFormulario.AllaRinfusa = true;
            else
                _currentFormulario.AllaRinfusa = false;

            // NOTA: La proprietà CaratteristicheChimiche viene sovrascritta qui.
            // Controllare se è il comportamento desiderato.
            _currentFormulario.CaratteristicheChimiche = txtChimicoFisiche.Text.Trim();

            try
            {
                if (_currentFormulario.Id == 0) // Nuovo formulario
                {
                    await _formularioRifiutiRepository.AddAsync(_currentFormulario);
                }
                else // Formulario esistente
                {
                    _formularioRifiutiRepository.Update(_currentFormulario);
                }
                await _formularioRifiutiRepository.SaveChangesAsync();
                MessageBox.Show("Formulario salvato con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _isFormularioSaved = true;
                UpdatePrintButtonState();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio del formulario: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isFormularioSaved = false;
                UpdatePrintButtonState();
            }
        }

        private void btnAnnulla_Click(object sender, EventArgs e)
        {
            // Nelle form non modali, basta chiudere la form.
            this.Close();
        }

        #region combobox

        /// <summary>
        /// Carica le ComboBox per Clienti e Automezzi.
        /// </summary>
        private async Task LoadComboBoxes()
        {
            try
            {
                var tuttiClienti = (await _clienteRepository.GetAllAsync()).ToList();

                // Popola Produttore
                cmbProduttore.DataSource = new List<Cliente>(tuttiClienti);
                cmbProduttore.DisplayMember = "RagSoc";
                cmbProduttore.ValueMember = "Id";
                cmbProduttore.SelectedIndex = -1;

                // Popola Destinatario
                cmbDestinatario.DataSource = new List<Cliente>(tuttiClienti);
                cmbDestinatario.DisplayMember = "RagSoc";
                cmbDestinatario.ValueMember = "Id";
                cmbDestinatario.SelectedIndex = -1;

                // Popola Trasportatore
                cmbTrasportatore.DataSource = new List<Cliente>(tuttiClienti);
                cmbTrasportatore.DisplayMember = "RagSoc";
                cmbTrasportatore.ValueMember = "Id";
                cmbTrasportatore.SelectedIndex = -1;

                // Popola Automezzi
                var automezzi = await _automezzoRepository.GetAllAsync();
                cmbAutomezzo.DataSource = automezzi.ToList();
                cmbAutomezzo.DisplayMember = "Descrizione"; // O "Targa" a seconda della preferenza
                cmbAutomezzo.ValueMember = "Id";
                cmbAutomezzo.SelectedIndex = -1; // Nessuna selezione iniziale
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento delle liste: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void cmbProduttore_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isLoading) return;
            await LoadIndirizziAsync(cmbProduttore, cmbProduttoreIndirizzo);
        }

        private async void cmbDestinatario_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isLoading) return;
            await LoadIndirizziAsync(cmbDestinatario, cmbDestinatarioIndirizzo);
        }

        private async void cmbTrasportatore_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isLoading) return;
            await LoadIndirizziAsync(cmbTrasportatore, cmbTrasportatoreIndirizzo);
        }

        private async Task LoadIndirizziAsync(ComboBox ownerCombo, ComboBox addressCombo, int? addressIdToSelect = null)
        {
            addressCombo.DataSource = null;

            if (ownerCombo.SelectedValue is int ownerId && ownerId > 0)
            {
                try
                {
                    var indirizzi = (await _clienteIndirizzoRepository.FindAsync(ci => ci.IdCli == ownerId)).ToList();
                    if (indirizzi.Any())
                    {
                        addressCombo.DataSource = indirizzi;
                        addressCombo.DisplayMember = "IndirizzoCompleto";
                        addressCombo.ValueMember = "Id";

                        int idToSelect;

                        // Priorità 1: ID specifico fornito (e valido)
                        if (addressIdToSelect.HasValue && indirizzi.Any(i => i.Id == addressIdToSelect.Value))
                        {
                            idToSelect = addressIdToSelect.Value;
                        }
                        // Priorità 2: Indirizzo predefinito, altrimenti il primo della lista
                        else
                        {
                            var defaultAddress = indirizzi.FirstOrDefault(ci => ci.Predefinito);
                            idToSelect = defaultAddress?.Id ?? indirizzi.First().Id;
                        }
                        addressCombo.SelectedValue = idToSelect;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante il caricamento degli indirizzi: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion


        #region validazione input

        private void txtQuantita_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string currentText = textBox.Text;
            char keyChar = e.KeyChar;
            string decimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            // 1. Consenti le cifre (0-9)
            if (char.IsDigit(keyChar))
            {
                e.Handled = false;
            }
            // 2. Consenti il tasto Backspace e altri tasti di controllo
            else if (char.IsControl(keyChar))
            {
                e.Handled = false;
            }
            // 3. Consenti un solo separatore decimale (virgola o punto)
            else if (keyChar.ToString() == decimalSeparator)
            {
                // Consenti il separatore solo se non è già presente nella TextBox
                if (currentText.Contains(decimalSeparator))
                {
                    e.Handled = true; // Blocca se il separatore decimale è già presente
                }
                else
                {
                    e.Handled = false; // Consenti il primo separatore decimale
                }
            }
            else
            {
                e.Handled = true; // Blocca qualsiasi carattere non consentito (incluso il segno meno)
            }
        }

        private void txtColli_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow digits (0-9)
            // Allow the Backspace key (char.IsControl(e.KeyChar) handles this and other control characters)
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtStatoFisco_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow digits (0-9)
            // Allow the Backspace key (char.IsControl(e.KeyChar) handles this and other control characters)
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private bool ValidateInput()
        {
            //if (string.IsNullOrWhiteSpace(txtNumeroFormulario.Text))
            //{
            //    MessageBox.Show("Numero Formulario è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    txtNumeroFormulario.Focus();
            //    return false;
            //}
            if (cmbProduttore.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Produttore.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbProduttore.Focus();
                return false;
            }
            if (cmbProduttoreIndirizzo.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Indirizzo del Produttore.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbProduttoreIndirizzo.Focus();
                return false;
            }
            if (cmbDestinatario.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Destinatario.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbDestinatario.Focus();
                return false;
            }
            if (cmbDestinatarioIndirizzo.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Indirizzo del Destinatario.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbDestinatarioIndirizzo.Focus();
                return false;
            }
            if (cmbTrasportatore.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Trasportatore.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbTrasportatore.Focus();
                return false;
            }
            if (cmbTrasportatoreIndirizzo.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Indirizzo del Trasportatore.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbTrasportatoreIndirizzo.Focus();
                return false;
            }
            if (cmbAutomezzo.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Automezzo.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbAutomezzo.Focus();
                return false;
            }
            return true;
        }

        #endregion


        #region stampa PDF

        private void UpdatePrintButtonState()
        {
            btStampa.Enabled = _isFormularioSaved;
        }

        private string getStrValore(string? val)
        {
            return (string.IsNullOrEmpty(val) ? "" : " " + val.Trim());
        }
        private string getIntValore(int? val)
        {
            return (val == 0 ? "" : " " + val.ToString());
        }
        private string getDateValore(DateTime? val)
        {
            return (val.HasValue ? " " + val.Value.ToString("dd/MM/yyyy") : string.Empty);
        }

        private async void btStampa_Click(object sender, EventArgs e)
        {
            // Verifica che _currentFormulario non sia null e che abbia un Id valido
            if (_currentFormulario == null || _currentFormulario.Id == 0)
            {
                MessageBox.Show("Impossibile stampare: il formulario non è stato salvato o è un nuovo formulario senza ID.", "Errore Stampa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var AppConfigData = await _configurazioneRepository.GetAllAsync();
            var conf = AppConfigData.FirstOrDefault();

            if (conf == null)
            {
                MessageBox.Show("Configurazione non trovata. Impossibile generare il PDF.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var clienteP = await _clienteRepository.GetByIdAsync(_currentFormulario.IdProduttore);
            var indirizzoP = await _clienteIndirizzoRepository.GetByIdAsync(_currentFormulario.IdProduttoreIndirizzo);
            var clienteD = await _clienteRepository.GetByIdAsync(_currentFormulario.IdDestinatario);
            var indirizzoD = await _clienteIndirizzoRepository.GetByIdAsync(_currentFormulario.IdDestinatarioIndirizzo);
            var clienteT = await _clienteRepository.GetByIdAsync(_currentFormulario.IdTrasportatore);
            var indirizzoT = await _clienteIndirizzoRepository.GetByIdAsync(_currentFormulario.IdTrasportatoreIndirizzo);
            var mezzo = await _automezzoRepository.GetByIdAsync(_currentFormulario.IdAutomezzo);

            if (clienteP == null || indirizzoP == null || clienteD == null || indirizzoD == null || clienteT == null || indirizzoT == null || mezzo == null)
            {
                MessageBox.Show("Dati correlati (produttore, destinatario, trasportatore, indirizzi o automezzo) mancanti. Impossibile generare il PDF.", "Errore Dati", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Prepara i dati in un dizionario, mappando i nomi dei campi PDF ai valori
            var datiFormulario = new Dictionary<string, string>
           {
                { "Data_Emissione", dtpData.Value.ToString("dd/MM/yyyy") },
                // Produttore
                { "Cli_Rag_Soc", clienteP.RagSoc.Trim() },
                { "Cli_Ind", getStrValore(indirizzoP.Indirizzo) + getIntValore(indirizzoP.Cap) + getStrValore(indirizzoP.Comune) },
                { "Cli_Cod_Fisc", getStrValore(clienteP.CodiceFiscale) },
                { "Cli_Iscrizione_Albo", getStrValore(clienteP.Iscrizione_Albo) },
                { "Cli_Auto_Comunic", getStrValore(clienteP.Auto_Comunicazione) },
                { "Cli_Tipo", getStrValore(clienteP.Tipo) },
                // Destinatario
                { "Dest_Rag_Soc",  clienteD.RagSoc.Trim() },
                { "Dest_Indirizzo", getStrValore(indirizzoD.Indirizzo) + getIntValore(indirizzoD.Cap) + getStrValore(indirizzoD.Comune) },
                { "Dest_Cod_Fisc", getStrValore(clienteD.CodiceFiscale) },
                { "Dest_Iscrizione_Albo", getStrValore(clienteD.Iscrizione_Albo) },
                { "Dest_R", "" }, // Campo da mappare se esiste nel modello Cliente
                { "Dest_D", "" }, // Campo da mappare se esiste nel modello Cliente
                { "Dest_Auto_Comunic", getStrValore(clienteD.Auto_Comunicazione) },
                { "Dest_Tipo1", getStrValore(clienteD.Tipo) },
                //{ "Dest_Tipo2", getStrValore(conf.DestTipo2) },
                // Trasportatore
                { "Trasp_Rag_Soc", clienteT.RagSoc.Trim() },
                { "Trasp_Indirizzo", getStrValore(indirizzoT.Indirizzo) + getIntValore(indirizzoT.Cap) + getStrValore(indirizzoT.Comune) },
                { "Trasp_Cod_Fisc", getStrValore(clienteT.CodiceFiscale) },
                { "Trasp_Iscrizione_Albo", getStrValore(clienteT.Iscrizione_Albo) },
                // Caratteristiche rifiuto
                { "Codice_EER", txtCodiceEER.Text.Trim() },
                { "Stato_Fisico", txtStatoFisco.Text.Trim() },
                { "Urbano", rbProvUrb.Checked ? "X" : string.Empty },
                { "Speciale", rbProvSpec.Checked ? "X" : string.Empty },
                { "Caratt_Pericolosita", txtCarattPericolosità.Text.Trim() },
                { "Descrizione", txtDescr.Text.Trim() },
                { "Quantita", txtQuantita.Text.Trim() },
                { "kg", rbKg.Checked ? "X" : string.Empty},
                { "lt", rbLitri.Checked ? "X" : string.Empty },
                { "Peso_Verificato", ckPesoVerificato.Checked ? "X" : string.Empty },
                { "Colli", txtColli.Text.Trim() },
                { "Rinfusa", ckAllaRinfusa.Checked ? "X" : string.Empty },
                { "Caratteristiche", txtChimicoFisiche.Text.Trim() },
                // Trasporto
                { "Automezzo", mezzo.Targa ?? string.Empty }

           };

            // Definisci i percorsi del template e dell'output
            // Assicurati che questi percorsi siano corretti per il tuo ambiente!
            // Potresti mettere il template nella cartella "Resources" o "Templates" del tuo progetto
            string templatePdfPath = Path.Combine(Application.StartupPath, "Resources", "ModuloFormulario.pdf");
            string outputPdfPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FormulariStampati", $"Formulario_{_currentFormulario.Id}.pdf");

            try
            {
                // Assicurati che la directory di output esista
                string outputDir = Path.GetDirectoryName(outputPdfPath);
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                var filler = new PDFGenerator(templatePdfPath, outputPdfPath);
                bool success = await filler.FillFatturaAsync(datiFormulario);

                if (success)
                {
                    // MessageBox.Show($"Formulario generata con successo in:\n{outputPdfPath}", "Generazione PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(outputPdfPath) { UseShellExecute = true });
                    }
                    catch (Exception openEx)
                    {
                        MessageBox.Show($"Impossibile aprire il file PDF. Errore: {openEx.Message}", "Errore Apertura PDF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Errore durante la stampa del formulario.", "Generazione PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (IOException ioEx)
            {
                string errorMessage = $"Errore di I/O: {ioEx.Message}\n";
                errorMessage += "Assicurati che il file non sia aperto in un altro programma o che il percorso di output sia valido.\n";
                errorMessage += $"Percorso output tentato: {outputPdfPath}\n";
                errorMessage += $"Codice Errore (HResult): {ioEx.HResult}\n";

                if (ioEx.InnerException != null)
                {
                    errorMessage += $"Inner Exception: {ioEx.InnerException.Message}\n";
                    // A volte l'inner exception ha anche un HResult
                    if (ioEx.InnerException is System.Runtime.InteropServices.COMException comEx)
                    {
                        errorMessage += $"Inner COM HResult: {comEx.HResult}\n";
                    }
                }
                MessageBox.Show(errorMessage, "Errore I/O Dettagliato", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Si è verificato un errore inatteso: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion       
    

        // Codice generato dal designer
        #region Windows Form Designer generated code

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblData = new Label();
            dtpData = new DateTimePicker();
            lblProduttore = new Label();
            cmbProduttore = new ComboBox();
            lblProduttoreIndirizzo = new Label();
            cmbProduttoreIndirizzo = new ComboBox();
            lblNumeroFormulario = new Label();
            txtNumeroFormulario = new TextBox();
            lblAutomezzo = new Label();
            cmbAutomezzo = new ComboBox();
            btnSalva = new Button();
            btnAnnulla = new Button();
            grCarattRifiuto = new GroupBox();
            grAspettoEsteriore = new GroupBox();
            txtColli = new TextBox();
            label6 = new Label();
            ckAllaRinfusa = new CheckBox();
            txtChimicoFisiche = new TextBox();
            label7 = new Label();
            ckPesoVerificato = new CheckBox();
            grKgLitri = new GroupBox();
            rbKg = new RadioButton();
            rbLitri = new RadioButton();
            txtQuantita = new TextBox();
            label5 = new Label();
            txtDescr = new TextBox();
            label4 = new Label();
            txtCarattPericolosità = new TextBox();
            label3 = new Label();
            grProvenienza = new GroupBox();
            rbProvUrb = new RadioButton();
            rbProvSpec = new RadioButton();
            txtStatoFisco = new TextBox();
            label2 = new Label();
            txtCodiceEER = new TextBox();
            label1 = new Label();
            btStampa = new Button();
            lblDestinatario = new Label();
            cmbDestinatario = new ComboBox();
            lblDestinatarioIndirizzo = new Label();
            cmbDestinatarioIndirizzo = new ComboBox();
            lblTrasportatore = new Label();
            cmbTrasportatore = new ComboBox();
            lblTrasportatoreIndirizzo = new Label();
            cmbTrasportatoreIndirizzo = new ComboBox();
            panel1 = new Panel();
            grCarattRifiuto.SuspendLayout();
            grAspettoEsteriore.SuspendLayout();
            grKgLitri.SuspendLayout();
            grProvenienza.SuspendLayout();
            SuspendLayout();
            // 
            // lblData
            // 
            lblData.AutoSize = true;
            lblData.Location = new Point(20, 25);
            lblData.Name = "lblData"; // 
            lblData.Size = new Size(34, 15);
            lblData.TabIndex = 0;
            lblData.Text = "Data:";
            // 
            // dtpData
            // 
            dtpData.Format = DateTimePickerFormat.Short;
            dtpData.Location = new Point(140, 22);
            dtpData.Name = "dtpData";
            dtpData.Size = new Size(230, 23);
            dtpData.TabIndex = 1;
            // 
            // lblProduttore
            // 
            lblProduttore.AutoSize = true;
            lblProduttore.Location = new Point(20, 57);
            lblProduttore.Name = "lblProduttore";
            lblProduttore.Size = new Size(66, 15);
            lblProduttore.TabIndex = 2;
            lblProduttore.Text = "Produttore:";
            // 
            // cmbProduttore
            // 
            cmbProduttore.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProduttore.FormattingEnabled = true;
            cmbProduttore.Location = new Point(140, 54);
            cmbProduttore.Name = "cmbProduttore";
            cmbProduttore.Size = new Size(230, 23);
            cmbProduttore.TabIndex = 3;
            // 
            // lblProduttoreIndirizzo
            // 
            lblProduttoreIndirizzo.AutoSize = true;
            lblProduttoreIndirizzo.Location = new Point(489, 57);
            lblProduttoreIndirizzo.Name = "lblProduttoreIndirizzo";
            lblProduttoreIndirizzo.Size = new Size(54, 15);
            lblProduttoreIndirizzo.TabIndex = 4;
            lblProduttoreIndirizzo.Text = "Indirizzo:";
            // 
            // cmbProduttoreIndirizzo
            // 
            cmbProduttoreIndirizzo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProduttoreIndirizzo.FormattingEnabled = true;
            cmbProduttoreIndirizzo.Location = new Point(609, 54);
            cmbProduttoreIndirizzo.Name = "cmbProduttoreIndirizzo";
            cmbProduttoreIndirizzo.Size = new Size(230, 23);
            cmbProduttoreIndirizzo.TabIndex = 5;
            // 
            // lblNumeroFormulario
            // 
            lblNumeroFormulario.AutoSize = true;
            lblNumeroFormulario.Location = new Point(489, 28);
            lblNumeroFormulario.Name = "lblNumeroFormulario";
            lblNumeroFormulario.Size = new Size(115, 15);
            lblNumeroFormulario.TabIndex = 6;
            lblNumeroFormulario.Text = "Numero Formulario:";
            // 
            // txtNumeroFormulario
            // 
            txtNumeroFormulario.Location = new Point(609, 25);
            txtNumeroFormulario.MaxLength = 50;
            txtNumeroFormulario.Name = "txtNumeroFormulario";
            txtNumeroFormulario.Size = new Size(230, 23);
            txtNumeroFormulario.TabIndex = 7;
            // 
            // lblAutomezzo
            // 
            lblAutomezzo.AutoSize = true;
            lblAutomezzo.Location = new Point(20, 145);
            lblAutomezzo.Name = "lblAutomezzo";
            lblAutomezzo.Size = new Size(70, 15);
            lblAutomezzo.TabIndex = 8;
            lblAutomezzo.Text = "Automezzo:";
            // 
            // cmbAutomezzo
            // 
            cmbAutomezzo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAutomezzo.FormattingEnabled = true;
            cmbAutomezzo.Location = new Point(140, 142);
            cmbAutomezzo.Name = "cmbAutomezzo";
            cmbAutomezzo.Size = new Size(230, 23);
            cmbAutomezzo.TabIndex = 9;
            // 
            // btnSalva
            // 
            btnSalva.Location = new Point(786, 434);
            btnSalva.Name = "btnSalva";
            btnSalva.Size = new Size(75, 30);
            btnSalva.TabIndex = 10;
            btnSalva.Text = "Salva";
            btnSalva.UseVisualStyleBackColor = true;
            btnSalva.Click += btnSalva_Click;
            // 
            // btnAnnulla
            // 
            btnAnnulla.Location = new Point(591, 434);
            btnAnnulla.Name = "btnAnnulla";
            btnAnnulla.Size = new Size(75, 30);
            btnAnnulla.TabIndex = 24;
            btnAnnulla.Text = "Annulla";
            btnAnnulla.UseVisualStyleBackColor = true;
            btnAnnulla.Click += btnAnnulla_Click;
            // 
            // grCarattRifiuto
            // 
            grCarattRifiuto.Controls.Add(grAspettoEsteriore);
            grCarattRifiuto.Controls.Add(txtChimicoFisiche);
            grCarattRifiuto.Controls.Add(label7);
            grCarattRifiuto.Controls.Add(ckPesoVerificato);
            grCarattRifiuto.Controls.Add(grKgLitri);
            grCarattRifiuto.Controls.Add(txtQuantita);
            grCarattRifiuto.Controls.Add(label5);
            grCarattRifiuto.Controls.Add(txtDescr);
            grCarattRifiuto.Controls.Add(label4);
            grCarattRifiuto.Controls.Add(txtCarattPericolosità);
            grCarattRifiuto.Controls.Add(label3);
            grCarattRifiuto.Controls.Add(grProvenienza);
            grCarattRifiuto.Controls.Add(txtStatoFisco);
            grCarattRifiuto.Controls.Add(label2);
            grCarattRifiuto.Controls.Add(txtCodiceEER);
            grCarattRifiuto.Controls.Add(label1);
            grCarattRifiuto.Location = new Point(20, 181);
            grCarattRifiuto.Name = "grCarattRifiuto";
            grCarattRifiuto.Size = new Size(841, 238);
            grCarattRifiuto.TabIndex = 33;
            grCarattRifiuto.TabStop = false;
            grCarattRifiuto.Text = "Caratteristiche del rifiuto";
            // 
            // grAspettoEsteriore
            // 
            grAspettoEsteriore.Controls.Add(txtColli);
            grAspettoEsteriore.Controls.Add(label6);
            grAspettoEsteriore.Controls.Add(ckAllaRinfusa);
            grAspettoEsteriore.Location = new Point(572, 131);
            grAspettoEsteriore.Name = "grAspettoEsteriore";
            grAspettoEsteriore.Size = new Size(249, 48);
            grAspettoEsteriore.TabIndex = 48;
            grAspettoEsteriore.TabStop = false;
            grAspettoEsteriore.Text = "Aspetto esteriore";
            // 
            // txtColli
            // 
            txtColli.Location = new Point(75, 17);
            txtColli.MaxLength = 5;
            txtColli.Name = "txtColli";
            txtColli.Size = new Size(76, 23);
            txtColli.TabIndex = 28;
            txtColli.KeyPress += txtColli_KeyPress;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(11, 20);
            label6.Name = "label6";
            label6.Size = new Size(53, 15);
            label6.TabIndex = 27;
            label6.Text = "Nr. Colli:";
            // 
            // ckAllaRinfusa
            // 
            ckAllaRinfusa.AutoSize = true;
            ckAllaRinfusa.Location = new Point(157, 20);
            ckAllaRinfusa.Name = "ckAllaRinfusa";
            ckAllaRinfusa.Size = new Size(88, 19);
            ckAllaRinfusa.TabIndex = 29;
            ckAllaRinfusa.Text = "Alla Rinfusa";
            ckAllaRinfusa.UseVisualStyleBackColor = true;
            // 
            // txtChimicoFisiche
            // 
            txtChimicoFisiche.Location = new Point(138, 189);
            txtChimicoFisiche.MaxLength = 25;
            txtChimicoFisiche.Name = "txtChimicoFisiche";
            txtChimicoFisiche.Size = new Size(683, 23);
            txtChimicoFisiche.TabIndex = 47;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(18, 192);
            label7.Name = "label7";
            label7.Size = new Size(111, 15);
            label7.TabIndex = 46;
            label7.Text = "Caratt. Chim.-Fisic.:";
            // 
            // ckPesoVerificato
            // 
            ckPesoVerificato.AutoSize = true;
            ckPesoVerificato.Location = new Point(454, 150);
            ckPesoVerificato.Name = "ckPesoVerificato";
            ckPesoVerificato.Size = new Size(103, 19);
            ckPesoVerificato.TabIndex = 45;
            ckPesoVerificato.Text = "Peso verificato";
            ckPesoVerificato.UseVisualStyleBackColor = true;
            // 
            // grKgLitri
            // 
            grKgLitri.Controls.Add(rbKg);
            grKgLitri.Controls.Add(rbLitri);
            grKgLitri.Location = new Point(311, 136);
            grKgLitri.Name = "grKgLitri";
            grKgLitri.Size = new Size(122, 38);
            grKgLitri.TabIndex = 44;
            grKgLitri.TabStop = false;
            // 
            // rbKg
            // 
            rbKg.AutoSize = true;
            rbKg.Location = new Point(19, 13);
            rbKg.Name = "rbKg";
            rbKg.Size = new Size(39, 19);
            rbKg.TabIndex = 15;
            rbKg.TabStop = true;
            rbKg.Text = "Kg";
            rbKg.UseVisualStyleBackColor = true;
            // 
            // rbLitri
            // 
            rbLitri.AutoSize = true;
            rbLitri.Location = new Point(64, 14);
            rbLitri.Name = "rbLitri";
            rbLitri.Size = new Size(45, 19);
            rbLitri.TabIndex = 16;
            rbLitri.TabStop = true;
            rbLitri.Text = "Litri";
            rbLitri.UseVisualStyleBackColor = true;
            // 
            // txtQuantita
            // 
            txtQuantita.Location = new Point(138, 149);
            txtQuantita.MaxLength = 12;
            txtQuantita.Name = "txtQuantita";
            txtQuantita.Size = new Size(163, 23);
            txtQuantita.TabIndex = 43;
            txtQuantita.KeyPress += txtQuantita_KeyPress;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(18, 152);
            label5.Name = "label5";
            label5.Size = new Size(56, 15);
            label5.TabIndex = 42;
            label5.Text = "Quantità:";
            // 
            // txtDescr
            // 
            txtDescr.Location = new Point(138, 106);
            txtDescr.MaxLength = 50;
            txtDescr.Name = "txtDescr";
            txtDescr.Size = new Size(683, 23);
            txtDescr.TabIndex = 41;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(18, 109);
            label4.Name = "label4";
            label4.Size = new Size(70, 15);
            label4.TabIndex = 40;
            label4.Text = "Descrizione:";
            // 
            // txtCarattPericolosità
            // 
            txtCarattPericolosità.Location = new Point(591, 69);
            txtCarattPericolosità.MaxLength = 25;
            txtCarattPericolosità.Name = "txtCarattPericolosità";
            txtCarattPericolosità.Size = new Size(230, 23);
            txtCarattPericolosità.TabIndex = 39;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(471, 72);
            label3.Name = "label3";
            label3.Size = new Size(109, 15);
            label3.TabIndex = 38;
            label3.Text = "Caratt. Pericolosità:";
            // 
            // grProvenienza
            // 
            grProvenienza.Controls.Add(rbProvUrb);
            grProvenienza.Controls.Add(rbProvSpec);
            grProvenienza.Location = new Point(591, 21);
            grProvenienza.Name = "grProvenienza";
            grProvenienza.Size = new Size(228, 42);
            grProvenienza.TabIndex = 37;
            grProvenienza.TabStop = false;
            grProvenienza.Text = "Provenienza";
            // 
            // rbProvUrb
            // 
            rbProvUrb.AutoSize = true;
            rbProvUrb.Location = new Point(19, 15);
            rbProvUrb.Name = "rbProvUrb";
            rbProvUrb.Size = new Size(64, 19);
            rbProvUrb.TabIndex = 15;
            rbProvUrb.TabStop = true;
            rbProvUrb.Text = "Urbano";
            rbProvUrb.UseVisualStyleBackColor = true;
            // 
            // rbProvSpec
            // 
            rbProvSpec.AutoSize = true;
            rbProvSpec.Location = new Point(89, 15);
            rbProvSpec.Name = "rbProvSpec";
            rbProvSpec.Size = new Size(68, 19);
            rbProvSpec.TabIndex = 16;
            rbProvSpec.TabStop = true;
            rbProvSpec.Text = "Speciale";
            rbProvSpec.UseVisualStyleBackColor = true;
            // 
            // txtStatoFisco
            // 
            txtStatoFisco.Location = new Point(357, 69);
            txtStatoFisco.MaxLength = 3;
            txtStatoFisco.Name = "txtStatoFisco";
            txtStatoFisco.Size = new Size(76, 23);
            txtStatoFisco.TabIndex = 36;
            txtStatoFisco.KeyPress += txtStatoFisco_KeyPress;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(269, 72);
            label2.Name = "label2";
            label2.Size = new Size(70, 15);
            label2.TabIndex = 35;
            label2.Text = "Stato Fisico:";
            // 
            // txtCodiceEER
            // 
            txtCodiceEER.Location = new Point(138, 69);
            txtCodiceEER.MaxLength = 10;
            txtCodiceEER.Name = "txtCodiceEER";
            txtCodiceEER.Size = new Size(115, 23);
            txtCodiceEER.TabIndex = 34;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(18, 72);
            label1.Name = "label1";
            label1.Size = new Size(69, 15);
            label1.TabIndex = 33;
            label1.Text = "Codice EER:";
            // 
            // btStampa
            // 
            btStampa.Enabled = false;
            btStampa.Location = new Point(688, 434);
            btStampa.Name = "btStampa";
            btStampa.Size = new Size(75, 30);
            btStampa.TabIndex = 34;
            btStampa.Text = "Stampa";
            btStampa.UseVisualStyleBackColor = true;
            btStampa.Click += btStampa_Click;
            // 
            // lblDestinatario
            // 
            lblDestinatario.AutoSize = true;
            lblDestinatario.Location = new Point(20, 86);
            lblDestinatario.Name = "lblDestinatario";
            lblDestinatario.Size = new Size(73, 15);
            lblDestinatario.TabIndex = 35;
            lblDestinatario.Text = "Destinatario:";
            // 
            // cmbDestinatario
            // 
            cmbDestinatario.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDestinatario.FormattingEnabled = true;
            cmbDestinatario.Location = new Point(140, 83);
            cmbDestinatario.Name = "cmbDestinatario";
            cmbDestinatario.Size = new Size(230, 23);
            cmbDestinatario.TabIndex = 36;
            // 
            // lblDestinatarioIndirizzo
            // 
            lblDestinatarioIndirizzo.AutoSize = true;
            lblDestinatarioIndirizzo.Location = new Point(489, 86);
            lblDestinatarioIndirizzo.Name = "lblDestinatarioIndirizzo";
            lblDestinatarioIndirizzo.Size = new Size(54, 15);
            lblDestinatarioIndirizzo.TabIndex = 37;
            lblDestinatarioIndirizzo.Text = "Indirizzo:";
            // 
            // cmbDestinatarioIndirizzo
            // 
            cmbDestinatarioIndirizzo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDestinatarioIndirizzo.FormattingEnabled = true;
            cmbDestinatarioIndirizzo.Location = new Point(609, 83);
            cmbDestinatarioIndirizzo.Name = "cmbDestinatarioIndirizzo";
            cmbDestinatarioIndirizzo.Size = new Size(230, 23);
            cmbDestinatarioIndirizzo.TabIndex = 38;
            // 
            // lblTrasportatore
            // 
            lblTrasportatore.AutoSize = true;
            lblTrasportatore.Location = new Point(20, 115);
            lblTrasportatore.Name = "lblTrasportatore";
            lblTrasportatore.Size = new Size(79, 15);
            lblTrasportatore.TabIndex = 39;
            lblTrasportatore.Text = "Trasportatore:";
            // 
            // cmbTrasportatore
            // 
            cmbTrasportatore.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTrasportatore.FormattingEnabled = true;
            cmbTrasportatore.Location = new Point(140, 112);
            cmbTrasportatore.Name = "cmbTrasportatore";
            cmbTrasportatore.Size = new Size(230, 23);
            cmbTrasportatore.TabIndex = 40;
            // 
            // lblTrasportatoreIndirizzo
            // 
            lblTrasportatoreIndirizzo.AutoSize = true;
            lblTrasportatoreIndirizzo.Location = new Point(489, 115);
            lblTrasportatoreIndirizzo.Name = "lblTrasportatoreIndirizzo";
            lblTrasportatoreIndirizzo.Size = new Size(54, 15);
            lblTrasportatoreIndirizzo.TabIndex = 41;
            lblTrasportatoreIndirizzo.Text = "Indirizzo:";
            // 
            // cmbTrasportatoreIndirizzo
            // 
            cmbTrasportatoreIndirizzo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTrasportatoreIndirizzo.FormattingEnabled = true;
            cmbTrasportatoreIndirizzo.Location = new Point(609, 112);
            cmbTrasportatoreIndirizzo.Name = "cmbTrasportatoreIndirizzo";
            cmbTrasportatoreIndirizzo.Size = new Size(230, 23);
            cmbTrasportatoreIndirizzo.TabIndex = 42;
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.Fixed3D;
            panel1.Location = new Point(20, 171);
            panel1.Name = "panel1";
            panel1.Size = new Size(841, 4);
            panel1.TabIndex = 43;
            // 
            // FormulariRifiutiDetailForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(893, 488);
            Controls.Add(panel1);
            Controls.Add(cmbTrasportatoreIndirizzo);
            Controls.Add(lblTrasportatoreIndirizzo);
            Controls.Add(cmbTrasportatore);
            Controls.Add(lblTrasportatore);
            Controls.Add(cmbDestinatarioIndirizzo);
            Controls.Add(lblDestinatarioIndirizzo);
            Controls.Add(cmbDestinatario);
            Controls.Add(lblDestinatario);
            Controls.Add(btStampa);
            Controls.Add(grCarattRifiuto);
            Controls.Add(btnAnnulla);
            Controls.Add(btnSalva);
            Controls.Add(cmbAutomezzo);
            Controls.Add(lblAutomezzo);
            Controls.Add(txtNumeroFormulario);
            Controls.Add(lblNumeroFormulario);
            Controls.Add(cmbProduttoreIndirizzo);
            Controls.Add(lblProduttoreIndirizzo);
            Controls.Add(cmbProduttore);
            Controls.Add(lblProduttore);
            Controls.Add(dtpData);
            Controls.Add(lblData);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormulariRifiutiDetailForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Dettagli Formulario Rifiuti";
            grCarattRifiuto.ResumeLayout(false);
            grCarattRifiuto.PerformLayout();
            grAspettoEsteriore.ResumeLayout(false);
            grAspettoEsteriore.PerformLayout();
            grKgLitri.ResumeLayout(false);
            grKgLitri.PerformLayout();
            grProvenienza.ResumeLayout(false);
            grProvenienza.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        private System.Windows.Forms.Label lblData;
        private System.Windows.Forms.DateTimePicker dtpData;
        private System.Windows.Forms.Label lblProduttore;
        private System.Windows.Forms.ComboBox cmbProduttore;
        private System.Windows.Forms.Label lblProduttoreIndirizzo;
        private System.Windows.Forms.ComboBox cmbProduttoreIndirizzo;
        private System.Windows.Forms.Label lblNumeroFormulario;
        private System.Windows.Forms.TextBox txtNumeroFormulario;
        private System.Windows.Forms.Label lblAutomezzo;
        private System.Windows.Forms.ComboBox cmbAutomezzo;
        private System.Windows.Forms.Button btnSalva;
        private System.Windows.Forms.Button btnAnnulla;
        private System.Windows.Forms.GroupBox grCarattRifiuto;
        private System.Windows.Forms.GroupBox grAspettoEsteriore;
        private System.Windows.Forms.TextBox txtColli;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox ckAllaRinfusa;
        private System.Windows.Forms.TextBox txtChimicoFisiche;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox ckPesoVerificato;
        private System.Windows.Forms.GroupBox grKgLitri;
        private System.Windows.Forms.RadioButton rbKg;
        private System.Windows.Forms.RadioButton rbLitri;
        private System.Windows.Forms.TextBox txtQuantita;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtDescr;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtCarattPericolosità;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox grProvenienza;
        private System.Windows.Forms.RadioButton rbProvUrb;
        private System.Windows.Forms.RadioButton rbProvSpec;
        private System.Windows.Forms.TextBox txtStatoFisco;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtCodiceEER;
        private System.Windows.Forms.Label label1;
        private Button btStampa;
        private Label lblDestinatario;
        private ComboBox cmbDestinatario;
        private Label lblDestinatarioIndirizzo;
        private ComboBox cmbDestinatarioIndirizzo;
        private Label lblTrasportatore;
        private ComboBox cmbTrasportatore;
        private Label lblTrasportatoreIndirizzo;
        private ComboBox cmbTrasportatoreIndirizzo;
        private Panel panel1;



        #endregion

        
    }
}

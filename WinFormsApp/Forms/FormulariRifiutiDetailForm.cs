// File: Forms/FormulariRifiutiDetailForm.cs
// Questo form permette di inserire o modificare un singolo formulario rifiuti.
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using FormulariRif_G.Utils;
using Microsoft.EntityFrameworkCore; // Per l'include delle proprietà di navigazione
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Drawing.Text;
using System.Linq;
using System.Globalization;

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
            cmbCliente.SelectedIndexChanged += cmbCliente_SelectedIndexChanged;
        }

        private async void FormulariRifiutiDetailForm_Load(object? sender, EventArgs e)
        {
            await LoadComboBoxes();
            LoadFormularioData();
        }

        /// <summary>
        /// Imposta il formulario da visualizzare o modificare.
        /// </summary>
        /// <param name="formulario">L'oggetto FormularioRifiuti.</param>
        public void SetFormulario(FormularioRifiuti formulario)
        {
            // La data viene impostata nel LoadFormularioData dopo che le combobox sono caricate
            _currentFormulario = formulario;
            _isFormularioSaved = (_currentFormulario != null && _currentFormulario.Id != 0);
        }

        /// <summary>
        /// Carica i dati del formulario nei controlli del form.
        /// </summary>
        private void LoadFormularioData()
        {
            if (_currentFormulario != null)
            {
                dtpData.Value = _currentFormulario.Data;
                txtNumeroFormulario.Text = _currentFormulario.NumeroFormulario;
                // Seleziona il cliente nella ComboBox
                // Questo triggererà cmbCliente_SelectedIndexChanged che caricherà gli indirizzi
                // e selezionerà l'indirizzo predefinito o quello del formulario.
                cmbCliente.SelectedValue = _currentFormulario.IdCli;
                // Dopo che cmbCliente_SelectedIndexChanged ha caricato gli indirizzi,
                // dobbiamo selezionare l'indirizzo specifico del formulario.
                if (_currentFormulario.IdClienteIndirizzo != 0)
                    cmbIndirizzo.SelectedValue = _currentFormulario.IdClienteIndirizzo;
                // Seleziona l'automezzo nella ComboBox
                cmbAutomezzo.SelectedValue = _currentFormulario.IdAutomezzo;
                //
                //Caratteristiche del rifiuto
                txtCodiceEER.Text = _currentFormulario.CodiceEER ?? string.Empty;
                txtStatoFisco.Text = _currentFormulario.SatoFisico ?? string.Empty;
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
                cmbCliente.SelectedIndex = -1;
                cmbIndirizzo.SelectedIndex = -1;
                cmbAutomezzo.SelectedIndex = -1;
                //
                //Caratteristiche del rifiuto
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
            if (cmbCliente.SelectedValue == null || cmbIndirizzo.SelectedValue == null || cmbAutomezzo.SelectedValue == null)
            {
                MessageBox.Show("Seleziona Cliente, Indirizzo e Automezzo.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _currentFormulario.IdCli = (int)cmbCliente.SelectedValue;
            _currentFormulario.IdClienteIndirizzo = (int)cmbIndirizzo.SelectedValue;
            _currentFormulario.IdAutomezzo = (int)cmbAutomezzo.SelectedValue;
            //
            //Caratteristiche del rifiuto
            _currentFormulario.CodiceEER = txtCodiceEER.Text.Trim();
            _currentFormulario.SatoFisico = txtStatoFisco.Text.Trim();
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
                this.DialogResult = DialogResult.OK;
                _isFormularioSaved = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio del formulario: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isFormularioSaved = false;
                UpdatePrintButtonState();
            }
        }

        #region combobox

        /// <summary>
        /// Carica le ComboBox per Clienti e Automezzi.
        /// </summary>
        private async Task LoadComboBoxes()
        {
            try
            {
                var clienti = await _clienteRepository.GetAllAsync();
                cmbCliente.DataSource = clienti.ToList();
                cmbCliente.DisplayMember = "RagSoc";
                cmbCliente.ValueMember = "Id";
                cmbCliente.SelectedIndex = -1; // Nessuna selezione iniziale

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

        /// <summary>
        /// Gestisce la selezione di un cliente nella ComboBox.
        /// Carica gli indirizzi associati al cliente selezionato e imposta il predefinito.
        /// </summary>
        private async void cmbCliente_SelectedIndexChanged(object? sender, EventArgs e)
        {
            cmbIndirizzo.DataSource = null; // Pulisci la ComboBox degli indirizzi

            if (cmbCliente.SelectedValue != null && cmbCliente.SelectedValue is int clienteId)
            {
                try
                {
                    var indirizzi = await _clienteIndirizzoRepository.FindAsync(ci => ci.IdCli == clienteId);
                    cmbIndirizzo.DataSource = indirizzi.ToList();
                    cmbIndirizzo.DisplayMember = "Indirizzo"; // Puoi personalizzare la visualizzazione (es. "Indirizzo, Comune")
                    cmbIndirizzo.ValueMember = "Id";

                    // Seleziona l'indirizzo predefinito se esiste
                    var defaultAddress = indirizzi.FirstOrDefault(ci => ci.Predefinito);
                    if (defaultAddress != null)
                    {
                        cmbIndirizzo.SelectedValue = defaultAddress.Id;
                    }
                    else if (indirizzi.Any())
                    {
                        cmbIndirizzo.SelectedIndex = 0;
                    }
                    else
                    {
                        MessageBox.Show("Il cliente selezionato non ha indirizzi registrati. Si prega di aggiungere un indirizzo prima di creare un formulario.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante il caricamento degli indirizzi del cliente: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                
        private bool ValidateInput()
        {
            //if (string.IsNullOrWhiteSpace(txtNumeroFormulario.Text))
            //{
            //    MessageBox.Show("Numero Formulario è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    txtNumeroFormulario.Focus();
            //    return false;
            //}
            if (cmbCliente.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Cliente.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCliente.Focus();
                return false;
            }
            if (cmbIndirizzo.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Indirizzo del Cliente.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbIndirizzo.Focus();
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

        private async void btStampa_Click(object sender, EventArgs e)
        {
            var AppConfigData = await _configurazioneRepository.GetAllAsync();
            var conf = AppConfigData.FirstOrDefault();

            var cliente = await _clienteRepository.GetByIdAsync(_currentFormulario.IdCli);
            var indirizzo = await _clienteIndirizzoRepository.GetByIdAsync(_currentFormulario.IdClienteIndirizzo);

            // Prepara i dati in un dizionario, mappando i nomi dei campi PDF ai valori
            var datiFormulario = new Dictionary<string, string>
           {
                { "Data_Emissione", dtpData.Value.ToString("dd/MM/yyyy") },
                { "Cli_Rag_Soc", cliente.RagSoc.Trim() },
                { "Cli_Ind", getStrValore(indirizzo.Indirizzo) + getIntValore(indirizzo.Cap) + getStrValore(indirizzo.Comune) },
                { "Cli_Cod_Fisc", getStrValore(cliente.CodiceFiscale) },
                { "Cli_Iscrizione_Albo", "" },
                { "Cli_Auto_Comunic", "" },
                { "Cli_Tipo", "" },
                { "Dest_Rag_Soc",  conf.RagSoc1 + getStrValore(conf.RagSoc2)},
                { "Dest_Indirizzo", getStrValore(conf.Indirizzo) +getIntValore(conf.Cap) + getStrValore(conf.Comune) },
                { "Dest_Cod_Fisc", getStrValore(conf.CodiceFiscale) },
                { "Dest_Iscrizione_Albo", getStrValore(conf.DestNumeroIscrizioneAlbo) },
                { "Dest_R", getStrValore(conf.DestR) },
                { "Dest_D", getStrValore(conf.DestD) },
                { "Dest_Auto_Comunic", getStrValore(conf.DestAutoComunic) },
                { "Dest_Tipo1", getStrValore(conf.DestTipo1) },
                { "Dest_Tipo2", getStrValore(conf.DestTipo2) },
                { "Automezzo", cmbAutomezzo.SelectedItem?.ToString() ?? string.Empty },
                { "Codice_EER", txtCodiceEER.Text.Trim() },
                { "Stato_Fisico", txtStatoFisco.Text.Trim() },
                { "Provenienza", rbProvUrb.Checked ? "Urbano" : rbProvSpec.Checked ? "Speciale" : string.Empty },
                { "Caratt_Pericolosita", txtCarattPericolosità.Text.Trim() },
                { "Descrizione", txtDescr.Text.Trim() },
                { "Quantita", txtQuantita.Text.Trim() }
           };

            // Definisci i percorsi del template e dell'output
            // Assicurati che questi percorsi siano corretti per il tuo ambiente!
            // Potresti mettere il template nella cartella "Resources" o "Templates" del tuo progetto
            //string templatePdfPath = Path.Combine(Application.StartupPath, "Resources", "TemplateFormulario.pdf");
            string templatePdfPath = Path.Combine(Application.StartupPath, "Resources", "ModuloFormulario.pdf");
            string outputPdfPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FormulariStampati", $"Formulario_{_currentFormulario.Id}.pdf");

            try
            {
                var filler = new PDFGenerator(templatePdfPath, outputPdfPath);
                bool success = await filler.FillFatturaAsync(datiFormulario);

                if (success)
                {
                    // MessageBox.Show($"Formulario generata con successo in:\n{outputPdfPath}", "Generazione PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);                    
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(outputPdfPath) { UseShellExecute = true });
                    }
                    catch(Exception openEx)
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
            lblCliente = new Label();
            cmbCliente = new ComboBox();
            lblIndirizzo = new Label();
            cmbIndirizzo = new ComboBox();
            lblNumeroFormulario = new Label();
            txtNumeroFormulario = new TextBox();
            lblAutomezzo = new Label();
            cmbAutomezzo = new ComboBox();
            btnSalva = new Button();
            btStampa = new Button();
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
            lblData.Name = "lblData";
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
            // lblCliente
            // 
            lblCliente.AutoSize = true;
            lblCliente.Location = new Point(20, 57);
            lblCliente.Name = "lblCliente";
            lblCliente.Size = new Size(47, 15);
            lblCliente.TabIndex = 2;
            lblCliente.Text = "Cliente:";
            // 
            // cmbCliente
            // 
            cmbCliente.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCliente.FormattingEnabled = true;
            cmbCliente.Location = new Point(140, 51);
            cmbCliente.Name = "cmbCliente";
            cmbCliente.Size = new Size(230, 23);
            cmbCliente.TabIndex = 3;
            // 
            // lblIndirizzo
            // 
            lblIndirizzo.AutoSize = true;
            lblIndirizzo.Location = new Point(489, 60);
            lblIndirizzo.Name = "lblIndirizzo";
            lblIndirizzo.Size = new Size(54, 15);
            lblIndirizzo.TabIndex = 4;
            lblIndirizzo.Text = "Indirizzo:";
            // 
            // cmbIndirizzo
            // 
            cmbIndirizzo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbIndirizzo.FormattingEnabled = true;
            cmbIndirizzo.Location = new Point(609, 57);
            cmbIndirizzo.Name = "cmbIndirizzo";
            cmbIndirizzo.Size = new Size(230, 23);
            cmbIndirizzo.TabIndex = 5;
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
            lblAutomezzo.Location = new Point(20, 83);
            lblAutomezzo.Name = "lblAutomezzo";
            lblAutomezzo.Size = new Size(70, 15);
            lblAutomezzo.TabIndex = 8;
            lblAutomezzo.Text = "Automezzo:";
            // 
            // cmbAutomezzo
            // 
            cmbAutomezzo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAutomezzo.FormattingEnabled = true;
            cmbAutomezzo.Location = new Point(140, 80);
            cmbAutomezzo.Name = "cmbAutomezzo";
            cmbAutomezzo.Size = new Size(230, 23);
            cmbAutomezzo.TabIndex = 9;
            // 
            // btnSalva
            // 
            btnSalva.Location = new Point(786, 390);
            btnSalva.Name = "btnSalva";
            btnSalva.Size = new Size(75, 30);
            btnSalva.TabIndex = 10;
            btnSalva.Text = "Salva";
            btnSalva.UseVisualStyleBackColor = true;
            btnSalva.Click += btnSalva_Click;
            // 
            // btStampa
            // 
            btStampa.Enabled = false;
            btStampa.Location = new Point(675, 390);
            btStampa.Name = "btStampa";
            btStampa.Size = new Size(75, 30);
            btStampa.TabIndex = 24;
            btStampa.Text = "Stampa";
            btStampa.UseVisualStyleBackColor = true;
            btStampa.Click += btStampa_Click;
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
            grCarattRifiuto.Location = new Point(20, 127);
            grCarattRifiuto.Name = "grCarattRifiuto";
            grCarattRifiuto.Size = new Size(841, 233);
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
            // FormulariRifiutiDetailForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(893, 457);
            Controls.Add(grCarattRifiuto);
            Controls.Add(btStampa);
            Controls.Add(btnSalva);
            Controls.Add(cmbAutomezzo);
            Controls.Add(lblAutomezzo);
            Controls.Add(txtNumeroFormulario);
            Controls.Add(lblNumeroFormulario);
            Controls.Add(cmbIndirizzo);
            Controls.Add(lblIndirizzo);
            Controls.Add(cmbCliente);
            Controls.Add(lblCliente);
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
        private System.Windows.Forms.Label lblCliente;
        private System.Windows.Forms.ComboBox cmbCliente;
        private System.Windows.Forms.Label lblIndirizzo;
        private System.Windows.Forms.ComboBox cmbIndirizzo;
        private System.Windows.Forms.Label lblNumeroFormulario;
        private System.Windows.Forms.TextBox txtNumeroFormulario;
        private System.Windows.Forms.Label lblAutomezzo;
        private System.Windows.Forms.ComboBox cmbAutomezzo;
        private System.Windows.Forms.Button btnSalva;
        private System.Windows.Forms.Button btStampa;
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


        #endregion

        
    }
}

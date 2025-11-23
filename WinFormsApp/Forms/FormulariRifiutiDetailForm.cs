// File: Forms/FormulariRifiutiDetailForm.cs
// Questo form permette di inserire o modificare un singolo formulario rifiuti.
using FormulariRif_G.Controls;
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using FormulariRif_G.Utils;
using Microsoft.EntityFrameworkCore; // Per l'include delle proprietà di navigazione
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private bool _isLoading = true; // Inizializzato a true
        // Flag per evitare che gli eventi di ricerca si attivino durante aggiornamenti programmatici
        private bool _isFormularioSaved = false;
        private List<Cliente> _allClienti;
        private CheckBox ckDetentoreR;
        private ComboBox cmbDestD;
        private ComboBox cmbDestR;
        private Label label9;
        private Label label8;
        //private Label label8;
        private List<Automezzo> _allAutomezzi;
        private readonly IGenericRepository<Rimorchio> _rimorchioRepository;
        private readonly IGenericRepository<Conducente> _conducenteRepository;
        private SearchableComboBox scbRimorchio;
        private Label lbProduttore;
        private Label label10;
        private Label label11;
        private Label label12;
        private Label label13;
        private Label label14;
        private Label label15;
        private Label label16;
        private Label label17;
        private Label label18;
        private Label label19;
        private Label label20;
        private SearchableComboBox scbConducente;
        public FormulariRifiutiDetailForm(IGenericRepository<FormularioRifiuti> formularioRifiutiRepository,
                                         IGenericRepository<Cliente> clienteRepository,
                                         IGenericRepository<ClienteIndirizzo> clienteIndirizzoRepository,
                                         IGenericRepository<Automezzo> automezzoRepository,
                                         IGenericRepository<Rimorchio> rimorchioRepository,
                                         IGenericRepository<Conducente> conducenteRepository,
                                         IGenericRepository<Configurazione> configurazioneRepository)
        {
            InitializeComponent();
            _formularioRifiutiRepository = formularioRifiutiRepository;
            _clienteRepository = clienteRepository;
            _clienteIndirizzoRepository = clienteIndirizzoRepository;
            _automezzoRepository = automezzoRepository;
            scbProduttore.SelectedIndexChanged += scbProduttore_SelectedIndexChanged;
            scbDestinatario.SelectedIndexChanged += scbDestinatario_SelectedIndexChanged;
            scbTrasportatore.SelectedIndexChanged += scbTrasportatore_SelectedIndexChanged;
        }


        /// <summary>
        /// Imposta il formulario da visualizzare o modificare.
        /// </summary>
        /// <param name="formulario">L'oggetto FormularioRifiuti.</param>
        public async void SetFormulario(FormularioRifiuti formulario)
        {
            _currentFormulario = formulario;

            _isLoading = true;
            await LoadComboBoxes();
            await LoadFormularioData();
            _isLoading = false;

            this.Text = _currentFormulario.Id == 0 ? "Nuovo Formulario Rifiuti" : "Modifica Formulario Rifiuti";
            // La logica _isFormularioSaved ora riflette se il formulario esiste già nel DB (non è nuovo)
            _isFormularioSaved = (_currentFormulario != null && _currentFormulario.Id != 0);
        }

        /// <summary>
        /// Carica i dati del formulario nei controlli del form.
        /// </summary>
        /// <summary>
        /// Carica i dati del formulario nei controlli del form.
        /// </summary>
        private async Task LoadFormularioData()
        {
            if (_currentFormulario != null)
            {
                dtpData.Value = _currentFormulario.Data;
                txtNumeroFormulario.Text = _currentFormulario.NumeroFormulario;

                scbProduttore.SelectedValue = _currentFormulario.IdProduttore;
                await LoadIndirizziAsync(scbProduttore, cmbProduttoreIndirizzo, _currentFormulario.IdProduttoreIndirizzo);

                scbDestinatario.SelectedValue = _currentFormulario.IdDestinatario;
                await LoadIndirizziAsync(scbDestinatario, cmbDestinatarioIndirizzo, _currentFormulario.IdDestinatarioIndirizzo);

                scbTrasportatore.SelectedValue = _currentFormulario.IdTrasportatore;
                await LoadIndirizziAsync(scbTrasportatore, cmbTrasportatoreIndirizzo, _currentFormulario.IdTrasportatoreIndirizzo);

                // Imposta l'automezzo e carica le liste dipendenti
                scbAutomezzo.SelectedValue = _currentFormulario.IdAutomezzo;
                await LoadRimorchiAndConducentiAsync();

                // Imposta i valori selezionati per Rimorchio e Conducente
                if (_currentFormulario.IdRimorchio.HasValue)
                    scbRimorchio.SelectedValue = _currentFormulario.IdRimorchio.Value;
                if (_currentFormulario.IdConducente.HasValue)
                    scbConducente.SelectedValue = _currentFormulario.IdConducente.Value;

                // Caratteristiche del rifiuto
                txtCodiceEER.Text = _currentFormulario.CodiceEER ?? string.Empty;
                txtStatoFisco.Text = _currentFormulario.SatoFisico ?? string.Empty;

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
                if (_currentFormulario.Detentore_R.HasValue)
                    ckDetentoreR.Checked = _currentFormulario.Detentore_R.Value;
                else
                    ckDetentoreR.Checked = false;
                if (_currentFormulario.NumeroColli.HasValue)
                    txtColli.Text = _currentFormulario.NumeroColli.Value.ToString();
                else
                    txtColli.Text = string.Empty;
                if (_currentFormulario.AllaRinfusa.HasValue)
                    ckAllaRinfusa.Checked = _currentFormulario.AllaRinfusa.Value;
                else
                    ckAllaRinfusa.Checked = false;
                txtChimicoFisiche.Text = _currentFormulario.CaratteristicheChimiche ?? string.Empty;

                if (_currentFormulario.Detentore_R.HasValue)
                    ckDetentoreR.Checked = _currentFormulario.Detentore_R.Value;
                else
                    ckDetentoreR.Checked = false;

                _isFormularioSaved = (_currentFormulario.Id != 0);
            }
            else
            {
                dtpData.Value = DateTime.Now;
                // I campi delle ComboBox sono già vuoti grazie a SetComboBoxDataSource con ID null.
                // Pulisci tutti gli altri campi.
                txtNumeroFormulario.Text = string.Empty;
                scbProduttore.Clear();
                scbDestinatario.Clear();
                scbTrasportatore.Clear();
                scbAutomezzo.Clear();
                scbRimorchio.Clear();
                scbConducente.Clear();

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
                ckDetentoreR.Checked = false;
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

            _currentFormulario.IdProduttore = (int)scbProduttore.SelectedValue;
            _currentFormulario.IdProduttoreIndirizzo = (int)cmbProduttoreIndirizzo.SelectedValue;

            _currentFormulario.IdDestinatario = (int)scbDestinatario.SelectedValue;
            _currentFormulario.IdDestinatarioIndirizzo = (int)cmbDestinatarioIndirizzo.SelectedValue;

            _currentFormulario.IdTrasportatore = (int)scbTrasportatore.SelectedValue;
            _currentFormulario.IdTrasportatoreIndirizzo = (int)cmbTrasportatoreIndirizzo.SelectedValue;

            _currentFormulario.IdAutomezzo = (int)scbAutomezzo.SelectedValue;
            _currentFormulario.IdRimorchio = scbRimorchio.SelectedValue as int?;
            _currentFormulario.IdConducente = scbConducente.SelectedValue as int?;

            // Caratteristiche del rifiuto
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
            if (ckDetentoreR.Checked)
                _currentFormulario.Detentore_R = true;
            else
                _currentFormulario.Detentore_R = false;
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



        /// <summary>
        /// Carica le ComboBox per Clienti e Automezzi.
        /// </summary>
        private async Task LoadComboBoxes()
        {
            try
            {
                _allClienti = (await _clienteRepository.GetAllAsync()).ToList();
                _allAutomezzi = (await _automezzoRepository.GetAllAsync()).ToList();

                // Configura il componente per il Produttore

                scbProduttore.DisplayMember = "RagSoc";
                scbProduttore.ValueMember = "Id";
                scbProduttore.DataSource = _allClienti.Cast<object>().ToList();

                // Configura il componente per il Destinatario

                scbDestinatario.DisplayMember = "RagSoc";
                scbDestinatario.ValueMember = "Id";
                scbDestinatario.DataSource = _allClienti.Cast<object>().ToList();

                // Configura il componente per il Trasportatore

                scbTrasportatore.DisplayMember = "RagSoc";
                scbTrasportatore.ValueMember = "Id";
                scbTrasportatore.DataSource = _allClienti.Cast<object>().ToList();

                // Configura il componente per l'Automezzo

                scbAutomezzo.DisplayMember = "Descrizione";
                scbAutomezzo.ValueMember = "Id";
                scbAutomezzo.DataSource = _allAutomezzi.Cast<object>().ToList();

                // Configura il componente per il Rimorchio

                scbRimorchio.DisplayMember = "Descrizione"; // O Targa?
                scbRimorchio.ValueMember = "Id";

                // Configura il componente per il Conducente

                scbConducente.DisplayMember = "DisplayText";
                scbConducente.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento delle liste: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void scbProduttore_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isLoading) return;
            var ownerCombo = sender as SearchableComboBox;
            await LoadIndirizziAsync(ownerCombo, cmbProduttoreIndirizzo);
        }

        private async Task LoadIndirizziAsync(SearchableComboBox ownerCombo, ComboBox addressCombo, int? addressIdToSelect = null)
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

        private async void scbDestinatario_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isLoading) return;
            var ownerCombo = sender as SearchableComboBox;
            await LoadIndirizziAsync(ownerCombo, cmbDestinatarioIndirizzo);
        }

        private async void scbTrasportatore_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isLoading) return;
            var ownerCombo = sender as SearchableComboBox;
            await LoadIndirizziAsync(ownerCombo, cmbTrasportatoreIndirizzo);
        }

        private async void scbAutomezzo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isLoading) return;
            await LoadRimorchiAndConducentiAsync();
        }

        private async Task LoadRimorchiAndConducentiAsync()
        {
            scbRimorchio.Clear();
            scbConducente.Clear();
            scbRimorchio.DataSource = null;
            scbConducente.DataSource = null;

            if (scbAutomezzo.SelectedValue is int automezzoId && automezzoId > 0)
            {
                try
                {
                    // Carica l'automezzo con le relazioni
                    var automezzo = await _automezzoRepository.AsQueryable()
                        .Include(a => a.AutomezziRimorchi).ThenInclude(ar => ar.Rimorchio)
                        .Include(a => a.AutomezziConducenti).ThenInclude(ac => ac.Conducente)
                        .FirstOrDefaultAsync(a => a.Id == automezzoId);

                    if (automezzo != null)
                    {
                        var rimorchi = automezzo.AutomezziRimorchi.Select(ar => ar.Rimorchio).ToList();
                        var conducenti = automezzo.AutomezziConducenti.Select(ac => ac.Conducente).ToList();

                        scbRimorchio.DataSource = rimorchi.Cast<object>().ToList();
                        scbConducente.DataSource = conducenti.Cast<object>().ToList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante il caricamento di rimorchi e conducenti: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

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
            /*if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }*/
        }

        private bool ValidateInput()
        {
            //if (string.IsNullOrWhiteSpace(txtNumeroFormulario.Text))
            //{
            //    MessageBox.Show("Numero Formulario è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    txtNumeroFormulario.Focus();
            //    return false;
            //}
            if (scbProduttore.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Produttore.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                scbProduttore.Focus();
                return false;
            }
            if (cmbProduttoreIndirizzo.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Indirizzo del Produttore.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbProduttoreIndirizzo.Focus();
                return false;
            }
            if (scbDestinatario.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Destinatario.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                scbDestinatario.Focus();
                return false;
            }
            if (cmbDestinatarioIndirizzo.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Indirizzo del Destinatario.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbDestinatarioIndirizzo.Focus();
                return false;
            }
            if (scbTrasportatore.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Trasportatore.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                scbTrasportatore.Focus();
                return false;
            }
            if (cmbTrasportatoreIndirizzo.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Indirizzo del Trasportatore.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbTrasportatoreIndirizzo.Focus();
                return false;
            }
            if (scbAutomezzo.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Automezzo.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                scbAutomezzo.Focus();
                return false;
            }
            if (scbRimorchio.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Rimorchio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                scbRimorchio.Focus();
                return false;
            }
            if (scbConducente.SelectedValue == null)
            {
                MessageBox.Show("Seleziona un Conducente.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                scbConducente.Focus();
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
            cmbProduttoreIndirizzo = new ComboBox();
            lblNumeroFormulario = new Label();
            txtNumeroFormulario = new TextBox();
            btnSalva = new Button();
            btnAnnulla = new Button();
            ckDetentoreR = new CheckBox();
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
            grProvenienza = new GroupBox();
            rbProvUrb = new RadioButton();
            rbProvSpec = new RadioButton();
            txtStatoFisco = new TextBox();
            label2 = new Label();
            txtCodiceEER = new TextBox();
            label1 = new Label();
            label3 = new Label();
            label9 = new Label();
            label8 = new Label();
            cmbDestD = new ComboBox();
            cmbDestR = new ComboBox();
            btStampa = new Button();
            cmbDestinatarioIndirizzo = new ComboBox();
            cmbTrasportatoreIndirizzo = new ComboBox();
            scbProduttore = new SearchableComboBox();
            scbDestinatario = new SearchableComboBox();
            scbTrasportatore = new SearchableComboBox();
            scbAutomezzo = new SearchableComboBox();
            scbRimorchio = new SearchableComboBox();
            scbConducente = new SearchableComboBox();
            lbProduttore = new Label();
            label10 = new Label();
            label11 = new Label();
            label12 = new Label();
            label13 = new Label();
            label14 = new Label();
            label15 = new Label();
            label16 = new Label();
            label17 = new Label();
            label18 = new Label();
            label19 = new Label();
            label20 = new Label();
            grAspettoEsteriore.SuspendLayout();
            grKgLitri.SuspendLayout();
            grProvenienza.SuspendLayout();
            SuspendLayout();
            // 
            // lblData
            // 
            lblData.AutoSize = true;
            lblData.Font = new Font("Segoe UI", 10.875F);
            lblData.Location = new Point(37, 19);
            lblData.Margin = new Padding(6, 0, 6, 0);
            lblData.Name = "lblData";
            lblData.Size = new Size(83, 40);
            lblData.TabIndex = 0;
            lblData.Text = "Data:";
            // 
            // dtpData
            // 
            dtpData.Font = new Font("Segoe UI", 10.875F);
            dtpData.Format = DateTimePickerFormat.Short;
            dtpData.Location = new Point(320, 13);
            dtpData.Margin = new Padding(6);
            dtpData.Name = "dtpData";
            dtpData.Size = new Size(251, 46);
            dtpData.TabIndex = 0;
            // 
            // cmbProduttoreIndirizzo
            // 
            cmbProduttoreIndirizzo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProduttoreIndirizzo.FormattingEnabled = true;
            cmbProduttoreIndirizzo.Location = new Point(328, 211);
            cmbProduttoreIndirizzo.Margin = new Padding(6);
            cmbProduttoreIndirizzo.Name = "cmbProduttoreIndirizzo";
            cmbProduttoreIndirizzo.Size = new Size(561, 40);
            cmbProduttoreIndirizzo.TabIndex = 3;
            // 
            // lblNumeroFormulario
            // 
            lblNumeroFormulario.AutoSize = true;
            lblNumeroFormulario.Font = new Font("Segoe UI", 10.875F);
            lblNumeroFormulario.Location = new Point(669, 19);
            lblNumeroFormulario.Margin = new Padding(6, 0, 6, 0);
            lblNumeroFormulario.Name = "lblNumeroFormulario";
            lblNumeroFormulario.Size = new Size(274, 40);
            lblNumeroFormulario.TabIndex = 6;
            lblNumeroFormulario.Text = "Numero Formulario:";
            // 
            // txtNumeroFormulario
            // 
            txtNumeroFormulario.Font = new Font("Segoe UI", 10.875F);
            txtNumeroFormulario.Location = new Point(972, 13);
            txtNumeroFormulario.Margin = new Padding(6);
            txtNumeroFormulario.MaxLength = 50;
            txtNumeroFormulario.Name = "txtNumeroFormulario";
            txtNumeroFormulario.Size = new Size(291, 46);
            txtNumeroFormulario.TabIndex = 1;
            // 
            // btnSalva
            // 
            btnSalva.Font = new Font("Segoe UI", 10.875F);
            btnSalva.Location = new Point(1672, 19);
            btnSalva.Margin = new Padding(6);
            btnSalva.Name = "btnSalva";
            btnSalva.Size = new Size(144, 64);
            btnSalva.TabIndex = 45;
            btnSalva.Text = "Salva";
            // 
            // btnAnnulla
            // 
            btnAnnulla.Font = new Font("Segoe UI", 10.875F);
            btnAnnulla.Location = new Point(1672, 92);
            btnAnnulla.Name = "btnAnnulla";
            btnAnnulla.Size = new Size(144, 64);
            btnAnnulla.TabIndex = 44;
            btnAnnulla.Text = "Annulla";
            btnAnnulla.UseVisualStyleBackColor = true;
            btnAnnulla.Click += btnAnnulla_Click;
            // 
            // ckDetentoreR
            // 
            ckDetentoreR.AutoSize = true;
            ckDetentoreR.Font = new Font("Segoe UI", 10.875F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ckDetentoreR.Location = new Point(1155, 158);
            ckDetentoreR.Name = "ckDetentoreR";
            ckDetentoreR.Size = new Size(218, 44);
            ckDetentoreR.TabIndex = 4;
            ckDetentoreR.Text = "Detentore Rif";
            ckDetentoreR.UseVisualStyleBackColor = true;
            ckDetentoreR.CheckedChanged += ckDetentoreR_CheckedChanged;
            // 
            // grAspettoEsteriore
            // 
            grAspettoEsteriore.Controls.Add(txtColli);
            grAspettoEsteriore.Controls.Add(label6);
            grAspettoEsteriore.Controls.Add(ckAllaRinfusa);
            grAspettoEsteriore.Location = new Point(1133, 885);
            grAspettoEsteriore.Margin = new Padding(6);
            grAspettoEsteriore.Name = "grAspettoEsteriore";
            grAspettoEsteriore.Padding = new Padding(6);
            grAspettoEsteriore.Size = new Size(496, 84);
            grAspettoEsteriore.TabIndex = 48;
            grAspettoEsteriore.TabStop = false;
            grAspettoEsteriore.Text = "Aspetto esteriore";
            // 
            // txtColli
            // 
            txtColli.Location = new Point(140, 35);
            txtColli.Margin = new Padding(6);
            txtColli.MaxLength = 5;
            txtColli.Name = "txtColli";
            txtColli.Size = new Size(137, 39);
            txtColli.TabIndex = 21;
            txtColli.KeyPress += txtColli_KeyPress;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(20, 43);
            label6.Margin = new Padding(6, 0, 6, 0);
            label6.Name = "label6";
            label6.Size = new Size(104, 32);
            label6.TabIndex = 27;
            label6.Text = "Nr. Colli:";
            // 
            // ckAllaRinfusa
            // 
            ckAllaRinfusa.AutoSize = true;
            ckAllaRinfusa.Location = new Point(292, 43);
            ckAllaRinfusa.Margin = new Padding(6);
            ckAllaRinfusa.Name = "ckAllaRinfusa";
            ckAllaRinfusa.Size = new Size(170, 36);
            ckAllaRinfusa.TabIndex = 22;
            ckAllaRinfusa.Text = "Alla Rinfusa";
            ckAllaRinfusa.UseVisualStyleBackColor = true;
            // 
            // txtChimicoFisiche
            // 
            txtChimicoFisiche.Font = new Font("Segoe UI", 10.875F);
            txtChimicoFisiche.Location = new Point(320, 984);
            txtChimicoFisiche.Margin = new Padding(6);
            txtChimicoFisiche.MaxLength = 25;
            txtChimicoFisiche.Name = "txtChimicoFisiche";
            txtChimicoFisiche.Size = new Size(1309, 46);
            txtChimicoFisiche.TabIndex = 23;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 10.875F);
            label7.Location = new Point(38, 984);
            label7.Margin = new Padding(6, 0, 6, 0);
            label7.Name = "label7";
            label7.Size = new Size(258, 40);
            label7.TabIndex = 46;
            label7.Text = "Caratt. Chim.-Fisic.:";
            // 
            // ckPesoVerificato
            // 
            ckPesoVerificato.AutoSize = true;
            ckPesoVerificato.Font = new Font("Segoe UI", 10.875F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ckPesoVerificato.Location = new Point(871, 908);
            ckPesoVerificato.Margin = new Padding(6);
            ckPesoVerificato.Name = "ckPesoVerificato";
            ckPesoVerificato.Size = new Size(233, 44);
            ckPesoVerificato.TabIndex = 20;
            ckPesoVerificato.Text = "Peso verificato";
            ckPesoVerificato.UseVisualStyleBackColor = true;
            // 
            // grKgLitri
            // 
            grKgLitri.Controls.Add(rbKg);
            grKgLitri.Controls.Add(rbLitri);
            grKgLitri.Location = new Point(610, 885);
            grKgLitri.Margin = new Padding(6);
            grKgLitri.Name = "grKgLitri";
            grKgLitri.Padding = new Padding(6);
            grKgLitri.Size = new Size(228, 82);
            grKgLitri.TabIndex = 44;
            grKgLitri.TabStop = false;
            // 
            // rbKg
            // 
            rbKg.AutoSize = true;
            rbKg.Location = new Point(36, 29);
            rbKg.Margin = new Padding(6);
            rbKg.Name = "rbKg";
            rbKg.Size = new Size(73, 36);
            rbKg.TabIndex = 18;
            rbKg.TabStop = true;
            rbKg.Text = "Kg";
            rbKg.UseVisualStyleBackColor = true;
            // 
            // rbLitri
            // 
            rbLitri.AutoSize = true;
            rbLitri.Location = new Point(119, 30);
            rbLitri.Margin = new Padding(6);
            rbLitri.Name = "rbLitri";
            rbLitri.Size = new Size(84, 36);
            rbLitri.TabIndex = 19;
            rbLitri.TabStop = true;
            rbLitri.Text = "Litri";
            rbLitri.UseVisualStyleBackColor = true;
            // 
            // txtQuantita
            // 
            txtQuantita.Font = new Font("Segoe UI", 10.875F);
            txtQuantita.Location = new Point(320, 909);
            txtQuantita.Margin = new Padding(6);
            txtQuantita.MaxLength = 12;
            txtQuantita.Name = "txtQuantita";
            txtQuantita.Size = new Size(252, 46);
            txtQuantita.TabIndex = 17;
            txtQuantita.KeyPress += txtQuantita_KeyPress;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 10.875F);
            label5.Location = new Point(37, 905);
            label5.Margin = new Padding(6, 0, 6, 0);
            label5.Name = "label5";
            label5.Size = new Size(134, 40);
            label5.TabIndex = 42;
            label5.Text = "Quantità:";
            // 
            // txtDescr
            // 
            txtDescr.Font = new Font("Segoe UI", 10.875F);
            txtDescr.Location = new Point(320, 833);
            txtDescr.Margin = new Padding(6);
            txtDescr.MaxLength = 50;
            txtDescr.Name = "txtDescr";
            txtDescr.Size = new Size(1309, 46);
            txtDescr.TabIndex = 16;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 10.875F);
            label4.Location = new Point(37, 836);
            label4.Margin = new Padding(6, 0, 6, 0);
            label4.Name = "label4";
            label4.Size = new Size(168, 40);
            label4.TabIndex = 40;
            label4.Text = "Descrizione:";
            // 
            // txtCarattPericolosità
            // 
            txtCarattPericolosità.Font = new Font("Segoe UI", 10.875F);
            txtCarattPericolosità.Location = new Point(1189, 768);
            txtCarattPericolosità.Margin = new Padding(6);
            txtCarattPericolosità.MaxLength = 25;
            txtCarattPericolosità.Name = "txtCarattPericolosità";
            txtCarattPericolosità.Size = new Size(440, 46);
            txtCarattPericolosità.TabIndex = 15;
            // 
            // grProvenienza
            // 
            grProvenienza.Controls.Add(rbProvUrb);
            grProvenienza.Controls.Add(rbProvSpec);
            grProvenienza.Location = new Point(1189, 674);
            grProvenienza.Margin = new Padding(6);
            grProvenienza.Name = "grProvenienza";
            grProvenienza.Padding = new Padding(6);
            grProvenienza.Size = new Size(440, 82);
            grProvenienza.TabIndex = 37;
            grProvenienza.TabStop = false;
            // 
            // rbProvUrb
            // 
            rbProvUrb.AutoSize = true;
            rbProvUrb.Location = new Point(36, 32);
            rbProvUrb.Margin = new Padding(6);
            rbProvUrb.Name = "rbProvUrb";
            rbProvUrb.Size = new Size(123, 36);
            rbProvUrb.TabIndex = 13;
            rbProvUrb.TabStop = true;
            rbProvUrb.Text = "Urbano";
            rbProvUrb.UseVisualStyleBackColor = true;
            // 
            // rbProvSpec
            // 
            rbProvSpec.AutoSize = true;
            rbProvSpec.Location = new Point(174, 32);
            rbProvSpec.Margin = new Padding(6);
            rbProvSpec.Name = "rbProvSpec";
            rbProvSpec.Size = new Size(133, 36);
            rbProvSpec.TabIndex = 14;
            rbProvSpec.TabStop = true;
            rbProvSpec.Text = "Speciale";
            rbProvSpec.UseVisualStyleBackColor = true;
            // 
            // txtStatoFisco
            // 
            txtStatoFisco.Font = new Font("Segoe UI", 10.875F);
            txtStatoFisco.Location = new Point(670, 774);
            txtStatoFisco.Margin = new Padding(6);
            txtStatoFisco.MaxLength = 1;
            txtStatoFisco.Name = "txtStatoFisco";
            txtStatoFisco.Size = new Size(137, 46);
            txtStatoFisco.TabIndex = 12;
            txtStatoFisco.KeyPress += txtStatoFisco_KeyPress;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10.875F);
            label2.Location = new Point(491, 781);
            label2.Margin = new Padding(6, 0, 6, 0);
            label2.Name = "label2";
            label2.Size = new Size(167, 40);
            label2.TabIndex = 35;
            label2.Text = "Stato Fisico:";
            // 
            // txtCodiceEER
            // 
            txtCodiceEER.Font = new Font("Segoe UI", 10.875F);
            txtCodiceEER.Location = new Point(320, 774);
            txtCodiceEER.Margin = new Padding(6);
            txtCodiceEER.MaxLength = 10;
            txtCodiceEER.Name = "txtCodiceEER";
            txtCodiceEER.Size = new Size(134, 46);
            txtCodiceEER.TabIndex = 11;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10.875F);
            label1.Location = new Point(37, 777);
            label1.Margin = new Padding(6, 0, 6, 0);
            label1.Name = "label1";
            label1.Size = new Size(165, 40);
            label1.TabIndex = 33;
            label1.Text = "Codice EER:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 10.875F);
            label3.Location = new Point(917, 774);
            label3.Margin = new Padding(6, 0, 6, 0);
            label3.Name = "label3";
            label3.Size = new Size(260, 40);
            label3.TabIndex = 38;
            label3.Text = "Caratt. Pericolosità:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 10.875F);
            label9.Location = new Point(1484, 333);
            label9.Name = "label9";
            label9.Size = new Size(37, 40);
            label9.TabIndex = 53;
            label9.Text = "D";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 10.875F);
            label8.Location = new Point(1152, 333);
            label8.Name = "label8";
            label8.Size = new Size(205, 40);
            label8.TabIndex = 52;
            label8.Text = "Destinazione R";
            // 
            // cmbDestD
            // 
            cmbDestD.Enabled = false;
            cmbDestD.Font = new Font("Segoe UI", 10.875F);
            cmbDestD.FormattingEnabled = true;
            cmbDestD.Location = new Point(1527, 325);
            cmbDestD.Name = "cmbDestD";
            cmbDestD.Size = new Size(102, 48);
            cmbDestD.TabIndex = 8;
            // 
            // cmbDestR
            // 
            cmbDestR.Enabled = false;
            cmbDestR.Font = new Font("Segoe UI", 10.875F);
            cmbDestR.FormattingEnabled = true;
            cmbDestR.Items.AddRange(new object[] { "", "R13", "R4" });
            cmbDestR.Location = new Point(1363, 325);
            cmbDestR.Name = "cmbDestR";
            cmbDestR.Size = new Size(102, 48);
            cmbDestR.TabIndex = 7;
            // 
            // btStampa
            // 
            btStampa.Enabled = false;
            btStampa.Font = new Font("Segoe UI", 10.875F);
            btStampa.Location = new Point(1672, 168);
            btStampa.Margin = new Padding(6);
            btStampa.Name = "btStampa";
            btStampa.Size = new Size(144, 64);
            btStampa.TabIndex = 34;
            btStampa.Text = "Stampa";
            btStampa.UseVisualStyleBackColor = true;
            btStampa.Click += btStampa_Click;
            // 
            // cmbDestinatarioIndirizzo
            // 
            cmbDestinatarioIndirizzo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDestinatarioIndirizzo.FormattingEnabled = true;
            cmbDestinatarioIndirizzo.Location = new Point(328, 391);
            cmbDestinatarioIndirizzo.Margin = new Padding(6);
            cmbDestinatarioIndirizzo.Name = "cmbDestinatarioIndirizzo";
            cmbDestinatarioIndirizzo.Size = new Size(561, 40);
            cmbDestinatarioIndirizzo.TabIndex = 6;
            // 
            // cmbTrasportatoreIndirizzo
            // 
            cmbTrasportatoreIndirizzo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTrasportatoreIndirizzo.FormattingEnabled = true;
            cmbTrasportatoreIndirizzo.Location = new Point(328, 580);
            cmbTrasportatoreIndirizzo.Margin = new Padding(6);
            cmbTrasportatoreIndirizzo.Name = "cmbTrasportatoreIndirizzo";
            cmbTrasportatoreIndirizzo.Size = new Size(561, 40);
            cmbTrasportatoreIndirizzo.TabIndex = 10;
            // 
            // scbProduttore
            // 
            scbProduttore.DisplayMember = "";
            scbProduttore.Location = new Point(319, 139);
            scbProduttore.Margin = new Padding(6);
            scbProduttore.Name = "scbProduttore";
            scbProduttore.Size = new Size(800, 66);
            scbProduttore.TabIndex = 2;
            scbProduttore.ValueMember = "";
            // 
            // scbDestinatario
            // 
            scbDestinatario.DisplayMember = "";
            scbDestinatario.Location = new Point(319, 317);
            scbDestinatario.Margin = new Padding(6);
            scbDestinatario.Name = "scbDestinatario";
            scbDestinatario.Size = new Size(800, 62);
            scbDestinatario.TabIndex = 5;
            scbDestinatario.ValueMember = "";
            // 
            // scbTrasportatore
            // 
            scbTrasportatore.DisplayMember = "";
            scbTrasportatore.Location = new Point(319, 505);
            scbTrasportatore.Margin = new Padding(6);
            scbTrasportatore.Name = "scbTrasportatore";
            scbTrasportatore.Size = new Size(800, 66);
            scbTrasportatore.TabIndex = 9;
            scbTrasportatore.ValueMember = "";
            // 
            // scbAutomezzo
            // 
            scbAutomezzo.DisplayMember = "";
            scbAutomezzo.Location = new Point(320, 1111);
            scbAutomezzo.Margin = new Padding(6);
            scbAutomezzo.Name = "scbAutomezzo";
            scbAutomezzo.Size = new Size(800, 61);
            scbAutomezzo.TabIndex = 24;
            scbAutomezzo.ValueMember = "";
            // 
            // scbRimorchio
            // 
            scbRimorchio.DisplayMember = "";
            scbRimorchio.Location = new Point(320, 1173);
            scbRimorchio.Margin = new Padding(6);
            scbRimorchio.Name = "scbRimorchio";
            scbRimorchio.Size = new Size(800, 61);
            scbRimorchio.TabIndex = 25;
            scbRimorchio.ValueMember = "";
            // 
            // scbConducente
            // 
            scbConducente.DisplayMember = "";
            scbConducente.Location = new Point(320, 1234);
            scbConducente.Margin = new Padding(6);
            scbConducente.Name = "scbConducente";
            scbConducente.Size = new Size(800, 61);
            scbConducente.TabIndex = 26;
            scbConducente.ValueMember = "";
            // 
            // lbProduttore
            // 
            lbProduttore.BackColor = Color.FromArgb(224, 224, 224);
            lbProduttore.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbProduttore.ForeColor = Color.FromArgb(0, 0, 192);
            lbProduttore.Location = new Point(11, 84);
            lbProduttore.Name = "lbProduttore";
            lbProduttore.Padding = new Padding(20, 0, 0, 0);
            lbProduttore.Size = new Size(1641, 49);
            lbProduttore.TabIndex = 54;
            lbProduttore.Text = "Produttore";
            lbProduttore.TextAlign = ContentAlignment.MiddleLeft;
            lbProduttore.Click += lbProduttore_Click;
            // 
            // label10
            // 
            label10.BackColor = Color.FromArgb(224, 224, 224);
            label10.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label10.ForeColor = Color.FromArgb(0, 0, 192);
            label10.ImageAlign = ContentAlignment.MiddleLeft;
            label10.Location = new Point(11, 266);
            label10.Name = "label10";
            label10.Padding = new Padding(20, 0, 0, 0);
            label10.Size = new Size(1641, 49);
            label10.TabIndex = 55;
            label10.Text = "Destinatario";
            label10.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label11
            // 
            label11.BackColor = Color.FromArgb(224, 224, 224);
            label11.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label11.ForeColor = Color.FromArgb(0, 0, 192);
            label11.Location = new Point(11, 450);
            label11.Name = "label11";
            label11.Padding = new Padding(20, 0, 0, 0);
            label11.Size = new Size(1641, 49);
            label11.TabIndex = 56;
            label11.Text = "Trasportatore";
            label11.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label12
            // 
            label12.BackColor = Color.FromArgb(224, 224, 224);
            label12.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label12.ForeColor = Color.FromArgb(0, 0, 192);
            label12.Location = new Point(12, 1058);
            label12.Name = "label12";
            label12.Padding = new Padding(20, 0, 0, 0);
            label12.Size = new Size(1640, 47);
            label12.TabIndex = 57;
            label12.Text = "Trasporto";
            label12.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label13
            // 
            label13.BackColor = Color.FromArgb(224, 224, 224);
            label13.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label13.ForeColor = Color.FromArgb(0, 0, 192);
            label13.Location = new Point(11, 643);
            label13.Name = "label13";
            label13.Padding = new Padding(20, 0, 0, 0);
            label13.Size = new Size(1641, 49);
            label13.TabIndex = 58;
            label13.Text = "Caratteristiche del Rifiuto";
            label13.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Segoe UI", 10.875F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label14.Location = new Point(37, 1132);
            label14.Name = "label14";
            label14.Size = new Size(168, 40);
            label14.TabIndex = 59;
            label14.Text = "Automezzo:";
            label14.Click += label14_Click;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Font = new Font("Segoe UI", 10.875F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label15.Location = new Point(37, 1194);
            label15.Name = "label15";
            label15.Size = new Size(152, 40);
            label15.TabIndex = 60;
            label15.Text = "Rimorchio:";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Font = new Font("Segoe UI", 10.875F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label16.Location = new Point(37, 1255);
            label16.Name = "label16";
            label16.Size = new Size(176, 40);
            label16.TabIndex = 61;
            label16.Text = "Conducente:";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Font = new Font("Segoe UI", 10.875F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label17.Location = new Point(37, 333);
            label17.Name = "label17";
            label17.Size = new Size(222, 40);
            label17.TabIndex = 63;
            label17.Text = "Denominazione:";
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Font = new Font("Segoe UI", 10.875F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label18.Location = new Point(37, 519);
            label18.Name = "label18";
            label18.Size = new Size(222, 40);
            label18.TabIndex = 64;
            label18.Text = "Denominazione:";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Font = new Font("Segoe UI", 10.875F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label19.Location = new Point(37, 153);
            label19.Name = "label19";
            label19.Size = new Size(222, 40);
            label19.TabIndex = 65;
            label19.Text = "Denominazione:";
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Font = new Font("Segoe UI", 10.875F);
            label20.Location = new Point(917, 702);
            label20.Margin = new Padding(6, 0, 6, 0);
            label20.Name = "label20";
            label20.Size = new Size(177, 40);
            label20.TabIndex = 66;
            label20.Text = "Provenienza:";
            // 
            // FormulariRifiutiDetailForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1908, 1337);
            Controls.Add(label20);
            Controls.Add(label19);
            Controls.Add(label18);
            Controls.Add(label17);
            Controls.Add(label16);
            Controls.Add(label15);
            Controls.Add(label14);
            Controls.Add(txtChimicoFisiche);
            Controls.Add(label7);
            Controls.Add(grAspettoEsteriore);
            Controls.Add(ckDetentoreR);
            Controls.Add(label13);
            Controls.Add(label12);
            Controls.Add(ckPesoVerificato);
            Controls.Add(label11);
            Controls.Add(grKgLitri);
            Controls.Add(label10);
            Controls.Add(lbProduttore);
            Controls.Add(label9);
            Controls.Add(txtQuantita);
            Controls.Add(label5);
            Controls.Add(scbConducente);
            Controls.Add(label8);
            Controls.Add(txtDescr);
            Controls.Add(label4);
            Controls.Add(scbRimorchio);
            Controls.Add(scbAutomezzo);
            Controls.Add(grProvenienza);
            Controls.Add(txtCarattPericolosità);
            Controls.Add(cmbDestD);
            Controls.Add(label3);
            Controls.Add(cmbDestR);
            Controls.Add(txtStatoFisco);
            Controls.Add(label2);
            Controls.Add(scbTrasportatore);
            Controls.Add(scbDestinatario);
            Controls.Add(txtCodiceEER);
            Controls.Add(label1);
            Controls.Add(scbProduttore);
            Controls.Add(cmbTrasportatoreIndirizzo);
            Controls.Add(cmbDestinatarioIndirizzo);
            Controls.Add(btStampa);
            Controls.Add(btnAnnulla);
            Controls.Add(btnSalva);
            Controls.Add(txtNumeroFormulario);
            Controls.Add(lblNumeroFormulario);
            Controls.Add(cmbProduttoreIndirizzo);
            Controls.Add(dtpData);
            Controls.Add(lblData);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(6);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormulariRifiutiDetailForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Dettagli Formulario Rifiuti";
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
        private System.Windows.Forms.ComboBox cmbProduttoreIndirizzo;
        private System.Windows.Forms.Label lblNumeroFormulario;
        private System.Windows.Forms.TextBox txtNumeroFormulario;
        private System.Windows.Forms.Button btnSalva;
        private System.Windows.Forms.Button btnAnnulla;
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
        private ComboBox cmbDestinatarioIndirizzo;
        private ComboBox cmbTrasportatoreIndirizzo;
        private Controls.SearchableComboBox scbProduttore;
        private Controls.SearchableComboBox scbDestinatario;
        private Controls.SearchableComboBox scbTrasportatore;
        private Controls.SearchableComboBox scbAutomezzo;

        #endregion

        private void lbProduttore_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void ckDetentoreR_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}

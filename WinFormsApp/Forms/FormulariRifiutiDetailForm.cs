// File: Forms/FormulariRifiutiDetailForm.cs
// Questo form permette di inserire o modificare un singolo formulario rifiuti.
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore; // Per l'include delle proprietà di navigazione
using System.Linq;

namespace FormulariRif_G.Forms
{
    public partial class FormulariRifiutiDetailForm : Form
    {
        private readonly IGenericRepository<FormularioRifiuti> _formularioRifiutiRepository;
        private readonly IGenericRepository<Cliente> _clienteRepository;
        private readonly IGenericRepository<ClienteIndirizzo> _clienteIndirizzoRepository;
        private readonly IGenericRepository<Automezzo> _automezzoRepository;

        private FormularioRifiuti? _currentFormulario;

        public FormulariRifiutiDetailForm(IGenericRepository<FormularioRifiuti> formularioRifiutiRepository,
                                         IGenericRepository<Cliente> clienteRepository,
                                         IGenericRepository<ClienteIndirizzo> clienteIndirizzoRepository,
                                         IGenericRepository<Automezzo> automezzoRepository)
        {
            InitializeComponent();
            _formularioRifiutiRepository = formularioRifiutiRepository;
            _clienteRepository = clienteRepository;
            _clienteIndirizzoRepository = clienteIndirizzoRepository;
            _automezzoRepository = automezzoRepository;
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
            _currentFormulario = formulario;
            // La data viene impostata nel LoadFormularioData dopo che le combobox sono caricate
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
                cmbCliente.SelectedValue = _currentFormulario.IdCli;
                // Questo triggererà cmbCliente_SelectedIndexChanged che caricherà gli indirizzi
                // e selezionerà l'indirizzo predefinito o quello del formulario.

                // Seleziona l'automezzo nella ComboBox
                cmbAutomezzo.SelectedValue = _currentFormulario.IdAutomezzo;

                // Dopo che cmbCliente_SelectedIndexChanged ha caricato gli indirizzi,
                // dobbiamo selezionare l'indirizzo specifico del formulario.
                if (_currentFormulario.IdClienteIndirizzo != 0)
                {
                    cmbIndirizzo.SelectedValue = _currentFormulario.IdClienteIndirizzo;
                }
            }
            else
            {
                dtpData.Value = DateTime.Now;
                txtNumeroFormulario.Text = string.Empty;
                cmbCliente.SelectedIndex = -1; // Nessun cliente selezionato
                cmbIndirizzo.SelectedIndex = -1; // Nessun indirizzo selezionato
                cmbAutomezzo.SelectedIndex = -1; // Nessun automezzo selezionato
            }
        }

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
                        // Se non c'è un predefinito, seleziona il primo
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

        /// <summary>
        /// Gestisce il click sul pulsante "Salva".
        /// </summary>
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
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio del formulario: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Esegue la validazione dei campi di input.
        /// </summary>
        /// <returns>True se l'input è valido, altrimenti False.</returns>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtNumeroFormulario.Text))
            {
                MessageBox.Show("Numero Formulario è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNumeroFormulario.Focus();
                return false;
            }
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
            this.lblData = new System.Windows.Forms.Label();
            this.dtpData = new System.Windows.Forms.DateTimePicker();
            this.lblCliente = new System.Windows.Forms.Label();
            this.cmbCliente = new System.Windows.Forms.ComboBox();
            this.lblIndirizzo = new System.Windows.Forms.Label();
            this.cmbIndirizzo = new System.Windows.Forms.ComboBox();
            this.lblNumeroFormulario = new System.Windows.Forms.Label();
            this.txtNumeroFormulario = new System.Windows.Forms.TextBox();
            this.lblAutomezzo = new System.Windows.Forms.Label();
            this.cmbAutomezzo = new System.Windows.Forms.ComboBox();
            this.btnSalva = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblData
            //
            this.lblData.AutoSize = true;
            this.lblData.Location = new System.Drawing.Point(20, 30);
            this.lblData.Name = "lblData";
            this.lblData.Size = new System.Drawing.Size(35, 15);
            this.lblData.TabIndex = 0;
            this.lblData.Text = "Data:";
            //
            // dtpData
            //
            this.dtpData.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpData.Location = new System.Drawing.Point(140, 27);
            this.dtpData.Name = "dtpData";
            this.dtpData.Size = new System.Drawing.Size(230, 23);
            this.dtpData.TabIndex = 1;
            //
            // lblCliente
            //
            this.lblCliente.AutoSize = true;
            this.lblCliente.Location = new System.Drawing.Point(20, 70);
            this.lblCliente.Name = "lblCliente";
            this.lblCliente.Size = new System.Drawing.Size(47, 15);
            this.lblCliente.TabIndex = 2;
            this.lblCliente.Text = "Cliente:";
            //
            // cmbCliente
            //
            this.cmbCliente.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbCliente.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCliente.FormattingEnabled = true;
            this.cmbCliente.Location = new System.Drawing.Point(140, 67);
            this.cmbCliente.Name = "cmbCliente";
            this.cmbCliente.Size = new System.Drawing.Size(230, 23);
            this.cmbCliente.TabIndex = 3;
            //
            // lblIndirizzo
            //
            this.lblIndirizzo.AutoSize = true;
            this.lblIndirizzo.Location = new System.Drawing.Point(20, 110);
            this.lblIndirizzo.Name = "lblIndirizzo";
            this.lblIndirizzo.Size = new System.Drawing.Size(57, 15);
            this.lblIndirizzo.TabIndex = 4;
            this.lblIndirizzo.Text = "Indirizzo:";
            //
            // cmbIndirizzo
            //
            this.cmbIndirizzo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbIndirizzo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbIndirizzo.FormattingEnabled = true;
            this.cmbIndirizzo.Location = new System.Drawing.Point(140, 107);
            this.cmbIndirizzo.Name = "cmbIndirizzo";
            this.cmbIndirizzo.Size = new System.Drawing.Size(230, 23);
            this.cmbIndirizzo.TabIndex = 5;
            //
            // lblNumeroFormulario
            //
            this.lblNumeroFormulario.AutoSize = true;
            this.lblNumeroFormulario.Location = new System.Drawing.Point(20, 150);
            this.lblNumeroFormulario.Name = "lblNumeroFormulario";
            this.lblNumeroFormulario.Size = new System.Drawing.Size(107, 15);
            this.lblNumeroFormulario.TabIndex = 6;
            this.lblNumeroFormulario.Text = "Numero Formulario:";
            //
            // txtNumeroFormulario
            //
            this.txtNumeroFormulario.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNumeroFormulario.Location = new System.Drawing.Point(140, 147);
            this.txtNumeroFormulario.Name = "txtNumeroFormulario";
            this.txtNumeroFormulario.Size = new System.Drawing.Size(230, 23);
            this.txtNumeroFormulario.TabIndex = 7;
            //
            // lblAutomezzo
            //
            this.lblAutomezzo.AutoSize = true;
            this.lblAutomezzo.Location = new System.Drawing.Point(20, 190);
            this.lblAutomezzo.Name = "lblAutomezzo";
            this.lblAutomezzo.Size = new System.Drawing.Size(71, 15);
            this.lblAutomezzo.TabIndex = 8;
            this.lblAutomezzo.Text = "Automezzo:";
            //
            // cmbAutomezzo
            //
            this.cmbAutomezzo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbAutomezzo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAutomezzo.FormattingEnabled = true;
            this.cmbAutomezzo.Location = new System.Drawing.Point(140, 187);
            this.cmbAutomezzo.Name = "cmbAutomezzo";
            this.cmbAutomezzo.Size = new System.Drawing.Size(230, 23);
            this.cmbAutomezzo.TabIndex = 9;
            //
            // btnSalva
            //
            this.btnSalva.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalva.Location = new System.Drawing.Point(295, 230);
            this.btnSalva.Name = "btnSalva";
            this.btnSalva.Size = new System.Drawing.Size(75, 30);
            this.btnSalva.TabIndex = 10;
            this.btnSalva.Text = "Salva";
            this.btnSalva.UseVisualStyleBackColor = true;
            this.btnSalva.Click += new System.EventHandler(this.btnSalva_Click);
            //
            // FormulariRifiutiDetailForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 275);
            this.Controls.Add(this.btnSalva);
            this.Controls.Add(this.cmbAutomezzo);
            this.Controls.Add(this.lblAutomezzo);
            this.Controls.Add(this.txtNumeroFormulario);
            this.Controls.Add(this.lblNumeroFormulario);
            this.Controls.Add(this.cmbIndirizzo);
            this.Controls.Add(this.lblIndirizzo);
            this.Controls.Add(this.cmbCliente);
            this.Controls.Add(this.lblCliente);
            this.Controls.Add(this.dtpData);
            this.Controls.Add(this.lblData);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormulariRifiutiDetailForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dettagli Formulario Rifiuti";
            this.ResumeLayout(false);
            this.PerformLayout();

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

        #endregion
    }
}

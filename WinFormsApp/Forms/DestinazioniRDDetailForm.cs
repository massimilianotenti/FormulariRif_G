// File: Forms/ConducentiDetailForm.cs
// Questo form permette l'inserimento, la modifica e la visualizzazione dettagliata di un conducente.
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using System;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace FormulariRif_G.Forms
{
    public partial class DestinazioniRDDetailForm : Form
    {
        private readonly IGenericRepository<DestinatarioDest> _destinazioniRDRepository;
        private DestinatarioDest? _currentDestinatario;
        private bool _isReadOnly;

        // Dichiarazioni dei controlli
        private System.Windows.Forms.Label lblDescrizione;
        private System.Windows.Forms.TextBox txtDescrizione;
        private System.Windows.Forms.Label lblTipo;
        private System.Windows.Forms.RadioButton rdbR;
        private System.Windows.Forms.RadioButton rdbD;
        private System.Windows.Forms.Button btnSalva;
        private System.Windows.Forms.Button btnAnnulla;

        public DestinazioniRDDetailForm(IGenericRepository<DestinatarioDest> destinazioneRDRepository)
        {
            InitializeComponent();
            _destinazioniRDRepository = destinazioneRDRepository;

            if (btnSalva != null) btnSalva.Click += btnSalvaClick;
            if (btnAnnulla != null) btnAnnulla.Click += btnAnnullaClick;
        }

        /// <summary>
        /// Imposta il conducente da visualizzare o modificare.
        /// </summary>
        /// <param name="conducente">L'oggetto Conducente.</param>
        /// <param name="isReadOnly">Se true, il form sarà in modalità sola lettura.</param>
        public void SetConducente(DestinatarioDest dest, bool isReadOnly = false)
        {
            _currentDestinatario = dest;
            _isReadOnly = isReadOnly;
            LoadConducenteData();
            SetFormMode();
        }

        /// <summary>
        /// Carica i dati del conducente nei controlli del form.
        /// </summary>
        private void LoadConducenteData()
        {
            if (_currentDestinatario != null)
            {
                txtDescrizione.Text = _currentDestinatario.Desc;                
                if (_currentDestinatario.Tipo == 0)                
                    rdbR.Checked = true;                
                else if (_currentDestinatario.Tipo == 1)               
                    rdbD.Checked = true;                
            }
            else
            {
                txtDescrizione.Text = string.Empty;                
                rdbR.Checked = true; 
            }
        }

        /// <summary>
        /// Imposta i controlli del form in base alla modalità.
        /// </summary>
        private void SetFormMode()
        {
            txtDescrizione.ReadOnly = _isReadOnly;            
            rdbR.Enabled = !_isReadOnly;
            rdbD.Enabled = !_isReadOnly;

            if (btnSalva != null) btnSalva.Visible = !_isReadOnly;
            if (btnAnnulla != null) btnAnnulla.Visible = true;

            this.Text = _isReadOnly ? "Dettagli Conducente" : (_currentDestinatario?.Id == 0 ? "Nuova Destinazione" : "Modifica Destinazione");
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Salva".
        /// </summary>
        private async void btnSalvaClick(object? sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            if (_currentDestinatario == null)
            {
                _currentDestinatario = new DestinatarioDest();
            }

            _currentDestinatario.Desc = txtDescrizione.Text.Trim();
            _currentDestinatario.Tipo = rdbR.Checked ? 0 : 1;

            try
            {
                if (_currentDestinatario.Id == 0)
                {
                    await _destinazioniRDRepository.AddAsync(_currentDestinatario);
                    MessageBox.Show("Destinazione aggiunta con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _destinazioniRDRepository.Update(_currentDestinatario);
                    MessageBox.Show("Destinazione modificata con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                await _destinazioniRDRepository.SaveChangesAsync();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio della destinazione: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Esegue la validazione dei campi di input.
        /// </summary>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtDescrizione.Text))
            {
                MessageBox.Show("Descrizione è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDescrizione.Focus();
                return false;
            }
            if (!rdbR.Checked && !rdbD.Checked)
            {
                MessageBox.Show("Selezionare il tipo di destinazione (R o D).", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Annulla".
        /// </summary>
        private void btnAnnullaClick(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

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
            lblDescrizione = new Label();
            txtDescrizione = new TextBox();
            lblTipo = new Label();
            rdbR = new RadioButton();
            rdbD = new RadioButton();
            btnSalva = new Button();
            btnAnnulla = new Button();
            SuspendLayout();
            // 
            // lblDescrizione
            // 
            lblDescrizione.AutoSize = true;
            lblDescrizione.Location = new Point(56, 64);
            lblDescrizione.Margin = new Padding(6, 0, 6, 0);
            lblDescrizione.Name = "lblDescrizione";
            lblDescrizione.Size = new Size(142, 32);
            lblDescrizione.TabIndex = 0;
            lblDescrizione.Text = "Descrizione:";
            // 
            // txtDescrizione
            // 
            txtDescrizione.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDescrizione.Location = new Point(297, 58);
            txtDescrizione.Margin = new Padding(6, 6, 6, 6);
            txtDescrizione.MaxLength = 250;
            txtDescrizione.Name = "txtDescrizione";
            txtDescrizione.Size = new Size(405, 39);
            txtDescrizione.TabIndex = 1;
            // 
            // lblTipo
            // 
            lblTipo.AutoSize = true;
            lblTipo.Location = new Point(56, 161);
            lblTipo.Margin = new Padding(6, 0, 6, 0);
            lblTipo.Name = "lblTipo";
            lblTipo.Size = new Size(66, 32);
            lblTipo.TabIndex = 4;
            lblTipo.Text = "Tipo:";
            // 
            // rdbR
            // 
            rdbR.AutoSize = true;
            rdbR.Location = new Point(297, 156);
            rdbR.Margin = new Padding(6, 6, 6, 6);
            rdbR.Name = "rdbR";
            rdbR.Size = new Size(59, 36);
            rdbR.TabIndex = 5;
            rdbR.TabStop = true;
            rdbR.Text = "R";
            rdbR.UseVisualStyleBackColor = true;
            // 
            // rdbD
            // 
            rdbD.AutoSize = true;
            rdbD.Location = new Point(297, 210);
            rdbD.Margin = new Padding(6, 6, 6, 6);
            rdbD.Name = "rdbD";
            rdbD.Size = new Size(62, 36);
            rdbD.TabIndex = 6;
            rdbD.TabStop = true;
            rdbD.Text = "D";
            rdbD.UseVisualStyleBackColor = true;
            // 
            // btnSalva
            // 
            btnSalva.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSalva.Location = new Point(416, 469);
            btnSalva.Margin = new Padding(6, 6, 6, 6);
            btnSalva.Name = "btnSalva";
            btnSalva.Size = new Size(139, 64);
            btnSalva.TabIndex = 7;
            btnSalva.Text = "Salva";
            btnSalva.UseVisualStyleBackColor = true;
            // 
            // btnAnnulla
            // 
            btnAnnulla.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAnnulla.Location = new Point(566, 469);
            btnAnnulla.Margin = new Padding(6, 6, 6, 6);
            btnAnnulla.Name = "btnAnnulla";
            btnAnnulla.Size = new Size(139, 64);
            btnAnnulla.TabIndex = 8;
            btnAnnulla.Text = "Annulla";
            btnAnnulla.UseVisualStyleBackColor = true;
            // 
            // DestinazioniRDDetailForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(743, 576);
            Controls.Add(btnAnnulla);
            Controls.Add(btnSalva);
            Controls.Add(rdbD);
            Controls.Add(rdbR);
            Controls.Add(lblTipo);
            Controls.Add(txtDescrizione);
            Controls.Add(lblDescrizione);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(6, 6, 6, 6);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DestinazioniRDDetailForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Dettaglio Destinazioni R/D";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
    }
}

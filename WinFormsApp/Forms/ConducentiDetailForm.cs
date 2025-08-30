// File: Forms/ConducentiDetailForm.cs
// Questo form permette l'inserimento, la modifica e la visualizzazione dettagliata di un conducente.
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using System;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace FormulariRif_G.Forms
{
    public partial class ConducentiDetailForm : Form
    {
        private readonly IGenericRepository<Conducente> _conducenteRepository;
        private Conducente? _currentConducente;
        private bool _isReadOnly;

        // Dichiarazioni dei controlli
        private System.Windows.Forms.Label lblDescrizione;
        private System.Windows.Forms.TextBox txtDescrizione;
        private System.Windows.Forms.Label lblContatto;
        private System.Windows.Forms.TextBox txtContatto;
        private System.Windows.Forms.Label lblTipo;
        private System.Windows.Forms.RadioButton rdbDipendente;
        private System.Windows.Forms.RadioButton rdbTrasportatoreEsterno;
        private System.Windows.Forms.Button btnSalva;
        private System.Windows.Forms.Button btnAnnulla;

        public ConducentiDetailForm(IGenericRepository<Conducente> conducenteRepository)
        {
            InitializeComponent();
            _conducenteRepository = conducenteRepository;

            if (btnSalva != null) btnSalva.Click += btnSalvaClick;
            if (btnAnnulla != null) btnAnnulla.Click += btnAnnullaClick;
        }

        /// <summary>
        /// Imposta il conducente da visualizzare o modificare.
        /// </summary>
        /// <param name="conducente">L'oggetto Conducente.</param>
        /// <param name="isReadOnly">Se true, il form sarà in modalità sola lettura.</param>
        public void SetConducente(Conducente conducente, bool isReadOnly = false)
        {
            _currentConducente = conducente;
            _isReadOnly = isReadOnly;
            LoadConducenteData();
            SetFormMode();
        }

        /// <summary>
        /// Carica i dati del conducente nei controlli del form.
        /// </summary>
        private void LoadConducenteData()
        {
            if (_currentConducente != null)
            {
                txtDescrizione.Text = _currentConducente.Descrizione;
                txtContatto.Text = _currentConducente.Contatto;
                if (_currentConducente.Tipo == 0)
                {
                    rdbDipendente.Checked = true;
                }
                else if (_currentConducente.Tipo == 1)
                {
                    rdbTrasportatoreEsterno.Checked = true;
                }
            }
            else
            {
                txtDescrizione.Text = string.Empty;
                txtContatto.Text = string.Empty;
                rdbDipendente.Checked = true; // Default per nuovi conducenti
            }
        }

        /// <summary>
        /// Imposta i controlli del form in base alla modalità.
        /// </summary>
        private void SetFormMode()
        {
            txtDescrizione.ReadOnly = _isReadOnly;
            txtContatto.ReadOnly = _isReadOnly;
            rdbDipendente.Enabled = !_isReadOnly;
            rdbTrasportatoreEsterno.Enabled = !_isReadOnly;

            if (btnSalva != null) btnSalva.Visible = !_isReadOnly;
            if (btnAnnulla != null) btnAnnulla.Visible = true;

            this.Text = _isReadOnly ? "Dettagli Conducente" : (_currentConducente?.Id == 0 ? "Nuovo Conducente" : "Modifica Conducente");
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

            if (_currentConducente == null)
            {
                _currentConducente = new Conducente();
            }

            _currentConducente.Descrizione = txtDescrizione.Text.Trim();
            _currentConducente.Contatto = txtContatto.Text.Trim();
            _currentConducente.Tipo = rdbDipendente.Checked ? 0 : 1;

            try
            {
                if (_currentConducente.Id == 0)
                {
                    await _conducenteRepository.AddAsync(_currentConducente);
                    MessageBox.Show("Conducente aggiunto con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _conducenteRepository.Update(_currentConducente);
                    MessageBox.Show("Conducente modificato con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                await _conducenteRepository.SaveChangesAsync();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio del conducente: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (!rdbDipendente.Checked && !rdbTrasportatoreEsterno.Checked)
            {
                MessageBox.Show("Selezionare il tipo di conducente (Dipendente o Trasportatore Esterno).", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            this.lblDescrizione = new System.Windows.Forms.Label();
            this.txtDescrizione = new System.Windows.Forms.TextBox();
            this.lblContatto = new System.Windows.Forms.Label();
            this.txtContatto = new System.Windows.Forms.TextBox();
            this.lblTipo = new System.Windows.Forms.Label();
            this.rdbDipendente = new System.Windows.Forms.RadioButton();
            this.rdbTrasportatoreEsterno = new System.Windows.Forms.RadioButton();
            this.btnSalva = new System.Windows.Forms.Button();
            this.btnAnnulla = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblDescrizione
            // 
            this.lblDescrizione.AutoSize = true;
            this.lblDescrizione.Location = new System.Drawing.Point(30, 30);
            this.lblDescrizione.Name = "lblDescrizione";
            this.lblDescrizione.Size = new System.Drawing.Size(73, 15);
            this.lblDescrizione.TabIndex = 0;
            this.lblDescrizione.Text = "Descrizione:";
            // 
            // txtDescrizione
            // 
            this.txtDescrizione.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescrizione.Location = new System.Drawing.Point(160, 27);
            this.txtDescrizione.MaxLength = 250;
            this.txtDescrizione.Name = "txtDescrizione";
            this.txtDescrizione.Size = new System.Drawing.Size(220, 23);
            this.txtDescrizione.TabIndex = 1;
            // 
            // lblContatto
            // 
            this.lblContatto.AutoSize = true;
            this.lblContatto.Location = new System.Drawing.Point(30, 70);
            this.lblContatto.Name = "lblContatto";
            this.lblContatto.Size = new System.Drawing.Size(59, 15);
            this.lblContatto.TabIndex = 2;
            this.lblContatto.Text = "Contatto:";
            // 
            // txtContatto
            // 
            this.txtContatto.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtContatto.Location = new System.Drawing.Point(160, 67);
            this.txtContatto.MaxLength = 50;
            this.txtContatto.Name = "txtContatto";
            this.txtContatto.Size = new System.Drawing.Size(220, 23);
            this.txtContatto.TabIndex = 3;
            // 
            // lblTipo
            // 
            this.lblTipo.AutoSize = true;
            this.lblTipo.Location = new System.Drawing.Point(30, 110);
            this.lblTipo.Name = "lblTipo";
            this.lblTipo.Size = new System.Drawing.Size(33, 15);
            this.lblTipo.TabIndex = 4;
            this.lblTipo.Text = "Tipo:";
            // 
            // rdbDipendente
            // 
            this.rdbDipendente.AutoSize = true;
            this.rdbDipendente.Location = new System.Drawing.Point(160, 108);
            this.rdbDipendente.Name = "rdbDipendente";
            this.rdbDipendente.Size = new System.Drawing.Size(83, 19);
            this.rdbDipendente.TabIndex = 5;
            this.rdbDipendente.TabStop = true;
            this.rdbDipendente.Text = "Dipendente";
            this.rdbDipendente.UseVisualStyleBackColor = true;
            // 
            // rdbTrasportatoreEsterno
            // 
            this.rdbTrasportatoreEsterno.AutoSize = true;
            this.rdbTrasportatoreEsterno.Location = new System.Drawing.Point(160, 133);
            this.rdbTrasportatoreEsterno.Name = "rdbTrasportatoreEsterno";
            this.rdbTrasportatoreEsterno.Size = new System.Drawing.Size(135, 19);
            this.rdbTrasportatoreEsterno.TabIndex = 6;
            this.rdbTrasportatoreEsterno.TabStop = true;
            this.rdbTrasportatoreEsterno.Text = "Trasportatore Esterno";
            this.rdbTrasportatoreEsterno.UseVisualStyleBackColor = true;
            // 
            // btnSalva
            // 
            this.btnSalva.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalva.Location = new System.Drawing.Point(224, 220);
            this.btnSalva.Name = "btnSalva";
            this.btnSalva.Size = new System.Drawing.Size(75, 30);
            this.btnSalva.TabIndex = 7;
            this.btnSalva.Text = "Salva";
            this.btnSalva.UseVisualStyleBackColor = true;
            // 
            // btnAnnulla
            // 
            this.btnAnnulla.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAnnulla.Location = new System.Drawing.Point(305, 220);
            this.btnAnnulla.Name = "btnAnnulla";
            this.btnAnnulla.Size = new System.Drawing.Size(75, 30);
            this.btnAnnulla.TabIndex = 8;
            this.btnAnnulla.Text = "Annulla";
            this.btnAnnulla.UseVisualStyleBackColor = true;
            // 
            // ConducentiDetailForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 270);
            this.Controls.Add(this.btnAnnulla);
            this.Controls.Add(this.btnSalva);
            this.Controls.Add(this.rdbTrasportatoreEsterno);
            this.Controls.Add(this.rdbDipendente);
            this.Controls.Add(this.lblTipo);
            this.Controls.Add(this.txtContatto);
            this.Controls.Add(this.lblContatto);
            this.Controls.Add(this.txtDescrizione);
            this.Controls.Add(this.lblDescrizione);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConducentiDetailForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dettaglio Conducente";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}

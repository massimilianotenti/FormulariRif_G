// File: Forms/AutomezziDetailForm.cs
// Questo form permette di inserire o modificare un singolo automezzo.
using FormulariRif_G.Data;
using FormulariRif_G.Models;

namespace FormulariRif_G.Forms
{
    public partial class AutomezziDetailForm : Form
    {
        private readonly IGenericRepository<Automezzo> _automezzoRepository;
        private Automezzo? _currentAutomezzo;

        public AutomezziDetailForm(IGenericRepository<Automezzo> automezzoRepository)
        {
            InitializeComponent();
            _automezzoRepository = automezzoRepository;
        }

        /// <summary>
        /// Imposta l'automezzo da visualizzare o modificare.
        /// </summary>
        /// <param name="automezzo">L'oggetto Automezzo.</param>
        public void SetAutomezzo(Automezzo automezzo)
        {
            _currentAutomezzo = automezzo;
            LoadAutomezzoData();
        }

        /// <summary>
        /// Carica i dati dell'automezzo nei controlli del form.
        /// </summary>
        private void LoadAutomezzoData()
        {
            if (_currentAutomezzo != null)
            {
                txtDescrizione.Text = _currentAutomezzo.Descrizione;
                txtTarga.Text = _currentAutomezzo.Targa;
            }
            else
            {
                txtDescrizione.Text = string.Empty;
                txtTarga.Text = string.Empty;
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

            if (_currentAutomezzo == null)
            {
                _currentAutomezzo = new Automezzo();
            }

            _currentAutomezzo.Descrizione = txtDescrizione.Text.Trim();
            _currentAutomezzo.Targa = txtTarga.Text.Trim();

            try
            {
                if (_currentAutomezzo.Id == 0) // Nuovo automezzo
                {
                    await _automezzoRepository.AddAsync(_currentAutomezzo);
                }
                else // Automezzo esistente
                {
                    _automezzoRepository.Update(_currentAutomezzo);
                }
                await _automezzoRepository.SaveChangesAsync();
                MessageBox.Show("Automezzo salvato con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio dell'automezzo: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Esegue la validazione dei campi di input.
        /// </summary>
        /// <returns>True se l'input è valido, altrimenti False.</returns>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtDescrizione.Text))
            {
                MessageBox.Show("Descrizione è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDescrizione.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtTarga.Text))
            {
                MessageBox.Show("Targa è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTarga.Focus();
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
            this.lblDescrizione = new System.Windows.Forms.Label();
            this.txtDescrizione = new System.Windows.Forms.TextBox();
            this.lblTarga = new System.Windows.Forms.Label();
            this.txtTarga = new System.Windows.Forms.TextBox();
            this.btnSalva = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblDescrizione
            //
            this.lblDescrizione.AutoSize = true;
            this.lblDescrizione.Location = new System.Drawing.Point(20, 30);
            this.lblDescrizione.Name = "lblDescrizione";
            this.lblDescrizione.Size = new System.Drawing.Size(71, 15);
            this.lblDescrizione.TabIndex = 0;
            this.lblDescrizione.Text = "Descrizione:";
            //
            // txtDescrizione
            //
            this.txtDescrizione.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescrizione.Location = new System.Drawing.Point(100, 27);
            this.txtDescrizione.Name = "txtDescrizione";
            this.txtDescrizione.Size = new System.Drawing.Size(270, 23);
            this.txtDescrizione.TabIndex = 1;
            //
            // lblTarga
            //
            this.lblTarga.AutoSize = true;
            this.lblTarga.Location = new System.Drawing.Point(20, 70);
            this.lblTarga.Name = "lblTarga";
            this.lblTarga.Size = new System.Drawing.Size(40, 15);
            this.lblTarga.TabIndex = 2;
            this.lblTarga.Text = "Targa:";
            //
            // txtTarga
            //
            this.txtTarga.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTarga.Location = new System.Drawing.Point(100, 67);
            this.txtTarga.Name = "txtTarga";
            this.txtTarga.Size = new System.Drawing.Size(270, 23);
            this.txtTarga.TabIndex = 3;
            //
            // btnSalva
            //
            this.btnSalva.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalva.Location = new System.Drawing.Point(295, 110);
            this.btnSalva.Name = "btnSalva";
            this.btnSalva.Size = new System.Drawing.Size(75, 30);
            this.btnSalva.TabIndex = 4;
            this.btnSalva.Text = "Salva";
            this.btnSalva.UseVisualStyleBackColor = true;
            this.btnSalva.Click += new System.EventHandler(this.btnSalva_Click);
            //
            // AutomezziDetailForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 150);
            this.Controls.Add(this.btnSalva);
            this.Controls.Add(this.txtTarga);
            this.Controls.Add(this.lblTarga);
            this.Controls.Add(this.txtDescrizione);
            this.Controls.Add(this.lblDescrizione);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AutomezziDetailForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dettagli Automezzo";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblDescrizione;
        private System.Windows.Forms.TextBox txtDescrizione;
        private System.Windows.Forms.Label lblTarga;
        private System.Windows.Forms.TextBox txtTarga;
        private System.Windows.Forms.Button btnSalva;

        #endregion
    }
}

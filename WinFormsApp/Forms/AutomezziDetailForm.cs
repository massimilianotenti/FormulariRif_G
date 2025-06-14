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
            lblDescrizione = new Label();
            txtDescrizione = new TextBox();
            lblTarga = new Label();
            txtTarga = new TextBox();
            btnSalva = new Button();
            SuspendLayout();
            // 
            // lblDescrizione
            // 
            lblDescrizione.AutoSize = true;
            lblDescrizione.Location = new Point(20, 30);
            lblDescrizione.Name = "lblDescrizione";
            lblDescrizione.Size = new Size(70, 15);
            lblDescrizione.TabIndex = 0;
            lblDescrizione.Text = "Descrizione:";
            // 
            // txtDescrizione
            // 
            txtDescrizione.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDescrizione.Location = new Point(100, 27);
            txtDescrizione.MaxLength = 255;
            txtDescrizione.Name = "txtDescrizione";
            txtDescrizione.Size = new Size(270, 23);
            txtDescrizione.TabIndex = 1;
            // 
            // lblTarga
            // 
            lblTarga.AutoSize = true;
            lblTarga.Location = new Point(20, 70);
            lblTarga.Name = "lblTarga";
            lblTarga.Size = new Size(39, 15);
            lblTarga.TabIndex = 2;
            lblTarga.Text = "Targa:";
            // 
            // txtTarga
            // 
            txtTarga.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTarga.Location = new Point(100, 67);
            txtTarga.MaxLength = 20;
            txtTarga.Name = "txtTarga";
            txtTarga.Size = new Size(270, 23);
            txtTarga.TabIndex = 3;
            // 
            // btnSalva
            // 
            btnSalva.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSalva.Location = new Point(295, 110);
            btnSalva.Name = "btnSalva";
            btnSalva.Size = new Size(75, 30);
            btnSalva.TabIndex = 4;
            btnSalva.Text = "Salva";
            btnSalva.UseVisualStyleBackColor = true;
            btnSalva.Click += btnSalva_Click;
            // 
            // AutomezziDetailForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(390, 150);
            Controls.Add(btnSalva);
            Controls.Add(txtTarga);
            Controls.Add(lblTarga);
            Controls.Add(txtDescrizione);
            Controls.Add(lblDescrizione);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AutomezziDetailForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Dettagli Automezzo";
            ResumeLayout(false);
            PerformLayout();

        }

        private System.Windows.Forms.Label lblDescrizione;
        private System.Windows.Forms.TextBox txtDescrizione;
        private System.Windows.Forms.Label lblTarga;
        private System.Windows.Forms.TextBox txtTarga;
        private System.Windows.Forms.Button btnSalva;

        #endregion
    }
}

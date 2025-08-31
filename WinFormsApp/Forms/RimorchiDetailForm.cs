﻿// File: Forms/RimorchiDetailForm.cs
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using System;
using System.Windows.Forms;

namespace FormulariRif_G.Forms
{
    public partial class RimorchiDetailForm : Form
    {
        private readonly IGenericRepository<Rimorchio> _rimorchioRepository;
        private Rimorchio _currentRimorchio;
        private bool _isReadOnly = false;

        // Controlli
        private Label lblDescrizione;
        private TextBox txtDescrizione;
        private Label lblTarga;
        private TextBox txtTarga;
        private Button btnSalva;
        private Button btnAnnulla;

        public RimorchiDetailForm(IGenericRepository<Rimorchio> rimorchioRepository)
        {
            _rimorchioRepository = rimorchioRepository;
            InitializeComponent();
            this.Load += RimorchiDetailForm_Load;
        }

        public void SetRimorchio(Rimorchio rimorchio, bool isReadOnly)
        {
            _currentRimorchio = rimorchio;
            _isReadOnly = isReadOnly;
        }

        private void RimorchiDetailForm_Load(object sender, EventArgs e)
        {
            if (_currentRimorchio != null)
            {
                txtDescrizione.Text = _currentRimorchio.Descrizione;
                txtTarga.Text = _currentRimorchio.Targa;                

                this.Text = _currentRimorchio.Id == 0 ? "Nuovo Rimorchio" : "Modifica Rimorchio";

                if (_isReadOnly)
                {
                    this.Text = $"Dettaglio Rimorchio: {_currentRimorchio.Descrizione}";
                    txtDescrizione.ReadOnly = true;
                    txtTarga.ReadOnly = true;                    
                    btnSalva.Visible = false;
                    btnAnnulla.Text = "Chiudi";
                }
            }
        }

        private async void btnSalva_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDescrizione.Text) || string.IsNullOrWhiteSpace(txtTarga.Text))
            {
                MessageBox.Show("Descrizione e Targa sono campi obbligatori.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _currentRimorchio.Descrizione = txtDescrizione.Text.Trim();
            _currentRimorchio.Targa = txtTarga.Text.Trim();            

            try
            {
                if (_currentRimorchio.Id == 0)
                {
                    await _rimorchioRepository.AddAsync(_currentRimorchio);
                }
                else
                {
                    _rimorchioRepository.Update(_currentRimorchio);
                }
                await _rimorchioRepository.SaveChangesAsync();

                MessageBox.Show("Rimorchio salvato con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAnnulla_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblDescrizione = new Label();
            txtDescrizione = new TextBox();
            lblTarga = new Label();
            txtTarga = new TextBox();
            btnSalva = new Button();
            btnAnnulla = new Button();
            SuspendLayout();
            // 
            // lblDescrizione
            // 
            lblDescrizione.AutoSize = true;
            lblDescrizione.Location = new Point(44, 65);
            lblDescrizione.Name = "lblDescrizione";
            lblDescrizione.Size = new Size(142, 32);
            lblDescrizione.TabIndex = 6;
            lblDescrizione.Text = "Descrizione:";
            // 
            // txtDescrizione
            // 
            txtDescrizione.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDescrizione.Location = new Point(213, 62);
            txtDescrizione.Name = "txtDescrizione";
            txtDescrizione.Size = new Size(473, 39);
            txtDescrizione.TabIndex = 0;
            // 
            // lblTarga
            // 
            lblTarga.AutoSize = true;
            lblTarga.Location = new Point(44, 136);
            lblTarga.Name = "lblTarga";
            lblTarga.Size = new Size(75, 32);
            lblTarga.TabIndex = 5;
            lblTarga.Text = "Targa:";
            // 
            // txtTarga
            // 
            txtTarga.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTarga.Location = new Point(213, 133);
            txtTarga.Name = "txtTarga";
            txtTarga.Size = new Size(473, 39);
            txtTarga.TabIndex = 1;
            // 
            // btnSalva
            // 
            btnSalva.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSalva.Location = new Point(371, 245);
            btnSalva.Name = "btnSalva";
            btnSalva.Size = new Size(153, 59);
            btnSalva.TabIndex = 3;
            btnSalva.Text = "Salva";
            btnSalva.UseVisualStyleBackColor = true;
            btnSalva.Click += btnSalva_Click;
            // 
            // btnAnnulla
            // 
            btnAnnulla.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAnnulla.Location = new Point(542, 245);
            btnAnnulla.Name = "btnAnnulla";
            btnAnnulla.Size = new Size(144, 59);
            btnAnnulla.TabIndex = 4;
            btnAnnulla.Text = "Annulla";
            btnAnnulla.UseVisualStyleBackColor = true;
            btnAnnulla.Click += btnAnnulla_Click;
            // 
            // RimorchiDetailForm
            // 
            ClientSize = new Size(750, 367);
            Controls.Add(btnAnnulla);
            Controls.Add(btnSalva);
            Controls.Add(txtTarga);
            Controls.Add(lblTarga);
            Controls.Add(txtDescrizione);
            Controls.Add(lblDescrizione);
            MinimumSize = new Size(400, 200);
            Name = "RimorchiDetailForm";
            Text = "Dettaglio Rimorchio";            
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        
    }
}
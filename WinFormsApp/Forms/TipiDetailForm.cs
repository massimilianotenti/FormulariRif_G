// File: Forms/TipiDetailForm.cs
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using System;
using System.Windows.Forms;

namespace FormulariRif_G.Forms
{
    public partial class TipiDetailForm : Form
    {
        private readonly IGenericRepository<Tipo> _tipoRepository;
        private Tipo _currentTipo;
        private bool _isReadOnly = false;

        // Controlli
        private Label lblDescrizione;
        private TextBox txtDescrizione;
        private Button btnSalva;
        private Button btnAnnulla;

        public TipiDetailForm(IGenericRepository<Tipo> tipoRepository)
        {
            _tipoRepository = tipoRepository;
            InitializeComponent();
            this.Load += TipiDetailForm_Load;
        }

        public void SetTipo(Tipo tipo, bool isReadOnly)
        {
            _currentTipo = tipo;
            _isReadOnly = isReadOnly;
        }

        private void TipiDetailForm_Load(object sender, EventArgs e)
        {
            if (_currentTipo != null)
            {
                txtDescrizione.Text = _currentTipo.Descrizione;

                this.Text = _currentTipo.Id == 0 ? "Nuovo Tipo" : "Modifica Tipo";

                if (_isReadOnly)
                {
                    this.Text = $"Dettaglio Tipo: {_currentTipo.Descrizione}";
                    txtDescrizione.ReadOnly = true;
                    btnSalva.Visible = false;
                    btnAnnulla.Text = "Chiudi";
                }
            }
        }

        private async void btnSalva_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDescrizione.Text))
            {
                MessageBox.Show("Descrizione è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _currentTipo.Descrizione = txtDescrizione.Text.Trim();

            try
            {
                if (_currentTipo.Id == 0)
                {
                    await _tipoRepository.AddAsync(_currentTipo);
                }
                else
                {
                    _tipoRepository.Update(_currentTipo);
                }
                await _tipoRepository.SaveChangesAsync();

                MessageBox.Show("Tipo salvato con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            // btnSalva
            // 
            btnSalva.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSalva.Location = new Point(371, 160);
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
            btnAnnulla.Location = new Point(542, 160);
            btnAnnulla.Name = "btnAnnulla";
            btnAnnulla.Size = new Size(144, 59);
            btnAnnulla.TabIndex = 4;
            btnAnnulla.Text = "Annulla";
            btnAnnulla.UseVisualStyleBackColor = true;
            btnAnnulla.Click += btnAnnulla_Click;
            // 
            // TipiDetailForm
            // 
            ClientSize = new Size(750, 250);
            Controls.Add(btnAnnulla);
            Controls.Add(btnSalva);
            Controls.Add(txtDescrizione);
            Controls.Add(lblDescrizione);
            MinimumSize = new Size(400, 200);
            Name = "TipiDetailForm";
            Text = "Dettaglio Tipo";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}

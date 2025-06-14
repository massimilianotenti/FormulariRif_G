// File: Forms/ClientiContattiDetailForm.cs
// Questo form permette di inserire o modificare un singolo contatto per un cliente.
using FormulariRif_G.Data;
using FormulariRif_G.Models;

namespace FormulariRif_G.Forms
{
    public partial class ClientiContattiDetailForm : Form
    {
        private readonly IGenericRepository<ClienteContatto> _clienteContattoRepository;
        private ClienteContatto? _currentContatto;

        public ClientiContattiDetailForm(IGenericRepository<ClienteContatto> clienteContattoRepository)
        {
            InitializeComponent();
            _clienteContattoRepository = clienteContattoRepository;
        }

        /// <summary>
        /// Imposta il contatto da visualizzare o modificare.
        /// </summary>
        /// <param name="contatto">L'oggetto ClienteContatto.</param>
        public void SetContatto(ClienteContatto contatto)
        {
            _currentContatto = contatto;
            LoadContattoData();
        }

        /// <summary>
        /// Carica i dati del contatto nei controlli del form.
        /// </summary>
        private void LoadContattoData()
        {
            if (_currentContatto != null)
            {
                txtContatto.Text = _currentContatto.Contatto;
                txtTelefono.Text = _currentContatto.Telefono;
                txtEmail.Text = _currentContatto.Email;
                chkPredefinito.Checked = _currentContatto.Predefinito;
            }
            else
            {
                txtContatto.Text = string.Empty;
                txtTelefono.Text = string.Empty;
                txtEmail.Text = string.Empty;
                chkPredefinito.Checked = false;
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

            if (_currentContatto == null)
            {
                _currentContatto = new ClienteContatto();
            }

            _currentContatto.Contatto = txtContatto.Text.Trim();
            _currentContatto.Telefono = txtTelefono.Text.Trim();
            _currentContatto.Email = txtEmail.Text.Trim();
            _currentContatto.Predefinito = chkPredefinito.Checked;

            try
            {
                if (_currentContatto.Id == 0) // Nuovo contatto
                {
                    await _clienteContattoRepository.AddAsync(_currentContatto);
                }
                else // Contatto esistente
                {
                    _clienteContattoRepository.Update(_currentContatto);
                }
                await _clienteContattoRepository.SaveChangesAsync();
                MessageBox.Show("Contatto salvato con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio del contatto: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Esegue la validazione dei campi di input.
        /// </summary>
        /// <returns>True se l'input è valido, altrimenti False.</returns>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtContatto.Text))
            {
                MessageBox.Show("Contatto è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtContatto.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtTelefono.Text))
            {
                MessageBox.Show("Telefono è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTelefono.Focus();
                return false;
            }
            // Email non è obbligatorio nel modello, quindi non lo valido qui a meno che non sia un requisito specifico
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
            lblContatto = new Label();
            txtContatto = new TextBox();
            lblTelefono = new Label();
            txtTelefono = new TextBox();
            lblEmail = new Label();
            txtEmail = new TextBox();
            chkPredefinito = new CheckBox();
            btnSalva = new Button();
            SuspendLayout();
            // 
            // lblContatto
            // 
            lblContatto.AutoSize = true;
            lblContatto.Location = new Point(20, 30);
            lblContatto.Name = "lblContatto";
            lblContatto.Size = new Size(57, 15);
            lblContatto.TabIndex = 0;
            lblContatto.Text = "Contatto:";
            // 
            // txtContatto
            // 
            txtContatto.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtContatto.Location = new Point(100, 27);
            txtContatto.MaxLength = 100;
            txtContatto.Name = "txtContatto";
            txtContatto.Size = new Size(270, 23);
            txtContatto.TabIndex = 1;
            // 
            // lblTelefono
            // 
            lblTelefono.AutoSize = true;
            lblTelefono.Location = new Point(20, 70);
            lblTelefono.Name = "lblTelefono";
            lblTelefono.Size = new Size(56, 15);
            lblTelefono.TabIndex = 2;
            lblTelefono.Text = "Telefono:";
            // 
            // txtTelefono
            // 
            txtTelefono.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTelefono.Location = new Point(100, 67);
            txtTelefono.MaxLength = 50;
            txtTelefono.Name = "txtTelefono";
            txtTelefono.Size = new Size(270, 23);
            txtTelefono.TabIndex = 3;
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(20, 110);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(44, 15);
            lblEmail.TabIndex = 4;
            lblEmail.Text = "E-mail:";
            // 
            // txtEmail
            // 
            txtEmail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtEmail.Location = new Point(100, 107);
            txtEmail.MaxLength = 50;
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(270, 23);
            txtEmail.TabIndex = 5;
            // 
            // chkPredefinito
            // 
            chkPredefinito.AutoSize = true;
            chkPredefinito.Location = new Point(20, 145);
            chkPredefinito.Name = "chkPredefinito";
            chkPredefinito.Size = new Size(84, 19);
            chkPredefinito.TabIndex = 6;
            chkPredefinito.Text = "Predefinito";
            chkPredefinito.UseVisualStyleBackColor = true;
            // 
            // btnSalva
            // 
            btnSalva.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSalva.Location = new Point(295, 170);
            btnSalva.Name = "btnSalva";
            btnSalva.Size = new Size(75, 30);
            btnSalva.TabIndex = 7;
            btnSalva.Text = "Salva";
            btnSalva.UseVisualStyleBackColor = true;
            btnSalva.Click += btnSalva_Click;
            // 
            // ClientiContattiDetailForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(390, 215);
            Controls.Add(btnSalva);
            Controls.Add(chkPredefinito);
            Controls.Add(txtEmail);
            Controls.Add(lblEmail);
            Controls.Add(txtTelefono);
            Controls.Add(lblTelefono);
            Controls.Add(txtContatto);
            Controls.Add(lblContatto);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ClientiContattiDetailForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Dettagli Contatto Cliente";
            ResumeLayout(false);
            PerformLayout();

        }

        private System.Windows.Forms.Label lblContatto;
        private System.Windows.Forms.TextBox txtContatto;
        private System.Windows.Forms.Label lblTelefono;
        private System.Windows.Forms.TextBox txtTelefono;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.CheckBox chkPredefinito;
        private System.Windows.Forms.Button btnSalva;

        #endregion
    }
}

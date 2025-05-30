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
                chkPredefinito.Checked = _currentContatto.Predefinito ?? false;
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
            this.lblContatto = new System.Windows.Forms.Label();
            this.txtContatto = new System.Windows.Forms.TextBox();
            this.lblTelefono = new System.Windows.Forms.Label();
            this.txtTelefono = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.chkPredefinito = new System.Windows.Forms.CheckBox();
            this.btnSalva = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblContatto
            //
            this.lblContatto.AutoSize = true;
            this.lblContatto.Location = new System.Drawing.Point(20, 30);
            this.lblContatto.Name = "lblContatto";
            this.lblContatto.Size = new System.Drawing.Size(59, 15);
            this.lblContatto.TabIndex = 0;
            this.lblContatto.Text = "Contatto:";
            //
            // txtContatto
            //
            this.txtContatto.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtContatto.Location = new System.Drawing.Point(100, 27);
            this.txtContatto.Name = "txtContatto";
            this.txtContatto.Size = new System.Drawing.Size(270, 23);
            this.txtContatto.TabIndex = 1;
            //
            // lblTelefono
            //
            this.lblTelefono.AutoSize = true;
            this.lblTelefono.Location = new System.Drawing.Point(20, 70);
            this.lblTelefono.Name = "lblTelefono";
            this.lblTelefono.Size = new System.Drawing.Size(54, 15);
            this.lblTelefono.TabIndex = 2;
            this.lblTelefono.Text = "Telefono:";
            //
            // txtTelefono
            //
            this.txtTelefono.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTelefono.Location = new System.Drawing.Point(100, 67);
            this.txtTelefono.Name = "txtTelefono";
            this.txtTelefono.Size = new System.Drawing.Size(270, 23);
            this.txtTelefono.TabIndex = 3;
            //
            // lblEmail
            //
            this.lblEmail.AutoSize = true;
            this.lblEmail.Location = new System.Drawing.Point(20, 110);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(44, 15);
            this.lblEmail.TabIndex = 4;
            this.lblEmail.Text = "E-mail:";
            //
            // txtEmail
            //
            this.txtEmail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEmail.Location = new System.Drawing.Point(100, 107);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(270, 23);
            this.txtEmail.TabIndex = 5;
            //
            // chkPredefinito
            //
            this.chkPredefinito.AutoSize = true;
            this.chkPredefinito.Location = new System.Drawing.Point(20, 145);
            this.chkPredefinito.Name = "chkPredefinito";
            this.chkPredefinito.Size = new System.Drawing.Size(86, 19);
            this.chkPredefinito.TabIndex = 6;
            this.chkPredefinito.Text = "Predefinito";
            this.chkPredefinito.UseVisualStyleBackColor = true;
            //
            // btnSalva
            //
            this.btnSalva.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalva.Location = new System.Drawing.Point(295, 170);
            this.btnSalva.Name = "btnSalva";
            this.btnSalva.Size = new System.Drawing.Size(75, 30);
            this.btnSalva.TabIndex = 7;
            this.btnSalva.Text = "Salva";
            this.btnSalva.UseVisualStyleBackColor = true;
            this.btnSalva.Click += new System.EventHandler(this.btnSalva_Click);
            //
            // ClientiContattiDetailForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 215);
            this.Controls.Add(this.btnSalva);
            this.Controls.Add(this.chkPredefinito);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.lblEmail);
            this.Controls.Add(this.txtTelefono);
            this.Controls.Add(this.lblTelefono);
            this.Controls.Add(this.txtContatto);
            this.Controls.Add(this.lblContatto);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ClientiContattiDetailForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dettagli Contatto Cliente";
            this.ResumeLayout(false);
            this.PerformLayout();

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

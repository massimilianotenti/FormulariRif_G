// File: Forms/UtentiDetailForm.cs
// Questo form permette l'inserimento, la modifica e la visualizzazione dettagliata di un utente.
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using FormulariRif_G.Utils; // Per PasswordHasher (BCrypt)

namespace FormulariRif_G.Forms
{
    public partial class UtentiDetailForm : Form
    {
        private readonly IGenericRepository<Utente> _utenteRepository;
        private Utente? _currentUtente;
        private bool _isReadOnly;

        public UtentiDetailForm(IGenericRepository<Utente> utenteRepository)
        {
            InitializeComponent();
            _utenteRepository = utenteRepository;
        }

        /// <summary>
        /// Imposta l'utente da visualizzare o modificare.
        /// </summary>
        /// <param name="utente">L'oggetto Utente.</param>
        /// <param name="isReadOnly">Se true, il form sarà in modalità sola lettura.</param>
        public void SetUtente(Utente utente, bool isReadOnly = false)
        {
            _currentUtente = utente;
            _isReadOnly = isReadOnly;
            LoadUtenteData();
            SetFormMode();
        }

        /// <summary>
        /// Carica i dati dell'utente nei controlli del form.
        /// </summary>
        private void LoadUtenteData()
        {
            if (_currentUtente != null)
            {
                chkAdmin.Checked = _currentUtente.Admin ?? false;
                txtNomeUtente.Text = _currentUtente.NomeUtente;
                // Non caricare la password hash nel campo di testo della password per motivi di sicurezza.
                // Lascia il campo vuoto o usa una stringa fittizia.
                // txtPassword.Text = _currentUtente.Password; // NON FARE QUESTO!
                txtPassword.Text = string.Empty; // Lascia vuoto per non esporre l'hash
                txtEmail.Text = _currentUtente.Email;
                chkMustChangePassword.Checked = _currentUtente.MustChangePassword;
            }
            else
            {
                chkAdmin.Checked = false;
                txtNomeUtente.Text = string.Empty;
                txtPassword.Text = string.Empty;
                txtEmail.Text = string.Empty;
                chkMustChangePassword.Checked = true; // Per i nuovi utenti, forza il cambio password
            }
        }

        /// <summary>
        /// Imposta i controlli del form in base alla modalità.
        /// </summary>
        private void SetFormMode()
        {
            chkAdmin.Enabled = !_isReadOnly;
            txtNomeUtente.ReadOnly = _isReadOnly;
            txtPassword.ReadOnly = _isReadOnly;
            txtEmail.ReadOnly = _isReadOnly;
            chkMustChangePassword.Enabled = !_isReadOnly;

            btnSalva.Visible = !_isReadOnly;
            this.Text = _isReadOnly ? "Dettagli Utente" : (_currentUtente?.Id == 0 ? "Nuovo Utente" : "Modifica Utente");
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

            if (_currentUtente == null)
            {
                _currentUtente = new Utente();
            }

            _currentUtente.Admin = chkAdmin.Checked;
            _currentUtente.NomeUtente = txtNomeUtente.Text.Trim();
            _currentUtente.Email = txtEmail.Text.Trim();
            _currentUtente.MustChangePassword = chkMustChangePassword.Checked;

            // Hash della password solo se è stata inserita una nuova password
            if (!string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                _currentUtente.Password = BCrypt.Net.BCrypt.HashPassword(txtPassword.Text);
            }
            else if (_currentUtente.Id == 0) // Se è un nuovo utente e la password è vuota, è un errore
            {
                MessageBox.Show("Per un nuovo utente, la password è obbligatoria.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }
            // Se è un utente esistente e la password è vuota, significa che non è stata modificata,
            // quindi manteniamo l'hash esistente nel DB.

            try
            {
                if (_currentUtente.Id == 0)
                {
                    await _utenteRepository.AddAsync(_currentUtente);
                    MessageBox.Show("Utente aggiunto con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _utenteRepository.Update(_currentUtente);
                    MessageBox.Show("Utente modificato con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                await _utenteRepository.SaveChangesAsync();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio dell'utente: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Esegue la validazione dei campi di input.
        /// </summary>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtNomeUtente.Text))
            {
                MessageBox.Show("Nome Utente è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNomeUtente.Focus();
                return false;
            }
            // La validazione della password per i nuovi utenti è gestita direttamente nel btnSalva_Click
            return true;
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Annulla".
        /// </summary>
        private void btnAnnulla_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Codice generato dal designer per UtentiDetailForm
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
            this.lblNomeUtente = new System.Windows.Forms.Label();
            this.txtNomeUtente = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.chkAdmin = new System.Windows.Forms.CheckBox();
            this.btnSalva = new System.Windows.Forms.Button();
            this.btnAnnulla = new System.Windows.Forms.Button();
            this.chkMustChangePassword = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            //
            // lblNomeUtente
            //
            this.lblNomeUtente.AutoSize = true;
            this.lblNomeUtente.Location = new System.Drawing.Point(30, 30);
            this.lblNomeUtente.Name = "lblNomeUtente";
            this.lblNomeUtente.Size = new System.Drawing.Size(81, 15);
            this.lblNomeUtente.TabIndex = 0;
            this.lblNomeUtente.Text = "Nome Utente:";
            //
            // txtNomeUtente
            //
            this.txtNomeUtente.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNomeUtente.Location = new System.Drawing.Point(130, 27);
            this.txtNomeUtente.Name = "txtNomeUtente";
            this.txtNomeUtente.Size = new System.Drawing.Size(250, 23);
            this.txtNomeUtente.TabIndex = 1;
            //
            // lblPassword
            //
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(30, 70);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(60, 15);
            this.lblPassword.TabIndex = 2;
            this.lblPassword.Text = "Password:";
            //
            // txtPassword
            //
            this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPassword.Location = new System.Drawing.Point(130, 67);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(250, 23);
            this.txtPassword.TabIndex = 3;
            this.txtPassword.UseSystemPasswordChar = true; // Nasconde i caratteri della password
            //
            // lblEmail
            //
            this.lblEmail.AutoSize = true;
            this.lblEmail.Location = new System.Drawing.Point(30, 110);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(44, 15);
            this.lblEmail.TabIndex = 6;
            this.lblEmail.Text = "E-mail:";
            //
            // txtEmail
            //
            this.txtEmail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEmail.Location = new System.Drawing.Point(130, 107);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(250, 23);
            this.txtEmail.TabIndex = 7;
            //
            // chkAdmin
            //
            this.chkAdmin.AutoSize = true;
            this.chkAdmin.Location = new System.Drawing.Point(130, 140);
            this.chkAdmin.Name = "chkAdmin";
            this.chkAdmin.Size = new System.Drawing.Size(63, 19);
            this.chkAdmin.TabIndex = 8;
            this.chkAdmin.Text = "Admin";
            this.chkAdmin.UseVisualStyleBackColor = true;
            //
            // btnSalva
            //
            this.btnSalva.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalva.Location = new System.Drawing.Point(224, 220);
            this.btnSalva.Name = "btnSalva";
            this.btnSalva.Size = new System.Drawing.Size(75, 30);
            this.btnSalva.TabIndex = 9;
            this.btnSalva.Text = "Salva";
            this.btnSalva.UseVisualStyleBackColor = true;
            this.btnSalva.Click += new System.EventHandler(this.btnSalva_Click);
            //
            // btnAnnulla
            //
            this.btnAnnulla.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAnnulla.Location = new System.Drawing.Point(305, 220);
            this.btnAnnulla.Name = "btnAnnulla";
            this.btnAnnulla.Size = new System.Drawing.Size(75, 30);
            this.btnAnnulla.TabIndex = 10;
            this.btnAnnulla.Text = "Annulla";
            this.btnAnnulla.UseVisualStyleBackColor = true;
            this.btnAnnulla.Click += new System.EventHandler(this.btnAnnulla_Click);
            //
            // chkMustChangePassword
            //
            this.chkMustChangePassword.AutoSize = true;
            this.chkMustChangePassword.Location = new System.Drawing.Point(130, 165);
            this.chkMustChangePassword.Name = "chkMustChangePassword";
            this.chkMustChangePassword.Size = new System.Drawing.Size(149, 19);
            this.chkMustChangePassword.TabIndex = 11;
            this.chkMustChangePassword.Text = "Forza cambio password";
            this.chkMustChangePassword.UseVisualStyleBackColor = true;
            //
            // UtentiDetailForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 270);
            this.Controls.Add(this.chkMustChangePassword);
            this.Controls.Add(this.btnAnnulla);
            this.Controls.Add(this.btnSalva);
            this.Controls.Add(this.chkAdmin);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.lblEmail);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtNomeUtente);
            this.Controls.Add(this.lblNomeUtente);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UtentiDetailForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dettaglio Utente";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblNomeUtente;
        private System.Windows.Forms.TextBox txtNomeUtente;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.CheckBox chkAdmin;
        private System.Windows.Forms.Button btnSalva;
        private System.Windows.Forms.Button btnAnnulla;
        private System.Windows.Forms.CheckBox chkMustChangePassword;

        #endregion
    }
}

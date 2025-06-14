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
            lblNomeUtente = new Label();
            txtNomeUtente = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            lblEmail = new Label();
            txtEmail = new TextBox();
            chkAdmin = new CheckBox();
            btnSalva = new Button();
            btnAnnulla = new Button();
            chkMustChangePassword = new CheckBox();
            SuspendLayout();
            // 
            // lblNomeUtente
            // 
            lblNomeUtente.AutoSize = true;
            lblNomeUtente.Location = new Point(30, 30);
            lblNomeUtente.Name = "lblNomeUtente";
            lblNomeUtente.Size = new Size(81, 15);
            lblNomeUtente.TabIndex = 0;
            lblNomeUtente.Text = "Nome Utente:";
            // 
            // txtNomeUtente
            // 
            txtNomeUtente.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNomeUtente.Location = new Point(130, 27);
            txtNomeUtente.MaxLength = 50;
            txtNomeUtente.Name = "txtNomeUtente";
            txtNomeUtente.Size = new Size(250, 23);
            txtNomeUtente.TabIndex = 1;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(30, 70);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(60, 15);
            lblPassword.TabIndex = 2;
            lblPassword.Text = "Password:";
            // 
            // txtPassword
            // 
            txtPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPassword.Location = new Point(130, 67);
            txtPassword.MaxLength = 20;
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(250, 23);
            txtPassword.TabIndex = 3;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(30, 110);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(44, 15);
            lblEmail.TabIndex = 6;
            lblEmail.Text = "E-mail:";
            // 
            // txtEmail
            // 
            txtEmail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtEmail.Location = new Point(130, 107);
            txtEmail.MaxLength = 50;
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(250, 23);
            txtEmail.TabIndex = 7;
            // 
            // chkAdmin
            // 
            chkAdmin.AutoSize = true;
            chkAdmin.Location = new Point(130, 140);
            chkAdmin.Name = "chkAdmin";
            chkAdmin.Size = new Size(62, 19);
            chkAdmin.TabIndex = 8;
            chkAdmin.Text = "Admin";
            chkAdmin.UseVisualStyleBackColor = true;
            // 
            // btnSalva
            // 
            btnSalva.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSalva.Location = new Point(224, 220);
            btnSalva.Name = "btnSalva";
            btnSalva.Size = new Size(75, 30);
            btnSalva.TabIndex = 9;
            btnSalva.Text = "Salva";
            btnSalva.UseVisualStyleBackColor = true;
            btnSalva.Click += btnSalva_Click;
            // 
            // btnAnnulla
            // 
            btnAnnulla.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAnnulla.Location = new Point(305, 220);
            btnAnnulla.Name = "btnAnnulla";
            btnAnnulla.Size = new Size(75, 30);
            btnAnnulla.TabIndex = 10;
            btnAnnulla.Text = "Annulla";
            btnAnnulla.UseVisualStyleBackColor = true;
            btnAnnulla.Click += btnAnnulla_Click;
            // 
            // chkMustChangePassword
            // 
            chkMustChangePassword.AutoSize = true;
            chkMustChangePassword.Location = new Point(130, 165);
            chkMustChangePassword.Name = "chkMustChangePassword";
            chkMustChangePassword.Size = new Size(150, 19);
            chkMustChangePassword.TabIndex = 11;
            chkMustChangePassword.Text = "Forza cambio password";
            chkMustChangePassword.UseVisualStyleBackColor = true;
            // 
            // UtentiDetailForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(400, 270);
            Controls.Add(chkMustChangePassword);
            Controls.Add(btnAnnulla);
            Controls.Add(btnSalva);
            Controls.Add(chkAdmin);
            Controls.Add(txtEmail);
            Controls.Add(lblEmail);
            Controls.Add(txtPassword);
            Controls.Add(lblPassword);
            Controls.Add(txtNomeUtente);
            Controls.Add(lblNomeUtente);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UtentiDetailForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Dettaglio Utente";
            ResumeLayout(false);
            PerformLayout();

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

// File: Forms/LoginForm.cs
// Questo form gestisce il processo di autenticazione dell'utente.
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using FormulariRif_G.Utils; // Per CurrentUser, PasswordHasher
using Microsoft.Extensions.DependencyInjection; // Per IServiceProvider

namespace FormulariRif_G.Forms
{
    public partial class LoginForm : Form
    {
        private readonly IGenericRepository<Utente> _utenteRepository;
        private readonly IServiceProvider _serviceProvider; // Per risolvere MainForm

        public LoginForm(IGenericRepository<Utente> utenteRepository, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _utenteRepository = utenteRepository;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Login".
        /// Tenta di autenticare l'utente.
        /// </summary>
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Inserisci nome utente e password.", "Errore di Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Cerca l'utente per nome utente
                var user = (await _utenteRepository.FindAsync(u => u.NomeUtente == username)).FirstOrDefault();

                if (user != null)
                {
                    // Verifica la password usando BCrypt.Net.BCrypt.Verify
                    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);

                    if (isPasswordValid)
                    {
                        // Autenticazione riuscita
                        CurrentUser.SetLoggedInUser(user); // Imposta l'utente corrente
                        MessageBox.Show($"Benvenuto, {user.NomeUtente}!", "Login Riuscito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK; // Segnala successo al Program.cs
                        this.Close(); // Chiudi il form di login
                    }
                    else
                    {
                        MessageBox.Show("Password non valida.", "Errore di Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Nome utente non trovato.", "Errore di Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il login: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Esci".
        /// Chiude l'applicazione.
        /// </summary>
        private void btnEsci_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel; // Segnala annullamento al Program.cs
            this.Close(); // Chiudi il form di login
        }

        // Codice generato dal designer per LoginForm
        #region Windows Form Designer generated code

        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblUsername = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnEsci = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblUsername
            //
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new System.Drawing.Point(50, 50);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(63, 15);
            this.lblUsername.TabIndex = 0;
            this.lblUsername.Text = "Username:";
            //
            // txtUsername
            //
            this.txtUsername.Location = new System.Drawing.Point(130, 47);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(150, 23);
            this.txtUsername.TabIndex = 1;
            //
            // lblPassword
            //
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(50, 90);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(60, 15);
            this.lblPassword.TabIndex = 2;
            this.lblPassword.Text = "Password:";
            //
            // txtPassword
            //
            this.txtPassword.Location = new System.Drawing.Point(130, 87);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(150, 23);
            this.txtPassword.TabIndex = 3;
            this.txtPassword.UseSystemPasswordChar = true; // Nasconde i caratteri della password
            //
            // btnLogin
            //
            this.btnLogin.Location = new System.Drawing.Point(130, 140);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(70, 30);
            this.btnLogin.TabIndex = 4;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            //
            // btnEsci
            //
            this.btnEsci.Location = new System.Drawing.Point(210, 140);
            this.btnEsci.Name = "btnEsci";
            this.btnEsci.Size = new System.Drawing.Size(70, 30);
            this.btnEsci.TabIndex = 5;
            this.btnEsci.Text = "Esci";
            this.btnEsci.UseVisualStyleBackColor = true;
            this.btnEsci.Click += new System.EventHandler(this.btnEsci_Click);
            //
            // LoginForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 211);
            this.Controls.Add(this.btnEsci);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblUsername);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnEsci;

        #endregion
    }
}

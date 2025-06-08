// File: Forms/LoginForm.cs
// Questo form gestisce il processo di autenticazione dell'utente.
using FormulariRif_G.Data;
using FormulariRif_G.Models;
// Per CurrentUser, PasswordHasher
using FormulariRif_G.Utils;
// Per IServiceProvider
using Microsoft.Extensions.DependencyInjection; 

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

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {            
            if (e.KeyChar == (char)Keys.Enter)
            {         
                e.Handled = true;

                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;
                
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))                                
                    DoLogin(username, password);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Login".
        /// Tenta di autenticare l'utente.
        /// </summary>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Inserisci nome utente e password.", "Errore di Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DoLogin(username, password);
        }

        private async void DoLogin(string username, string password)
        {
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
                        // Imposta l'utente corrente
                        CurrentUser.SetLoggedInUser(user);
                        MessageBox.Show($"Benvenuto, {user.NomeUtente}!", "Login Riuscito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Segnala successo al Program.cs
                        this.DialogResult = DialogResult.OK; 
                        this.Close(); 
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
            // Segnala annullamento al Program.cs
            // Chiudi il form di login
            this.DialogResult = DialogResult.Cancel; 
            this.Close(); 
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
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            btnLogin = new Button();
            btnEsci = new Button();
            SuspendLayout();
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(93, 107);
            lblUsername.Margin = new Padding(6, 0, 6, 0);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(126, 32);
            lblUsername.TabIndex = 0;
            lblUsername.Text = "Username:";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(241, 100);
            txtUsername.Margin = new Padding(6, 6, 6, 6);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(275, 39);
            txtUsername.TabIndex = 1;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(93, 192);
            lblPassword.Margin = new Padding(6, 0, 6, 0);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(116, 32);
            lblPassword.TabIndex = 2;
            lblPassword.Text = "Password:";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(241, 186);
            txtPassword.Margin = new Padding(6, 6, 6, 6);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(275, 39);
            txtPassword.TabIndex = 3;
            txtPassword.UseSystemPasswordChar = true;            
            txtPassword.KeyPress += txtPassword_KeyPress;
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(241, 299);
            btnLogin.Margin = new Padding(6, 6, 6, 6);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(130, 64);
            btnLogin.TabIndex = 4;
            btnLogin.Text = "Login";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // btnEsci
            // 
            btnEsci.Location = new Point(390, 299);
            btnEsci.Margin = new Padding(6, 6, 6, 6);
            btnEsci.Name = "btnEsci";
            btnEsci.Size = new Size(130, 64);
            btnEsci.TabIndex = 5;
            btnEsci.Text = "Esci";
            btnEsci.UseVisualStyleBackColor = true;
            btnEsci.Click += btnEsci_Click;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(620, 450);
            Controls.Add(btnEsci);
            Controls.Add(btnLogin);
            Controls.Add(txtPassword);
            Controls.Add(lblPassword);
            Controls.Add(txtUsername);
            Controls.Add(lblUsername);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(6, 6, 6, 6);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Login";
            ResumeLayout(false);
            PerformLayout();

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

// File: Forms/MainForm.cs
// Questo è il form principale dell'applicazione che funge da menu.
// Ora include pulsanti per le nuove gestioni (Automezzi, Formulari Rifiuti).
using Microsoft.Extensions.DependencyInjection; // Per IServiceProvider
using FormulariRif_G.Utils; // Per CurrentUser

namespace FormulariRif_G.Forms
{
    public partial class MainForm : Form
    {
        private readonly IServiceProvider _serviceProvider;

        public MainForm(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            // Imposta la visibilità del pulsante di configurazione in base allo stato di amministratore dell'utente corrente
            btnConfigurazione.Visible = CurrentUser.IsAdmin;
            // I pulsanti per Automezzi e Formulari sono sempre visibili, ma puoi aggiungere logica di visibilità se necessario
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Gestione Clienti".
        /// Apre il form per la gestione dei clienti.
        /// </summary>
        private void btnGestioneClienti_Click(object sender, EventArgs e)
        {
            using (var clientiListForm = _serviceProvider.GetRequiredService<ClientiListForm>())
            {
                clientiListForm.ShowDialog();
            }
        }

        

        /// <summary>
        /// Gestisce il click sul pulsante "Gestione Utenti".
        /// Apre il form per la gestione degli utenti.
        /// </summary>
        private void btnGestioneUtenti_Click(object sender, EventArgs e)
        {
            using (var utentiListForm = _serviceProvider.GetRequiredService<UtentiListForm>())
            {
                utentiListForm.ShowDialog();
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Gestione Automezzi".
        /// Apre il form per la gestione degli automezzi.
        /// </summary>
        private void btnGestioneAutomezzi_Click(object sender, EventArgs e)
        {
            using (var automezziListForm = _serviceProvider.GetRequiredService<AutomezziListForm>())
            {
                automezziListForm.ShowDialog();
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Gestione Formulari Rifiuti".
        /// Apre il form per la gestione dei formulari rifiuti.
        /// </summary>
        private void btnGestioneFormulariRifiuti_Click(object sender, EventArgs e)
        {
            using (var formulariListForm = _serviceProvider.GetRequiredService<FormulariRifiutiListForm>())
            {
                formulariListForm.ShowDialog();
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Configurazione".
        /// Apre il form di configurazione. Se la configurazione viene salvata,
        /// segnala a Program.cs di riavviare l'applicazione.
        /// </summary>
        private async void btnConfigurazione_Click(object sender, EventArgs e)
        {
            using (var configForm = _serviceProvider.GetRequiredService<ConfigurazioneForm>())
            {
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    // La configurazione è stata salvata (potrebbe aver cambiato la stringa di connessione o il flag dati test).
                    // Segnaliamo a Program.cs di riavviare l'applicazione.
                    this.DialogResult = DialogResult.Retry; // Usiamo DialogResult.Retry come segnale di riavvio personalizzato
                    this.Close(); // Chiudi il MainForm per permettere il riavvio
                }
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Logout".
        /// Reindirizza alla schermata di login.
        /// </summary>
        private void btnLogout_Click(object sender, EventArgs e)
        {
            //CurrentUser.Clear(); // Pulisce l'utente corrente
            this.DialogResult = DialogResult.Cancel; // Segnala al LoginForm di non chiudere l'applicazione
            this.Close(); // Chiudi il MainForm
        }

        // Codice generato dal designer per MainForm
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
            btnGestioneClienti = new Button();
            btnGestioneUtenti = new Button();
            btnLogout = new Button();
            btnConfigurazione = new Button();
            btnGestioneAutomezzi = new Button();
            btnGestioneFormulariRifiuti = new Button();
            SuspendLayout();
            // 
            // btnGestioneClienti
            // 
            btnGestioneClienti.Location = new Point(93, 64);
            btnGestioneClienti.Margin = new Padding(6, 6, 6, 6);
            btnGestioneClienti.Name = "btnGestioneClienti";
            btnGestioneClienti.Size = new Size(371, 107);
            btnGestioneClienti.TabIndex = 0;
            btnGestioneClienti.Text = "Gestione Clienti";
            btnGestioneClienti.UseVisualStyleBackColor = true;
            btnGestioneClienti.Click += btnGestioneClienti_Click;
            // 
            // btnGestioneUtenti
            // 
            btnGestioneUtenti.Location = new Point(93, 383);
            btnGestioneUtenti.Margin = new Padding(6, 6, 6, 6);
            btnGestioneUtenti.Name = "btnGestioneUtenti";
            btnGestioneUtenti.Size = new Size(371, 107);
            btnGestioneUtenti.TabIndex = 2;
            btnGestioneUtenti.Text = "Gestione Utenti";
            btnGestioneUtenti.UseVisualStyleBackColor = true;
            btnGestioneUtenti.Click += btnGestioneUtenti_Click;
            // 
            // btnLogout
            // 
            btnLogout.Location = new Point(93, 832);
            btnLogout.Margin = new Padding(6, 6, 6, 6);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(371, 107);
            btnLogout.TabIndex = 6;
            btnLogout.Text = "Logout";
            btnLogout.UseVisualStyleBackColor = true;
            btnLogout.Click += btnLogout_Click;
            // 
            // btnConfigurazione
            // 
            btnConfigurazione.Location = new Point(93, 704);
            btnConfigurazione.Margin = new Padding(6, 6, 6, 6);
            btnConfigurazione.Name = "btnConfigurazione";
            btnConfigurazione.Size = new Size(371, 107);
            btnConfigurazione.TabIndex = 5;
            btnConfigurazione.Text = "Configurazione";
            btnConfigurazione.UseVisualStyleBackColor = true;
            btnConfigurazione.Click += btnConfigurazione_Click;
            // 
            // btnGestioneAutomezzi
            // 
            btnGestioneAutomezzi.Location = new Point(93, 511);
            btnGestioneAutomezzi.Margin = new Padding(6, 6, 6, 6);
            btnGestioneAutomezzi.Name = "btnGestioneAutomezzi";
            btnGestioneAutomezzi.Size = new Size(371, 107);
            btnGestioneAutomezzi.TabIndex = 3;
            btnGestioneAutomezzi.Text = "Gestione Automezzi";
            btnGestioneAutomezzi.UseVisualStyleBackColor = true;
            btnGestioneAutomezzi.Click += btnGestioneAutomezzi_Click;
            // 
            // btnGestioneFormulariRifiuti
            // 
            btnGestioneFormulariRifiuti.Location = new Point(93, 196);
            btnGestioneFormulariRifiuti.Margin = new Padding(6, 6, 6, 6);
            btnGestioneFormulariRifiuti.Name = "btnGestioneFormulariRifiuti";
            btnGestioneFormulariRifiuti.Size = new Size(371, 107);
            btnGestioneFormulariRifiuti.TabIndex = 4;
            btnGestioneFormulariRifiuti.Text = "Gestione Formulari Rifiuti";
            btnGestioneFormulariRifiuti.UseVisualStyleBackColor = true;
            btnGestioneFormulariRifiuti.Click += btnGestioneFormulariRifiuti_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(557, 981);
            Controls.Add(btnGestioneFormulariRifiuti);
            Controls.Add(btnGestioneAutomezzi);
            Controls.Add(btnLogout);
            Controls.Add(btnConfigurazione);
            Controls.Add(btnGestioneUtenti);
            Controls.Add(btnGestioneClienti);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(6, 6, 6, 6);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Menu Principale";
            ResumeLayout(false);

        }

        private System.Windows.Forms.Button btnGestioneClienti;
        private System.Windows.Forms.Button btnGestioneUtenti;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Button btnConfigurazione;
        private System.Windows.Forms.Button btnGestioneAutomezzi; // Dichiarazione del nuovo pulsante
        private System.Windows.Forms.Button btnGestioneFormulariRifiuti; // Dichiarazione del nuovo pulsante

        #endregion
    }
}

// File: Forms/MainForm.cs
// Questo è il form principale dell'applicazione che funge da menu.
// Ora include pulsanti per le nuove gestioni (Automezzi, Formulari Rifiuti).

// 07/07/2205 - AGGIORNAMENTO PER DEPENDENCY INJECTION E FORM MANAGER
// ------------------------------------------------------------------------------------------------------------------------------
// Il lifetime della form e del dbcontext era gestito tramite la using, la showdialog() blocca l'esecuzione del codice
// sotto fino alla chiusura della form. Se si cambia con la show() viene aperta la form e nel frattempo si chiude la
// using e quindi il dbcontext, visto che viene usato un caricamento asincrono nella form.
//
// - Va tolto la using per evitare che il dbcontext venga chiuso prima che la form sia effettivamente utilizzata.
// - Va usato AddScoped<YourDbContext>(), è lifetime più comune e generalmente consigliato per applicazioni desktop: significa
//   che una nuova istanza del DBContext viene creata per ogni "scope". Se ogni form ha il suo DBContext iniettato come "scoped",
//   allora ogni form avrà la sua istanza di DBContext e verrà disposta solo quando la form stessa viene disposta.
// - Va usato AddTransient<YourDbContext>(): Crea una nuova istanza ogni volta che viene richiesta. Potrebbe essere un'opzione,
//   ma spesso Scoped è preferibile per i DBContext per motivi di unit of work.
//
// Con AddScoped<YourDbContext>() e AddTransient<ClientiListForm>(), ogni volta che richiedi una ClientiListForm, ti verrà data
// una nuova istanza della form, e quella form riceverà una nuova istanza del DBContext (o quella esistente se è all'interno
// dello stesso scope di un servizio "genitore"). Il DBContext sarà dismesso solo quando la form che lo contiene viene dismessa.
//
// - Nella form va poi fatto il dispose del DBContext (questo sarà gestito dal DI e FormClosed event, non manualmente qui)
// - Va fatta una gestione delle form aperte per evitare che si aprano più volte (gestito dal FormManager)

using FormulariRif_G.Utils; // Per CurrentUser
using Microsoft.Extensions.DependencyInjection; // Per IServiceProvider
using FormulariRif_G.Service; // NUOVO: Namespace per il FormManager

namespace FormulariRif_G.Forms
{
    public partial class MainForm : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private Button btnGestioneRimorchi;
        private readonly FormManager _formManager; // NUOVO: Riferimento al FormManager

        // Modifica del costruttore per iniettare il FormManager
        public MainForm(IServiceProvider serviceProvider, FormManager formManager)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _formManager = formManager; // Inizializza il FormManager
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
        /// Apre il form per la gestione dei clienti tramite FormManager.
        /// </summary>
        private void btnGestioneClienti_Click(object sender, EventArgs e)
        {
            // Usa il FormManager per mostrare o attivare la form.
            _formManager.ShowOrActivate<ClientiListForm>();
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Gestione Utenti".
        /// Apre il form per la gestione degli utenti tramite FormManager.
        /// </summary>
        private void btnGestioneUtenti_Click(object sender, EventArgs e)
        {
            // Usa il FormManager per mostrare o attivare la form.
            _formManager.ShowOrActivate<UtentiListForm>();
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Gestione Automezzi".
        /// Apre il form per la gestione degli automezzi tramite FormManager.
        /// </summary>
        private void btnGestioneAutomezzi_Click(object sender, EventArgs e)
        {
            // Usa il FormManager per mostrare o attivare la form.
            _formManager.ShowOrActivate<AutomezziListForm>();
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Gestione Conducenti".
        /// Apre il form per la gestione dei conducenti tramite FormManager.
        /// </summary>
        private void btnGestioneConducenti_Click(object sender, EventArgs e)
        {
            _formManager.ShowOrActivate<ConducentiListForm>();
        }

        private void btnGestioneRimorchi_Click(object sender, EventArgs e)
        {
            _formManager.ShowOrActivate<RimorchiListForm>();
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Gestione Formulari Rifiuti".
        /// Apre il form per la gestione dei formulari rifiuti tramite FormManager.
        /// </summary>
        private void btnGestioneFormulariRifiuti_Click(object sender, EventArgs e)
        {
            // Usa il FormManager per mostrare o attivare la form.
            _formManager.ShowOrActivate<FormulariRifiutiListForm>();
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Configurazione".
        /// Apre il form di configurazione in modalità modale. Se la configurazione viene salvata,
        /// segnala a Program.cs di riavviare l'applicazione.
        /// </summary>
        private async void btnConfigurazione_Click(object sender, EventArgs e)
        {
            // La ConfigurazioneForm rimane modale e non è gestita dal FormManager,
            // in quanto il suo scopo è bloccare l'applicazione per la configurazione.
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
            // CurrentUser.Clear(); // Pulisce l'utente corrente (decommenta se necessario)
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
            btnGestioneConducenti = new Button();
            btnGestioneRimorchi = new Button();
            SuspendLayout();
            // 
            // btnGestioneClienti
            // 
            btnGestioneClienti.Location = new Point(43, 55);
            btnGestioneClienti.Margin = new Padding(6);
            btnGestioneClienti.Name = "btnGestioneClienti";
            btnGestioneClienti.Size = new Size(550, 107);
            btnGestioneClienti.TabIndex = 0;
            btnGestioneClienti.Text = "Gestione Clienti";
            btnGestioneClienti.UseVisualStyleBackColor = true;
            btnGestioneClienti.Click += btnGestioneClienti_Click;
            // 
            // btnGestioneUtenti
            // 
            btnGestioneUtenti.Location = new Point(43, 760);
            btnGestioneUtenti.Margin = new Padding(6);
            btnGestioneUtenti.Name = "btnGestioneUtenti";
            btnGestioneUtenti.Size = new Size(269, 107);
            btnGestioneUtenti.TabIndex = 2;
            btnGestioneUtenti.Text = "Gestione Utenti";
            btnGestioneUtenti.UseVisualStyleBackColor = true;
            btnGestioneUtenti.Click += btnGestioneUtenti_Click;
            // 
            // btnLogout
            // 
            btnLogout.Location = new Point(639, 760);
            btnLogout.Margin = new Padding(6);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(371, 107);
            btnLogout.TabIndex = 6;
            btnLogout.Text = "Logout";
            btnLogout.UseVisualStyleBackColor = true;
            btnLogout.Click += btnLogout_Click;
            // 
            // btnConfigurazione
            // 
            btnConfigurazione.Location = new Point(324, 760);
            btnConfigurazione.Margin = new Padding(6);
            btnConfigurazione.Name = "btnConfigurazione";
            btnConfigurazione.Size = new Size(269, 107);
            btnConfigurazione.TabIndex = 5;
            btnConfigurazione.Text = "Configurazione";
            btnConfigurazione.UseVisualStyleBackColor = true;
            btnConfigurazione.Click += btnConfigurazione_Click;
            // 
            // btnGestioneAutomezzi
            // 
            btnGestioneAutomezzi.Location = new Point(43, 360);
            btnGestioneAutomezzi.Margin = new Padding(6);
            btnGestioneAutomezzi.Name = "btnGestioneAutomezzi";
            btnGestioneAutomezzi.Size = new Size(550, 107);
            btnGestioneAutomezzi.TabIndex = 3;
            btnGestioneAutomezzi.Text = "Gestione Automezzi";
            btnGestioneAutomezzi.UseVisualStyleBackColor = true;
            btnGestioneAutomezzi.Click += btnGestioneAutomezzi_Click;
            // 
            // btnGestioneFormulariRifiuti
            // 
            btnGestioneFormulariRifiuti.Location = new Point(43, 201);
            btnGestioneFormulariRifiuti.Margin = new Padding(6);
            btnGestioneFormulariRifiuti.Name = "btnGestioneFormulariRifiuti";
            btnGestioneFormulariRifiuti.Size = new Size(550, 107);
            btnGestioneFormulariRifiuti.TabIndex = 1;
            btnGestioneFormulariRifiuti.Text = "Gestione Formulari Rifiuti";
            btnGestioneFormulariRifiuti.UseVisualStyleBackColor = true;
            btnGestioneFormulariRifiuti.Click += btnGestioneFormulariRifiuti_Click;
            // 
            // btnGestioneConducenti
            // 
            btnGestioneConducenti.Location = new Point(43, 479);
            btnGestioneConducenti.Margin = new Padding(6);
            btnGestioneConducenti.Name = "btnGestioneConducenti";
            btnGestioneConducenti.Size = new Size(550, 107);
            btnGestioneConducenti.TabIndex = 4;
            btnGestioneConducenti.Text = "Gestione Conducenti";
            btnGestioneConducenti.UseVisualStyleBackColor = true;
            btnGestioneConducenti.Click += btnGestioneConducenti_Click;
            // 
            // btnGestioneRimorchi
            // 
            btnGestioneRimorchi.Location = new Point(43, 602);
            btnGestioneRimorchi.Name = "btnGestioneRimorchi";
            btnGestioneRimorchi.Size = new Size(550, 107);
            btnGestioneRimorchi.TabIndex = 7;
            btnGestioneRimorchi.Text = "Gestione Rimorchi";
            btnGestioneRimorchi.UseVisualStyleBackColor = true;
            btnGestioneRimorchi.Click += btnGestioneRimorchi_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1054, 911);
            Controls.Add(btnGestioneRimorchi);
            Controls.Add(btnGestioneFormulariRifiuti);
            Controls.Add(btnGestioneAutomezzi);
            Controls.Add(btnLogout);
            Controls.Add(btnConfigurazione);
            Controls.Add(btnGestioneUtenti);
            Controls.Add(btnGestioneClienti);
            Controls.Add(btnGestioneConducenti);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(6);
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
        private System.Windows.Forms.Button btnGestioneConducenti; // NUOVO: Dichiarazione del pulsante per Gestione Conducenti

        #endregion

        
    }
}
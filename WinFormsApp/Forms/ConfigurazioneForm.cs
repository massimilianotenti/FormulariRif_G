// File: Forms/ConfigurazioneForm.cs
// Questo form permette di configurare la stringa di connessione al database,
// le credenziali (criptate) e i dati della tabella di configurazione dell'applicazione.
// Include anche la funzionalità per generare dati di test.
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using FormulariRif_G.Utils;
using Faker;
using Microsoft.Extensions.DependencyInjection;

namespace FormulariRif_G.Forms
{
    public partial class ConfigurazioneForm : Form
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        // Proprietà per esporre i dati di configurazione dell'applicazione
        public Configurazione AppConfigData { get; private set; }

        public ConfigurazioneForm(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _configuration = configuration;
            _serviceProvider = serviceProvider;

            AppConfigData = new Configurazione(); // Inizializza l'oggetto per esporre i dati
            this.Load += ConfigurazioneForm_Load;
            btnSalvaConfigurazione.Enabled = false;
            btnGeneraDatiTest.Enabled = false;
        }

        private async void ConfigurazioneForm_Load(object? sender, EventArgs e)
        {
            LoadAppSettingsData();
            btnSalvaConfigurazione.Enabled = true;

            if (await TestDbConnectionAsync(suppressMessageBox: true))
            {
                btnGeneraDatiTest.Enabled = true;
                await LoadConfigurationFromDbAsync();
            }
        }

        /// <summary>
        /// Carica i dati di connessione da appsettings.json nei controlli del form.
        /// </summary>
        private void LoadAppSettingsData()
        {
            txtServerName.Text = _configuration["ConnectionStrings:ServerName"] ?? string.Empty;
            txtDatabaseName.Text = _configuration["ConnectionStrings:DatabaseName"] ?? string.Empty;

            var encryptionKey = _configuration["EncryptionKey"];
            if (!string.IsNullOrEmpty(encryptionKey))
            {
                try
                {
                    EncryptionHelper.SetKey(encryptionKey);
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show($"Errore nella chiave di criptazione: {ex.Message}. Controlla appsettings.json.", "Errore di Criptazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            var encryptedUsername = _configuration["EncryptedCredentials:EncryptedUsername"];
            var encryptedPassword = _configuration["EncryptedCredentials:EncryptedPassword"];

            if (!string.IsNullOrEmpty(encryptedUsername) && !string.IsNullOrEmpty(encryptedPassword))
            {
                try
                {
                    txtDbUsername.Text = EncryptionHelper.Decrypt(encryptedUsername);
                    txtDbPassword.Text = EncryptionHelper.Decrypt(encryptedPassword);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante la decriptazione delle credenziali: {ex.Message}. Potrebbe essere necessaria una nuova configurazione.", "Errore di Criptazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtDbUsername.Text = string.Empty;
                    txtDbPassword.Text = string.Empty;
                }
            }
        }

        /// <summary>
        /// Carica i dati della tabella di configurazione dal database.
        /// Questo metodo sarà chiamato solo dopo che la connessione al DB è stata stabilita.
        /// </summary>
        private async Task LoadConfigurationFromDbAsync()
        {
            try
            {
                var configRepo = _serviceProvider.GetRequiredService<IGenericRepository<Configurazione>>();
                var configs = await configRepo.GetAllAsync();
                var loadedConfig = configs.FirstOrDefault();

                if (loadedConfig != null)
                {
                    // Aggiorna l'oggetto AppConfigData con i dati dal DB
                    AppConfigData = loadedConfig;
                    chkDatiTest.Checked = AppConfigData.DatiTest ?? false;
                    txtRagSoc.Text = AppConfigData.RagSoc;
                    txtIndirizzo.Text = AppConfigData.Indirizzo;
                    txtComune.Text = AppConfigData.Comune;
                    numCap.Value = AppConfigData.Cap;
                    txtEmail.Text = AppConfigData.Email;
                    // Carica i nuovi campi
                    txtPartitaIva.Text = AppConfigData.PartitaIva;
                    txtCodiceFiscale.Text = AppConfigData.CodiceFiscale;
                    txtNumeroIscrizioneAlbo.Text = AppConfigData.NumeroIscrizioneAlbo;
                    dtpDataIscrizioneAlbo.Value = AppConfigData.DataIscrizioneAlbo ?? DateTime.Now; // Imposta la data o la data corrente
                }
                else
                {
                    // Se non esiste una configurazione nel DB, i campi rimangono vuoti (o con i valori predefiniti di AppConfigData)
                    chkDatiTest.Checked = false;
                    txtRagSoc.Text = string.Empty;
                    txtIndirizzo.Text = string.Empty;
                    txtComune.Text = string.Empty;
                    numCap.Value = 0;
                    txtEmail.Text = string.Empty;
                    // Inizializza i nuovi campi
                    txtPartitaIva.Text = string.Empty;
                    txtCodiceFiscale.Text = string.Empty;
                    txtNumeroIscrizioneAlbo.Text = string.Empty;
                    dtpDataIscrizioneAlbo.Value = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento della configurazione dal database: {ex.Message}", "Errore Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Gestisce il click sul pulsante "Salva Configurazione".
        /// Salva i dati di connessione in appsettings.json e i dati di configurazione dell'applicazione nel database.
        /// </summary>
        private async void btnSalvaConfigurazione_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            // 1. Salva i dati della connessione e le credenziali criptate in appsettings.json
            SaveAppSettings();

            // 2. Tenta di testare la connessione con i nuovi dati
            if (!await TestDbConnectionAsync())
            {
                MessageBox.Show("Impossibile connettersi al database con le credenziali fornite. Controlla i dati e riprova.", "Errore di Connessione", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 3. Popola l'oggetto AppConfigData con i valori attuali dei controlli
            AppConfigData.DatiTest = chkDatiTest.Checked;
            AppConfigData.RagSoc = txtRagSoc.Text.Trim();
            AppConfigData.Indirizzo = txtIndirizzo.Text.Trim();
            AppConfigData.Comune = txtComune.Text.Trim();
            AppConfigData.Cap = (int)numCap.Value;
            AppConfigData.Email = txtEmail.Text.Trim();
            // Salva i nuovi campi
            AppConfigData.PartitaIva = txtPartitaIva.Text.Trim();
            AppConfigData.CodiceFiscale = txtCodiceFiscale.Text.Trim();
            AppConfigData.NumeroIscrizioneAlbo = txtNumeroIscrizioneAlbo.Text.Trim();
            AppConfigData.DataIscrizioneAlbo = dtpDataIscrizioneAlbo.Value;


            try
            {
                // Risolvi il repository per Configurazione
                var configRepo = _serviceProvider.GetRequiredService<IGenericRepository<Configurazione>>();
                var existingConfig = (await configRepo.GetAllAsync()).FirstOrDefault();

                if (existingConfig == null)
                {
                    // Se non esiste, aggiungi la nuova configurazione
                    await configRepo.AddAsync(AppConfigData);
                }
                else
                {
                    // Se esiste, aggiorna l'entità esistente con i nuovi valori
                    existingConfig.DatiTest = AppConfigData.DatiTest;
                    existingConfig.RagSoc = AppConfigData.RagSoc;
                    existingConfig.Indirizzo = AppConfigData.Indirizzo;
                    existingConfig.Comune = AppConfigData.Comune;
                    existingConfig.Cap = AppConfigData.Cap;
                    existingConfig.Email = AppConfigData.Email;
                    // Aggiorna i nuovi campi
                    existingConfig.PartitaIva = AppConfigData.PartitaIva;
                    existingConfig.CodiceFiscale = AppConfigData.CodiceFiscale;
                    existingConfig.NumeroIscrizioneAlbo = AppConfigData.NumeroIscrizioneAlbo;
                    existingConfig.DataIscrizioneAlbo = AppConfigData.DataIscrizioneAlbo;

                    configRepo.Update(existingConfig);
                }
                await configRepo.SaveChangesAsync(); // Salva le modifiche nel database

                // Segnala a Program.cs che la configurazione è stata salvata con successo
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio della configurazione nel database: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Tenta di testare la connessione al database con i dati attuali nei campi di input.
        /// </summary>
        /// <param name="suppressMessageBox">Se true, non mostra MessageBox in caso di errore.</param>
        /// <returns>True se la connessione ha successo, altrimenti False.</returns>
        private async Task<bool> TestDbConnectionAsync(bool suppressMessageBox = false)
        {
            var serverName = txtServerName.Text.Trim();
            var databaseName = txtDatabaseName.Text.Trim();
            var dbUsername = txtDbUsername.Text.Trim();
            var dbPassword = txtDbPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(serverName) || string.IsNullOrWhiteSpace(databaseName) ||
                string.IsNullOrWhiteSpace(dbUsername) || string.IsNullOrWhiteSpace(dbPassword))
            {
                if (!suppressMessageBox)
                {
                    MessageBox.Show("Compila tutti i campi di connessione al database per testare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false;
            }

            string testConnectionString = $"Server={serverName};Database={databaseName};User Id={dbUsername};Password={dbPassword};TrustServerCertificate=True;";

            try
            {
                using (var conn = new SqlConnection(testConnectionString))
                {
                    await conn.OpenAsync();
                    conn.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                if (!suppressMessageBox)
                {
                    MessageBox.Show($"Test connessione fallito: {ex.Message}", "Errore Connessione", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Test Connessione".
        /// </summary>
        private async void btnTestConnessione_Click(object sender, EventArgs e)
        {
            if (await TestDbConnectionAsync())
            {
                MessageBox.Show("Connessione al database riuscita!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Salva i dettagli della connessione e le credenziali criptate in appsettings.json.
        /// </summary>
        private void SaveAppSettings()
        {
            var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            var encryptionKey = _configuration["EncryptionKey"];
            if (string.IsNullOrEmpty(encryptionKey))
            {
                encryptionKey = "SixteenByteKey123!";
            }
            EncryptionHelper.SetKey(encryptionKey);

            var encryptedUsername = EncryptionHelper.Encrypt(txtDbUsername.Text.Trim());
            var encryptedPassword = EncryptionHelper.Encrypt(txtDbPassword.Text.Trim());

            var newSettings = new
            {
                ConnectionStrings = new
                {
                    ServerName = txtServerName.Text.Trim(),
                    DatabaseName = txtDatabaseName.Text.Trim()
                },
                EncryptedCredentials = new
                {
                    EncryptedUsername = encryptedUsername,
                    EncryptedPassword = encryptedPassword
                },
                EncryptionKey = encryptionKey
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(appSettingsPath, JsonSerializer.Serialize(newSettings, options));
        }


        /// <summary>
        /// Esegue la validazione dei campi di input.
        /// </summary>
        /// <returns>True se l'input è valido, altrimenti False.</returns>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtServerName.Text))
            {
                MessageBox.Show("Nome Server è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtServerName.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtDatabaseName.Text))
            {
                MessageBox.Show("Nome Database è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDatabaseName.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtDbUsername.Text))
            {
                MessageBox.Show("Username Database è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDbUsername.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtDbPassword.Text))
            {
                MessageBox.Show("Password Database è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDbPassword.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtRagSoc.Text))
            {
                MessageBox.Show("Ragione Sociale (Configurazione) è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRagSoc.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtIndirizzo.Text))
            {
                MessageBox.Show("Indirizzo (Configurazione) è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIndirizzo.Focus();
                return false;
            }
            if (numCap.Value == 0)
            {
                MessageBox.Show("CAP (Configurazione) è un campo obbligatorio e non può essere 0.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numCap.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Email (Configurazione) è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return false;
            }
            // Nuova validazione per i campi aggiunti
            if (string.IsNullOrWhiteSpace(txtPartitaIva.Text))
            {
                MessageBox.Show("Partita IVA è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPartitaIva.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtCodiceFiscale.Text))
            {
                MessageBox.Show("Codice Fiscale è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCodiceFiscale.Focus();
                return false;
            }
            // NumeroIscrizioneAlbo e DataIscrizioneAlbo non sono obbligatori per default, ma puoi aggiungerli se necessario.

            return true;
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Genera Dati Test".
        /// Popola le tabelle Clienti e ClientiContatti con dati fittizi.
        /// </summary>
        private async void btnGeneraDatiTest_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("Questa operazione genererà dati di test. Continuare?", "Conferma", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    var clienteRepo = _serviceProvider.GetRequiredService<IGenericRepository<Cliente>>();
                    var clienteContattoRepo = _serviceProvider.GetRequiredService<IGenericRepository<ClienteContatto>>();
                    var clienteIndirizzoRepo = _serviceProvider.GetRequiredService<IGenericRepository<ClienteIndirizzo>>(); // Nuovo repository

                    for (int i = 0; i < 10; i++)
                    {
                        var cliente = new Cliente
                        {
                            RagSoc = Faker.Company.Name(),
                            CodiceFiscale = Faker.Identification.SocialSecurityNumber(), // Aggiunto Codice Fiscale
                            IsTestData = true
                        };
                        await clienteRepo.AddAsync(cliente);
                        await clienteRepo.SaveChangesAsync();

                        // Genera 1-3 indirizzi per ogni cliente di test
                        var numIndirizzi = new Random().Next(1, 4);
                        for (int k = 0; k < numIndirizzi; k++)
                        {
                            var indirizzo = new ClienteIndirizzo
                            {
                                IdCli = cliente.Id,
                                Indirizzo = Faker.Address.StreetAddress(),
                                Comune = Faker.Address.City(),
                                Cap = Convert.ToInt32(Faker.Address.ZipCode().Substring(0, Math.Min(5, Faker.Address.ZipCode().Length))),
                                Predefinito = (k == 0), // Il primo indirizzo è predefinito
                                IsTestData = true
                            };
                            await clienteIndirizzoRepo.AddAsync(indirizzo);
                        }
                        await clienteIndirizzoRepo.SaveChangesAsync();


                        var numContatti = new Random().Next(1, 4);
                        for (int j = 0; j < numContatti; j++)
                        {
                            var contatto = new ClienteContatto
                            {
                                IdCli = cliente.Id,
                                Predefinito = (j == 0),
                                Contatto = Faker.Name.FullName(),
                                Telefono = Faker.Phone.Number(),
                                Email = Faker.Internet.Email(),
                                IsTestData = true
                            };
                            await clienteContattoRepo.AddAsync(contatto);
                        }
                        await clienteContattoRepo.SaveChangesAsync();
                    }

                    MessageBox.Show("Dati di test generati con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante la generazione dei dati di test: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

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
            groupBoxDbConnection = new GroupBox();
            btnTestConnessione = new Button();
            txtDbPassword = new TextBox();
            lblDbPassword = new Label();
            txtDbUsername = new TextBox();
            lblDbUsername = new Label();
            txtDatabaseName = new TextBox();
            lblDatabaseName = new Label();
            txtServerName = new TextBox();
            lblServerName = new Label();
            groupBoxAppSettings = new GroupBox();
            dtpDataIscrizioneAlbo = new DateTimePicker();
            lblDataIscrizioneAlbo = new Label();
            txtNumeroIscrizioneAlbo = new TextBox();
            lblNumeroIscrizioneAlbo = new Label();
            txtCodiceFiscale = new TextBox();
            lblCodiceFiscale = new Label();
            txtPartitaIva = new TextBox();
            lblPartitaIva = new Label();
            txtEmail = new TextBox();
            lblEmail = new Label();
            numCap = new NumericUpDown();
            lblCap = new Label();
            txtComune = new TextBox();
            lblComune = new Label();
            txtIndirizzo = new TextBox();
            lblIndirizzo = new Label();
            txtRagSoc = new TextBox();
            lblRagSoc = new Label();
            chkDatiTest = new CheckBox();
            btnSalvaConfigurazione = new Button();
            btnGeneraDatiTest = new Button();
            groupBoxDbConnection.SuspendLayout();
            groupBoxAppSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCap).BeginInit();
            SuspendLayout();
            // 
            // groupBoxDbConnection
            // 
            groupBoxDbConnection.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxDbConnection.Controls.Add(btnTestConnessione);
            groupBoxDbConnection.Controls.Add(txtDbPassword);
            groupBoxDbConnection.Controls.Add(lblDbPassword);
            groupBoxDbConnection.Controls.Add(txtDbUsername);
            groupBoxDbConnection.Controls.Add(lblDbUsername);
            groupBoxDbConnection.Controls.Add(txtDatabaseName);
            groupBoxDbConnection.Controls.Add(lblDatabaseName);
            groupBoxDbConnection.Controls.Add(txtServerName);
            groupBoxDbConnection.Controls.Add(lblServerName);
            groupBoxDbConnection.Location = new Point(12, 12);
            groupBoxDbConnection.Name = "groupBoxDbConnection";
            groupBoxDbConnection.Size = new Size(460, 180);
            groupBoxDbConnection.TabIndex = 0;
            groupBoxDbConnection.TabStop = false;
            groupBoxDbConnection.Text = "Connessione Database";
            // 
            // btnTestConnessione
            // 
            btnTestConnessione.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTestConnessione.Location = new Point(345, 140);
            btnTestConnessione.Name = "btnTestConnessione";
            btnTestConnessione.Size = new Size(95, 23);
            btnTestConnessione.TabIndex = 8;
            btnTestConnessione.Text = "Test Connessione";
            btnTestConnessione.UseVisualStyleBackColor = true;
            btnTestConnessione.Click += btnTestConnessione_Click;
            // 
            // txtDbPassword
            // 
            txtDbPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDbPassword.Location = new Point(120, 140);
            txtDbPassword.Name = "txtDbPassword";
            txtDbPassword.Size = new Size(219, 23);
            txtDbPassword.TabIndex = 7;
            txtDbPassword.UseSystemPasswordChar = true;
            // 
            // lblDbPassword
            // 
            lblDbPassword.AutoSize = true;
            lblDbPassword.Location = new Point(15, 143);
            lblDbPassword.Name = "lblDbPassword";
            lblDbPassword.Size = new Size(86, 15);
            lblDbPassword.TabIndex = 6;
            lblDbPassword.Text = "Password (DB):";
            // 
            // txtDbUsername
            // 
            txtDbUsername.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDbUsername.Location = new Point(120, 105);
            txtDbUsername.Name = "txtDbUsername";
            txtDbUsername.Size = new Size(320, 23);
            txtDbUsername.TabIndex = 5;
            // 
            // lblDbUsername
            // 
            lblDbUsername.AutoSize = true;
            lblDbUsername.Location = new Point(15, 108);
            lblDbUsername.Name = "lblDbUsername";
            lblDbUsername.Size = new Size(89, 15);
            lblDbUsername.TabIndex = 4;
            lblDbUsername.Text = "Username (DB):";
            // 
            // txtDatabaseName
            // 
            txtDatabaseName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDatabaseName.Location = new Point(120, 70);
            txtDatabaseName.Name = "txtDatabaseName";
            txtDatabaseName.Size = new Size(320, 23);
            txtDatabaseName.TabIndex = 3;
            // 
            // lblDatabaseName
            // 
            lblDatabaseName.AutoSize = true;
            lblDatabaseName.Location = new Point(15, 73);
            lblDatabaseName.Name = "lblDatabaseName";
            lblDatabaseName.Size = new Size(94, 15);
            lblDatabaseName.TabIndex = 2;
            lblDatabaseName.Text = "Nome Database:";
            // 
            // txtServerName
            // 
            txtServerName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtServerName.Location = new Point(120, 35);
            txtServerName.Name = "txtServerName";
            txtServerName.Size = new Size(320, 23);
            txtServerName.TabIndex = 1;
            // 
            // lblServerName
            // 
            lblServerName.AutoSize = true;
            lblServerName.Location = new Point(15, 38);
            lblServerName.Name = "lblServerName";
            lblServerName.Size = new Size(78, 15);
            lblServerName.TabIndex = 0;
            lblServerName.Text = "Nome Server:";
            // 
            // groupBoxAppSettings
            // 
            groupBoxAppSettings.Controls.Add(dtpDataIscrizioneAlbo);
            groupBoxAppSettings.Controls.Add(lblDataIscrizioneAlbo);
            groupBoxAppSettings.Controls.Add(txtNumeroIscrizioneAlbo);
            groupBoxAppSettings.Controls.Add(lblNumeroIscrizioneAlbo);
            groupBoxAppSettings.Controls.Add(txtCodiceFiscale);
            groupBoxAppSettings.Controls.Add(lblCodiceFiscale);
            groupBoxAppSettings.Controls.Add(txtPartitaIva);
            groupBoxAppSettings.Controls.Add(lblPartitaIva);
            groupBoxAppSettings.Controls.Add(txtEmail);
            groupBoxAppSettings.Controls.Add(lblEmail);
            groupBoxAppSettings.Controls.Add(numCap);
            groupBoxAppSettings.Controls.Add(lblCap);
            groupBoxAppSettings.Controls.Add(txtComune);
            groupBoxAppSettings.Controls.Add(lblComune);
            groupBoxAppSettings.Controls.Add(txtIndirizzo);
            groupBoxAppSettings.Controls.Add(lblIndirizzo);
            groupBoxAppSettings.Controls.Add(txtRagSoc);
            groupBoxAppSettings.Controls.Add(lblRagSoc);
            groupBoxAppSettings.Controls.Add(chkDatiTest);
            groupBoxAppSettings.Location = new Point(12, 200);
            groupBoxAppSettings.Name = "groupBoxAppSettings";
            groupBoxAppSettings.Size = new Size(460, 386);
            groupBoxAppSettings.TabIndex = 1;
            groupBoxAppSettings.TabStop = false;
            groupBoxAppSettings.Text = "Configurazione Applicazione";
            // 
            // dtpDataIscrizioneAlbo
            // 
            dtpDataIscrizioneAlbo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dtpDataIscrizioneAlbo.Format = DateTimePickerFormat.Short;
            dtpDataIscrizioneAlbo.Location = new Point(120, 309);
            dtpDataIscrizioneAlbo.Name = "dtpDataIscrizioneAlbo";
            dtpDataIscrizioneAlbo.Size = new Size(320, 23);
            dtpDataIscrizioneAlbo.TabIndex = 18;
            // 
            // lblDataIscrizioneAlbo
            // 
            lblDataIscrizioneAlbo.AutoSize = true;
            lblDataIscrizioneAlbo.Location = new Point(15, 312);
            lblDataIscrizioneAlbo.Name = "lblDataIscrizioneAlbo";
            lblDataIscrizioneAlbo.Size = new Size(82, 15);
            lblDataIscrizioneAlbo.TabIndex = 17;
            lblDataIscrizioneAlbo.Text = "Data Isc. Albo:";
            // 
            // txtNumeroIscrizioneAlbo
            // 
            txtNumeroIscrizioneAlbo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNumeroIscrizioneAlbo.Location = new Point(120, 274);
            txtNumeroIscrizioneAlbo.Name = "txtNumeroIscrizioneAlbo";
            txtNumeroIscrizioneAlbo.Size = new Size(320, 23);
            txtNumeroIscrizioneAlbo.TabIndex = 16;
            // 
            // lblNumeroIscrizioneAlbo
            // 
            lblNumeroIscrizioneAlbo.AutoSize = true;
            lblNumeroIscrizioneAlbo.Location = new Point(15, 277);
            lblNumeroIscrizioneAlbo.Name = "lblNumeroIscrizioneAlbo";
            lblNumeroIscrizioneAlbo.Size = new Size(88, 15);
            lblNumeroIscrizioneAlbo.TabIndex = 15;
            lblNumeroIscrizioneAlbo.Text = "Num. Isc. Albo:";
            // 
            // txtCodiceFiscale
            // 
            txtCodiceFiscale.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCodiceFiscale.Location = new Point(120, 239);
            txtCodiceFiscale.Name = "txtCodiceFiscale";
            txtCodiceFiscale.Size = new Size(320, 23);
            txtCodiceFiscale.TabIndex = 14;
            // 
            // lblCodiceFiscale
            // 
            lblCodiceFiscale.AutoSize = true;
            lblCodiceFiscale.Location = new Point(15, 242);
            lblCodiceFiscale.Name = "lblCodiceFiscale";
            lblCodiceFiscale.Size = new Size(85, 15);
            lblCodiceFiscale.TabIndex = 13;
            lblCodiceFiscale.Text = "Codice Fiscale:";
            // 
            // txtPartitaIva
            // 
            txtPartitaIva.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPartitaIva.Location = new Point(120, 204);
            txtPartitaIva.Name = "txtPartitaIva";
            txtPartitaIva.Size = new Size(320, 23);
            txtPartitaIva.TabIndex = 12;
            // 
            // lblPartitaIva
            // 
            lblPartitaIva.AutoSize = true;
            lblPartitaIva.Location = new Point(15, 207);
            lblPartitaIva.Name = "lblPartitaIva";
            lblPartitaIva.Size = new Size(64, 15);
            lblPartitaIva.TabIndex = 11;
            lblPartitaIva.Text = "Partita IVA:";
            // 
            // txtEmail
            // 
            txtEmail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtEmail.Location = new Point(120, 169);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(320, 23);
            txtEmail.TabIndex = 10;
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(15, 172);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(44, 15);
            lblEmail.TabIndex = 9;
            lblEmail.Text = "E-mail:";
            // 
            // numCap
            // 
            numCap.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            numCap.Location = new Point(120, 134);
            numCap.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            numCap.Name = "numCap";
            numCap.Size = new Size(120, 23);
            numCap.TabIndex = 8;
            // 
            // lblCap
            // 
            lblCap.AutoSize = true;
            lblCap.Location = new Point(15, 136);
            lblCap.Name = "lblCap";
            lblCap.Size = new Size(33, 15);
            lblCap.TabIndex = 7;
            lblCap.Text = "CAP:";
            // 
            // txtComune
            // 
            txtComune.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtComune.Location = new Point(120, 99);
            txtComune.Name = "txtComune";
            txtComune.Size = new Size(320, 23);
            txtComune.TabIndex = 6;
            // 
            // lblComune
            // 
            lblComune.AutoSize = true;
            lblComune.Location = new Point(15, 102);
            lblComune.Name = "lblComune";
            lblComune.Size = new Size(56, 15);
            lblComune.TabIndex = 5;
            lblComune.Text = "Comune:";
            // 
            // txtIndirizzo
            // 
            txtIndirizzo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtIndirizzo.Location = new Point(120, 64);
            txtIndirizzo.Name = "txtIndirizzo";
            txtIndirizzo.Size = new Size(320, 23);
            txtIndirizzo.TabIndex = 4;
            // 
            // lblIndirizzo
            // 
            lblIndirizzo.AutoSize = true;
            lblIndirizzo.Location = new Point(15, 67);
            lblIndirizzo.Name = "lblIndirizzo";
            lblIndirizzo.Size = new Size(54, 15);
            lblIndirizzo.TabIndex = 3;
            lblIndirizzo.Text = "Indirizzo:";
            // 
            // txtRagSoc
            // 
            txtRagSoc.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtRagSoc.Location = new Point(120, 29);
            txtRagSoc.Name = "txtRagSoc";
            txtRagSoc.Size = new Size(320, 23);
            txtRagSoc.TabIndex = 2;
            // 
            // lblRagSoc
            // 
            lblRagSoc.AutoSize = true;
            lblRagSoc.Location = new Point(15, 32);
            lblRagSoc.Name = "lblRagSoc";
            lblRagSoc.Size = new Size(93, 15);
            lblRagSoc.TabIndex = 1;
            lblRagSoc.Text = "Ragione Sociale:";
            // 
            // chkDatiTest
            // 
            chkDatiTest.AutoSize = true;
            chkDatiTest.Location = new Point(15, 344);
            chkDatiTest.Name = "chkDatiTest";
            chkDatiTest.Size = new Size(108, 19);
            chkDatiTest.TabIndex = 0;
            chkDatiTest.Text = "Abilita Dati Test";
            chkDatiTest.UseVisualStyleBackColor = true;
            // 
            // btnSalvaConfigurazione
            // 
            btnSalvaConfigurazione.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSalvaConfigurazione.Location = new Point(322, 598);
            btnSalvaConfigurazione.Name = "btnSalvaConfigurazione";
            btnSalvaConfigurazione.Size = new Size(150, 30);
            btnSalvaConfigurazione.TabIndex = 2;
            btnSalvaConfigurazione.Text = "Salva Configurazione";
            btnSalvaConfigurazione.UseVisualStyleBackColor = true;
            btnSalvaConfigurazione.Click += btnSalvaConfigurazione_Click;
            // 
            // btnGeneraDatiTest
            // 
            btnGeneraDatiTest.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnGeneraDatiTest.Location = new Point(12, 598);
            btnGeneraDatiTest.Name = "btnGeneraDatiTest";
            btnGeneraDatiTest.Size = new Size(150, 30);
            btnGeneraDatiTest.TabIndex = 3;
            btnGeneraDatiTest.Text = "Genera Dati Test";
            btnGeneraDatiTest.UseVisualStyleBackColor = true;
            btnGeneraDatiTest.Click += btnGeneraDatiTest_Click;
            // 
            // ConfigurazioneForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 639);
            Controls.Add(btnGeneraDatiTest);
            Controls.Add(btnSalvaConfigurazione);
            Controls.Add(groupBoxAppSettings);
            Controls.Add(groupBoxDbConnection);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConfigurazioneForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Configurazione Applicazione";
            groupBoxDbConnection.ResumeLayout(false);
            groupBoxDbConnection.PerformLayout();
            groupBoxAppSettings.ResumeLayout(false);
            groupBoxAppSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCap).EndInit();
            ResumeLayout(false);

        }

        private System.Windows.Forms.GroupBox groupBoxDbConnection;
        private System.Windows.Forms.Button btnTestConnessione;
        private System.Windows.Forms.TextBox txtDbPassword;
        private System.Windows.Forms.Label lblDbPassword;
        private System.Windows.Forms.TextBox txtDbUsername;
        private System.Windows.Forms.Label lblDbUsername;
        private System.Windows.Forms.TextBox txtDatabaseName;
        private System.Windows.Forms.Label lblDatabaseName;
        private System.Windows.Forms.TextBox txtServerName;
        private System.Windows.Forms.Label lblServerName;
        private System.Windows.Forms.GroupBox groupBoxAppSettings;
        public System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblEmail;
        public System.Windows.Forms.NumericUpDown numCap;
        private System.Windows.Forms.Label lblCap;
        public System.Windows.Forms.TextBox txtComune;
        private System.Windows.Forms.Label lblComune;
        public System.Windows.Forms.TextBox txtIndirizzo;
        private System.Windows.Forms.Label lblIndirizzo;
        public System.Windows.Forms.TextBox txtRagSoc;
        private System.Windows.Forms.Label lblRagSoc;
        public System.Windows.Forms.CheckBox chkDatiTest;
        private System.Windows.Forms.Button btnSalvaConfigurazione;
        private System.Windows.Forms.Button btnGeneraDatiTest;
        private System.Windows.Forms.TextBox txtPartitaIva; // Nuovo
        private System.Windows.Forms.Label lblPartitaIva; // Nuovo
        private System.Windows.Forms.TextBox txtCodiceFiscale; // Nuovo
        private System.Windows.Forms.Label lblCodiceFiscale; // Nuovo
        private System.Windows.Forms.TextBox txtNumeroIscrizioneAlbo; // Nuovo
        private System.Windows.Forms.Label lblNumeroIscrizioneAlbo; // Nuovo
        private System.Windows.Forms.DateTimePicker dtpDataIscrizioneAlbo; // Nuovo
        private System.Windows.Forms.Label lblDataIscrizioneAlbo; // Nuovo

        #endregion
    }
}

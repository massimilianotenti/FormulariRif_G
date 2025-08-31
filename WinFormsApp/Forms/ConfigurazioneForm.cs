// File: Forms/ConfigurazioneForm.cs
// Questo form permette di configurare la stringa di connessione al database,
// le credenziali (criptate) e i dati della tabella di configurazione dell'applicazione.
// Include anche la funzionalità per generare dati di test.
using Faker;
using Bogus;
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using FormulariRif_G.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;

namespace FormulariRif_G.Forms
{
    public partial class ConfigurazioneForm : Form
    {
        private readonly IConfiguration _configuration;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private TextBox txtDestNumIscr;
        private Label label1;
        private TextBox txtDestD;
        private TextBox txtDestR;
        private Label label5;
        private TextBox txtDestTipoR2;
        private Label label4;
        private TextBox txtDestTipoR1;
        private Label label3;
        private TextBox txtDestAutoCom;
        private Label label2;
        public TextBox txtRagSoc2;
        private readonly IServiceProvider _serviceProvider;
        private ProgressBar progressBar1;
        private static readonly Random _random = new Random();

        // Proprietà per esporre i dati di configurazione dell'applicazione
        public Configurazione AppConfigData { get; private set; }

        private readonly IGenericRepository<Automezzo> _automezzoRepository;
        private readonly IGenericRepository<Conducente> _conducenteRepository;
        private readonly IGenericRepository<Autom_Cond> _automCondRepository;
        private readonly IGenericRepository<Rimorchio> _rimorchioRepository;
        private Button btTestSqlite;
        private TextBox txtDbSqlite;
        private Label label6;
        private readonly IGenericRepository<Autom_Rim> _automRimRepository;

        public ConfigurazioneForm(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _configuration = configuration;
            _serviceProvider = serviceProvider;

            // I repository vengono risolti qui per essere disponibili in tutta la classe
            _automezzoRepository = _serviceProvider.GetRequiredService<IGenericRepository<Automezzo>>();
            _conducenteRepository = _serviceProvider.GetRequiredService<IGenericRepository<Conducente>>();
            _automCondRepository = _serviceProvider.GetRequiredService<IGenericRepository<Autom_Cond>>();
            _rimorchioRepository = _serviceProvider.GetRequiredService<IGenericRepository<Rimorchio>>();
            _automRimRepository = _serviceProvider.GetRequiredService<IGenericRepository<Autom_Rim>>();

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
            //txtServerName.Text = _configuration["ConnectionStrings:ServerName"] ?? string.Empty;
            //txtDatabaseName.Text = _configuration["ConnectionStrings:DatabaseName"] ?? string.Empty;
            //txtDbUsername.Text = _configuration["EncryptedCredentials:EncryptedUsername"];
            //txtDbPassword.Text = _configuration["EncryptedCredentials:EncryptedPassword"];
            txtDbSqlite.Text = _configuration["ConnectionStrings:DefaultConnection"] ?? string.Empty;
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
                    AppConfigData = loadedConfig;
                    //Dati azienda
                    chkDatiTest.Checked = AppConfigData.DatiTest ?? false;
                    txtRagSoc1.Text = AppConfigData.RagSoc1;
                    txtRagSoc2.Text = AppConfigData.RagSoc2;
                    txtIndirizzo.Text = AppConfigData.Indirizzo;
                    txtComune.Text = AppConfigData.Comune;
                    numCap.Value = AppConfigData.Cap;
                    txtEmail.Text = AppConfigData.Email;
                    txtPartitaIva.Text = AppConfigData.PartitaIva;
                    txtCodiceFiscale.Text = AppConfigData.CodiceFiscale;
                    // Destinatario
                    txtDestNumIscr.Text = AppConfigData.DestNumeroIscrizioneAlbo;
                    txtDestR.Text = AppConfigData.DestR;
                    txtDestD.Text = AppConfigData.DestD;
                    txtDestAutoCom.Text = AppConfigData.DestAutoComunic;
                    txtDestTipoR1.Text = AppConfigData.DestTipo1;
                    txtDestTipoR2.Text = AppConfigData.DestTipo2;
                    // Trasportatore
                    txtNumeroIscrizioneAlbo.Text = AppConfigData.NumeroIscrizioneAlbo;
                    if (AppConfigData.DataIscrizioneAlbo.HasValue)
                        dtpDataIscrizioneAlbo.Value = AppConfigData.DataIscrizioneAlbo.Value;
                    else
                        dtpDataIscrizioneAlbo.Text = string.Empty;
                }
                else
                {
                    // Se non esiste una configurazione nel DB, i campi rimangono vuoti (o con i valori predefiniti di AppConfigData)
                    chkDatiTest.Checked = false;
                    txtRagSoc1.Text = string.Empty;
                    txtRagSoc2.Text = string.Empty;
                    txtIndirizzo.Text = string.Empty;
                    txtComune.Text = string.Empty;
                    numCap.Value = 0;
                    txtEmail.Text = string.Empty;
                    txtPartitaIva.Text = string.Empty;
                    txtCodiceFiscale.Text = string.Empty;

                    txtDestNumIscr.Text = string.Empty;
                    txtDestR.Text = string.Empty;
                    txtDestD.Text = string.Empty;
                    txtDestAutoCom.Text = string.Empty;
                    txtDestTipoR1.Text = string.Empty;
                    txtDestTipoR2.Text = string.Empty;

                    txtNumeroIscrizioneAlbo.Text = string.Empty;
                    dtpDataIscrizioneAlbo.Text = string.Empty;
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
            AppConfigData.RagSoc1 = txtRagSoc1.Text.Trim();
            AppConfigData.RagSoc2 = txtRagSoc2.Text.Trim();
            AppConfigData.Indirizzo = txtIndirizzo.Text.Trim();
            AppConfigData.Comune = txtComune.Text.Trim();
            AppConfigData.Cap = (int)numCap.Value;
            AppConfigData.Email = txtEmail.Text.Trim();
            AppConfigData.PartitaIva = txtPartitaIva.Text.Trim();
            AppConfigData.CodiceFiscale = txtCodiceFiscale.Text.Trim();

            AppConfigData.DestNumeroIscrizioneAlbo = txtDestNumIscr.Text.Trim();
            AppConfigData.DestR = txtDestR.Text.Trim();
            AppConfigData.DestD = txtDestD.Text.Trim();
            AppConfigData.DestAutoComunic = txtDestAutoCom.Text.Trim();
            AppConfigData.DestTipo1 = txtDestTipoR1.Text.Trim();
            AppConfigData.DestTipo2 = txtDestTipoR2.Text.Trim();

            AppConfigData.NumeroIscrizioneAlbo = txtNumeroIscrizioneAlbo.Text.Trim();
            if (dtpDataIscrizioneAlbo.Value == null || dtpDataIscrizioneAlbo.Value == DateTime.MinValue)
                AppConfigData.DataIscrizioneAlbo = null;
            else
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
                    existingConfig.RagSoc1 = AppConfigData.RagSoc1;
                    existingConfig.Indirizzo = AppConfigData.Indirizzo;
                    existingConfig.Comune = AppConfigData.Comune;
                    existingConfig.Cap = AppConfigData.Cap;
                    existingConfig.Email = AppConfigData.Email;
                    existingConfig.PartitaIva = AppConfigData.PartitaIva;
                    existingConfig.CodiceFiscale = AppConfigData.CodiceFiscale;

                    existingConfig.DestNumeroIscrizioneAlbo = AppConfigData.DestNumeroIscrizioneAlbo;
                    existingConfig.DestR = AppConfigData.DestR;
                    existingConfig.DestD = AppConfigData.DestD;
                    existingConfig.DestAutoComunic = AppConfigData.DestAutoComunic;
                    existingConfig.DestTipo1 = AppConfigData.DestTipo1;
                    existingConfig.DestTipo2 = AppConfigData.DestTipo2;

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
            var dbNameCon = txtDbSqlite.Text.Trim();            

            if (string.IsNullOrWhiteSpace(dbNameCon))
            {
                if (!suppressMessageBox)
                {
                    MessageBox.Show("Compila tutti i campi di connessione al database per testare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false;
            }            

            try
            {
                using (var conn = new SqliteConnection(dbNameCon))
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

        private async void btTestSqlite_Click(object sender, EventArgs e)
        {
            if (await TestDbConnectionAsync())
            {
                MessageBox.Show("Connessione al database riuscita!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            var dbSqlite = txtDbSqlite.Text.Trim();            

            var newSettings = new
            {
                ConnectionStrings = new
                {
                    DefaultConnection = txtDbSqlite.Text.Trim()
                }                
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
            if (string.IsNullOrWhiteSpace(txtDbSqlite.Text))
            {
                MessageBox.Show("Nome Database è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtServerName.Focus();
                return false;
            }            

            if (string.IsNullOrWhiteSpace(txtRagSoc1.Text))
            {
                MessageBox.Show("Ragione Sociale (Configurazione) è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRagSoc1.Focus();
                return false;
            }
            if (numCap.Value == 0)
            {
                MessageBox.Show("CAP (Configurazione) è un campo obbligatorio e non può essere 0.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numCap.Focus();
                return false;
            }

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
                    this.Cursor = Cursors.WaitCursor;
                    progressBar1.Visible = true;
                    progressBar1.Maximum = 5000;
                    progressBar1.Value = 0;

                    var clienteRepo = _serviceProvider.GetRequiredService<IGenericRepository<Cliente>>();
                    var clienteContattoRepo = _serviceProvider.GetRequiredService<IGenericRepository<ClienteContatto>>();
                    var clienteIndirizzoRepo = _serviceProvider.GetRequiredService<IGenericRepository<ClienteIndirizzo>>();

                    var automezzoRepo = _serviceProvider.GetRequiredService<IGenericRepository<Automezzo>>();
                    var conducenteRepo = _serviceProvider.GetRequiredService<IGenericRepository<Conducente>>();
                    var rimorchioRepo = _serviceProvider.GetRequiredService<IGenericRepository<Rimorchio>>();
                    var auto_condRepo = _serviceProvider.GetRequiredService<IGenericRepository<Autom_Cond>>();
                    var auto_rimRepo = _serviceProvider.GetRequiredService<IGenericRepository<Autom_Rim>>();

                    for (int i = 0; i < 5000; i++)
                    {
                        var cliente = new Cliente
                        {
                            RagSoc = Faker.Company.Name(),
                            CodiceFiscale = Faker.Identification.SocialSecurityNumber(),
                            PartitaIva = GeneratePartitaIva(),
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

                        progressBar1.Value = i + 1;
                        Application.DoEvents();
                    }

                    // Creo 40 automezzi                    
                    var fk = new Bogus.Faker("it");
                    for (int i = 1; i <= 40; i++)
                    {
                        var automezzo = new Automezzo
                        {
                            Descrizione = fk.Vehicle.Model() + " " + fk.Vehicle.Type(),
                            Targa = fk.Vehicle.Vin(),
                            IsTestData = true
                        };
                        await automezzoRepo.AddAsync(automezzo);
                        await automezzoRepo.SaveChangesAsync();
                    }


                    // Creo 100 rimorchi
                    for (int i = 1; i <= 100; i++)
                    {
                        var rimorchio = new Rimorchio
                        {
                            Descrizione = fk.Vehicle.Manufacturer() + " " + i.ToString(),
                            Targa = fk.Vehicle.Vin(),
                            IsTestData = true
                        };
                        await rimorchioRepo.AddAsync(rimorchio);
                        await rimorchioRepo.SaveChangesAsync();
                    }

                    // Creo 10 conducenti dipendenti
                    for (int i = 0; i < 10; i++)
                    {
                        var conducente = new Conducente
                        {
                            Descrizione = Faker.Name.FullName(),
                            Contatto = Faker.Phone.Number(),
                            Tipo = 0,
                            IsTestData = true
                        };
                        await conducenteRepo.AddAsync(conducente);
                        await conducenteRepo.SaveChangesAsync();
                    }

                    // Creo 100 conducenti esterni
                    for (int i = 0; i < 100; i++)
                    {
                        var conducente = new Conducente
                        {
                            Descrizione = Faker.Name.FullName(),
                            Contatto = Faker.Phone.Number(),
                            Tipo = 1,
                            IsTestData = true
                        };
                        await conducenteRepo.AddAsync(conducente);
                        await conducenteRepo.SaveChangesAsync();
                    }

                    // Creo gli abbinamenti
                    var random = new Random();
                    var automezziIds = (await _automezzoRepository.FindAsync(a => a.IsTestData)).Select(a => a.Id).ToList();
                    var conducentiIds = (await _conducenteRepository.FindAsync(c => c.IsTestData)).Select(c => c.Id).ToList();
                    var rimorchiIds = (await _rimorchioRepository.FindAsync(c => c.IsTestData)).Select(c => c.Id).ToList();

                    var associazioni = new List<Autom_Cond>();
                    foreach (var id in automezziIds)
                    {
                        int numeroConducenti = random.Next(1, 11);
                        var conducentiDaAssiociare = conducentiIds.OrderBy(c => random.Next()).Take(numeroConducenti).ToList();
                        foreach (var conducenteId in conducentiDaAssiociare)
                        {
                            associazioni.Add(new Autom_Cond
                            {
                                Id_Automezzo = id,
                                Id_Conducente = conducenteId
                            });
                        }
                    }

                    var associazioniRim = new List<Autom_Rim>();
                    foreach (var id in automezziIds)
                    {
                        int numeroRim = random.Next(1, 21);
                        var rimorchiDaAssiociare = rimorchiIds.OrderBy(c => random.Next()).Take(numeroRim).ToList();
                        foreach (var rimorchioId in rimorchiDaAssiociare)
                        {
                            associazioniRim.Add(new Autom_Rim
                            {
                                Id_Automezzo = id,
                                Id_Rimorchio = rimorchioId
                            });
                        }
                    }

                    MessageBox.Show("Dati di test generati con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante la generazione dei dati di test: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    progressBar1.Visible = false;
                }
            }
        }


        private static string GeneratePartitaIva()
        {
            // Una Partita IVA italiana ha 11 cifre.
            // Questo genererà un numero di 11 cifre, ma senza un checksum valido.
            return GenerateDigits(11);
        }

        // Genera un numero generico di N cifre come stringa
        private static string GenerateDigits(int length)
        {
            const string digits = "0123456789";
            return new string(Enumerable.Repeat(digits, length)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
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
            btnTestConnessione = new Button();
            txtDbPassword = new TextBox();
            lblDbPassword = new Label();
            txtDbUsername = new TextBox();
            lblDbUsername = new Label();
            txtDatabaseName = new TextBox();
            lblDatabaseName = new Label();
            txtServerName = new TextBox();
            lblServerName = new Label();
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
            txtRagSoc1 = new TextBox();
            lblRagSoc = new Label();
            chkDatiTest = new CheckBox();
            btnSalvaConfigurazione = new Button();
            btnGeneraDatiTest = new Button();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            btTestSqlite = new Button();
            txtDbSqlite = new TextBox();
            label6 = new Label();
            tabPage2 = new TabPage();
            txtRagSoc2 = new TextBox();
            tabPage3 = new TabPage();
            txtDestD = new TextBox();
            txtDestR = new TextBox();
            label5 = new Label();
            txtDestTipoR2 = new TextBox();
            label4 = new Label();
            txtDestTipoR1 = new TextBox();
            label3 = new Label();
            txtDestAutoCom = new TextBox();
            label2 = new Label();
            txtDestNumIscr = new TextBox();
            label1 = new Label();
            tabPage4 = new TabPage();
            progressBar1 = new ProgressBar();
            ((System.ComponentModel.ISupportInitialize)numCap).BeginInit();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            SuspendLayout();
            // 
            // btnTestConnessione
            // 
            btnTestConnessione.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTestConnessione.Enabled = false;
            btnTestConnessione.Location = new Point(659, 277);
            btnTestConnessione.Margin = new Padding(6);
            btnTestConnessione.Name = "btnTestConnessione";
            btnTestConnessione.Size = new Size(176, 49);
            btnTestConnessione.TabIndex = 8;
            btnTestConnessione.Text = "Test Connessione";
            btnTestConnessione.UseVisualStyleBackColor = true;
            btnTestConnessione.Click += btnTestConnessione_Click;
            // 
            // txtDbPassword
            // 
            txtDbPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDbPassword.Enabled = false;
            txtDbPassword.Location = new Point(245, 277);
            txtDbPassword.Margin = new Padding(6);
            txtDbPassword.MaxLength = 20;
            txtDbPassword.Name = "txtDbPassword";
            txtDbPassword.Size = new Size(400, 39);
            txtDbPassword.TabIndex = 7;
            txtDbPassword.UseSystemPasswordChar = true;
            // 
            // lblDbPassword
            // 
            lblDbPassword.AutoSize = true;
            lblDbPassword.Location = new Point(50, 284);
            lblDbPassword.Margin = new Padding(6, 0, 6, 0);
            lblDbPassword.Name = "lblDbPassword";
            lblDbPassword.Size = new Size(168, 32);
            lblDbPassword.TabIndex = 6;
            lblDbPassword.Text = "Password (DB):";
            // 
            // txtDbUsername
            // 
            txtDbUsername.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDbUsername.Enabled = false;
            txtDbUsername.Location = new Point(245, 203);
            txtDbUsername.Margin = new Padding(6);
            txtDbUsername.MaxLength = 50;
            txtDbUsername.Name = "txtDbUsername";
            txtDbUsername.Size = new Size(587, 39);
            txtDbUsername.TabIndex = 5;
            // 
            // lblDbUsername
            // 
            lblDbUsername.AutoSize = true;
            lblDbUsername.Location = new Point(50, 209);
            lblDbUsername.Margin = new Padding(6, 0, 6, 0);
            lblDbUsername.Name = "lblDbUsername";
            lblDbUsername.Size = new Size(178, 32);
            lblDbUsername.TabIndex = 4;
            lblDbUsername.Text = "Username (DB):";
            // 
            // txtDatabaseName
            // 
            txtDatabaseName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDatabaseName.Enabled = false;
            txtDatabaseName.Location = new Point(245, 128);
            txtDatabaseName.Margin = new Padding(6);
            txtDatabaseName.MaxLength = 255;
            txtDatabaseName.Name = "txtDatabaseName";
            txtDatabaseName.Size = new Size(587, 39);
            txtDatabaseName.TabIndex = 3;
            // 
            // lblDatabaseName
            // 
            lblDatabaseName.AutoSize = true;
            lblDatabaseName.Location = new Point(50, 134);
            lblDatabaseName.Margin = new Padding(6, 0, 6, 0);
            lblDatabaseName.Name = "lblDatabaseName";
            lblDatabaseName.Size = new Size(190, 32);
            lblDatabaseName.TabIndex = 2;
            lblDatabaseName.Text = "Nome Database:";
            // 
            // txtServerName
            // 
            txtServerName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtServerName.Enabled = false;
            txtServerName.Location = new Point(245, 53);
            txtServerName.Margin = new Padding(6);
            txtServerName.MaxLength = 255;
            txtServerName.Name = "txtServerName";
            txtServerName.Size = new Size(587, 39);
            txtServerName.TabIndex = 1;
            // 
            // lblServerName
            // 
            lblServerName.AutoSize = true;
            lblServerName.Location = new Point(50, 60);
            lblServerName.Margin = new Padding(6, 0, 6, 0);
            lblServerName.Name = "lblServerName";
            lblServerName.Size = new Size(159, 32);
            lblServerName.TabIndex = 0;
            lblServerName.Text = "Nome Server:";
            // 
            // dtpDataIscrizioneAlbo
            // 
            dtpDataIscrizioneAlbo.Format = DateTimePickerFormat.Short;
            dtpDataIscrizioneAlbo.Location = new Point(245, 130);
            dtpDataIscrizioneAlbo.Margin = new Padding(6);
            dtpDataIscrizioneAlbo.Name = "dtpDataIscrizioneAlbo";
            dtpDataIscrizioneAlbo.Size = new Size(591, 39);
            dtpDataIscrizioneAlbo.TabIndex = 18;
            // 
            // lblDataIscrizioneAlbo
            // 
            lblDataIscrizioneAlbo.AutoSize = true;
            lblDataIscrizioneAlbo.Location = new Point(50, 137);
            lblDataIscrizioneAlbo.Margin = new Padding(6, 0, 6, 0);
            lblDataIscrizioneAlbo.Name = "lblDataIscrizioneAlbo";
            lblDataIscrizioneAlbo.Size = new Size(163, 32);
            lblDataIscrizioneAlbo.TabIndex = 17;
            lblDataIscrizioneAlbo.Text = "Data Isc. Albo:";
            // 
            // txtNumeroIscrizioneAlbo
            // 
            txtNumeroIscrizioneAlbo.Location = new Point(245, 55);
            txtNumeroIscrizioneAlbo.Margin = new Padding(6);
            txtNumeroIscrizioneAlbo.MaxLength = 50;
            txtNumeroIscrizioneAlbo.Name = "txtNumeroIscrizioneAlbo";
            txtNumeroIscrizioneAlbo.Size = new Size(591, 39);
            txtNumeroIscrizioneAlbo.TabIndex = 16;
            // 
            // lblNumeroIscrizioneAlbo
            // 
            lblNumeroIscrizioneAlbo.AutoSize = true;
            lblNumeroIscrizioneAlbo.Location = new Point(50, 62);
            lblNumeroIscrizioneAlbo.Margin = new Padding(6, 0, 6, 0);
            lblNumeroIscrizioneAlbo.Name = "lblNumeroIscrizioneAlbo";
            lblNumeroIscrizioneAlbo.Size = new Size(172, 32);
            lblNumeroIscrizioneAlbo.TabIndex = 15;
            lblNumeroIscrizioneAlbo.Text = "Num. Isc. Albo:";
            // 
            // txtCodiceFiscale
            // 
            txtCodiceFiscale.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCodiceFiscale.Location = new Point(245, 559);
            txtCodiceFiscale.Margin = new Padding(6);
            txtCodiceFiscale.MaxLength = 16;
            txtCodiceFiscale.Name = "txtCodiceFiscale";
            txtCodiceFiscale.Size = new Size(591, 39);
            txtCodiceFiscale.TabIndex = 14;
            // 
            // lblCodiceFiscale
            // 
            lblCodiceFiscale.AutoSize = true;
            lblCodiceFiscale.Location = new Point(50, 563);
            lblCodiceFiscale.Margin = new Padding(6, 0, 6, 0);
            lblCodiceFiscale.Name = "lblCodiceFiscale";
            lblCodiceFiscale.Size = new Size(169, 32);
            lblCodiceFiscale.TabIndex = 13;
            lblCodiceFiscale.Text = "Codice Fiscale:";
            // 
            // txtPartitaIva
            // 
            txtPartitaIva.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPartitaIva.Location = new Point(245, 482);
            txtPartitaIva.Margin = new Padding(6);
            txtPartitaIva.MaxLength = 20;
            txtPartitaIva.Name = "txtPartitaIva";
            txtPartitaIva.Size = new Size(591, 39);
            txtPartitaIva.TabIndex = 12;
            // 
            // lblPartitaIva
            // 
            lblPartitaIva.AutoSize = true;
            lblPartitaIva.Location = new Point(50, 491);
            lblPartitaIva.Margin = new Padding(6, 0, 6, 0);
            lblPartitaIva.Name = "lblPartitaIva";
            lblPartitaIva.Size = new Size(127, 32);
            lblPartitaIva.TabIndex = 11;
            lblPartitaIva.Text = "Partita IVA:";
            // 
            // txtEmail
            // 
            txtEmail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtEmail.Location = new Point(245, 410);
            txtEmail.Margin = new Padding(6);
            txtEmail.MaxLength = 100;
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(591, 39);
            txtEmail.TabIndex = 10;
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(50, 416);
            lblEmail.Margin = new Padding(6, 0, 6, 0);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(86, 32);
            lblEmail.TabIndex = 9;
            lblEmail.Text = "E-mail:";
            // 
            // numCap
            // 
            numCap.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            numCap.Location = new Point(245, 335);
            numCap.Margin = new Padding(6);
            numCap.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            numCap.Name = "numCap";
            numCap.Size = new Size(223, 39);
            numCap.TabIndex = 8;
            // 
            // lblCap
            // 
            lblCap.AutoSize = true;
            lblCap.Location = new Point(50, 307);
            lblCap.Margin = new Padding(6, 0, 6, 0);
            lblCap.Name = "lblCap";
            lblCap.Size = new Size(62, 32);
            lblCap.TabIndex = 7;
            lblCap.Text = "CAP:";
            // 
            // txtComune
            // 
            txtComune.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtComune.Location = new Point(245, 258);
            txtComune.Margin = new Padding(6);
            txtComune.MaxLength = 50;
            txtComune.Name = "txtComune";
            txtComune.Size = new Size(591, 39);
            txtComune.TabIndex = 6;
            // 
            // lblComune
            // 
            lblComune.AutoSize = true;
            lblComune.Location = new Point(50, 267);
            lblComune.Margin = new Padding(6, 0, 6, 0);
            lblComune.Name = "lblComune";
            lblComune.Size = new Size(110, 32);
            lblComune.TabIndex = 5;
            lblComune.Text = "Comune:";
            // 
            // txtIndirizzo
            // 
            txtIndirizzo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtIndirizzo.Location = new Point(245, 186);
            txtIndirizzo.Margin = new Padding(6);
            txtIndirizzo.MaxLength = 100;
            txtIndirizzo.Name = "txtIndirizzo";
            txtIndirizzo.Size = new Size(591, 39);
            txtIndirizzo.TabIndex = 4;
            // 
            // lblIndirizzo
            // 
            lblIndirizzo.AutoSize = true;
            lblIndirizzo.Location = new Point(50, 192);
            lblIndirizzo.Margin = new Padding(6, 0, 6, 0);
            lblIndirizzo.Name = "lblIndirizzo";
            lblIndirizzo.Size = new Size(109, 32);
            lblIndirizzo.TabIndex = 3;
            lblIndirizzo.Text = "Indirizzo:";
            // 
            // txtRagSoc1
            // 
            txtRagSoc1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtRagSoc1.Location = new Point(245, 49);
            txtRagSoc1.Margin = new Padding(6);
            txtRagSoc1.MaxLength = 255;
            txtRagSoc1.Name = "txtRagSoc1";
            txtRagSoc1.Size = new Size(591, 39);
            txtRagSoc1.TabIndex = 2;
            // 
            // lblRagSoc
            // 
            lblRagSoc.AutoSize = true;
            lblRagSoc.Location = new Point(50, 55);
            lblRagSoc.Margin = new Padding(6, 0, 6, 0);
            lblRagSoc.Name = "lblRagSoc";
            lblRagSoc.Size = new Size(188, 32);
            lblRagSoc.TabIndex = 1;
            lblRagSoc.Text = "Ragione Sociale:";
            // 
            // chkDatiTest
            // 
            chkDatiTest.AutoSize = true;
            chkDatiTest.Location = new Point(41, 770);
            chkDatiTest.Margin = new Padding(6);
            chkDatiTest.Name = "chkDatiTest";
            chkDatiTest.Size = new Size(212, 36);
            chkDatiTest.TabIndex = 0;
            chkDatiTest.Text = "Abilita Dati Test";
            chkDatiTest.UseVisualStyleBackColor = true;
            // 
            // btnSalvaConfigurazione
            // 
            btnSalvaConfigurazione.Location = new Point(641, 819);
            btnSalvaConfigurazione.Margin = new Padding(6);
            btnSalvaConfigurazione.Name = "btnSalvaConfigurazione";
            btnSalvaConfigurazione.Size = new Size(279, 64);
            btnSalvaConfigurazione.TabIndex = 2;
            btnSalvaConfigurazione.Text = "Salva Configurazione";
            btnSalvaConfigurazione.UseVisualStyleBackColor = true;
            btnSalvaConfigurazione.Click += btnSalvaConfigurazione_Click;
            // 
            // btnGeneraDatiTest
            // 
            btnGeneraDatiTest.Location = new Point(22, 819);
            btnGeneraDatiTest.Margin = new Padding(6);
            btnGeneraDatiTest.Name = "btnGeneraDatiTest";
            btnGeneraDatiTest.Size = new Size(279, 64);
            btnGeneraDatiTest.TabIndex = 3;
            btnGeneraDatiTest.Text = "Genera Dati Test";
            btnGeneraDatiTest.UseVisualStyleBackColor = true;
            btnGeneraDatiTest.Click += btnGeneraDatiTest_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Location = new Point(22, 13);
            tabControl1.Margin = new Padding(4, 2, 4, 2);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(904, 727);
            tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(btTestSqlite);
            tabPage1.Controls.Add(txtDbSqlite);
            tabPage1.Controls.Add(label6);
            tabPage1.Controls.Add(btnTestConnessione);
            tabPage1.Controls.Add(txtServerName);
            tabPage1.Controls.Add(txtDbPassword);
            tabPage1.Controls.Add(lblServerName);
            tabPage1.Controls.Add(lblDbPassword);
            tabPage1.Controls.Add(lblDatabaseName);
            tabPage1.Controls.Add(txtDbUsername);
            tabPage1.Controls.Add(txtDatabaseName);
            tabPage1.Controls.Add(lblDbUsername);
            tabPage1.Location = new Point(8, 46);
            tabPage1.Margin = new Padding(4, 2, 4, 2);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(4, 2, 4, 2);
            tabPage1.Size = new Size(888, 673);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Configurazione Database";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // btTestSqlite
            // 
            btTestSqlite.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btTestSqlite.Location = new Point(656, 478);
            btTestSqlite.Margin = new Padding(6);
            btTestSqlite.Name = "btTestSqlite";
            btTestSqlite.Size = new Size(176, 49);
            btTestSqlite.TabIndex = 11;
            btTestSqlite.Text = "Test Connessione";
            btTestSqlite.UseVisualStyleBackColor = true;
            btTestSqlite.Click += btTestSqlite_Click;
            // 
            // txtDbSqlite
            // 
            txtDbSqlite.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDbSqlite.Location = new Point(245, 416);
            txtDbSqlite.Margin = new Padding(6);
            txtDbSqlite.MaxLength = 255;
            txtDbSqlite.Name = "txtDbSqlite";
            txtDbSqlite.Size = new Size(587, 39);
            txtDbSqlite.TabIndex = 10;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(50, 423);
            label6.Margin = new Padding(6, 0, 6, 0);
            label6.Name = "label6";
            label6.Size = new Size(117, 32);
            label6.TabIndex = 9;
            label6.Text = "Database:";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(txtRagSoc2);
            tabPage2.Controls.Add(txtRagSoc1);
            tabPage2.Controls.Add(lblRagSoc);
            tabPage2.Controls.Add(lblIndirizzo);
            tabPage2.Controls.Add(txtCodiceFiscale);
            tabPage2.Controls.Add(txtIndirizzo);
            tabPage2.Controls.Add(lblCodiceFiscale);
            tabPage2.Controls.Add(lblComune);
            tabPage2.Controls.Add(txtPartitaIva);
            tabPage2.Controls.Add(txtComune);
            tabPage2.Controls.Add(lblPartitaIva);
            tabPage2.Controls.Add(lblCap);
            tabPage2.Controls.Add(txtEmail);
            tabPage2.Controls.Add(numCap);
            tabPage2.Controls.Add(lblEmail);
            tabPage2.Location = new Point(8, 46);
            tabPage2.Margin = new Padding(4, 2, 4, 2);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(4, 2, 4, 2);
            tabPage2.Size = new Size(888, 673);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Dati Azienda";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // txtRagSoc2
            // 
            txtRagSoc2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtRagSoc2.Location = new Point(245, 117);
            txtRagSoc2.Margin = new Padding(6);
            txtRagSoc2.MaxLength = 255;
            txtRagSoc2.Name = "txtRagSoc2";
            txtRagSoc2.Size = new Size(591, 39);
            txtRagSoc2.TabIndex = 16;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(txtDestD);
            tabPage3.Controls.Add(txtDestR);
            tabPage3.Controls.Add(label5);
            tabPage3.Controls.Add(txtDestTipoR2);
            tabPage3.Controls.Add(label4);
            tabPage3.Controls.Add(txtDestTipoR1);
            tabPage3.Controls.Add(label3);
            tabPage3.Controls.Add(txtDestAutoCom);
            tabPage3.Controls.Add(label2);
            tabPage3.Controls.Add(txtDestNumIscr);
            tabPage3.Controls.Add(label1);
            tabPage3.Location = new Point(8, 46);
            tabPage3.Margin = new Padding(4, 2, 4, 2);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(888, 673);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Destinatario";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // txtDestD
            // 
            txtDestD.Location = new Point(427, 126);
            txtDestD.Margin = new Padding(4, 2, 4, 2);
            txtDestD.MaxLength = 3;
            txtDestD.Name = "txtDestD";
            txtDestD.Size = new Size(145, 39);
            txtDestD.TabIndex = 10;
            // 
            // txtDestR
            // 
            txtDestR.Location = new Point(247, 126);
            txtDestR.Margin = new Padding(4, 2, 4, 2);
            txtDestR.MaxLength = 3;
            txtDestR.Name = "txtDestR";
            txtDestR.Size = new Size(145, 39);
            txtDestR.TabIndex = 9;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(52, 128);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(175, 32);
            label5.TabIndex = 8;
            label5.Text = "Destinaz. R e D";
            // 
            // txtDestTipoR2
            // 
            txtDestTipoR2.Location = new Point(247, 341);
            txtDestTipoR2.Margin = new Padding(4, 2, 4, 2);
            txtDestTipoR2.MaxLength = 50;
            txtDestTipoR2.Name = "txtDestTipoR2";
            txtDestTipoR2.Size = new Size(587, 39);
            txtDestTipoR2.TabIndex = 7;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(52, 346);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(134, 32);
            label4.TabIndex = 6;
            label4.Text = "Tipo Riga 2";
            // 
            // txtDestTipoR1
            // 
            txtDestTipoR1.Location = new Point(247, 269);
            txtDestTipoR1.Margin = new Padding(4, 2, 4, 2);
            txtDestTipoR1.MaxLength = 50;
            txtDestTipoR1.Name = "txtDestTipoR1";
            txtDestTipoR1.Size = new Size(587, 39);
            txtDestTipoR1.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(52, 273);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(134, 32);
            label3.TabIndex = 4;
            label3.Text = "Tipo Riga 1";
            // 
            // txtDestAutoCom
            // 
            txtDestAutoCom.Location = new Point(247, 198);
            txtDestAutoCom.Margin = new Padding(4, 2, 4, 2);
            txtDestAutoCom.MaxLength = 30;
            txtDestAutoCom.Name = "txtDestAutoCom";
            txtDestAutoCom.Size = new Size(587, 39);
            txtDestAutoCom.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(52, 203);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(186, 32);
            label2.TabIndex = 2;
            label2.Text = "Aut. Comunicaz.";
            // 
            // txtDestNumIscr
            // 
            txtDestNumIscr.Location = new Point(247, 58);
            txtDestNumIscr.Margin = new Padding(4, 2, 4, 2);
            txtDestNumIscr.MaxLength = 50;
            txtDestNumIscr.Name = "txtDestNumIscr";
            txtDestNumIscr.Size = new Size(587, 39);
            txtDestNumIscr.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(52, 62);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(175, 32);
            label1.TabIndex = 0;
            label1.Text = "Num. Iscr. Albo";
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(dtpDataIscrizioneAlbo);
            tabPage4.Controls.Add(txtNumeroIscrizioneAlbo);
            tabPage4.Controls.Add(lblNumeroIscrizioneAlbo);
            tabPage4.Controls.Add(lblDataIscrizioneAlbo);
            tabPage4.Location = new Point(8, 46);
            tabPage4.Margin = new Padding(4, 2, 4, 2);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(888, 673);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Trasportatore";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(312, 819);
            progressBar1.Margin = new Padding(6);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(318, 64);
            progressBar1.TabIndex = 5;
            progressBar1.Visible = false;
            // 
            // ConfigurazioneForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(973, 939);
            Controls.Add(progressBar1);
            Controls.Add(tabControl1);
            Controls.Add(btnGeneraDatiTest);
            Controls.Add(btnSalvaConfigurazione);
            Controls.Add(chkDatiTest);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(6);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConfigurazioneForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Configurazione Applicazione";
            ((System.ComponentModel.ISupportInitialize)numCap).EndInit();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }
        private System.Windows.Forms.Button btnTestConnessione;
        private System.Windows.Forms.TextBox txtDbPassword;
        private System.Windows.Forms.Label lblDbPassword;
        private System.Windows.Forms.TextBox txtDbUsername;
        private System.Windows.Forms.Label lblDbUsername;
        private System.Windows.Forms.TextBox txtDatabaseName;
        private System.Windows.Forms.Label lblDatabaseName;
        private System.Windows.Forms.TextBox txtServerName;
        private System.Windows.Forms.Label lblServerName;
        public System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblEmail;
        public System.Windows.Forms.NumericUpDown numCap;
        private System.Windows.Forms.Label lblCap;
        public System.Windows.Forms.TextBox txtComune;
        private System.Windows.Forms.Label lblComune;
        public System.Windows.Forms.TextBox txtIndirizzo;
        private System.Windows.Forms.Label lblIndirizzo;
        public System.Windows.Forms.TextBox txtRagSoc1;
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

// File: Program.cs
// Questo file � il punto di ingresso dell'applicazione WinForms.
// Gestisce la configurazione iniziale, la connessione al database e l'avvio della UI.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FormulariRif_G.Data; // Contiene il tuo DbContext (AppDbContext) e IGenericRepository
using FormulariRif_G.Forms; // Per le tue Form (MainForm, LoginForm, ConfigurazioneForm, etc.)
using FormulariRif_G.Models; // Per i tuoi modelli (Configurazione, Utente, Cliente, etc.)
using FormulariRif_G.Utils; // Per CurrentUser, PasswordHasher, EncryptionHelper
using Microsoft.Data.Sqlite;
using System.Text.Json;
using FormulariRif_G.Service; // NUOVO: Namespace per il FormManager (da aggiungere tu)

namespace FormulariRif_G
{
    internal static class Program
    {
        private static IHost? _host;
        
        static async Task Main()
        {
            // Inizializzazione standard dell'applicazione Windows Forms
            // Questa linea � stata spostata qui per essere eseguita una sola volta
            // all'inizio del Main, prima di qualsiasi ciclo o logica UI.
            ApplicationConfiguration.Initialize();

            bool restartApp = true;



            // Ciclo principale dell'applicazione per la gestione della riconfigurazione
            while (restartApp)
            {                
                restartApp = false;

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                string? connectionString = configuration.GetConnectionString("DefaultConnection");

                // --- Logica per la Configurazione Iniziale dell'Applicazione ---
                // A differenza di prima, non serve testare la connessione con credenziali,
                // ma verificare se la stringa di connessione esiste.
                bool configNeeded = string.IsNullOrEmpty(connectionString);

                if (configNeeded)
                {
                    // La logica per mostrare la ConfigurazioneForm rimane simile, ma il modulo
                    // di configurazione ora dovrebbe salvare solo la stringa di connessione SQLite.
                    _host = Host.CreateDefaultBuilder()
                        .ConfigureAppConfiguration((context, config) =>
                        {
                            config.AddConfiguration(configuration);
                        })
                        .ConfigureServices((context, services) =>
                        {
                            services.AddTransient<ConfigurazioneForm>();
                        }).Build();

                    using (var scope = _host.Services.CreateScope())
                    {
                        var configForm = scope.ServiceProvider.GetRequiredService<ConfigurazioneForm>();
                        if (configForm.ShowDialog() == DialogResult.OK)
                        {
                            restartApp = true;
                        }
                        else
                        {
                            Application.Exit();
                        }
                    }
                }

                // --- Logica per l'Avvio Completo dell'Applicazione dopo la Configurazione (o se gi� presente) ---
                // Se � stato richiesto un riavvio o se la configurazione non era necessaria
                if (restartApp || !configNeeded) 
                {                    

                    if (restartApp)
                    {
                        // Ricarica la configurazione se è stata modificata
                        configuration = new ConfigurationBuilder()
                            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .Build();
                        connectionString = configuration.GetConnectionString("DefaultConnection");
                    }

                    // IMPORTANTE: Dispone il vecchio host temporaneo prima di crearne uno nuovo completo
                    _host?.Dispose();

                    // Il metodo che crea l'host ora deve passare solo la stringa di connessione,
                    // non le credenziali di SQL Server
                    _host = CreateFullHostBuilder(configuration, connectionString!).Build();
                    
                    var backup = new Backup();                    
                    backup.CreateBackup(connectionString!);
                    backup.CleanOldBackups(connectionString!);

                    // Applica le migrazioni del database (se necessarie)
                    await ApplyMigrations(_host.Services);
                    await EnsureDefaultUserExists(_host.Services.CreateScope().ServiceProvider);                 

                    // Assicura l'esistenza dell'utente di default e della configurazione aziendale
                    using (var dbScope = _host.Services.CreateScope())
                    {
                        var configRepo = dbScope.ServiceProvider.GetRequiredService<IGenericRepository<Configurazione>>();
                        var existingConfig = (await configRepo.GetAllAsync()).FirstOrDefault();
                        if (existingConfig == null)
                        {
                            // Inserisci la configurazione aziendale di default se non esiste
                            existingConfig = new Configurazione
                            {
                                DatiTest = false,
                                RagSoc1 = "LAMBERTI MARCO",
                                RagSoc2 = "Recupero Materiali Ferroso",
                                Indirizzo = "U.O. C.S. OSSAIA, 40",
                                Comune = "CORTONA (AR)",
                                Cap = 52044,
                                Email = "info@azienda.com",
                                PartitaIva = "01563000510",
                                CodiceFiscale = "LMBMRC79S16D077Q",
                                DestR = "13",
                                DestAutoComunic = "DD 10000 del 06/10/2016",
                                DestTipo1 = "OP. REG PROG. SEM. ART. 214 e 216",
                                DestTipo2 = "152/2006 e P.P.R. 69/203",
                                NumeroIscrizioneAlbo = @"F / 02143",
                                DataIscrizioneAlbo = new DateTime(2025, 05, 08)
                            };
                            await configRepo.AddAsync(existingConfig);
                            await configRepo.SaveChangesAsync();
                        }

                        // Assicura l'esistenza dell'utente di default
                        await EnsureDefaultUserExists(dbScope.ServiceProvider);
                    }

                    // Avvia la LoginForm (modalmente, come era gi�)
                    var loginForm = _host.Services.GetRequiredService<LoginForm>();
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        // Se il login ha successo, avvia la MainForm (ora gestita dal DI)
                        var mainForm = _host.Services.GetRequiredService<MainForm>();
                        // La MainForm ora verr� mostrata usando ShowOrActivate tramite FormManager
                        // Tuttavia, qui stiamo ancora usando ShowDialog. Se la MainForm
                        // ha un proprio ShowDialog che pu� restituire DialogResult.Retry,
                        // allora questa logica rimane.
                        if (mainForm.ShowDialog() == DialogResult.Retry)
                        {
                            restartApp = true; // Se la MainForm richiede un riavvio (es. per cambio utente)
                        }
                    }
                    else
                    {
                        // Se l'utente annulla il login, esci dall'applicazione
                        restartApp = false;
                    }
                }
            }
        }

        /// <summary>
        /// Crea e configura un host completo dell'applicazione con il DbContext e tutti i servizi.
        /// </summary>
        /// <param name="configuration">L'istanza di IConfiguration.</param>
        /// <param name="connectionString">La stringa di connessione completa al database.</param>
        /// <returns>Un'istanza di IHostBuilder.</returns>
        static IHostBuilder CreateFullHostBuilder(IConfiguration configuration, string connectionString)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddConfiguration(configuration);
                })
                .ConfigureServices((context, services) =>
                {
                    // Registrazione del DbContext con la stringa di connessione e lifetime Scoped
                    //services.AddDbContext<AppDbContext>(options =>
                    //{
                    //    // options.UseLazyLoadingProxies(); // Abilita il lazy loading (se vuoi usarlo, assicurati del pacchetto NuGet)
                    //    options.UseSqlServer(connectionString);
                    //}, ServiceLifetime.Scoped); // <-- Imposta il lifetime a Scoped

                    services.AddDbContext<AppDbContext>(options =>
                    {
                        // options.UseLazyLoadingProxies(); // Abilita il lazy loading (se vuoi usarlo)
                        options.UseSqlite(connectionString);
                    }, ServiceLifetime.Scoped);

                    // Registrazione del Generic Repository
                    services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

                    // Registrazione dei Form come Transient (nuova istanza ad ogni richiesta)
                    services.AddTransient<LoginForm>();
                    services.AddTransient<MainForm>();
                    services.AddTransient<ConfigurazioneForm>(); // Gi� presente, la manteniamo

                    // Registrazione di tutti i tuoi Form esistenti e nuovi
                    services.AddTransient<ClientiListForm>();
                    services.AddTransient<ClientiDetailForm>();
                    services.AddTransient<ClientiContattiDetailForm>();
                    services.AddTransient<UtentiListForm>();
                    services.AddTransient<UtentiDetailForm>();
                    services.AddTransient<ClientiIndirizzoDetailForm>();
                    services.AddTransient<AutomezziListForm>();
                    services.AddTransient<AutomezziDetailForm>();
                    services.AddTransient<FormulariRifiutiListForm>();
                    services.AddTransient<FormulariRifiutiDetailForm>();
                    services.AddTransient<ConducentiListForm>();
                    services.AddTransient<ConducentiDetailForm>();
                    services.AddTransient<RimorchiListForm>();
                    services.AddTransient<RimorchiDetailForm>();
                    services.AddTransient<DestinazioniRDListForm>();
                    services.AddTransient<DestinazioniRDDetailForm>();

                    // Aggiungi qui eventuali altri Form del tuo progetto

                    // Registrazione del FormManager come Singleton
                    services.AddSingleton<FormManager>(); // NUOVO: Registrazione del FormManager
                });
        }

        /// <summary>
        /// Applica le migrazioni del database.
        /// </summary>
        /// <param name="serviceProvider">Il service provider per risolvere il DbContext.</param>
        private static async Task ApplyMigrations(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                try
                {
                    await dbContext.Database.MigrateAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante l'applicazione delle migrazioni del database: {ex.Message}", "Errore Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
            }
        }

        /// <summary>
        /// Verifica se esistono utenti nel database e, in caso contrario, ne inserisce uno di default (chris).
        /// </summary>
        /// <param name="serviceProvider">Il service provider per risolvere i servizi.</param>
        private static async Task EnsureDefaultUserExists(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var utenteRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<Utente>>();
                var existingUsers = await utenteRepository.GetAllAsync();

                if (!existingUsers.Any())
                {
                    string defaultUsername = "chris";
                    string defaultPassword = "chris143";

                    string psalt = PasswordHasher.NewPasswordSalt();
                    string hashResult = PasswordHasher.HashPassword(defaultPassword, psalt);

                    var defaultUser = new Utente
                    {
                        NomeUtente = defaultUsername,
                        Password = hashResult,
                        PasswordSalt = psalt,
                        Admin = true,
                        Email = "chris@example.com",
                        MustChangePassword = false
                    };

                    try
                    {
                        await utenteRepository.AddAsync(defaultUser);
                        await utenteRepository.SaveChangesAsync();
                        MessageBox.Show($"Utente di default '{defaultUsername}' creato con successo.", "Setup Utente", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errore durante la creazione dell'utente di default: {ex.Message}", "Errore Setup Utente", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
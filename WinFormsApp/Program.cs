// File: Program.cs
// Questo file è il punto di ingresso dell'applicazione WinForms.
// Gestisce la configurazione iniziale, la connessione al database e l'avvio della UI.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FormulariRif_G.Data;
using FormulariRif_G.Forms;
using FormulariRif_G.Models;
using FormulariRif_G.Utils; // Per CurrentUser, PasswordHasher, EncryptionHelper
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace FormulariRif_G
{
    internal static class Program
    {
        private static IHost? _host;

        [STAThread]
        static async Task Main()
        {
            ApplicationConfiguration.Initialize();

            bool restartApp = true;

            while (restartApp)
            {
                restartApp = false;

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var encryptionKey = configuration["EncryptionKey"];
                if (string.IsNullOrEmpty(encryptionKey))
                {
                    MessageBox.Show("Chiave di criptazione non trovata in appsettings.json. Impossibile procedere.", "Errore di Configurazione", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                try
                {
                    EncryptionHelper.SetKey(encryptionKey);
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show($"Errore nella chiave di criptazione: {ex.Message}", "Errore di Configurazione", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string? serverName = configuration["ConnectionStrings:ServerName"];
                string? databaseName = configuration["ConnectionStrings:DatabaseName"];
                string? encryptedUsername = configuration["EncryptedCredentials:EncryptedUsername"];
                string? encryptedPassword = configuration["EncryptedCredentials:EncryptedPassword"];

                string? dbUsername = null;
                string? dbPassword = null;
                string? connectionString = null;

                bool configNeeded = false;

                if (!string.IsNullOrEmpty(serverName) && !string.IsNullOrEmpty(databaseName) &&
                    !string.IsNullOrEmpty(encryptedUsername) && !string.IsNullOrEmpty(encryptedPassword))
                {
                    try
                    {
                        dbUsername = EncryptionHelper.Decrypt(encryptedUsername);
                        dbPassword = EncryptionHelper.Decrypt(encryptedPassword);
                        connectionString = $"Server={serverName};Database={databaseName};User Id={dbUsername};Password={dbPassword};TrustServerCertificate=True;";

                        using (var conn = new SqlConnection(connectionString))
                        {
                            await conn.OpenAsync();
                            conn.Close();
                        }
                    }
                    catch (Exception)
                    {
                        configNeeded = true;
                    }
                }
                else
                {
                    configNeeded = true;
                }

                if (configNeeded)
                {
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
                            // La ConfigurazioneForm ha già salvato i dati dell'azienda nel DB.
                            // Qui dobbiamo solo segnalare il riavvio.
                            restartApp = true;
                        }
                        else
                        {
                            Application.Exit();
                        }
                    }
                }

                if (restartApp || !configNeeded)
                {
                    if (restartApp)
                    {
                        configuration = new ConfigurationBuilder()
                            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .Build();

                        serverName = configuration["ConnectionStrings:ServerName"];
                        databaseName = configuration["ConnectionStrings:DatabaseName"];
                        encryptedUsername = configuration["EncryptedCredentials:EncryptedUsername"];
                        encryptedPassword = configuration["EncryptedCredentials:EncryptedPassword"];

                        try
                        {
                            dbUsername = EncryptionHelper.Decrypt(encryptedUsername!);
                            dbPassword = EncryptionHelper.Decrypt(encryptedPassword!);
                            connectionString = $"Server={serverName};Database={databaseName};User Id={dbUsername};Password={dbPassword};TrustServerCertificate=True;";
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Errore durante la decriptazione dopo il salvataggio: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Application.Exit();
                        }
                    }

                    _host?.Dispose();
                    _host = CreateFullHostBuilder(configuration, connectionString!).Build();

                    await ApplyMigrations(_host.Services);

                    using (var dbScope = _host.Services.CreateScope())
                    {
                        var configRepo = dbScope.ServiceProvider.GetRequiredService<IGenericRepository<Configurazione>>();
                        var existingConfig = (await configRepo.GetAllAsync()).FirstOrDefault();
                        if (existingConfig == null)
                        {
                            existingConfig = new Configurazione
                            {
                                DatiTest = false,
                                RagSoc1 = "Azienda Standard",
                                Indirizzo = "Via Roma 1",
                                Comune = "Milano",
                                Cap = 20100,
                                Email = "info@azienda.com",
                                PartitaIva = "01234567890", 
                                CodiceFiscale = "RSSMRA80A01H501Z",
                                NumeroIscrizioneAlbo = "ABC/123",
                                DataIscrizioneAlbo = DateTime.Now
                            };
                            await configRepo.AddAsync(existingConfig);
                            await configRepo.SaveChangesAsync();
                        }

                        await EnsureDefaultUserExists(dbScope.ServiceProvider);
                    }

                    var loginForm = _host.Services.GetRequiredService<LoginForm>();
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        var mainForm = _host.Services.GetRequiredService<MainForm>();
                        if (mainForm.ShowDialog() == DialogResult.Retry)
                        {
                            restartApp = true;
                        }
                    }
                    else
                    {
                        restartApp = false;
                    }
                }
            }
        }

        /// <summary>
        /// Crea e configura un host completo dell'applicazione con il DbContext.
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
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        //options.UseLazyLoadingProxies(); // Abilita il lazy loading
                        options.UseSqlServer(connectionString);
                    });

                    services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

                    services.AddTransient<LoginForm>();
                    services.AddTransient<MainForm>();
                    services.AddTransient<ClientiListForm>();
                    services.AddTransient<ClientiDetailForm>();                    
                    services.AddTransient<ClientiContattiDetailForm>();
                    services.AddTransient<UtentiListForm>();
                    services.AddTransient<UtentiDetailForm>();
                    services.AddTransient<ConfigurazioneForm>();
                    // NUOVO: Registrazione dei nuovi form e repository                    
                    services.AddTransient<ClientiIndirizzoDetailForm>();
                    services.AddTransient<AutomezziListForm>();
                    services.AddTransient<AutomezziDetailForm>();
                    services.AddTransient<FormulariRifiutiListForm>();
                    services.AddTransient<FormulariRifiutiDetailForm>();
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

                    // Utilizza la struttura HashResult definita in PasswordHasher
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

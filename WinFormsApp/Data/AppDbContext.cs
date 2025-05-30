// File: Data/AppDbContext.cs
// Questa classe rappresenta il contesto del database per Entity Framework Core.
// Contiene i DbSet per ogni entità e configura il mapping tra le classi e le tabelle del database.
using Microsoft.EntityFrameworkCore;
using FormulariRif_G.Models;

namespace FormulariRif_G.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet per la tabella Clienti
        public DbSet<Cliente> Clienti { get; set; }
        // DbSet per la tabella ClientiContatti
        public DbSet<ClienteContatto> ClientiContatti { get; set; }
        // DbSet per la tabella Utenti
        public DbSet<Utente> Utenti { get; set; }
        // DbSet per la tabella Configurazione
        public DbSet<Configurazione> Configurazioni { get; set; }
        // NUOVO: DbSet per la tabella Automezzi
        public DbSet<Automezzo> Automezzi { get; set; }
        // NUOVO: DbSet per la tabella ClientiIndirizzi
        public DbSet<ClienteIndirizzo> ClientiIndirizzi { get; set; }
        // NUOVO: DbSet per la tabella FormulariRifiuti
        public DbSet<FormularioRifiuti> FormulariRifiuti { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurazione della relazione tra Cliente e ClienteContatto
            modelBuilder.Entity<ClienteContatto>()
                .HasOne(cc => cc.Cliente)
                .WithMany(c => c.Contatti)
                .HasForeignKey(cc => cc.IdCli)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurazione per le colonne nchar in SQL Server, mappate a stringhe con lunghezza fissa in C#.
            modelBuilder.Entity<ClienteContatto>(entity =>
            {
                entity.Property(e => e.Contatto).HasColumnType("nchar(100)");
                entity.Property(e => e.Telefono).HasColumnType("nchar(50)");
                entity.Property(e => e.Email).HasColumnType("nchar(50)");
            });

            // Configurazione per la colonna must_change_password (se il nome della colonna nel DB è diverso dal nome della proprietà)
            modelBuilder.Entity<Utente>(entity =>
            {
                entity.Property(e => e.MustChangePassword).HasColumnName("must_change_password");
            });

            // Configurazione per la colonna is_test_data per Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.Property(e => e.IsTestData).HasColumnName("is_test_data");
                // NUOVO: Aggiunto mapping per CodiceFiscale
                entity.Property(e => e.CodiceFiscale).HasColumnName("codice_fiscale");
            });

            // Configurazione per la colonna is_test_data per ClienteContatto
            modelBuilder.Entity<ClienteContatto>(entity =>
            {
                entity.Property(e => e.IsTestData).HasColumnName("is_test_data");
            });

            // Configurazione per la colonna dati_test per Configurazione
            modelBuilder.Entity<Configurazione>(entity =>
            {
                entity.Property(e => e.DatiTest).HasColumnName("dati_test");
                // NUOVO: Mapping per le nuove proprietà di Configurazione
                entity.Property(e => e.PartitaIva).HasColumnName("partita_iva");
                entity.Property(e => e.CodiceFiscale).HasColumnName("codice_fiscale");
                entity.Property(e => e.NumeroIscrizioneAlbo).HasColumnName("numero_iscrizione_albo");
                entity.Property(e => e.DataIscrizioneAlbo).HasColumnName("data_iscrizione_albo");
            });

            // NUOVO: Configurazione per Automezzo
            modelBuilder.Entity<Automezzo>(entity =>
            {
                entity.Property(e => e.Descrizione).HasColumnName("descrizione");
                entity.Property(e => e.Targa).HasColumnName("targa");
            });

            // NUOVO: Configurazione per ClienteIndirizzo
            modelBuilder.Entity<ClienteIndirizzo>(entity =>
            {
                entity.Property(e => e.IdCli).HasColumnName("id_cli");
                entity.Property(e => e.Indirizzo).HasColumnName("indirizzo");
                entity.Property(e => e.Comune).HasColumnName("comune");
                entity.Property(e => e.Cap).HasColumnName("cap");
                entity.Property(e => e.Predefinito).HasColumnName("predefinito");
                entity.Property(e => e.IsTestData).HasColumnName("is_test_data");

                // Relazione uno-a-molti con Cliente
                entity.HasOne(ci => ci.Cliente)
                      .WithMany(c => c.Indirizzi)
                      .HasForeignKey(ci => ci.IdCli)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indice unico per garantire un solo indirizzo predefinito per cliente (se necessario)
                // Questa regola è più complessa da implementare a livello di DB con un indice unico
                // perché richiede una condizione (WHERE Predefinito = 1).
                // È più semplice gestirla a livello di applicazione.
                // modelBuilder.Entity<ClienteIndirizzo>()
                //     .HasIndex(ci => new { ci.IdCli, ci.Predefinito })
                //     .IsUnique()
                //     .HasFilter("[predefinito] = 1"); // Richiede SQL Server 2016+
            });

            // NUOVO: Configurazione per FormularioRifiuti
            modelBuilder.Entity<FormularioRifiuti>(entity =>
            {
                entity.Property(e => e.Data).HasColumnName("data");
                entity.Property(e => e.IdCli).HasColumnName("id_cli");
                entity.Property(e => e.IdClienteIndirizzo).HasColumnName("id_cliente_indirizzo");
                entity.Property(e => e.NumeroFormulario).HasColumnName("numero_formulario");
                entity.Property(e => e.IdAutomezzo).HasColumnName("id_automezzo");

                // Relazioni con Cliente, ClienteIndirizzo, Automezzo
                entity.HasOne(fr => fr.Cliente)
                      .WithMany() // Formulari non hanno una collezione diretta su Cliente
                      .HasForeignKey(fr => fr.IdCli)
                      .OnDelete(DeleteBehavior.Restrict); // Non eliminare il cliente se ha formulari

                entity.HasOne(fr => fr.ClienteIndirizzo)
                      .WithMany() // Formulari non hanno una collezione diretta su ClienteIndirizzo
                      .HasForeignKey(fr => fr.IdClienteIndirizzo)
                      .OnDelete(DeleteBehavior.Restrict); // Non eliminare l'indirizzo se ha formulari

                entity.HasOne(fr => fr.Automezzo)
                      .WithMany() // Formulari non hanno una collezione diretta su Automezzo
                      .HasForeignKey(fr => fr.IdAutomezzo)
                      .OnDelete(DeleteBehavior.Restrict); // Non eliminare l'automezzo se ha formulari
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

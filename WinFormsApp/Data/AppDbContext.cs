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

        
        public DbSet<Cliente> Clienti { get; set; }
        
        public DbSet<ClienteContatto> ClientiContatti { get; set; }
        
        public DbSet<Utente> Utenti { get; set; }
        
        public DbSet<Configurazione> Configurazioni { get; set; }
        
        public DbSet<Automezzo> Automezzi { get; set; }
        
        public DbSet<ClienteIndirizzo> ClientiIndirizzi { get; set; }
        
        public DbSet<FormularioRifiuti> FormulariRifiuti { get; set; }

        public DbSet<Conducente> Conducenti { get; set; }

        public DbSet<Autom_Cond> FKAutomCond { get; set; }


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
            
            modelBuilder.Entity<Configurazione>(entity =>
            {
                entity.Property(e => e.DatiTest).HasColumnName("dati_test");
                // NUOVO: Mapping per le nuove proprietà di Configurazione
                entity.Property(e => e.PartitaIva).HasColumnName("partita_iva");
                entity.Property(e => e.CodiceFiscale).HasColumnName("codice_fiscale");
                entity.Property(e => e.NumeroIscrizioneAlbo).HasColumnName("numero_iscrizione_albo");
                entity.Property(e => e.DataIscrizioneAlbo).HasColumnName("data_iscrizione_albo");
            });
            
            modelBuilder.Entity<Automezzo>(entity =>
            {
                entity.Property(e => e.Descrizione).HasColumnName("descrizione");
                entity.Property(e => e.Targa).HasColumnName("targa");
            });

            modelBuilder.Entity<Conducente>(entity =>
            {
                entity.Property(e => e.Descrizione).HasColumnName("descrizione");
                entity.Property(e => e.Contatto).HasColumnName("contatto");
                entity.Property(e => e.Tipo).HasColumnName("tipo");
            });

            modelBuilder.Entity<Autom_Cond>()
                .HasKey(ac => new { ac.Id_Automezzo, ac.Id_Conducente });

            // Configura la relazione tra Autom_Cond e Automezzo
            modelBuilder.Entity<Autom_Cond>()
                .HasOne(ac => ac.Automezzo)
                .WithMany(a => a.AutomezziConducenti)
                .HasForeignKey(ac => ac.Id_Automezzo);

            // Configura la relazione tra Autom_Cond e Conducente
            modelBuilder.Entity<Autom_Cond>()
                .HasOne(ac => ac.Conducente)
                .WithMany(c => c.ConducentiAutomezzi)
                .HasForeignKey(ac => ac.Id_Conducente);

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
                      .OnDelete(DeleteBehavior.Restrict); // Modificato da Cascade a Restrict per evitare cicli di eliminazione
            });

            
            modelBuilder.Entity<FormularioRifiuti>(entity =>
            {
                entity.Property(e => e.Data).HasColumnName("data");
                entity.Property(e => e.IdProduttore).HasColumnName("id_produttore");
                entity.Property(e => e.IdProduttoreIndirizzo).HasColumnName("id_produttore_indirizzo");
                entity.Property(e => e.IdDestinatario).HasColumnName("id_destinatario");
                entity.Property(e => e.IdDestinatarioIndirizzo).HasColumnName("id_destinatario_indirizzo");
                entity.Property(e => e.IdTrasportatore).HasColumnName("id_trasportatore");
                entity.Property(e => e.IdTrasportatoreIndirizzo).HasColumnName("id_trasportatore_indirizzo");
                entity.Property(e => e.NumeroFormulario).HasColumnName("numero_formulario");
                entity.Property(e => e.IdAutomezzo).HasColumnName("id_automezzo");

                // Relazioni con Cliente, ClienteIndirizzo, Automezzo
                entity.HasOne(fr => fr.Produttore)
                      .WithMany() // Formulari non hanno una collezione diretta su Cliente
                      .HasForeignKey(fr => fr.IdProduttore)
                      .OnDelete(DeleteBehavior.Restrict); // Non eliminare il cliente se ha formulari

                entity.HasOne(fr => fr.ProduttoreIndirizzo)
                      .WithMany() // Formulari non hanno una collezione diretta su ClienteIndirizzo
                      .HasForeignKey(fr => fr.IdProduttoreIndirizzo)
                      .OnDelete(DeleteBehavior.Restrict); // Non eliminare l'indirizzo se ha formulari

                entity.HasOne(fr => fr.Destinatario)
                      .WithMany() // Formulari non hanno una collezione diretta su Cliente
                      .HasForeignKey(fr => fr.IdDestinatario)
                      .OnDelete(DeleteBehavior.Restrict); // Non eliminare il cliente se ha formulari

                entity.HasOne(fr => fr.DestinatarioIndirizzo)
                      .WithMany() // Formulari non hanno una collezione diretta su ClienteIndirizzo
                      .HasForeignKey(fr => fr.IdDestinatarioIndirizzo)
                      .OnDelete(DeleteBehavior.Restrict); // Non eliminare l'indirizzo se ha formulari

                entity.HasOne(fr => fr.Trasportatore)
                      .WithMany() // Formulari non hanno una collezione diretta su Cliente
                      .HasForeignKey(fr => fr.IdTrasportatore)
                      .OnDelete(DeleteBehavior.Restrict); // Non eliminare il cliente se ha formulari

                entity.HasOne(fr => fr.TrasportatoreIndirizzo)
                      .WithMany() // Formulari non hanno una collezione diretta su ClienteIndirizzo
                      .HasForeignKey(fr => fr.IdTrasportatoreIndirizzo)
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

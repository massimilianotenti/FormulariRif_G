// NOTA: La logica di logging è stata cambiata per scrivere su un file di testo.
// La dipendenza da AppDbContext è stata rimossa.
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormulariRif_G.Utils
{
    public class Backup
    {
        public void CreateBackup(string connectionString)
        {
            string actualDbPath = GetActualDbPath(connectionString);

            try
            {
                // --- CONTROLLO AGGIUNTO ---
                // Per un backup sicuro di SQLite, i file di journaling (-wal e -shm) non devono esistere.
                // La loro presenza indica che il database è in uso o non è stato chiuso correttamente.
                string walFile = actualDbPath + "-wal";
                string shmFile = actualDbPath + "-shm";

                if (File.Exists(walFile) || File.Exists(shmFile))
                {
                    Log(connectionString, "WARN", "Backup annullato. Il database è in uso o non è stato chiuso correttamente (file -wal o -shm presenti).");
                    return; // Interrompe l'operazione di backup.
                }

                string dbDirectory = Path.GetDirectoryName(actualDbPath);
                string dbFileName = Path.GetFileName(actualDbPath);
                string backupFolder = Path.Combine(dbDirectory, "Backups");

                // Nome del file zip con timestamp per evitare sovrascritture e nome del file originale per chiarezza.
                string zipFileName = $"bk{DateTime.Now:yyyyMMddHHmmss}_{dbFileName}.zip";
                string zipFilePath = Path.Combine(backupFolder, zipFileName);

                // Crea la cartella dei backup se non esiste
                Directory.CreateDirectory(backupFolder);

                // Il metodo originale ZipFile.CreateFromDirectory si aspetta una cartella, ma il parametro
                // 'fileDB' suggerisce un file. Ho modificato la logica per zippare un singolo file.
                // Se 'fileDB' è una cartella, ripristinare la riga: ZipFile.CreateFromDirectory(fileDB, zipFilePath);
                using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    zipArchive.CreateEntryFromFile(actualDbPath, dbFileName);
                }

                Log(connectionString, "INFO", $"Backup creato con successo: {zipFilePath}");
            }
            catch (Exception ex)
            {
                Log(connectionString, "ERROR", $"Errore durante la creazione del backup: {ex.Message}");
            }
        }

        public void CleanOldBackups(string connectionString)
        {
            string actualDbPath = GetActualDbPath(connectionString);

            try
            {
                string dbDirectory = Path.GetDirectoryName(actualDbPath);
                string backupFolder = Path.Combine(dbDirectory, "Backups");
                if (!Directory.Exists(backupFolder))
                {
                    // Non è un errore, semplicemente non c'è nulla da pulire.
                    return;
                }

                var backupFiles = Directory.GetFiles(backupFolder, "bk*.zip");
                const int maxBackupsPerDay = 5;
                int totalCleanedFiles = 0;

                // Raggruppa tutti i backup per giorno (es. "20231027")
                var backupsByDay = backupFiles
                    .Where(file => Path.GetFileName(file).Length >= 18 && Path.GetFileName(file).StartsWith("bk")) // Nome file deve contenere almeno bkYYYYMMDDHHMMSS_
                    .GroupBy(file => Path.GetFileName(file).Substring(2, 8));

                foreach (var dayGroup in backupsByDay)
                {
                    var dailyBackups = dayGroup.OrderBy(f => f).ToList();

                    if (dailyBackups.Count > maxBackupsPerDay)
                    {
                        // Ci sono troppi backup per questo giorno, è necessario "diradarli".
                        var filesToKeep = new HashSet<string>();

                        // 1. Conserva sempre il primo e l'ultimo backup della giornata.
                        filesToKeep.Add(dailyBackups.First());
                        filesToKeep.Add(dailyBackups.Last());

                        // 2. Seleziona i restanti backup da conservare in modo distribuito.
                        var middleBackups = dailyBackups.Skip(1).Take(dailyBackups.Count - 2).ToList();
                        int numToKeepFromMiddle = maxBackupsPerDay - 2;

                        if (numToKeepFromMiddle > 0 && middleBackups.Any())
                        {
                            // Calcola l'intervallo per scegliere i file in modo uniforme.
                            double interval = (double)middleBackups.Count / (numToKeepFromMiddle + 1);
                            for (int i = 1; i <= numToKeepFromMiddle; i++)
                            {
                                int indexToKeep = (int)Math.Round(i * interval) - 1;
                                if (indexToKeep >= 0 && indexToKeep < middleBackups.Count)
                                {
                                    filesToKeep.Add(middleBackups[indexToKeep]);
                                }
                            }
                        }

                        // 3. Elimina tutti i file che non sono nella lista di quelli da conservare.
                        foreach (var fileToDelete in dailyBackups.Where(f => !filesToKeep.Contains(f)))
                        {
                            File.Delete(fileToDelete);
                            Log(connectionString, "INFO", $"Backup diradato eliminato: {Path.GetFileName(fileToDelete)}");
                            totalCleanedFiles++;
                        }
                    }
                }

                if (totalCleanedFiles > 0)
                {
                    Log(connectionString, "INFO", $"Pulizia per diradamento completata. Eliminati {totalCleanedFiles} backup.");
                }
                else
                {
                    Log(connectionString, "INFO", "Pulizia dei backup completata. Nessun file in eccesso da eliminare.");
                }
            }
            catch (Exception ex)
            {
                Log(connectionString, "ERROR", $"Errore durante la pulizia dei backup: {ex.Message}");
            }
        }


        private void Log(string connectionString, string level, string message)
        {
            try
            {
                string actualDbPath = GetActualDbPath(connectionString);
                string dbDirectory = Path.GetDirectoryName(actualDbPath);
                string logFilePath = Path.Combine(dbDirectory, "backup_log.txt");

                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level.ToUpper()}] {message}{Environment.NewLine}";

                // Aggiunge il testo al file. Crea il file se non esiste.
                File.AppendAllText(logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                // Fallback sulla console se il logging su file fallisce per qualsiasi motivo.
                Console.WriteLine($"[FILE LOG FALLITO] Level: {level}, Message: {message}, Errore: {ex.Message}");
            }
        }

        /// <summary>
        /// Estrae il percorso fisico del file di database da una stringa di connessione SQLite.
        /// Se la stringa non è una connection string, la restituisce così com'è.
        /// </summary>
        /// <param name="connectionStringOrPath">La stringa di connessione o un percorso diretto.</param>
        /// <returns>Il percorso fisico del file .db</returns>
        private string GetActualDbPath(string connectionStringOrPath)
        {
            // Questo gestisce in modo robusto le stringhe di connessione come "Data Source=C:\path\to\db.sqlite"
            // usando un approccio semplice senza aggiungere dipendenze esterne.
            // Per una maggiore robustezza si potrebbe usare Microsoft.Data.Sqlite.SqliteConnectionStringBuilder.
            var parts = connectionStringOrPath.Split(';');
            var dataSourcePart = parts.FirstOrDefault(p => p.Trim().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase));

            if (dataSourcePart != null)
            {
                return dataSourcePart.Trim().Substring("Data Source=".Length);
            }

            // Se non trova "Data Source=", assume che sia già un percorso di file.
            return connectionStringOrPath;
        }
    }
}

Per la aggiornare il database servono questi pacchetti:
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.EntityFrameworkCore.Desig

Dal momento che utente e password nel file di appsettings.json sono criptati aprire *Visualizza -> Terminale* 
e lanciare il comando:

```dotnet ef migrations add [NomeDellaMigrazione]```

ad esempio:

```dotnet ef migrations add CreazioneDB --connection "Server=dsk03\sqlexpress;Database=FormulariRifiuti;User ID=sa;Password=white!3CURVER;"```


Normalmente in produzione le credenziali vengono gestite in modo più sicuro, come le variabili d'ambiente, un vault di chiavi (es. Azure Key Vault). In quel caso si possono usare i comandi usano i comandi nella console di gestione pacchetti aprendo *Strumenti -> Gestione pacchetti NuGet -> Console di gestione pacchetti* e lanciando il comando: 

```Add-Migration [NomeDellaMigration]```

Su ```Program.cs``` c'è il metodo ApplyMigrations che si occupa di passare aggiornare il database

```await ApplyMigrations(_host.Services);```
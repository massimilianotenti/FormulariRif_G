## Gestione Anagrafiche e Dati

* **Gestione Anagrafiche Conducenti:**
    * [x] Implementare la gestione delle anagrafiche conducenti, associando uno o più conducenti (dipendenti o trasportatori esterni) a ciascun mezzo.
    * [ ] quando inserisco un formulario devo poi indicare il conducente scegliendo tra quelli associati al mezzo
* **Centralizzazione Anagrafiche Soggetti:**
    * [ ] Assicurarsi che produttore, destinatario e trasportatore attingano da un'unica anagrafica soggetti.
    * [ ] Prevedere la possibilità che un soggetto possa ricoprire più ruoli contemporaneamente (es. destinatario che è anche trasportatore, o produttore che è anche trasportatore).
* **Flag Detentore:**
    * [ ] Prevedere un flag "detentore" sul modulo, spuntabile sul produttore se è anche il detentore dei rifiuti.
* **Codice R13/R4:**
    * [ ] Il codice non è fisso R13; può essere **R13** o **R4**. Va inserito nel formulario, non nell'anagrafica.
* **Alternazione Comunale:**
    * [ ] L'alternazione comunale è fissa per l'anagrafica soggetto.
* **Tipologia:**
    * [ ] La tipologia deve essere un elenco tabellato, basato sulle circolari ministeriali fornite.
* **Intermediario Commerciale:**
    * [ ] Valutare la compilazione dell'intermediario commerciale nel questionario.
* **Stato Fisico:**
    * [ ] Lo stato fisico deve essere una lettera, non un valore numerico.
* **Gestione Automezzi e Rimorchi:**
    * [ ] La targa è fissa per l'automezzo.
    * [ ] Il rimorchio è variabile: un automezzo può avere uno o più rimorchi.
* **Unità Locale:**
    * [ ] L'unità locale deve essere tabellata tra gli indirizzi dei soggetti. La ragione sociale, partita IVA e indirizzo della sede principale vanno nella parte superiore del modulo, mentre l'unità locale (uno degli indirizzi dell'anagrafica del soggetto) va nel rigo dedicato.

---

## Stampa e Layout

* **Problemi di Stampa:**
    * [ ] Risolvere i problemi di disallineamento del layout (margini diversi) in fase di stampa, basandosi sul PDF fornito.
    * [ ] Aumentare la dimensione del carattere in stampa per migliorarne la leggibilità.
    * [ ] Utilizzare ProBat Pro per le modifiche al PDF.

---

## Architettura e Performance

* **Migrazione Database (Valutazione):**
    * [ ] Valutare il passaggio da SQL Server a **SQLite**, in quanto SQL Server 2022 Express non è compatibile con Windows 7, presente sulla macchina del cliente.
* **Debug Funzioni di Caricamento Asincrone:**
    * [ ] Indagare e risolvere i problemi di caricamento (es. "data source già aperto") che si verificano con le funzioni asincrone lanciate al `load` (ad esempio, nella lista dei formulari).
    * [ ] Verificare se il problema è legato alla latenza, data la differenza di comportamento tra computer desktop e notebook.
    * [ ] Controllare se l'evento `load` e `on change` del campo di ricerca scattano contemporaneamente.
* **Filtro Dati di Test:**
    * [ ] Assicurarsi che le form di produzione non mostrino i dati di test (attualmente vengono esclusi nell'elenco clienti ma inclusi nel formulario).
    * [ ] Valutare la creazione di un comando specifico per popolare il DB con dati di test solo negli ambienti di sviluppo.

---

## Gestione Form e Interfaccia Utente

* **Logica di Salvataggio:**
    * [ ] Rifattorizzare la logica di salvataggio per sfruttare il data binding del designer di Visual Studio, evitando `load` asincrone a codice. Il bottone salva dovrebbe richiamare `dbContext.SaveChanges()` e aggiornare la data grid.
* **Long Closing (Dispose Context):**
    * [ ] Implementare il `Long Closing` (dispose del DB Context) alla chiusura delle form, specialmente se il `NewDB Context` è creato all'interno della form, per evitare problemi di connessioni massime raggiunte o instanze multiple. Testare lo scenario di apertura/chiusura/riapertura delle form.
* **Gestione Form Modali (Dialoghi):**
    * [ ] Rifattorizzare la gestione delle form per evitare l'uso esclusivo di dialoghi modali.
    * [ ] Valutare l'implementazione di un **service provider** per la gestione delle form, che consenta di avere più form aperte contemporaneamente (es. clienti e formulari). Il service provider dovrebbe gestire l'apertura delle form in base alla classe, portando in primo piano un'istanza esistente o creandone una nuova se non presente, per evitare problemi con query sugli stessi dati o istanze multiple dello stesso oggetto.

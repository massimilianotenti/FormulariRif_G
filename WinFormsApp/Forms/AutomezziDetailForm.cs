// File: Forms/AutomezziDetailForm.cs
// Questo form permette di inserire o modificare un singolo automezzo.
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using System; // Per EventArgs e Exception
using System.Windows.Forms; // Per Form, MessageBox, DialogResult
using System.Threading.Tasks; // Per Task
using System.Collections.Generic; // Per List<T>
using System.Linq; // Per LINQ

namespace FormulariRif_G.Forms
{
    public partial class AutomezziDetailForm : Form
    {
        private readonly IGenericRepository<Automezzo> _automezzoRepository;
        private readonly IGenericRepository<Conducente> _conducenteRepository;
        private readonly IGenericRepository<Autom_Cond> _automCondRepository;
        private Automezzo? _currentAutomezzo;

        // Dichiarazioni dei controlli (corrispondenti al tuo Designer.cs)
        private System.Windows.Forms.Label lblDescrizione;
        private System.Windows.Forms.TextBox txtDescrizione;
        private System.Windows.Forms.Label lblTarga;
        private System.Windows.Forms.TextBox txtTarga;
        private System.Windows.Forms.Button btnSalva;
        // Se hai un pulsante Annulla nel tuo designer, aggiungilo qui:
        private System.Windows.Forms.Button btnAnnulla;
        private System.Windows.Forms.CheckedListBox clbConducenti;
        private System.Windows.Forms.Label lblConducenti;

        public AutomezziDetailForm(IGenericRepository<Automezzo> automezzoRepository, IGenericRepository<Conducente> conducenteRepository, IGenericRepository<Autom_Cond> automCondRepository)
        {
            InitializeComponent();
            _automezzoRepository = automezzoRepository;

            _conducenteRepository = conducenteRepository;
            _automCondRepository = automCondRepository;

            // Collega gli handler degli eventi ai pulsanti.
            // Questi collegamenti sono stati spostati qui dal Designer.cs.
            if (btnSalva != null) btnSalva.Click += btnSalvaClick;
            // Se hai un pulsante Annulla, scommenta o aggiungi la riga qui sotto:
            if (btnAnnulla != null) btnAnnulla.Click += btnAnnullaClick;
        }

        /// <summary>
        /// Imposta l'automezzo da visualizzare o modificare.
        /// </summary>
        /// <param name="automezzo">L'oggetto Automezzo.</param>
        public async void SetAutomezzo(Automezzo automezzo)
        {
            _currentAutomezzo = automezzo;
            LoadAutomezzoData();
            // Aggiorna il titolo del form in base alla modalità (nuovo/modifica)
            this.Text = _currentAutomezzo.Id == 0 ? "Nuovo Automezzo" : "Modifica Automezzo";
            await LoadConducentiForAutomezzo();
        }

        /// <summary>
        /// Carica i dati dell'automezzo nei controlli del form.
        /// </summary>
        private void LoadAutomezzoData()
        {
            if (_currentAutomezzo != null)
            {
                txtDescrizione.Text = _currentAutomezzo.Descrizione;
                txtTarga.Text = _currentAutomezzo.Targa;
            }
            else
            {
                txtDescrizione.Text = string.Empty;
                txtTarga.Text = string.Empty;
            }
        }

        /// <summary>
        /// Carica la lista di tutti i conducenti e seleziona quelli associati all'automezzo corrente.
        /// </summary>
        private async Task LoadConducentiForAutomezzo()
        {
            clbConducenti.Items.Clear();
            var allConducenti = (await _conducenteRepository.GetAllAsync()).ToList();

            foreach (var conducente in allConducenti)
            {
                clbConducenti.Items.Add(conducente, false);
            }

            if (_currentAutomezzo != null && _currentAutomezzo.Id != 0)
            {
                var associatedConducentiIds = (await _automCondRepository.FindAsync(ac => ac.Id_Automezzo == _currentAutomezzo.Id))
                                                .Select(ac => ac.Id_Conducente)
                                                .ToList();

                for (int i = 0; i < clbConducenti.Items.Count; i++)
                {
                    if (clbConducenti.Items[i] is Conducente conducenteItem)
                    {
                        if (associatedConducentiIds.Contains(conducenteItem.Id))
                        {
                            clbConducenti.SetItemChecked(i, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Salva".
        /// </summary>
        private async void btnSalvaClick(object? sender, EventArgs e) // Aggiunto ? per nullable
        {
            if (!ValidateInput())
            {
                return;
            }

            if (_currentAutomezzo == null)
            {
                _currentAutomezzo = new Automezzo();
            }

            _currentAutomezzo.Descrizione = txtDescrizione.Text.Trim();
            _currentAutomezzo.Targa = txtTarga.Text.Trim();

            try
            {
                if (_currentAutomezzo.Id == 0) // Nuovo automezzo
                {
                    await _automezzoRepository.AddAsync(_currentAutomezzo);
                }
                else // Automezzo esistente
                {
                    _automezzoRepository.Update(_currentAutomezzo);
                }
                await _automezzoRepository.SaveChangesAsync();

                // Gestione delle associazioni Conducente-Automezzo
                if (_currentAutomezzo.Id != 0)
                {
                    // Approccio più efficiente: Sincronizza le associazioni invece di cancellare e ricreare.

                    // 1. Ottieni gli ID dei conducenti attualmente selezionati nella UI.
                    var selectedConducenteIds = clbConducenti.CheckedItems
                                                             .OfType<Conducente>()
                                                             .Select(c => c.Id)
                                                             .ToHashSet();

                    // 2. Ottieni le associazioni esistenti dal database.
                    var existingAssociations = (await _automCondRepository.FindAsync(ac => ac.Id_Automezzo == _currentAutomezzo.Id)).ToList();
                    var existingConducenteIds = existingAssociations.Select(ac => ac.Id_Conducente).ToHashSet();

                    // 3. Trova e cancella le associazioni che non sono più selezionate.
                    var associationsToDelete = existingAssociations.Where(assoc => !selectedConducenteIds.Contains(assoc.Id_Conducente));
                    foreach (var association in associationsToDelete)
                    {
                        _automCondRepository.Delete(association);
                    }

                    // 4. Trova e aggiungi le nuove associazioni. È qui che si verifica l'errore IDENTITY_INSERT.
                    //    La riga seguente crea un nuovo oggetto Autom_Cond. La sua chiave primaria (es. Id)
                    //    ha il valore predefinito (0). Il database si rifiuta di inserire questo valore esplicito.
                    var conducenteIdsToAdd = selectedConducenteIds.Where(id => !existingConducenteIds.Contains(id));
                    foreach (var conducenteId in conducenteIdsToAdd)
                    {
                        // La soluzione definitiva è correggere il modello Autom_Cond, come spiegato sopra.
                        await _automCondRepository.AddAsync(new Autom_Cond { Id_Automezzo = _currentAutomezzo.Id, Id_Conducente = conducenteId });
                    }
                    await _automCondRepository.SaveChangesAsync(); // Salva tutte le modifiche (delete e add) in una sola volta.
                }

                MessageBox.Show("Automezzo salvato con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK; // Imposta il DialogResult a OK
                this.Close(); // Chiude il form
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio dell'automezzo: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Esegue la validazione dei campi di input.
        /// </summary>
        /// <returns>True se l'input è valido, altrimenti False.</returns>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtDescrizione.Text))
            {
                MessageBox.Show("Descrizione è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDescrizione.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtTarga.Text))
            {
                MessageBox.Show("Targa è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTarga.Focus();
                return false;
            }
            return true;
        }

        // Se hai un pulsante Annulla, implementa il suo handler:
        private void btnAnnullaClick(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Codice generato dal designer
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
            lblDescrizione = new Label();
            txtDescrizione = new TextBox();
            lblTarga = new Label();
            txtTarga = new TextBox();
            btnSalva = new Button();
            // Se hai un pulsante Annulla nel tuo designer, devi dichiararlo qui.
            btnAnnulla = new Button();
            clbConducenti = new CheckedListBox();
            lblConducenti = new Label();
            SuspendLayout();
            //
            // lblDescrizione
            //
            lblDescrizione.AutoSize = true;
            lblDescrizione.Location = new Point(20, 30);
            lblDescrizione.Name = "lblDescrizione";
            lblDescrizione.Size = new Size(70, 15);
            lblDescrizione.TabIndex = 0;
            lblDescrizione.Text = "Descrizione:";
            //
            // txtDescrizione
            //
            txtDescrizione.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDescrizione.Location = new Point(100, 27);
            txtDescrizione.MaxLength = 255;
            txtDescrizione.Name = "txtDescrizione";
            txtDescrizione.Size = new Size(270, 23);
            txtDescrizione.TabIndex = 1;
            //
            // lblTarga
            //
            lblTarga.AutoSize = true;
            lblTarga.Location = new Point(20, 70);
            lblTarga.Name = "lblTarga";
            lblTarga.Size = new Size(39, 15);
            lblTarga.TabIndex = 2;
            lblTarga.Text = "Targa:";
            //
            // txtTarga
            //
            txtTarga.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTarga.Location = new Point(100, 67);
            txtTarga.MaxLength = 20;
            txtTarga.Name = "txtTarga";
            txtTarga.Size = new Size(270, 23);
            txtTarga.TabIndex = 3;
            //
            // lblConducenti
            //
            lblConducenti.AutoSize = true;
            lblConducenti.Location = new Point(20, 100);
            lblConducenti.Name = "lblConducenti";
            lblConducenti.Size = new Size(68, 15);
            lblConducenti.TabIndex = 4;
            lblConducenti.Text = "Conducenti:";
            //
            // clbConducenti
            //
            clbConducenti.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            clbConducenti.FormattingEnabled = true;
            clbConducenti.Location = new Point(100, 100);
            clbConducenti.Name = "clbConducenti";
            clbConducenti.Size = new Size(270, 94);
            clbConducenti.TabIndex = 5;
            clbConducenti.DisplayMember = "Descrizione"; // Set DisplayMember to show Description
            //
            // btnSalva
            //
            btnSalva.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSalva.Location = new Point(295, 230);
            btnSalva.Name = "btnSalva";
            btnSalva.Size = new Size(75, 30);
            btnSalva.TabIndex = 6;
            btnSalva.Text = "Salva";
            btnSalva.UseVisualStyleBackColor = true;
            //
            // btnAnnulla
            //
            btnAnnulla.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAnnulla.Location = new Point(210, 230);
            btnAnnulla.Name = "btnAnnulla";
            btnAnnulla.Size = new Size(75, 30);
            btnAnnulla.TabIndex = 7;
            btnAnnulla.Text = "Annulla";
            btnAnnulla.UseVisualStyleBackColor = true;
            //
            // AutomezziDetailForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(390, 275);
            Controls.Add(clbConducenti);
            Controls.Add(lblConducenti);
            Controls.Add(btnAnnulla);
            Controls.Add(btnSalva);
            Controls.Add(txtTarga);
            Controls.Add(lblTarga);
            Controls.Add(txtDescrizione);
            Controls.Add(lblDescrizione);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AutomezziDetailForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Dettagli Automezzo"; // Verrà aggiornato da SetAutomezzo
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
    }
}
// File: Forms/ClientiIndirizzoDetailForm.cs
// Questo form permette di inserire o modificare un singolo indirizzo per un cliente.
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using System.Linq; // Per Count()

namespace FormulariRif_G.Forms
{
    public partial class ClientiIndirizzoDetailForm : Form
    {
        private readonly IGenericRepository<ClienteIndirizzo> _clienteIndirizzoRepository;
        private ClienteIndirizzo? _currentIndirizzo;
        private TextBox numCap;
        private int _idCliCorrente; // Aggiunto per tenere traccia dell'IdCli per i nuovi indirizzi

        public ClientiIndirizzoDetailForm(IGenericRepository<ClienteIndirizzo> clienteIndirizzoRepository)
        {
            InitializeComponent();
            _clienteIndirizzoRepository = clienteIndirizzoRepository;
            // Aggiungi un gestore per il bottone Annulla se presente sul form
            // Assicurati di avere un bottone 'btnAnnulla' nel designer
            // this.btnAnnulla.Click += new System.EventHandler(this.btnAnnulla_Click);
        }

        /// <summary>
        /// Imposta l'indirizzo da visualizzare o modificare.
        /// Questo metodo dovrebbe essere chiamato dalla form chiamante.
        /// </summary>
        /// <param name="indirizzo">L'oggetto ClienteIndirizzo (per modifica) o un nuovo oggetto con IdCli già impostato (per nuovo inserimento).</param>
        public void SetIndirizzo(ClienteIndirizzo indirizzo)
        {
            _currentIndirizzo = indirizzo;
            _idCliCorrente = indirizzo.IdCli; // Assicurati che IdCli sia impostato qui
            LoadIndirizzoData();
        }

        /// <summary>
        /// Carica i dati dell'indirizzo nei controlli del form.
        /// </summary>
        private void LoadIndirizzoData()
        {
            if (_currentIndirizzo != null)
            {
                txtIndirizzo.Text = _currentIndirizzo.Indirizzo;
                txtComune.Text = _currentIndirizzo.Comune;
                if (_currentIndirizzo.Cap.HasValue)
                    numCap.Text = _currentIndirizzo.Cap.Value.ToString();
                else
                    numCap.Text = string.Empty;
                chkPredefinito.Checked = _currentIndirizzo.Predefinito;
            }
            else
            {
                txtIndirizzo.Text = string.Empty;
                txtComune.Text = string.Empty;
                numCap.Text = string.Empty;
                chkPredefinito.Checked = false;
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Salva".
        /// </summary>
        private async void btnSalva_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            if (_currentIndirizzo == null)
            {
                _currentIndirizzo = new ClienteIndirizzo();
                _currentIndirizzo.IdCli = _idCliCorrente; // Assicurati che l'IdCli sia impostato per il nuovo indirizzo
            }

            _currentIndirizzo.Indirizzo = txtIndirizzo.Text.Trim();
            _currentIndirizzo.Comune = txtComune.Text.Trim();
            if (!string.IsNullOrWhiteSpace(numCap.Text) && decimal.TryParse(numCap.Text, out decimal cap))
                _currentIndirizzo.Cap = (int)cap;
            else
                _currentIndirizzo.Cap = null;
            bool newPredefinitoState = chkPredefinito.Checked;

            try
            {
                // Recupera tutti gli indirizzi esistenti per questo cliente
                // Usa _currentIndirizzo.IdCli che deve essere già impostato correttamente
                var allAddressesForClient = (await _clienteIndirizzoRepository.FindAsync(ci => ci.IdCli == _currentIndirizzo.IdCli)).ToList();

                if (newPredefinitoState) // Se l'utente vuole impostare questo indirizzo come predefinito
                {
                    // Deseleziona tutti gli altri indirizzi predefiniti per questo cliente
                    foreach (var address in allAddressesForClient.Where(a => a.Predefinito && a.Id != _currentIndirizzo.Id))
                    {
                        address.Predefinito = false;
                        _clienteIndirizzoRepository.Update(address);
                    }
                    _currentIndirizzo.Predefinito = true;
                }
                else // Se l'utente vuole deselezionare questo indirizzo come predefinito
                {
                    // Controlla se ci sono altri indirizzi predefiniti o se è l'unico
                    if (_currentIndirizzo.Id != 0 && allAddressesForClient.Count(a => a.Predefinito) == 1 && allAddressesForClient.First(a => a.Predefinito).Id == _currentIndirizzo.Id)
                    {
                        MessageBox.Show("Un cliente deve avere almeno un indirizzo predefinito. Impossibile deselezionare l'unico indirizzo predefinito.", "Validazione Indirizzo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return; // Non permettere il salvataggio
                    }
                    _currentIndirizzo.Predefinito = false;
                }

                if (_currentIndirizzo.Id == 0) // Nuovo indirizzo
                {
                    // Se è il primo indirizzo per il cliente, deve essere predefinito
                    if (!allAddressesForClient.Any())
                    {
                        _currentIndirizzo.Predefinito = true;
                    }
                    await _clienteIndirizzoRepository.AddAsync(_currentIndirizzo);
                }
                else // Indirizzo esistente
                {
                    _clienteIndirizzoRepository.Update(_currentIndirizzo);
                }

                await _clienteIndirizzoRepository.SaveChangesAsync();
                MessageBox.Show("Indirizzo salvato con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // --- INIZIO MODIFICA QUI ---
                // Rimuovi questa riga
                // this.DialogResult = DialogResult.OK;
                this.Close(); // Chiudi la form dopo il salvataggio
                // --- FINE MODIFICA ---
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio dell'indirizzo: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Annulla" (se presente).
        /// </summary>
        private void btnAnnulla_Click(object sender, EventArgs e)
        {
            this.Close(); // Semplicemente chiude la form senza salvare
        }

        /// <summary>
        /// Esegue la validazione dei campi di input.
        /// </summary>
        /// <returns>True se l'input è valido, altrimenti False.</returns>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtIndirizzo.Text))
            {
                MessageBox.Show("Indirizzo è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIndirizzo.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtComune.Text))
            {
                MessageBox.Show("Comune è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtComune.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(numCap.Text))
            {
                MessageBox.Show("CAP è un campo obbligatorio e non può essere 0.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numCap.Focus();
                return false;
            }
            return true;
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
            lblIndirizzo = new Label();
            txtIndirizzo = new TextBox();
            lblComune = new Label();
            txtComune = new TextBox();
            lblCap = new Label();
            chkPredefinito = new CheckBox();
            btnSalva = new Button();
            btnAnnulla = new Button();
            numCap = new TextBox();
            SuspendLayout();
            // 
            // lblIndirizzo
            // 
            lblIndirizzo.AutoSize = true;
            lblIndirizzo.Location = new Point(37, 64);
            lblIndirizzo.Margin = new Padding(6, 0, 6, 0);
            lblIndirizzo.Name = "lblIndirizzo";
            lblIndirizzo.Size = new Size(109, 32);
            lblIndirizzo.TabIndex = 0;
            lblIndirizzo.Text = "Indirizzo:";
            // 
            // txtIndirizzo
            // 
            txtIndirizzo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtIndirizzo.Location = new Point(186, 58);
            txtIndirizzo.Margin = new Padding(6);
            txtIndirizzo.MaxLength = 255;
            txtIndirizzo.Name = "txtIndirizzo";
            txtIndirizzo.Size = new Size(498, 39);
            txtIndirizzo.TabIndex = 1;
            // 
            // lblComune
            // 
            lblComune.AutoSize = true;
            lblComune.Location = new Point(37, 149);
            lblComune.Margin = new Padding(6, 0, 6, 0);
            lblComune.Name = "lblComune";
            lblComune.Size = new Size(110, 32);
            lblComune.TabIndex = 2;
            lblComune.Text = "Comune:";
            // 
            // txtComune
            // 
            txtComune.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtComune.Location = new Point(186, 143);
            txtComune.Margin = new Padding(6);
            txtComune.MaxLength = 100;
            txtComune.Name = "txtComune";
            txtComune.Size = new Size(498, 39);
            txtComune.TabIndex = 3;
            // 
            // lblCap
            // 
            lblCap.AutoSize = true;
            lblCap.Location = new Point(37, 235);
            lblCap.Margin = new Padding(6, 0, 6, 0);
            lblCap.Name = "lblCap";
            lblCap.Size = new Size(62, 32);
            lblCap.TabIndex = 4;
            lblCap.Text = "CAP:";
            // 
            // chkPredefinito
            // 
            chkPredefinito.AutoSize = true;
            chkPredefinito.Location = new Point(37, 309);
            chkPredefinito.Margin = new Padding(6);
            chkPredefinito.Name = "chkPredefinito";
            chkPredefinito.Size = new Size(163, 36);
            chkPredefinito.TabIndex = 6;
            chkPredefinito.Text = "Predefinito";
            chkPredefinito.UseVisualStyleBackColor = true;
            // 
            // btnSalva
            // 
            btnSalva.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSalva.Location = new Point(548, 363);
            btnSalva.Margin = new Padding(6);
            btnSalva.Name = "btnSalva";
            btnSalva.Size = new Size(139, 64);
            btnSalva.TabIndex = 7;
            btnSalva.Text = "Salva";
            btnSalva.UseVisualStyleBackColor = true;
            btnSalva.Click += btnSalva_Click;
            // 
            // btnAnnulla
            // 
            btnAnnulla.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAnnulla.Location = new Point(390, 363);
            btnAnnulla.Margin = new Padding(6);
            btnAnnulla.Name = "btnAnnulla";
            btnAnnulla.Size = new Size(139, 64);
            btnAnnulla.TabIndex = 8;
            btnAnnulla.Text = "Annulla";
            btnAnnulla.UseVisualStyleBackColor = true;
            btnAnnulla.Click += btnAnnulla_Click;
            // 
            // numCap
            // 
            numCap.Location = new Point(186, 232);
            numCap.Name = "numCap";
            numCap.Size = new Size(200, 39);
            numCap.TabIndex = 9;
            numCap.KeyPress += numCap_KeyPress;
            // 
            // ClientiIndirizzoDetailForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(724, 459);
            Controls.Add(numCap);
            Controls.Add(btnAnnulla);
            Controls.Add(btnSalva);
            Controls.Add(chkPredefinito);
            Controls.Add(lblCap);
            Controls.Add(txtComune);
            Controls.Add(lblComune);
            Controls.Add(txtIndirizzo);
            Controls.Add(lblIndirizzo);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(6);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ClientiIndirizzoDetailForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Dettagli Indirizzo Cliente";
            ResumeLayout(false);
            PerformLayout();

        }

        private System.Windows.Forms.Label lblIndirizzo;
        private System.Windows.Forms.TextBox txtIndirizzo;
        private System.Windows.Forms.Label lblComune;
        private System.Windows.Forms.TextBox txtComune;
        private System.Windows.Forms.Label lblCap;
        private System.Windows.Forms.CheckBox chkPredefinito;
        private System.Windows.Forms.Button btnSalva;
        private System.Windows.Forms.Button btnAnnulla; // Dichiarazione per il bottone Annulla

        #endregion

        private void numCap_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
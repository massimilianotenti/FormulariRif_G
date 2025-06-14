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

        public ClientiIndirizzoDetailForm(IGenericRepository<ClienteIndirizzo> clienteIndirizzoRepository)
        {
            InitializeComponent();
            _clienteIndirizzoRepository = clienteIndirizzoRepository;
        }

        /// <summary>
        /// Imposta l'indirizzo da visualizzare o modificare.
        /// </summary>
        /// <param name="indirizzo">L'oggetto ClienteIndirizzo.</param>
        public void SetIndirizzo(ClienteIndirizzo indirizzo)
        {
            _currentIndirizzo = indirizzo;
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
                numCap.Value = _currentIndirizzo.Cap;
                chkPredefinito.Checked = _currentIndirizzo.Predefinito;
            }
            else
            {
                txtIndirizzo.Text = string.Empty;
                txtComune.Text = string.Empty;
                numCap.Value = 0;
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
            }

            _currentIndirizzo.Indirizzo = txtIndirizzo.Text.Trim();
            _currentIndirizzo.Comune = txtComune.Text.Trim();
            _currentIndirizzo.Cap = (int)numCap.Value;
            bool newPredefinitoState = chkPredefinito.Checked;

            try
            {
                // Recupera tutti gli indirizzi esistenti per questo cliente
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
                    // Se è l'unico indirizzo e lo si sta deselezionando come predefinito, impedirlo
                    if (allAddressesForClient.Count == 1 && allAddressesForClient.First().Id == _currentIndirizzo.Id && _currentIndirizzo.Predefinito)
                    {
                        MessageBox.Show("Un cliente deve avere almeno un indirizzo predefinito. Impossibile deselezionare questo indirizzo come predefinito.", "Validazione Indirizzo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio dell'indirizzo: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            if (numCap.Value == 0)
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
            numCap = new NumericUpDown();
            chkPredefinito = new CheckBox();
            btnSalva = new Button();
            ((System.ComponentModel.ISupportInitialize)numCap).BeginInit();
            SuspendLayout();
            // 
            // lblIndirizzo
            // 
            lblIndirizzo.AutoSize = true;
            lblIndirizzo.Location = new Point(20, 30);
            lblIndirizzo.Name = "lblIndirizzo";
            lblIndirizzo.Size = new Size(54, 15);
            lblIndirizzo.TabIndex = 0;
            lblIndirizzo.Text = "Indirizzo:";
            // 
            // txtIndirizzo
            // 
            txtIndirizzo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtIndirizzo.Location = new Point(100, 27);
            txtIndirizzo.MaxLength = 255;
            txtIndirizzo.Name = "txtIndirizzo";
            txtIndirizzo.Size = new Size(270, 23);
            txtIndirizzo.TabIndex = 1;
            // 
            // lblComune
            // 
            lblComune.AutoSize = true;
            lblComune.Location = new Point(20, 70);
            lblComune.Name = "lblComune";
            lblComune.Size = new Size(56, 15);
            lblComune.TabIndex = 2;
            lblComune.Text = "Comune:";
            // 
            // txtComune
            // 
            txtComune.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtComune.Location = new Point(100, 67);
            txtComune.MaxLength = 100;
            txtComune.Name = "txtComune";
            txtComune.Size = new Size(270, 23);
            txtComune.TabIndex = 3;
            // 
            // lblCap
            // 
            lblCap.AutoSize = true;
            lblCap.Location = new Point(20, 110);
            lblCap.Name = "lblCap";
            lblCap.Size = new Size(33, 15);
            lblCap.TabIndex = 4;
            lblCap.Text = "CAP:";
            // 
            // numCap
            // 
            numCap.Location = new Point(100, 107);
            numCap.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            numCap.Name = "numCap";
            numCap.Size = new Size(120, 23);
            numCap.TabIndex = 5;
            // 
            // chkPredefinito
            // 
            chkPredefinito.AutoSize = true;
            chkPredefinito.Location = new Point(20, 145);
            chkPredefinito.Name = "chkPredefinito";
            chkPredefinito.Size = new Size(84, 19);
            chkPredefinito.TabIndex = 6;
            chkPredefinito.Text = "Predefinito";
            chkPredefinito.UseVisualStyleBackColor = true;
            // 
            // btnSalva
            // 
            btnSalva.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSalva.Location = new Point(295, 170);
            btnSalva.Name = "btnSalva";
            btnSalva.Size = new Size(75, 30);
            btnSalva.TabIndex = 7;
            btnSalva.Text = "Salva";
            btnSalva.UseVisualStyleBackColor = true;
            btnSalva.Click += btnSalva_Click;
            // 
            // ClientiIndirizzoDetailForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(390, 215);
            Controls.Add(btnSalva);
            Controls.Add(chkPredefinito);
            Controls.Add(numCap);
            Controls.Add(lblCap);
            Controls.Add(txtComune);
            Controls.Add(lblComune);
            Controls.Add(txtIndirizzo);
            Controls.Add(lblIndirizzo);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ClientiIndirizzoDetailForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Dettagli Indirizzo Cliente";
            ((System.ComponentModel.ISupportInitialize)numCap).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        private System.Windows.Forms.Label lblIndirizzo;
        private System.Windows.Forms.TextBox txtIndirizzo;
        private System.Windows.Forms.Label lblComune;
        private System.Windows.Forms.TextBox txtComune;
        private System.Windows.Forms.Label lblCap;
        private System.Windows.Forms.NumericUpDown numCap;
        private System.Windows.Forms.CheckBox chkPredefinito;
        private System.Windows.Forms.Button btnSalva;

        #endregion
    }
}

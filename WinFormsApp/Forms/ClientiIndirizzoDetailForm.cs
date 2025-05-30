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
            this.lblIndirizzo = new System.Windows.Forms.Label();
            this.txtIndirizzo = new System.Windows.Forms.TextBox();
            this.lblComune = new System.Windows.Forms.Label();
            this.txtComune = new System.Windows.Forms.TextBox();
            this.lblCap = new System.Windows.Forms.Label();
            this.numCap = new System.Windows.Forms.NumericUpDown();
            this.chkPredefinito = new System.Windows.Forms.CheckBox();
            this.btnSalva = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numCap)).BeginInit();
            this.SuspendLayout();
            //
            // lblIndirizzo
            //
            this.lblIndirizzo.AutoSize = true;
            this.lblIndirizzo.Location = new System.Drawing.Point(20, 30);
            this.lblIndirizzo.Name = "lblIndirizzo";
            this.lblIndirizzo.Size = new System.Drawing.Size(57, 15);
            this.lblIndirizzo.TabIndex = 0;
            this.lblIndirizzo.Text = "Indirizzo:";
            //
            // txtIndirizzo
            //
            this.txtIndirizzo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIndirizzo.Location = new System.Drawing.Point(100, 27);
            this.txtIndirizzo.Name = "txtIndirizzo";
            this.txtIndirizzo.Size = new System.Drawing.Size(270, 23);
            this.txtIndirizzo.TabIndex = 1;
            //
            // lblComune
            //
            this.lblComune.AutoSize = true;
            this.lblComune.Location = new System.Drawing.Point(20, 70);
            this.lblComune.Name = "lblComune";
            this.lblComune.Size = new System.Drawing.Size(58, 15);
            this.lblComune.TabIndex = 2;
            this.lblComune.Text = "Comune:";
            //
            // txtComune
            //
            this.txtComune.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtComune.Location = new System.Drawing.Point(100, 67);
            this.txtComune.Name = "txtComune";
            this.txtComune.Size = new System.Drawing.Size(270, 23);
            this.txtComune.TabIndex = 3;
            //
            // lblCap
            //
            this.lblCap.AutoSize = true;
            this.lblCap.Location = new System.Drawing.Point(20, 110);
            this.lblCap.Name = "lblCap";
            this.lblCap.Size = new System.Drawing.Size(32, 15);
            this.lblCap.TabIndex = 4;
            this.lblCap.Text = "CAP:";
            //
            // numCap
            //
            this.numCap.Location = new System.Drawing.Point(100, 107);
            this.numCap.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numCap.Name = "numCap";
            this.numCap.Size = new System.Drawing.Size(120, 23);
            this.numCap.TabIndex = 5;
            //
            // chkPredefinito
            //
            this.chkPredefinito.AutoSize = true;
            this.chkPredefinito.Location = new System.Drawing.Point(20, 145);
            this.chkPredefinito.Name = "chkPredefinito";
            this.chkPredefinito.Size = new System.Drawing.Size(86, 19);
            this.chkPredefinito.TabIndex = 6;
            this.chkPredefinito.Text = "Predefinito";
            this.chkPredefinito.UseVisualStyleBackColor = true;
            //
            // btnSalva
            //
            this.btnSalva.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalva.Location = new System.Drawing.Point(295, 170);
            this.btnSalva.Name = "btnSalva";
            this.btnSalva.Size = new System.Drawing.Size(75, 30);
            this.btnSalva.TabIndex = 7;
            this.btnSalva.Text = "Salva";
            this.btnSalva.UseVisualStyleBackColor = true;
            this.btnSalva.Click += new System.EventHandler(this.btnSalva_Click);
            //
            // ClientiIndirizzoDetailForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 215);
            this.Controls.Add(this.btnSalva);
            this.Controls.Add(this.chkPredefinito);
            this.Controls.Add(this.numCap);
            this.Controls.Add(this.lblCap);
            this.Controls.Add(this.txtComune);
            this.Controls.Add(this.lblComune);
            this.Controls.Add(this.txtIndirizzo);
            this.Controls.Add(this.lblIndirizzo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ClientiIndirizzoDetailForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dettagli Indirizzo Cliente";
            ((System.ComponentModel.ISupportInitialize)(this.numCap)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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

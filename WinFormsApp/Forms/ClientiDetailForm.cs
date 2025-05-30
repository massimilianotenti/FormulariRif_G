// File: Forms/ClientiDetailForm.cs
// Questo form permette di visualizzare, creare e modificare i dettagli di un cliente.
// Ora funziona in modalità master-detail, mostrando e permettendo la gestione
// degli indirizzi e dei contatti del cliente direttamente da qui.
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using Microsoft.Extensions.DependencyInjection; // Per IServiceProvider
using System.Linq;
using Microsoft.EntityFrameworkCore; // Per l'Include (anche se usiamo proiezione)

namespace FormulariRif_G.Forms
{
    public partial class ClientiDetailForm : Form
    {
        private readonly IGenericRepository<Cliente> _clienteRepository;
        private readonly IGenericRepository<ClienteIndirizzo> _clienteIndirizzoRepository;
        private readonly IGenericRepository<ClienteContatto> _clienteContattoRepository;
        private readonly IServiceProvider _serviceProvider;
        private Cliente? _currentCliente;
        private bool _isReadOnly;

        public ClientiDetailForm(IGenericRepository<Cliente> clienteRepository,
                                 IGenericRepository<ClienteIndirizzo> clienteIndirizzoRepository,
                                 IGenericRepository<ClienteContatto> clienteContattoRepository,
                                 IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _clienteRepository = clienteRepository;
            _clienteIndirizzoRepository = clienteIndirizzoRepository;
            _clienteContattoRepository = clienteContattoRepository;
            _serviceProvider = serviceProvider;
            this.Load += ClientiDetailForm_Load;
        }

        private async void ClientiDetailForm_Load(object? sender, EventArgs e)
        {
            if (_currentCliente?.Id != 0) // Se è un cliente esistente
            {
                await LoadIndirizziAsync();
                await LoadContattiAsync();
            }
            else
            {
                // Per un nuovo cliente, le griglie rimarranno vuote fino al salvataggio
                // e i pulsanti di gestione indirizzi/contatti saranno disabilitati.
            }
        }

        /// <summary>
        /// Imposta il cliente da visualizzare o modificare.
        /// </summary>
        /// <param name="cliente">L'oggetto Cliente.</param>
        /// <param name="isReadOnly">True per la modalità di sola lettura, False per la modifica.</param>
        public void SetCliente(Cliente cliente, bool isReadOnly = false)
        {
            _currentCliente = cliente;
            _isReadOnly = isReadOnly;
            LoadClienteData();
            SetFormState();
        }

        /// <summary>
        /// Carica i dati del cliente nei controlli del form.
        /// </summary>
        private void LoadClienteData()
        {
            if (_currentCliente != null)
            {
                txtRagSoc.Text = _currentCliente.RagSoc;
                txtCodiceFiscale.Text = _currentCliente.CodiceFiscale;
            }
            else
            {
                // Inizializza i campi per un nuovo cliente
                txtRagSoc.Text = string.Empty;
                txtCodiceFiscale.Text = string.Empty;
            }
        }

        /// <summary>
        /// Carica gli indirizzi del cliente nella DataGridView degli indirizzi.
        /// </summary>
        private async Task LoadIndirizziAsync()
        {
            if (_currentCliente == null || _currentCliente.Id == 0) return;

            try
            {
                var indirizzi = await _clienteIndirizzoRepository.FindAsync(ci => ci.IdCli == _currentCliente.Id);
                // Proietta i dati in un tipo anonimo per evitare problemi di binding con proprietà complesse
                var displayIndirizzi = indirizzi.Select(i => new
                {
                    i.Id,
                    i.Indirizzo,
                    i.Comune,
                    i.Cap,
                    i.Predefinito
                }).ToList();

                dataGridViewIndirizzi.DataSource = displayIndirizzi;
                // Nascondi la colonna Id se non necessaria per la visualizzazione
                if (dataGridViewIndirizzi.Columns.Contains("Id"))
                {
                    dataGridViewIndirizzi.Columns["Id"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento degli indirizzi: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Carica i contatti del cliente nella DataGridView dei contatti.
        /// </summary>
        private async Task LoadContattiAsync()
        {
            if (_currentCliente == null || _currentCliente.Id == 0) return;

            try
            {
                var contatti = await _clienteContattoRepository.FindAsync(cc => cc.IdCli == _currentCliente.Id);
                // Proietta i dati in un tipo anonimo per evitare problemi di binding con proprietà complesse
                var displayContatti = contatti.Select(c => new
                {
                    c.Id,
                    c.Contatto,
                    c.Telefono,
                    c.Email,
                    c.Predefinito
                }).ToList();

                dataGridViewContatti.DataSource = displayContatti;
                // Nascondi la colonna Id se non necessaria per la visualizzazione
                if (dataGridViewContatti.Columns.Contains("Id"))
                {
                    dataGridViewContatti.Columns["Id"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento dei contatti: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Imposta lo stato dei controlli del form (sola lettura o modifica).
        /// </summary>
        private void SetFormState()
        {
            txtRagSoc.ReadOnly = _isReadOnly;
            txtCodiceFiscale.ReadOnly = _isReadOnly;
            btnSalva.Visible = !_isReadOnly;

            // I pulsanti di gestione indirizzi/contatti sono abilitati solo se il cliente è esistente e non in sola lettura
            bool enableSubCrud = !_isReadOnly && _currentCliente?.Id != 0;
            btnNuovoIndirizzo.Enabled = enableSubCrud;
            btnModificaIndirizzo.Enabled = enableSubCrud;
            btnEliminaIndirizzo.Enabled = enableSubCrud;
            btnNuovoContatto.Enabled = enableSubCrud;
            btnModificaContatto.Enabled = enableSubCrud;
            btnEliminaContatto.Enabled = enableSubCrud;
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

            if (_currentCliente == null)
            {
                _currentCliente = new Cliente();
            }

            _currentCliente.RagSoc = txtRagSoc.Text.Trim();
            _currentCliente.CodiceFiscale = txtCodiceFiscale.Text.Trim();

            try
            {
                if (_currentCliente.Id == 0) // Nuovo cliente
                {
                    await _clienteRepository.AddAsync(_currentCliente);
                    await _clienteRepository.SaveChangesAsync(); // Salva per ottenere l'ID del nuovo cliente
                    MessageBox.Show("Cliente salvato con successo! Ora puoi aggiungere indirizzi e contatti.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Ricarica lo stato per abilitare i pulsanti di gestione
                    SetFormState();
                    // Non chiudere il form, l'utente potrebbe voler aggiungere indirizzi/contatti subito
                    this.DialogResult = DialogResult.OK; // Segnala che il cliente è stato salvato
                }
                else // Cliente esistente
                {
                    _clienteRepository.Update(_currentCliente);
                    await _clienteRepository.SaveChangesAsync();
                    MessageBox.Show("Cliente aggiornato con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio del cliente: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Esegue la validazione dei campi di input del cliente.
        /// </summary>
        /// <returns>True se l'input è valido, altrimenti False.</returns>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtRagSoc.Text))
            {
                MessageBox.Show("Ragione Sociale è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRagSoc.Focus();
                return false;
            }
            return true;
        }

        #region Gestione Indirizzi

        /// <summary>
        /// Gestisce il click sul pulsante "Nuovo Indirizzo".
        /// </summary>
        private async void btnNuovoIndirizzo_Click(object sender, EventArgs e)
        {
            if (_currentCliente == null || _currentCliente.Id == 0)
            {
                MessageBox.Show("Salva il cliente prima di aggiungere indirizzi.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var detailForm = _serviceProvider.GetRequiredService<ClientiIndirizzoDetailForm>())
            {
                var newIndirizzo = new ClienteIndirizzo { IdCli = _currentCliente.Id };
                detailForm.SetIndirizzo(newIndirizzo);
                if (detailForm.ShowDialog() == DialogResult.OK)
                {
                    // La logica di salvataggio e gestione del predefinito è nel detail form
                    await LoadIndirizziAsync(); // Ricarica la griglia dopo il salvataggio
                }
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Modifica Indirizzo".
        /// </summary>
        private async void btnModificaIndirizzo_Click(object sender, EventArgs e)
        {
            if (dataGridViewIndirizzi.SelectedRows.Count > 0)
            {
                // Recupera l'ID dal tipo anonimo e poi l'oggetto completo dal repository
                var selectedRowData = dataGridViewIndirizzi.SelectedRows[0].DataBoundItem;
                int selectedId = (int)selectedRowData.GetType().GetProperty("Id").GetValue(selectedRowData);
                var selectedIndirizzo = await _clienteIndirizzoRepository.GetByIdAsync(selectedId);

                if (selectedIndirizzo != null)
                {
                    using (var detailForm = _serviceProvider.GetRequiredService<ClientiIndirizzoDetailForm>())
                    {
                        detailForm.SetIndirizzo(selectedIndirizzo);
                        if (detailForm.ShowDialog() == DialogResult.OK)
                        {
                            // La logica di salvataggio e gestione del predefinito è nel detail form
                            await LoadIndirizziAsync(); // Ricarica la griglia dopo il salvataggio
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un indirizzo da modificare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Elimina Indirizzo".
        /// </summary>
        private async void btnEliminaIndirizzo_Click(object sender, EventArgs e)
        {
            if (dataGridViewIndirizzi.SelectedRows.Count > 0)
            {
                // Recupera l'ID dal tipo anonimo e poi l'oggetto completo dal repository
                var selectedRowData = dataGridViewIndirizzi.SelectedRows[0].DataBoundItem;
                int selectedId = (int)selectedRowData.GetType().GetProperty("Id").GetValue(selectedRowData);
                var selectedIndirizzo = await _clienteIndirizzoRepository.GetByIdAsync(selectedId);

                if (selectedIndirizzo != null)
                {
                    // Logica per prevenire l'eliminazione dell'unico indirizzo predefinito
                    var allAddresses = await _clienteIndirizzoRepository.FindAsync(ci => ci.IdCli == _currentCliente!.Id);
                    if (allAddresses.Count() == 1 && selectedIndirizzo.Predefinito)
                    {
                        MessageBox.Show("Impossibile eliminare l'unico indirizzo predefinito. Un cliente deve avere almeno un indirizzo predefinito.", "Errore Eliminazione", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var confirmResult = MessageBox.Show($"Sei sicuro di voler eliminare l'indirizzo '{selectedIndirizzo.Indirizzo}'?", "Conferma Eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            _clienteIndirizzoRepository.Delete(selectedIndirizzo);
                            await _clienteIndirizzoRepository.SaveChangesAsync();
                            await LoadIndirizziAsync();

                            // Se l'indirizzo eliminato era predefinito e ce ne sono altri, imposta il primo come predefinito
                            var remainingAddresses = await _clienteIndirizzoRepository.FindAsync(ci => ci.IdCli == _currentCliente!.Id);
                            if (!remainingAddresses.Any(a => a.Predefinito) && remainingAddresses.Any())
                            {
                                var firstRemaining = remainingAddresses.First();
                                firstRemaining.Predefinito = true;
                                _clienteIndirizzoRepository.Update(firstRemaining);
                                await _clienteIndirizzoRepository.SaveChangesAsync();
                                await LoadIndirizziAsync(); // Ricarica per riflettere il nuovo predefinito
                            }

                            MessageBox.Show("Indirizzo eliminato con successo.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Errore durante l'eliminazione dell'indirizzo: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un indirizzo da eliminare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region Gestione Contatti

        /// <summary>
        /// Gestisce il click sul pulsante "Nuovo Contatto".
        /// </summary>
        private async void btnNuovoContatto_Click(object sender, EventArgs e)
        {
            if (_currentCliente == null || _currentCliente.Id == 0)
            {
                MessageBox.Show("Salva il cliente prima di aggiungere contatti.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var detailForm = _serviceProvider.GetRequiredService<ClientiContattiDetailForm>())
            {
                var newContatto = new ClienteContatto { IdCli = _currentCliente.Id };
                detailForm.SetContatto(newContatto);
                if (detailForm.ShowDialog() == DialogResult.OK)
                {
                    await LoadContattiAsync(); // Ricarica la griglia dopo il salvataggio
                }
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Modifica Contatto".
        /// </summary>
        private async void btnModificaContatto_Click(object sender, EventArgs e)
        {
            if (dataGridViewContatti.SelectedRows.Count > 0)
            {
                // Recupera l'ID dal tipo anonimo e poi l'oggetto completo dal repository
                var selectedRowData = dataGridViewContatti.SelectedRows[0].DataBoundItem;
                int selectedId = (int)selectedRowData.GetType().GetProperty("Id").GetValue(selectedRowData);
                var selectedContatto = await _clienteContattoRepository.GetByIdAsync(selectedId);

                if (selectedContatto != null)
                {
                    using (var detailForm = _serviceProvider.GetRequiredService<ClientiContattiDetailForm>())
                    {
                        detailForm.SetContatto(selectedContatto);
                        if (detailForm.ShowDialog() == DialogResult.OK)
                        {
                            await LoadContattiAsync(); // Ricarica la griglia dopo il salvataggio
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un contatto da modificare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Elimina Contatto".
        /// </summary>
        private async void btnEliminaContatto_Click(object sender, EventArgs e)
        {
            if (dataGridViewContatti.SelectedRows.Count > 0)
            {
                // Recupera l'ID dal tipo anonimo e poi l'oggetto completo dal repository
                var selectedRowData = dataGridViewContatti.SelectedRows[0].DataBoundItem;
                int selectedId = (int)selectedRowData.GetType().GetProperty("Id").GetValue(selectedRowData);
                var selectedContatto = await _clienteContattoRepository.GetByIdAsync(selectedId);

                if (selectedContatto != null)
                {
                    var confirmResult = MessageBox.Show($"Sei sicuro di voler eliminare il contatto '{selectedContatto.Contatto}'?", "Conferma Eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            _clienteContattoRepository.Delete(selectedContatto);
                            await _clienteContattoRepository.SaveChangesAsync();
                            await LoadContattiAsync();
                            MessageBox.Show("Contatto eliminato con successo.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Errore durante l'eliminazione del contatto: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un contatto da eliminare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

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
            this.lblRagSoc = new System.Windows.Forms.Label();
            this.txtRagSoc = new System.Windows.Forms.TextBox();
            this.btnSalva = new System.Windows.Forms.Button();
            this.lblCodiceFiscale = new System.Windows.Forms.Label();
            this.txtCodiceFiscale = new System.Windows.Forms.TextBox();
            this.groupBoxIndirizzi = new System.Windows.Forms.GroupBox();
            this.btnEliminaIndirizzo = new System.Windows.Forms.Button();
            this.btnModificaIndirizzo = new System.Windows.Forms.Button();
            this.btnNuovoIndirizzo = new System.Windows.Forms.Button();
            this.dataGridViewIndirizzi = new System.Windows.Forms.DataGridView();
            this.groupBoxContatti = new System.Windows.Forms.GroupBox();
            this.btnEliminaContatto = new System.Windows.Forms.Button();
            this.btnModificaContatto = new System.Windows.Forms.Button();
            this.btnNuovoContatto = new System.Windows.Forms.Button();
            this.dataGridViewContatti = new System.Windows.Forms.DataGridView();
            this.groupBoxIndirizzi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewIndirizzi)).BeginInit();
            this.groupBoxContatti.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewContatti)).BeginInit();
            this.SuspendLayout();
            //
            // lblRagSoc
            //
            this.lblRagSoc.AutoSize = true;
            this.lblRagSoc.Location = new System.Drawing.Point(20, 30);
            this.lblRagSoc.Name = "lblRagSoc";
            this.lblRagSoc.Size = new System.Drawing.Size(95, 15);
            this.lblRagSoc.TabIndex = 0;
            this.lblRagSoc.Text = "Ragione Sociale:";
            //
            // txtRagSoc
            //
            this.txtRagSoc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRagSoc.Location = new System.Drawing.Point(120, 27);
            this.txtRagSoc.Name = "txtRagSoc";
            this.txtRagSoc.Size = new System.Drawing.Size(350, 23);
            this.txtRagSoc.TabIndex = 1;
            //
            // btnSalva
            //
            this.btnSalva.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalva.Location = new System.Drawing.Point(700, 580);
            this.btnSalva.Name = "btnSalva";
            this.btnSalva.Size = new System.Drawing.Size(75, 30);
            this.btnSalva.TabIndex = 2;
            this.btnSalva.Text = "Salva";
            this.btnSalva.UseVisualStyleBackColor = true;
            this.btnSalva.Click += new System.EventHandler(this.btnSalva_Click);
            //
            // lblCodiceFiscale
            //
            this.lblCodiceFiscale.AutoSize = true;
            this.lblCodiceFiscale.Location = new System.Drawing.Point(20, 70);
            this.lblCodiceFiscale.Name = "lblCodiceFiscale";
            this.lblCodiceFiscale.Size = new System.Drawing.Size(83, 15);
            this.lblCodiceFiscale.TabIndex = 3;
            this.lblCodiceFiscale.Text = "Codice Fiscale:";
            //
            // txtCodiceFiscale
            //
            this.txtCodiceFiscale.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCodiceFiscale.Location = new System.Drawing.Point(120, 67);
            this.txtCodiceFiscale.Name = "txtCodiceFiscale";
            this.txtCodiceFiscale.Size = new System.Drawing.Size(350, 23);
            this.txtCodiceFiscale.TabIndex = 4;
            //
            // groupBoxIndirizzi
            //
            this.groupBoxIndirizzi.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxIndirizzi.Controls.Add(this.btnEliminaIndirizzo);
            this.groupBoxIndirizzi.Controls.Add(this.btnModificaIndirizzo);
            this.groupBoxIndirizzi.Controls.Add(this.btnNuovoIndirizzo);
            this.groupBoxIndirizzi.Controls.Add(this.dataGridViewIndirizzi);
            this.groupBoxIndirizzi.Location = new System.Drawing.Point(12, 110);
            this.groupBoxIndirizzi.Name = "groupBoxIndirizzi";
            this.groupBoxIndirizzi.Size = new System.Drawing.Size(760, 220);
            this.groupBoxIndirizzi.TabIndex = 5;
            this.groupBoxIndirizzi.TabStop = false;
            this.groupBoxIndirizzi.Text = "Indirizzi";
            //
            // btnEliminaIndirizzo
            //
            this.btnEliminaIndirizzo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEliminaIndirizzo.Location = new System.Drawing.Point(174, 180);
            this.btnEliminaIndirizzo.Name = "btnEliminaIndirizzo";
            this.btnEliminaIndirizzo.Size = new System.Drawing.Size(75, 30);
            this.btnEliminaIndirizzo.TabIndex = 3;
            this.btnEliminaIndirizzo.Text = "Elimina";
            this.btnEliminaIndirizzo.UseVisualStyleBackColor = true;
            this.btnEliminaIndirizzo.Click += new System.EventHandler(this.btnEliminaIndirizzo_Click);
            //
            // btnModificaIndirizzo
            //
            this.btnModificaIndirizzo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnModificaIndirizzo.Location = new System.Drawing.Point(93, 180);
            this.btnModificaIndirizzo.Name = "btnModificaIndirizzo";
            this.btnModificaIndirizzo.Size = new System.Drawing.Size(75, 30);
            this.btnModificaIndirizzo.TabIndex = 2;
            this.btnModificaIndirizzo.Text = "Modifica";
            this.btnModificaIndirizzo.UseVisualStyleBackColor = true;
            this.btnModificaIndirizzo.Click += new System.EventHandler(this.btnModificaIndirizzo_Click);
            //
            // btnNuovoIndirizzo
            //
            this.btnNuovoIndirizzo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNuovoIndirizzo.Location = new System.Drawing.Point(12, 180);
            this.btnNuovoIndirizzo.Name = "btnNuovoIndirizzo";
            this.btnNuovoIndirizzo.Size = new System.Drawing.Size(75, 30);
            this.btnNuovoIndirizzo.TabIndex = 1;
            this.btnNuovoIndirizzo.Text = "Nuovo";
            this.btnNuovoIndirizzo.UseVisualStyleBackColor = true;
            this.btnNuovoIndirizzo.Click += new System.EventHandler(this.btnNuovoIndirizzo_Click);
            //
            // dataGridViewIndirizzi
            //
            this.dataGridViewIndirizzi.AllowUserToAddRows = false;
            this.dataGridViewIndirizzi.AllowUserToDeleteRows = false;
            this.dataGridViewIndirizzi.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewIndirizzi.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewIndirizzi.Location = new System.Drawing.Point(12, 22);
            this.dataGridViewIndirizzi.MultiSelect = false;
            this.dataGridViewIndirizzi.Name = "dataGridViewIndirizzi";
            this.dataGridViewIndirizzi.ReadOnly = true;
            this.dataGridViewIndirizzi.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewIndirizzi.Size = new System.Drawing.Size(736, 150);
            this.dataGridViewIndirizzi.TabIndex = 0;
            //
            // groupBoxContatti
            //
            this.groupBoxContatti.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxContatti.Controls.Add(this.btnEliminaContatto);
            this.groupBoxContatti.Controls.Add(this.btnModificaContatto);
            this.groupBoxContatti.Controls.Add(this.btnNuovoContatto);
            this.groupBoxContatti.Controls.Add(this.dataGridViewContatti);
            this.groupBoxContatti.Location = new System.Drawing.Point(12, 340);
            this.groupBoxContatti.Name = "groupBoxContatti";
            this.groupBoxContatti.Size = new System.Drawing.Size(760, 220);
            this.groupBoxContatti.TabIndex = 6;
            this.groupBoxContatti.TabStop = false;
            this.groupBoxContatti.Text = "Contatti";
            //
            // btnEliminaContatto
            //
            this.btnEliminaContatto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEliminaContatto.Location = new System.Drawing.Point(174, 180);
            this.btnEliminaContatto.Name = "btnEliminaContatto";
            this.btnEliminaContatto.Size = new System.Drawing.Size(75, 30);
            this.btnEliminaContatto.TabIndex = 3;
            this.btnEliminaContatto.Text = "Elimina";
            this.btnEliminaContatto.UseVisualStyleBackColor = true;
            this.btnEliminaContatto.Click += new System.EventHandler(this.btnEliminaContatto_Click);
            //
            // btnModificaContatto
            //
            this.btnModificaContatto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnModificaContatto.Location = new System.Drawing.Point(93, 180);
            this.btnModificaContatto.Name = "btnModificaContatto";
            this.btnModificaContatto.Size = new System.Drawing.Size(75, 30);
            this.btnModificaContatto.TabIndex = 2;
            this.btnModificaContatto.Text = "Modifica";
            this.btnModificaContatto.UseVisualStyleBackColor = true;
            this.btnModificaContatto.Click += new System.EventHandler(this.btnModificaContatto_Click);
            //
            // btnNuovoContatto
            //
            this.btnNuovoContatto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNuovoContatto.Location = new System.Drawing.Point(12, 180);
            this.btnNuovoContatto.Name = "btnNuovoContatto";
            this.btnNuovoContatto.Size = new System.Drawing.Size(75, 30);
            this.btnNuovoContatto.TabIndex = 1;
            this.btnNuovoContatto.Text = "Nuovo";
            this.btnNuovoContatto.UseVisualStyleBackColor = true;
            this.btnNuovoContatto.Click += new System.EventHandler(this.btnNuovoContatto_Click);
            //
            // dataGridViewContatti
            //
            this.dataGridViewContatti.AllowUserToAddRows = false;
            this.dataGridViewContatti.AllowUserToDeleteRows = false;
            this.dataGridViewContatti.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewContatti.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewContatti.Location = new System.Drawing.Point(12, 22);
            this.dataGridViewContatti.MultiSelect = false;
            this.dataGridViewContatti.Name = "dataGridViewContatti";
            this.dataGridViewContatti.ReadOnly = true;
            this.dataGridViewContatti.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewContatti.Size = new System.Drawing.Size(736, 150);
            this.dataGridViewContatti.TabIndex = 0;
            //
            // ClientiDetailForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 621);
            this.Controls.Add(this.groupBoxContatti);
            this.Controls.Add(this.groupBoxIndirizzi);
            this.Controls.Add(this.txtCodiceFiscale);
            this.Controls.Add(this.lblCodiceFiscale);
            this.Controls.Add(this.btnSalva);
            this.Controls.Add(this.txtRagSoc);
            this.Controls.Add(this.lblRagSoc);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ClientiDetailForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dettagli Cliente";
            this.groupBoxIndirizzi.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewIndirizzi)).EndInit();
            this.groupBoxContatti.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewContatti)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblRagSoc;
        private System.Windows.Forms.TextBox txtRagSoc;
        private System.Windows.Forms.Button btnSalva;
        private System.Windows.Forms.Label lblCodiceFiscale;
        private System.Windows.Forms.TextBox txtCodiceFiscale;
        private System.Windows.Forms.GroupBox groupBoxIndirizzi;
        private System.Windows.Forms.Button btnEliminaIndirizzo;
        private System.Windows.Forms.Button btnModificaIndirizzo;
        private System.Windows.Forms.Button btnNuovoIndirizzo;
        private System.Windows.Forms.DataGridView dataGridViewIndirizzi;
        private System.Windows.Forms.GroupBox groupBoxContatti;
        private System.Windows.Forms.Button btnEliminaContatto;
        private System.Windows.Forms.Button btnModificaContatto;
        private System.Windows.Forms.Button btnNuovoContatto;
        private System.Windows.Forms.DataGridView dataGridViewContatti;

        #endregion
    }
}

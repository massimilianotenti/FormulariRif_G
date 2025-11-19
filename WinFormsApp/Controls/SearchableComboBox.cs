using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FormulariRif_G.Controls
{
    public partial class SearchableComboBox : UserControl
    {
        // Campi privati per la gestione dei dati
        private List<object> _fullDataSource;
        private const int InitialLoadSize = 100;
        private bool _isProgrammaticChange = false;

        #region Public Properties

        [Category("Appearance")]
        [Description("Il testo da visualizzare nell'etichetta a sinistra del controllo.")]
        public string LabelText
        {
            get => lblText.Text;
            set => lblText.Text = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<object> DataSource
        {
            get => _fullDataSource;
            set
            {
                _fullDataSource = value ?? new List<object>();
                SetInitialDataSource();
            }
        }

        [Category("Data")]
        [Description("La proprietà da visualizzare nella ComboBox.")]
        public string DisplayMember
        {
            get => cmbBox.DisplayMember;
            set => cmbBox.DisplayMember = value;
        }

        [Category("Data")]
        [Description("La proprietà da usare come valore per gli elementi.")]
        public string ValueMember
        {
            get => cmbBox.ValueMember;
            set => cmbBox.ValueMember = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedValue
        {
            get => cmbBox.SelectedValue;
            set
            {
                if (_fullDataSource == null || _fullDataSource.Count == 0) return;

                _isProgrammaticChange = true;
                EnsureItemIsVisible(value);
                cmbBox.SelectedValue = value;
                _isProgrammaticChange = false;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem => cmbBox.SelectedItem;

        #endregion

        #region Public Events

        [Category("Action")]
        [Description("Si verifica quando l'indice selezionato cambia.")]
        public event EventHandler SelectedIndexChanged;

        #endregion

        public SearchableComboBox()
        {
            InitializeComponent();
            _fullDataSource = new List<object>();
        }

        public void Clear()
        {
            _isProgrammaticChange = true;
            cmbBox.DataSource = null; // Reset datasource to clear internal bindings if needed
            SetInitialDataSource();
            cmbBox.SelectedIndex = -1;
            cmbBox.Text = string.Empty;
            _isProgrammaticChange = false;
        }

        private void SetInitialDataSource(object valueToSelect = null)
        {
            if (_fullDataSource == null) return;

            var displayList = _fullDataSource.Take(InitialLoadSize).ToList();

            if (valueToSelect != null)
            {
                var itemToSelect = _fullDataSource.FirstOrDefault(item =>
                {
                    var itemValue = item.GetType().GetProperty(ValueMember)?.GetValue(item);
                    return itemValue != null && itemValue.Equals(valueToSelect);
                });

                if (itemToSelect != null && !displayList.Contains(itemToSelect))
                {
                    displayList.Add(itemToSelect);
                }
            }

            cmbBox.DataSource = displayList;
        }

        private void EnsureItemIsVisible(object valueToSelect)
        {
            if (valueToSelect == null || (valueToSelect is int id && id <= 0))
            {
                SetInitialDataSource();
                cmbBox.SelectedIndex = -1;
                return;
            }

            bool isPresent = (cmbBox.DataSource as IEnumerable<object>)?.Any(item =>
            {
                var itemValue = item.GetType().GetProperty(ValueMember)?.GetValue(item);
                return itemValue != null && itemValue.Equals(valueToSelect);
            }) ?? false;

            if (!isPresent)
            {
                SetInitialDataSource(valueToSelect);
            }
        }

        private void cmbBox_TextUpdate(object sender, EventArgs e)
        {
            if (_isProgrammaticChange) return;

            string searchText = cmbBox.Text;
            int cursorPosition = cmbBox.SelectionStart;

            if (string.IsNullOrEmpty(searchText))
            {
                SetInitialDataSource();
                cmbBox.SelectedIndex = -1;
                cmbBox.Text = "";
                return;
            }

            var filtered = _fullDataSource.Where(item =>
            {
                var displayValue = item.GetType().GetProperty(DisplayMember)?.GetValue(item)?.ToString() ?? "";
                bool match = displayValue.Contains(searchText, StringComparison.OrdinalIgnoreCase);

                if (!match && item.GetType().GetProperty("Targa") != null)
                {
                    var targaValue = item.GetType().GetProperty("Targa")?.GetValue(item)?.ToString() ?? "";
                    match = targaValue.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                }
                return match;
            }).Take(InitialLoadSize).ToList();

            _isProgrammaticChange = true;
            cmbBox.DataSource = filtered;
            cmbBox.SelectedIndex = -1;
            cmbBox.Text = searchText; // Restore text
            cmbBox.SelectionStart = cursorPosition; // Restore cursor
            cmbBox.DroppedDown = true; // Keep dropdown open
            Cursor.Current = Cursors.Default; // Prevent cursor flickering
            _isProgrammaticChange = false;
        }

        private void cmbBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // Optional: Logic when user explicitly selects an item
        }

        private void cmbBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isProgrammaticChange) return;
            SelectedIndexChanged?.Invoke(this, e);
        }
    }
}


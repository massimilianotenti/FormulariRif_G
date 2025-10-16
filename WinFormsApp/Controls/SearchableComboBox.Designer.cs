namespace FormulariRif_G.Controls
{
    partial class SearchableComboBox
    {
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblText = new Label();
            txtSearch = new TextBox();
            cmbBox = new ComboBox();
            SuspendLayout();
            // 
            // lblText
            // 
            lblText.Anchor = AnchorStyles.Left;
            lblText.AutoSize = true;
            lblText.Location = new Point(16, 14);
            lblText.Margin = new Padding(0, 0, 6, 0);
            lblText.Name = "lblText";
            lblText.Size = new Size(75, 32);
            lblText.TabIndex = 0;
            lblText.Text = "Label:";
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(156, 11);
            txtSearch.Margin = new Padding(6, 6, 6, 6);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Cerca...";
            txtSearch.Size = new Size(205, 39);
            txtSearch.TabIndex = 1;
            txtSearch.TextChanged += txtSearch_TextChanged;
            txtSearch.KeyDown += txtSearch_KeyDown;
            // 
            // cmbBox
            // 
            cmbBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            cmbBox.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBox.FormattingEnabled = true;
            cmbBox.Location = new Point(371, 11);
            cmbBox.Margin = new Padding(6, 6, 6, 6);
            cmbBox.Name = "cmbBox";
            cmbBox.Size = new Size(448, 40);
            cmbBox.TabIndex = 2;
            cmbBox.SelectedIndexChanged += cmbBox_SelectedIndexChanged;
            cmbBox.SelectionChangeCommitted += cmbBox_SelectionChangeCommitted;
            // 
            // SearchableComboBox
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(cmbBox);
            Controls.Add(txtSearch);
            Controls.Add(lblText);
            Margin = new Padding(6, 6, 6, 6);
            Name = "SearchableComboBox";
            Size = new Size(844, 64);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.ComboBox cmbBox;
    }
}



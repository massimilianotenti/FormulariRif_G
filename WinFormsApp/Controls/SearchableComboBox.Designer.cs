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
            // cmbBox
            // 
            cmbBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            cmbBox.FormattingEnabled = true;
            cmbBox.Location = new Point(156, 11);
            cmbBox.Margin = new Padding(6);
            cmbBox.Name = "cmbBox";
            cmbBox.Size = new Size(663, 40);
            cmbBox.TabIndex = 1;
            cmbBox.SelectedIndexChanged += cmbBox_SelectedIndexChanged;
            cmbBox.SelectionChangeCommitted += cmbBox_SelectionChangeCommitted;
            cmbBox.TextUpdate += cmbBox_TextUpdate;
            // 
            // SearchableComboBox
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(cmbBox);
            Controls.Add(lblText);
            Margin = new Padding(6);
            Name = "SearchableComboBox";
            Size = new Size(844, 64);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.ComboBox cmbBox;
    }
}



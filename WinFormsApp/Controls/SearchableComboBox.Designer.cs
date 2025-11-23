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
            cmbBox = new ComboBox();
            SuspendLayout();
            // 
            // cmbBox
            // 
            cmbBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            cmbBox.Font = new Font("Segoe UI", 10.875F);
            cmbBox.FormattingEnabled = true;
            cmbBox.Location = new Point(6, 10);
            cmbBox.Margin = new Padding(6);
            cmbBox.Name = "cmbBox";
            cmbBox.Size = new Size(495, 48);
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
            Margin = new Padding(6);
            Name = "SearchableComboBox";
            Size = new Size(507, 64);
            ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ComboBox cmbBox;
    }
}



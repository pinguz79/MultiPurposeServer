namespace Finance.Desktop
{
    partial class RiallineamentoDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            referenceLabel = new Label();
            principalInput = new NumericUpDown();
            saveButton = new Button();
            cancelButton = new Button();
            ((System.ComponentModel.ISupportInitialize)principalInput).BeginInit();
            SuspendLayout();
            // 
            // referenceLabel
            // 
            referenceLabel.AutoSize = true;
            referenceLabel.Location = new Point(16, 18);
            referenceLabel.Name = "referenceLabel";
            referenceLabel.Size = new Size(210, 15);
            referenceLabel.TabIndex = 0;
            referenceLabel.Text = "Capitale residuo dopo questa rata";
            // 
            // principalInput
            // 
            principalInput.DecimalPlaces = 2;
            principalInput.Location = new Point(16, 48);
            principalInput.Maximum = new decimal(new int[] { -1, 2147483647, 0, 131072 });
            principalInput.Name = "principalInput";
            principalInput.Size = new Size(230, 23);
            principalInput.TabIndex = 1;
            principalInput.ThousandsSeparator = true;
            // 
            // saveButton
            // 
            saveButton.DialogResult = DialogResult.OK;
            saveButton.Location = new Point(244, 94);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(90, 28);
            saveButton.TabIndex = 2;
            saveButton.Text = "&Salva";
            saveButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(340, 94);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(90, 28);
            cancelButton.TabIndex = 3;
            cancelButton.Text = "Annulla";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // RiallineamentoDialog
            // 
            AcceptButton = saveButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(446, 140);
            Controls.Add(referenceLabel);
            Controls.Add(principalInput);
            Controls.Add(saveButton);
            Controls.Add(cancelButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RiallineamentoDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Residuo verificato";
            ((System.ComponentModel.ISupportInitialize)principalInput).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label referenceLabel;
        private NumericUpDown principalInput;
        private Button saveButton;
        private Button cancelButton;
    }
}

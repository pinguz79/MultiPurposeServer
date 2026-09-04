namespace Finance.Desktop
{
    partial class ParametroContoDefinitionDialog
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
            displayNameLabel = new Label();
            displayNameTextBox = new TextBox();
            valueLabel = new Label();
            valueInput = new NumericUpDown();
            valueSuffixLabel = new Label();
            validFromLabel = new Label();
            validFromInput = new DateTimePicker();
            validToLabel = new Label();
            validToInput = new DateTimePicker();
            saveButton = new Button();
            cancelButton = new Button();
            ((System.ComponentModel.ISupportInitialize)valueInput).BeginInit();
            SuspendLayout();
            // 
            // displayNameLabel
            // 
            displayNameLabel.AutoSize = true;
            displayNameLabel.Location = new Point(18, 18);
            displayNameLabel.Name = "displayNameLabel";
            displayNameLabel.Size = new Size(105, 15);
            displayNameLabel.TabIndex = 0;
            displayNameLabel.Text = "Nome &visualizzato";
            // 
            // displayNameTextBox
            // 
            displayNameTextBox.Location = new Point(18, 40);
            displayNameTextBox.Name = "displayNameTextBox";
            displayNameTextBox.Size = new Size(350, 23);
            displayNameTextBox.TabIndex = 1;
            // 
            // valueLabel
            // 
            valueLabel.AutoSize = true;
            valueLabel.Location = new Point(18, 78);
            valueLabel.Name = "valueLabel";
            valueLabel.Size = new Size(39, 15);
            valueLabel.TabIndex = 2;
            valueLabel.Text = "&Valore";
            // 
            // valueInput
            // 
            valueInput.Location = new Point(18, 100);
            valueInput.Maximum = 1000000000;
            valueInput.Minimum = -1000000000;
            valueInput.Name = "valueInput";
            valueInput.Size = new Size(160, 23);
            valueInput.TabIndex = 3;
            valueInput.TextAlign = HorizontalAlignment.Right;
            valueInput.ThousandsSeparator = true;
            // 
            // valueSuffixLabel
            // 
            valueSuffixLabel.AutoSize = true;
            valueSuffixLabel.Location = new Point(184, 104);
            valueSuffixLabel.Name = "valueSuffixLabel";
            valueSuffixLabel.Size = new Size(0, 15);
            valueSuffixLabel.TabIndex = 4;
            // 
            // validFromLabel
            // 
            validFromLabel.AutoSize = true;
            validFromLabel.Location = new Point(18, 138);
            validFromLabel.Name = "validFromLabel";
            validFromLabel.Size = new Size(59, 15);
            validFromLabel.TabIndex = 5;
            validFromLabel.Text = "Valido &dal";
            // 
            // validFromInput
            // 
            validFromInput.Checked = false;
            validFromInput.Format = DateTimePickerFormat.Short;
            validFromInput.Location = new Point(18, 160);
            validFromInput.Name = "validFromInput";
            validFromInput.ShowCheckBox = true;
            validFromInput.Size = new Size(160, 23);
            validFromInput.TabIndex = 6;
            // 
            // validToLabel
            // 
            validToLabel.AutoSize = true;
            validToLabel.Location = new Point(198, 138);
            validToLabel.Name = "validToLabel";
            validToLabel.Size = new Size(52, 15);
            validToLabel.TabIndex = 7;
            validToLabel.Text = "Valido &al";
            // 
            // validToInput
            // 
            validToInput.Checked = false;
            validToInput.Format = DateTimePickerFormat.Short;
            validToInput.Location = new Point(198, 160);
            validToInput.Name = "validToInput";
            validToInput.ShowCheckBox = true;
            validToInput.Size = new Size(170, 23);
            validToInput.TabIndex = 8;
            // 
            // saveButton
            // 
            saveButton.Location = new Point(212, 210);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(75, 28);
            saveButton.TabIndex = 9;
            saveButton.Text = "&Salva";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += SaveButtonClick;
            // 
            // cancelButton
            // 
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(293, 210);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 28);
            cancelButton.TabIndex = 10;
            cancelButton.Text = "&Annulla";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // ParametroContoDefinitionDialog
            // 
            AcceptButton = saveButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(386, 256);
            Controls.Add(cancelButton);
            Controls.Add(saveButton);
            Controls.Add(validToInput);
            Controls.Add(validToLabel);
            Controls.Add(validFromInput);
            Controls.Add(validFromLabel);
            Controls.Add(valueSuffixLabel);
            Controls.Add(valueInput);
            Controls.Add(valueLabel);
            Controls.Add(displayNameTextBox);
            Controls.Add(displayNameLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ParametroContoDefinitionDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Definizione parametro conto";
            ((System.ComponentModel.ISupportInitialize)valueInput).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label displayNameLabel;
        private TextBox displayNameTextBox;
        private Label valueLabel;
        private NumericUpDown valueInput;
        private Label valueSuffixLabel;
        private Label validFromLabel;
        private DateTimePicker validFromInput;
        private Label validToLabel;
        private DateTimePicker validToInput;
        private Button saveButton;
        private Button cancelButton;
    }
}

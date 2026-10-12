namespace Finance.Desktop
{
    partial class TariffaDefinitionDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            valueLabel = new Label();
            valueInput = new NumericUpDown();
            fromLabel = new Label();
            fromInput = new DateTimePicker();
            toLabel = new Label();
            toInput = new DateTimePicker();
            saveButton = new Button();
            cancelButton = new Button();
            ((System.ComponentModel.ISupportInitialize)valueInput).BeginInit();
            SuspendLayout();
            //
            // valueLabel
            //
            valueLabel.Name = "valueLabel";
            valueLabel.Text = "Importo (€) — 0 significa tariffa sconosciuta";
            valueLabel.AutoSize = true;
            valueLabel.Location = new Point(16, 16);
            //
            // valueInput
            //
            valueInput.Name = "valueInput";
            valueInput.Location = new Point(16, 42);
            valueInput.Size = new Size(180, 23);
            valueInput.DecimalPlaces = 2;
            valueInput.Maximum = 1000000000;
            valueInput.TabIndex = 0;
            valueInput.Enter += ValueEnter;
            valueInput.KeyPress += ValueKeyPress;
            //
            // fromLabel
            //
            fromLabel.Name = "fromLabel";
            fromLabel.Text = "Dal (deselezionare per nessun limite)";
            fromLabel.AutoSize = true;
            fromLabel.Location = new Point(16, 84);
            //
            // fromInput
            //
            fromInput.Name = "fromInput";
            fromInput.Location = new Point(16, 108);
            fromInput.Size = new Size(260, 23);
            fromInput.Format = DateTimePickerFormat.Short;
            fromInput.ShowCheckBox = true;
            fromInput.TabIndex = 1;
            //
            // toLabel
            //
            toLabel.Name = "toLabel";
            toLabel.Text = "Al (deselezionare per nessun limite)";
            toLabel.AutoSize = true;
            toLabel.Location = new Point(16, 148);
            //
            // toInput
            //
            toInput.Name = "toInput";
            toInput.Location = new Point(16, 172);
            toInput.Size = new Size(260, 23);
            toInput.Format = DateTimePickerFormat.Short;
            toInput.ShowCheckBox = true;
            toInput.TabIndex = 2;
            //
            // saveButton
            //
            saveButton.Name = "saveButton";
            saveButton.Text = "&Salva";
            saveButton.Location = new Point(166, 220);
            saveButton.Size = new Size(90, 28);
            saveButton.TabIndex = 3;
            saveButton.Click += SaveClick;
            //
            // cancelButton
            //
            cancelButton.Name = "cancelButton";
            cancelButton.Text = "&Annulla";
            cancelButton.Location = new Point(266, 220);
            cancelButton.Size = new Size(90, 28);
            cancelButton.TabIndex = 4;
            cancelButton.DialogResult = DialogResult.Cancel;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(cancelButton);
            Controls.Add(saveButton);
            Controls.Add(toInput);
            Controls.Add(toLabel);
            Controls.Add(fromInput);
            Controls.Add(fromLabel);
            Controls.Add(valueInput);
            Controls.Add(valueLabel);
            ClientSize = new Size(374, 264);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AcceptButton = saveButton;
            CancelButton = cancelButton;
            Name = "TariffaDefinitionDialog";
            ((System.ComponentModel.ISupportInitialize)valueInput).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label valueLabel;
        private NumericUpDown valueInput;
        private Label fromLabel;
        private DateTimePicker fromInput;
        private Label toLabel;
        private DateTimePicker toInput;
        private Button saveButton;
        private Button cancelButton;
    }
}

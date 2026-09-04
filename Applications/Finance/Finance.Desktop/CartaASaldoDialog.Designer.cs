namespace Finance.Desktop
{
    partial class CartaASaldoDialog
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
            titleLabel = new Label();
            plafondLabel = new Label();
            plafondInput = new NumericUpDown();
            percentualeScopertoLabel = new Label();
            percentualeScopertoInput = new NumericUpDown();
            chiusuraCicloLabel = new Label();
            chiusuraCicloInput = new NumericUpDown();
            addebitoLabel = new Label();
            addebitoInput = new NumericUpDown();
            ripristinoPlafondLabel = new Label();
            ripristinoPlafondInput = new NumericUpDown();
            contoAddebitoLabel = new Label();
            contoAddebitoComboBox = new ComboBox();
            validFromLabel = new Label();
            validFromInput = new DateTimePicker();
            validToLabel = new Label();
            validToInput = new DateTimePicker();
            saveButton = new Button();
            cancelButton = new Button();
            ((System.ComponentModel.ISupportInitialize)plafondInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)percentualeScopertoInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chiusuraCicloInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)addebitoInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ripristinoPlafondInput).BeginInit();
            SuspendLayout();
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            titleLabel.Location = new Point(18, 16);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(209, 25);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Configura carta a saldo";
            // 
            // plafondLabel
            // 
            plafondLabel.AutoSize = true;
            plafondLabel.Location = new Point(18, 58);
            plafondLabel.Text = "&Plafond";
            // 
            // plafondInput
            // 
            plafondInput.DecimalPlaces = 2;
            plafondInput.Location = new Point(18, 80);
            plafondInput.Maximum = 1000000000;
            plafondInput.Name = "plafondInput";
            plafondInput.Size = new Size(160, 23);
            plafondInput.TabIndex = 2;
            plafondInput.TextAlign = HorizontalAlignment.Right;
            plafondInput.ThousandsSeparator = true;
            // 
            // percentualeScopertoLabel
            // 
            percentualeScopertoLabel.AutoSize = true;
            percentualeScopertoLabel.Location = new Point(204, 58);
            percentualeScopertoLabel.Text = "Percentuale &scoperto";
            // 
            // percentualeScopertoInput
            // 
            percentualeScopertoInput.DecimalPlaces = 2;
            percentualeScopertoInput.Location = new Point(204, 80);
            percentualeScopertoInput.Name = "percentualeScopertoInput";
            percentualeScopertoInput.Size = new Size(160, 23);
            percentualeScopertoInput.TabIndex = 4;
            // 
            // chiusuraCicloLabel
            // 
            chiusuraCicloLabel.AutoSize = true;
            chiusuraCicloLabel.Location = new Point(18, 122);
            chiusuraCicloLabel.Text = "&Chiusura ciclo";
            // 
            // chiusuraCicloInput
            // 
            chiusuraCicloInput.Location = new Point(18, 144);
            chiusuraCicloInput.Maximum = 31;
            chiusuraCicloInput.Minimum = 1;
            chiusuraCicloInput.Name = "chiusuraCicloInput";
            chiusuraCicloInput.Size = new Size(98, 23);
            chiusuraCicloInput.TabIndex = 6;
            chiusuraCicloInput.TextAlign = HorizontalAlignment.Right;
            // 
            // addebitoLabel
            // 
            addebitoLabel.AutoSize = true;
            addebitoLabel.Location = new Point(142, 122);
            addebitoLabel.Text = "&Addebito";
            // 
            // addebitoInput
            // 
            addebitoInput.Location = new Point(142, 144);
            addebitoInput.Maximum = 31;
            addebitoInput.Minimum = 1;
            addebitoInput.Name = "addebitoInput";
            addebitoInput.Size = new Size(98, 23);
            addebitoInput.TabIndex = 8;
            addebitoInput.TextAlign = HorizontalAlignment.Right;
            // 
            // ripristinoPlafondLabel
            // 
            ripristinoPlafondLabel.AutoSize = true;
            ripristinoPlafondLabel.Location = new Point(266, 122);
            ripristinoPlafondLabel.Text = "&Ripristino plafond";
            // 
            // ripristinoPlafondInput
            // 
            ripristinoPlafondInput.Location = new Point(266, 144);
            ripristinoPlafondInput.Maximum = 31;
            ripristinoPlafondInput.Minimum = 1;
            ripristinoPlafondInput.Name = "ripristinoPlafondInput";
            ripristinoPlafondInput.Size = new Size(98, 23);
            ripristinoPlafondInput.TabIndex = 10;
            ripristinoPlafondInput.TextAlign = HorizontalAlignment.Right;
            // 
            // contoAddebitoLabel
            // 
            contoAddebitoLabel.AutoSize = true;
            contoAddebitoLabel.Location = new Point(18, 186);
            contoAddebitoLabel.Text = "Conto di a&ddebito";
            // 
            // contoAddebitoComboBox
            // 
            contoAddebitoComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            contoAddebitoComboBox.Location = new Point(18, 208);
            contoAddebitoComboBox.Name = "contoAddebitoComboBox";
            contoAddebitoComboBox.Size = new Size(346, 23);
            contoAddebitoComboBox.TabIndex = 12;
            // 
            // validFromLabel
            // 
            validFromLabel.AutoSize = true;
            validFromLabel.Location = new Point(18, 250);
            validFromLabel.Text = "Pianifica &dal";
            // 
            // validFromInput
            // 
            validFromInput.Format = DateTimePickerFormat.Short;
            validFromInput.Location = new Point(18, 272);
            validFromInput.Name = "validFromInput";
            validFromInput.Size = new Size(160, 23);
            validFromInput.TabIndex = 14;
            // 
            // validToLabel
            // 
            validToLabel.AutoSize = true;
            validToLabel.Location = new Point(204, 250);
            validToLabel.Text = "Pianifica &fino al";
            // 
            // validToInput
            // 
            validToInput.Format = DateTimePickerFormat.Short;
            validToInput.Location = new Point(204, 272);
            validToInput.Name = "validToInput";
            validToInput.Size = new Size(160, 23);
            validToInput.TabIndex = 16;
            // 
            // saveButton
            // 
            saveButton.Location = new Point(208, 320);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(75, 28);
            saveButton.TabIndex = 17;
            saveButton.Text = "&Configura";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += SaveButtonClick;
            // 
            // cancelButton
            // 
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(289, 320);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 28);
            cancelButton.TabIndex = 18;
            cancelButton.Text = "&Annulla";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // CartaASaldoDialog
            // 
            AcceptButton = saveButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(384, 368);
            Controls.AddRange(new Control[] { titleLabel, plafondLabel, plafondInput, percentualeScopertoLabel,
                percentualeScopertoInput, chiusuraCicloLabel, chiusuraCicloInput, addebitoLabel, addebitoInput,
                ripristinoPlafondLabel, ripristinoPlafondInput, contoAddebitoLabel, contoAddebitoComboBox,
                validFromLabel, validFromInput, validToLabel, validToInput, saveButton, cancelButton });
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CartaASaldoDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Carta a saldo";
            ((System.ComponentModel.ISupportInitialize)plafondInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)percentualeScopertoInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)chiusuraCicloInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)addebitoInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)ripristinoPlafondInput).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label titleLabel;
        private Label plafondLabel;
        private NumericUpDown plafondInput;
        private Label percentualeScopertoLabel;
        private NumericUpDown percentualeScopertoInput;
        private Label chiusuraCicloLabel;
        private NumericUpDown chiusuraCicloInput;
        private Label addebitoLabel;
        private NumericUpDown addebitoInput;
        private Label ripristinoPlafondLabel;
        private NumericUpDown ripristinoPlafondInput;
        private Label contoAddebitoLabel;
        private ComboBox contoAddebitoComboBox;
        private Label validFromLabel;
        private DateTimePicker validFromInput;
        private Label validToLabel;
        private DateTimePicker validToInput;
        private Button saveButton;
        private Button cancelButton;
    }
}

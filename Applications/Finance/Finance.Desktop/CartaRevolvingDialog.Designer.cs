namespace Finance.Desktop
{
    partial class CartaRevolvingDialog
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
            inputPanel = new Panel();
            noteLabel = new Label();
            errorLabel = new Label();
            plafondLabel = new Label();
            plafondInput = new NumericUpDown();
            percentualeScopertoLabel = new Label();
            percentualeScopertoInput = new NumericUpDown();
            quotaRataLabel = new Label();
            quotaRataInput = new NumericUpDown();
            rataMinimaLabel = new Label();
            rataMinimaInput = new NumericUpDown();
            tanLabel = new Label();
            tanInput = new NumericUpDown();
            bolloLabel = new Label();
            bolloInput = new NumericUpDown();
            sogliaBolloLabel = new Label();
            sogliaBolloInput = new NumericUpDown();
            chiusuraCicloLabel = new Label();
            chiusuraCicloInput = new NumericUpDown();
            addebitoLabel = new Label();
            addebitoInput = new NumericUpDown();
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
            ((System.ComponentModel.ISupportInitialize)quotaRataInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rataMinimaInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tanInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bolloInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)sogliaBolloInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chiusuraCicloInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)addebitoInput).BeginInit();
            inputPanel.SuspendLayout();
            SuspendLayout();
            // 
            // inputPanel
            // 
            inputPanel.Location = new Point(0, 0);
            inputPanel.Name = "inputPanel";
            inputPanel.Size = new Size(600, 350);
            inputPanel.TabIndex = 0;
            // 
            // plafondLabel
            // 
            plafondLabel.AutoSize = true;
            plafondLabel.Location = new Point(18, 12);
            plafondLabel.Name = "plafondLabel";
            plafondLabel.TabIndex = 0;
            plafondLabel.Text = "&Plafond (€)";
            // 
            // plafondInput
            // 
            plafondInput.DecimalPlaces = 2;
            plafondInput.Location = new Point(18, 36);
            plafondInput.Maximum = 1000000000m;
            plafondInput.Minimum = 0m;
            plafondInput.Name = "plafondInput";
            plafondInput.Size = new Size(174, 23);
            plafondInput.TabIndex = 1;
            plafondInput.TextAlign = HorizontalAlignment.Right;
            plafondInput.ThousandsSeparator = true;
            plafondInput.Value = 1600m;
            plafondInput.ValueChanged += InputValueChanged;
            // 
            // percentualeScopertoLabel
            // 
            percentualeScopertoLabel.AutoSize = true;
            percentualeScopertoLabel.Location = new Point(214, 12);
            percentualeScopertoLabel.Name = "percentualeScopertoLabel";
            percentualeScopertoLabel.TabIndex = 2;
            percentualeScopertoLabel.Text = "&Scoperto (%)";
            // 
            // percentualeScopertoInput
            // 
            percentualeScopertoInput.DecimalPlaces = 2;
            percentualeScopertoInput.Location = new Point(214, 36);
            percentualeScopertoInput.Maximum = 100m;
            percentualeScopertoInput.Minimum = 0m;
            percentualeScopertoInput.Name = "percentualeScopertoInput";
            percentualeScopertoInput.Size = new Size(174, 23);
            percentualeScopertoInput.TabIndex = 3;
            percentualeScopertoInput.TextAlign = HorizontalAlignment.Right;
            percentualeScopertoInput.ThousandsSeparator = true;
            percentualeScopertoInput.Value = 10m;
            percentualeScopertoInput.ValueChanged += InputValueChanged;
            // 
            // quotaRataLabel
            // 
            quotaRataLabel.AutoSize = true;
            quotaRataLabel.Location = new Point(410, 12);
            quotaRataLabel.Name = "quotaRataLabel";
            quotaRataLabel.TabIndex = 4;
            quotaRataLabel.Text = "&Quota rata (%)";
            // 
            // quotaRataInput
            // 
            quotaRataInput.DecimalPlaces = 2;
            quotaRataInput.Location = new Point(410, 36);
            quotaRataInput.Maximum = 100m;
            quotaRataInput.Minimum = 0m;
            quotaRataInput.Name = "quotaRataInput";
            quotaRataInput.Size = new Size(174, 23);
            quotaRataInput.TabIndex = 5;
            quotaRataInput.TextAlign = HorizontalAlignment.Right;
            quotaRataInput.ThousandsSeparator = true;
            quotaRataInput.Value = 10m;
            quotaRataInput.ValueChanged += InputValueChanged;
            // 
            // rataMinimaLabel
            // 
            rataMinimaLabel.AutoSize = true;
            rataMinimaLabel.Location = new Point(18, 82);
            rataMinimaLabel.Name = "rataMinimaLabel";
            rataMinimaLabel.TabIndex = 6;
            rataMinimaLabel.Text = "Rata &minima (€)";
            // 
            // rataMinimaInput
            // 
            rataMinimaInput.DecimalPlaces = 2;
            rataMinimaInput.Location = new Point(18, 106);
            rataMinimaInput.Maximum = 1000000000m;
            rataMinimaInput.Minimum = 0m;
            rataMinimaInput.Name = "rataMinimaInput";
            rataMinimaInput.Size = new Size(174, 23);
            rataMinimaInput.TabIndex = 7;
            rataMinimaInput.TextAlign = HorizontalAlignment.Right;
            rataMinimaInput.ThousandsSeparator = true;
            rataMinimaInput.Value = 72.32m;
            rataMinimaInput.ValueChanged += InputValueChanged;
            // 
            // tanLabel
            // 
            tanLabel.AutoSize = true;
            tanLabel.Location = new Point(214, 82);
            tanLabel.Name = "tanLabel";
            tanLabel.TabIndex = 8;
            tanLabel.Text = "&TAN (%)";
            // 
            // tanInput
            // 
            tanInput.DecimalPlaces = 4;
            tanInput.Location = new Point(214, 106);
            tanInput.Maximum = 100m;
            tanInput.Minimum = 0m;
            tanInput.Name = "tanInput";
            tanInput.Size = new Size(174, 23);
            tanInput.TabIndex = 9;
            tanInput.TextAlign = HorizontalAlignment.Right;
            tanInput.ThousandsSeparator = true;
            tanInput.Value = 12m;
            tanInput.ValueChanged += InputValueChanged;
            // 
            // bolloLabel
            // 
            bolloLabel.AutoSize = true;
            bolloLabel.Location = new Point(410, 82);
            bolloLabel.Name = "bolloLabel";
            bolloLabel.TabIndex = 10;
            bolloLabel.Text = "&Bollo (€)";
            // 
            // bolloInput
            // 
            bolloInput.DecimalPlaces = 2;
            bolloInput.Location = new Point(410, 106);
            bolloInput.Maximum = 1000000000m;
            bolloInput.Minimum = 0m;
            bolloInput.Name = "bolloInput";
            bolloInput.Size = new Size(174, 23);
            bolloInput.TabIndex = 11;
            bolloInput.TextAlign = HorizontalAlignment.Right;
            bolloInput.ThousandsSeparator = true;
            bolloInput.Value = 2m;
            bolloInput.ValueChanged += InputValueChanged;
            // 
            // sogliaBolloLabel
            // 
            sogliaBolloLabel.AutoSize = true;
            sogliaBolloLabel.Location = new Point(18, 152);
            sogliaBolloLabel.Name = "sogliaBolloLabel";
            sogliaBolloLabel.TabIndex = 12;
            sogliaBolloLabel.Text = "S&oglia bollo (€)";
            // 
            // sogliaBolloInput
            // 
            sogliaBolloInput.DecimalPlaces = 2;
            sogliaBolloInput.Location = new Point(18, 176);
            sogliaBolloInput.Maximum = 1000000000m;
            sogliaBolloInput.Minimum = 0m;
            sogliaBolloInput.Name = "sogliaBolloInput";
            sogliaBolloInput.Size = new Size(174, 23);
            sogliaBolloInput.TabIndex = 13;
            sogliaBolloInput.TextAlign = HorizontalAlignment.Right;
            sogliaBolloInput.ThousandsSeparator = true;
            sogliaBolloInput.Value = 70m;
            sogliaBolloInput.ValueChanged += InputValueChanged;
            // 
            // chiusuraCicloLabel
            // 
            chiusuraCicloLabel.AutoSize = true;
            chiusuraCicloLabel.Location = new Point(214, 152);
            chiusuraCicloLabel.Name = "chiusuraCicloLabel";
            chiusuraCicloLabel.TabIndex = 14;
            chiusuraCicloLabel.Text = "Giorno &chiusura ciclo";
            // 
            // chiusuraCicloInput
            // 
            chiusuraCicloInput.DecimalPlaces = 0;
            chiusuraCicloInput.Location = new Point(214, 176);
            chiusuraCicloInput.Maximum = 31m;
            chiusuraCicloInput.Minimum = 1m;
            chiusuraCicloInput.Name = "chiusuraCicloInput";
            chiusuraCicloInput.Size = new Size(174, 23);
            chiusuraCicloInput.TabIndex = 15;
            chiusuraCicloInput.TextAlign = HorizontalAlignment.Right;
            chiusuraCicloInput.ThousandsSeparator = true;
            chiusuraCicloInput.Value = 6m;
            chiusuraCicloInput.ValueChanged += InputValueChanged;
            // 
            // addebitoLabel
            // 
            addebitoLabel.AutoSize = true;
            addebitoLabel.Location = new Point(410, 152);
            addebitoLabel.Name = "addebitoLabel";
            addebitoLabel.TabIndex = 16;
            addebitoLabel.Text = "Giorno &addebito / rimborso";
            // 
            // addebitoInput
            // 
            addebitoInput.DecimalPlaces = 0;
            addebitoInput.Location = new Point(410, 176);
            addebitoInput.Maximum = 31m;
            addebitoInput.Minimum = 1m;
            addebitoInput.Name = "addebitoInput";
            addebitoInput.Size = new Size(174, 23);
            addebitoInput.TabIndex = 17;
            addebitoInput.TextAlign = HorizontalAlignment.Right;
            addebitoInput.ThousandsSeparator = true;
            addebitoInput.Value = 19m;
            addebitoInput.ValueChanged += InputValueChanged;
            // 
            // contoAddebitoLabel
            // 
            contoAddebitoLabel.AutoSize = true;
            contoAddebitoLabel.Location = new Point(18, 222);
            contoAddebitoLabel.Name = "contoAddebitoLabel";
            contoAddebitoLabel.TabIndex = 18;
            contoAddebitoLabel.Text = "Conto di a&ddebito";
            // 
            // contoAddebitoComboBox
            // 
            contoAddebitoComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            contoAddebitoComboBox.Location = new Point(18, 246);
            contoAddebitoComboBox.Name = "contoAddebitoComboBox";
            contoAddebitoComboBox.Size = new Size(566, 23);
            contoAddebitoComboBox.TabIndex = 19;
            contoAddebitoComboBox.SelectedIndexChanged += InputValueChanged;
            // 
            // validFromLabel
            // 
            validFromLabel.AutoSize = true;
            validFromLabel.Location = new Point(18, 286);
            validFromLabel.Name = "validFromLabel";
            validFromLabel.TabIndex = 20;
            validFromLabel.Text = "Pianifica da&l";
            // 
            // validFromInput
            // 
            validFromInput.CustomFormat = "dd/MM/yyyy";
            validFromInput.Format = DateTimePickerFormat.Custom;
            validFromInput.Location = new Point(18, 310);
            validFromInput.Name = "validFromInput";
            validFromInput.Size = new Size(272, 23);
            validFromInput.TabIndex = 21;
            validFromInput.ValueChanged += InputValueChanged;
            // 
            // validToLabel
            // 
            validToLabel.AutoSize = true;
            validToLabel.Location = new Point(312, 286);
            validToLabel.Name = "validToLabel";
            validToLabel.TabIndex = 22;
            validToLabel.Text = "Pianifica &fino al";
            // 
            // validToInput
            // 
            validToInput.CustomFormat = "dd/MM/yyyy";
            validToInput.Format = DateTimePickerFormat.Custom;
            validToInput.Location = new Point(312, 310);
            validToInput.Name = "validToInput";
            validToInput.Size = new Size(272, 23);
            validToInput.TabIndex = 23;
            validToInput.ValueChanged += InputValueChanged;
            // 
            // noteLabel
            // 
            noteLabel.Location = new Point(18, 360);
            noteLabel.Name = "noteLabel";
            noteLabel.Size = new Size(566, 72);
            noteLabel.TabIndex = 1;
            noteLabel.Text = "Crea parametri e pianificazioni di interessi, bollo, rimborso e addebito.\r\nScegliere il periodo evitando sovrapposizioni con movimenti già presenti.\r\nI valori economici saranno modificabili dalla griglia Parametri.";
            // 
            // errorLabel
            // 
            errorLabel.ForeColor = Color.Firebrick;
            errorLabel.Location = new Point(18, 438);
            errorLabel.Name = "errorLabel";
            errorLabel.Size = new Size(566, 74);
            errorLabel.TabIndex = 2;
            // 
            // saveButton
            // 
            saveButton.Location = new Point(364, 528);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(106, 30);
            saveButton.TabIndex = 3;
            saveButton.Text = "Con&figura";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += SaveButtonClick;
            // 
            // cancelButton
            // 
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(478, 528);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(106, 30);
            cancelButton.TabIndex = 4;
            cancelButton.Text = "&Annulla";
            cancelButton.UseVisualStyleBackColor = true;
            inputPanel.Controls.Add(validToInput);
            inputPanel.Controls.Add(validToLabel);
            inputPanel.Controls.Add(validFromInput);
            inputPanel.Controls.Add(validFromLabel);
            inputPanel.Controls.Add(contoAddebitoComboBox);
            inputPanel.Controls.Add(contoAddebitoLabel);
            inputPanel.Controls.Add(addebitoInput);
            inputPanel.Controls.Add(addebitoLabel);
            inputPanel.Controls.Add(chiusuraCicloInput);
            inputPanel.Controls.Add(chiusuraCicloLabel);
            inputPanel.Controls.Add(sogliaBolloInput);
            inputPanel.Controls.Add(sogliaBolloLabel);
            inputPanel.Controls.Add(bolloInput);
            inputPanel.Controls.Add(bolloLabel);
            inputPanel.Controls.Add(tanInput);
            inputPanel.Controls.Add(tanLabel);
            inputPanel.Controls.Add(rataMinimaInput);
            inputPanel.Controls.Add(rataMinimaLabel);
            inputPanel.Controls.Add(quotaRataInput);
            inputPanel.Controls.Add(quotaRataLabel);
            inputPanel.Controls.Add(percentualeScopertoInput);
            inputPanel.Controls.Add(percentualeScopertoLabel);
            inputPanel.Controls.Add(plafondInput);
            inputPanel.Controls.Add(plafondLabel);
            AcceptButton = saveButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(602, 578);
            Controls.Add(cancelButton);
            Controls.Add(saveButton);
            Controls.Add(errorLabel);
            Controls.Add(noteLabel);
            Controls.Add(inputPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CartaRevolvingDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Carta revolving";
            ((System.ComponentModel.ISupportInitialize)plafondInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)percentualeScopertoInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)quotaRataInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)rataMinimaInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)tanInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)bolloInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)sogliaBolloInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)chiusuraCicloInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)addebitoInput).EndInit();
            inputPanel.ResumeLayout(false);
            inputPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel inputPanel;
        private Label noteLabel;
        private Label errorLabel;
        private Label plafondLabel;
        private NumericUpDown plafondInput;
        private Label percentualeScopertoLabel;
        private NumericUpDown percentualeScopertoInput;
        private Label quotaRataLabel;
        private NumericUpDown quotaRataInput;
        private Label rataMinimaLabel;
        private NumericUpDown rataMinimaInput;
        private Label tanLabel;
        private NumericUpDown tanInput;
        private Label bolloLabel;
        private NumericUpDown bolloInput;
        private Label sogliaBolloLabel;
        private NumericUpDown sogliaBolloInput;
        private Label chiusuraCicloLabel;
        private NumericUpDown chiusuraCicloInput;
        private Label addebitoLabel;
        private NumericUpDown addebitoInput;
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


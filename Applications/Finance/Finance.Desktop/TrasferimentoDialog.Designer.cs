namespace Finance.Desktop
{
    partial class TrasferimentoDialog
    {
        private System.ComponentModel.IContainer components = new System.ComponentModel.Container();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            fieldsPanel = new Panel();
            originLabel = new Label();
            destinationLabel = new Label();
            dateLabel = new Label();
            amountLabel = new Label();
            descriptionLabel = new Label();
            originInput = new Controls.SearchComboBox();
            destinationInput = new Controls.SearchComboBox();
            dateInput = new DateTimePicker();
            amountInput = new NumericUpDown();
            descriptionInput = new TextBox();
            confirmedCheck = new CheckBox();
            previewLabel = new Label();
            noticeLabel = new Label();
            errorLabel = new Label();
            saveButton = new Button();
            cancelButton = new Button();
            ((System.ComponentModel.ISupportInitialize)amountInput).BeginInit();
            fieldsPanel.SuspendLayout();
            SuspendLayout();
            fieldsPanel.Location = new Point(16, 12);
            fieldsPanel.Size = new Size(568, 216);
            fieldsPanel.TabIndex = 0;
            originLabel.Text = "Da (origine)";
            originLabel.Location = new Point(0, 0);
            originLabel.Size = new Size(264, 22);
            originInput.Name = "originInput";
            originInput.Location = new Point(0, 24);
            originInput.Size = new Size(264, 25);
            originInput.DropDownStyle = ComboBoxStyle.DropDownList;
            originInput.TabIndex = 0;
            originInput.SelectedIndexChanged += InputChanged;
            destinationLabel.Text = "A (destinazione)";
            destinationLabel.Location = new Point(292, 0);
            destinationLabel.Size = new Size(264, 22);
            destinationInput.Name = "destinationInput";
            destinationInput.Location = new Point(292, 24);
            destinationInput.Size = new Size(264, 25);
            destinationInput.DropDownStyle = ComboBoxStyle.DropDownList;
            destinationInput.TabIndex = 1;
            destinationInput.SelectedIndexChanged += InputChanged;
            dateLabel.Text = "Data";
            dateLabel.Location = new Point(0, 62);
            dateLabel.Size = new Size(264, 22);
            dateInput.Name = "dateInput";
            dateInput.Location = new Point(0, 86);
            dateInput.Size = new Size(180, 25);
            dateInput.Format = DateTimePickerFormat.Short;
            dateInput.TabIndex = 2;
            dateInput.ValueChanged += InputChanged;
            amountLabel.Text = "Importo da trasferire (€)";
            amountLabel.Location = new Point(292, 62);
            amountLabel.Size = new Size(264, 22);
            amountInput.Name = "amountInput";
            amountInput.Location = new Point(292, 86);
            amountInput.Size = new Size(180, 25);
            amountInput.DecimalPlaces = 2;
            amountInput.Maximum = 1000000000;
            amountInput.TabIndex = 3;
            amountInput.ValueChanged += InputChanged;
            amountInput.Enter += AmountEnter;
            amountInput.KeyPress += AmountKeyPress;
            descriptionLabel.Text = "Descrizione (per entrambi i movimenti)";
            descriptionLabel.Location = new Point(0, 122);
            descriptionLabel.Size = new Size(550, 22);
            descriptionInput.Name = "descriptionInput";
            descriptionInput.Location = new Point(0, 146);
            descriptionInput.Size = new Size(556, 25);
            descriptionInput.TabIndex = 4;
            descriptionInput.TextChanged += InputChanged;
            confirmedCheck.Name = "confirmedCheck";
            confirmedCheck.Text = "Già confermato";
            confirmedCheck.Location = new Point(0, 185);
            confirmedCheck.Size = new Size(264, 25);
            confirmedCheck.TabIndex = 5;
            confirmedCheck.CheckedChanged += InputChanged;
            fieldsPanel.Controls.AddRange([originLabel, originInput, destinationLabel, destinationInput, dateLabel, dateInput,
                amountLabel, amountInput, descriptionLabel, descriptionInput, confirmedCheck]);
            previewLabel.Name = "previewLabel";
            previewLabel.Location = new Point(16, 238);
            previewLabel.Size = new Size(556, 156);
            previewLabel.BackColor = Color.AliceBlue;
            previewLabel.Padding = new Padding(10);
            noticeLabel.Text = "Registra il trasferimento in Finance; non esegue operazioni bancarie reali.";
            noticeLabel.Location = new Point(16, 402);
            noticeLabel.Size = new Size(556, 38);
            errorLabel.Name = "errorLabel";
            errorLabel.ForeColor = Color.Firebrick;
            errorLabel.Location = new Point(16, 444);
            errorLabel.Size = new Size(556, 76);
            saveButton.Name = "saveButton";
            saveButton.Text = "Crea trasferimento";
            saveButton.Location = new Point(318, 530);
            saveButton.Size = new Size(150, 30);
            saveButton.Enabled = false;
            saveButton.TabIndex = 1;
            saveButton.Click += SaveButtonClick;
            cancelButton.Text = "Annulla";
            cancelButton.Location = new Point(478, 530);
            cancelButton.Size = new Size(94, 30);
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.TabIndex = 2;
            AcceptButton = saveButton;
            CancelButton = cancelButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 576);
            Controls.AddRange([fieldsPanel, previewLabel, noticeLabel, errorLabel, saveButton, cancelButton]);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Trasferimento";
            ((System.ComponentModel.ISupportInitialize)amountInput).EndInit();
            fieldsPanel.ResumeLayout(false);
            fieldsPanel.PerformLayout();
            ResumeLayout(false);
        }
        #endregion

        private Panel fieldsPanel;
        private Label originLabel;
        private Label destinationLabel;
        private Label dateLabel;
        private Label amountLabel;
        private Label descriptionLabel;
        private ComboBox originInput;
        private ComboBox destinationInput;
        private DateTimePicker dateInput;
        private NumericUpDown amountInput;
        private TextBox descriptionInput;
        private CheckBox confirmedCheck;
        private Label previewLabel;
        private Label noticeLabel;
        private Label errorLabel;
        private Button saveButton;
        private Button cancelButton;
    }
}

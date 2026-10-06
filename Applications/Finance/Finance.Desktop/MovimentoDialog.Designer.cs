namespace Finance.Desktop
{
    partial class MovimentoDialog
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
            dateLabel = new Label();
            contoLabel = new Label();
            contoInput = new ComboBox();
            dateInput = new DateTimePicker();
            descriptionLabel = new Label();
            descriptionInput = new TextBox();
            amountLabel = new Label();
            amountInput = new NumericUpDown();
            expenseCheck = new CheckBox();
            categoryLabel = new Label();
            categoryInput = new ComboBox();
            confirmedCheck = new CheckBox();
            formulaLabel = new Label();
            errorLabel = new Label();
            saveButton = new Button();
            cancelButton = new Button();
            ((System.ComponentModel.ISupportInitialize)amountInput).BeginInit();
            SuspendLayout();
            contoLabel.Location = new Point(210, 16);
            contoLabel.Name = "contoLabel";
            contoLabel.Size = new Size(260, 20);
            contoLabel.Text = "Conto";
            contoInput.Location = new Point(210, 40);
            contoInput.Name = "contoInput";
            contoInput.Size = new Size(266, 25);
            contoInput.DropDownStyle = ComboBoxStyle.DropDownList;
            contoInput.TabIndex = 2;
            //
            // dateLabel
            //
            dateLabel.Location = new Point(16, 16);
            dateLabel.Name = "dateLabel";
            dateLabel.Size = new Size(100, 20);
            dateLabel.TabIndex = 0;
            dateLabel.Text = "Data";
            //
            // dateInput
            //
            dateInput.Location = new Point(16, 40);
            dateInput.Name = "dateInput";
            dateInput.Size = new Size(160, 25);
            dateInput.TabIndex = 1;
            dateInput.Format = DateTimePickerFormat.Custom;
            dateInput.CustomFormat = "dd/MM/yyyy";
            dateInput.ValueChanged += DateInputValueChanged;
            //
            // descriptionLabel
            //
            descriptionLabel.Location = new Point(16, 80);
            descriptionLabel.Name = "descriptionLabel";
            descriptionLabel.Size = new Size(200, 20);
            descriptionLabel.TabIndex = 2;
            descriptionLabel.Text = "Descrizione";
            //
            // descriptionInput
            //
            descriptionInput.Location = new Point(16, 104);
            descriptionInput.Name = "descriptionInput";
            descriptionInput.Size = new Size(460, 25);
            descriptionInput.TabIndex = 3;
            //
            // amountLabel
            //
            amountLabel.Location = new Point(16, 144);
            amountLabel.Name = "amountLabel";
            amountLabel.Size = new Size(140, 20);
            amountLabel.TabIndex = 4;
            amountLabel.Text = "Importo (€)";
            //
            // amountInput
            //
            amountInput.Location = new Point(16, 168);
            amountInput.Name = "amountInput";
            amountInput.Size = new Size(170, 25);
            amountInput.TabIndex = 5;
            amountInput.DecimalPlaces = 2;
            amountInput.Maximum = 999999999m;
            amountInput.ThousandsSeparator = true;
            amountInput.ValueChanged += AmountChanged;
            amountInput.Enter += AmountInputEnter;
            amountInput.KeyPress += AmountInputKeyPress;
            //
            // expenseCheck
            //
            expenseCheck.Location = new Point(210, 168);
            expenseCheck.Name = "expenseCheck";
            expenseCheck.Size = new Size(260, 25);
            expenseCheck.TabIndex = 6;
            expenseCheck.Text = "Spesa (deseleziona per accredito)";
            expenseCheck.CheckedChanged += AmountChanged;
            //
            // categoryLabel
            //
            categoryLabel.Location = new Point(16, 207);
            categoryLabel.Name = "categoryLabel";
            categoryLabel.Size = new Size(140, 20);
            categoryLabel.TabIndex = 7;
            categoryLabel.Text = "Categoria";
            //
            // categoryInput
            //
            categoryInput.Location = new Point(16, 231);
            categoryInput.Name = "categoryInput";
            categoryInput.Size = new Size(270, 25);
            categoryInput.TabIndex = 8;
            categoryInput.DropDownStyle = ComboBoxStyle.DropDownList;
            //
            // confirmedCheck
            //
            confirmedCheck.Location = new Point(305, 231);
            confirmedCheck.Name = "confirmedCheck";
            confirmedCheck.Size = new Size(160, 25);
            confirmedCheck.TabIndex = 9;
            confirmedCheck.Text = "Confermato";
            //
            // formulaLabel
            //
            formulaLabel.Location = new Point(16, 270);
            formulaLabel.Name = "formulaLabel";
            formulaLabel.Size = new Size(460, 35);
            formulaLabel.TabIndex = 10;
            formulaLabel.AutoEllipsis = true;
            //
            // errorLabel
            //
            errorLabel.Location = new Point(16, 311);
            errorLabel.Name = "errorLabel";
            errorLabel.Size = new Size(460, 65);
            errorLabel.TabIndex = 11;
            errorLabel.ForeColor = Color.Firebrick;
            //
            // saveButton
            //
            saveButton.Location = new Point(300, 390);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(85, 30);
            saveButton.TabIndex = 12;
            saveButton.Text = "&Salva";
            saveButton.Click += SaveButtonClick;
            //
            // cancelButton
            //
            cancelButton.Location = new Point(391, 390);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(85, 30);
            cancelButton.TabIndex = 13;
            cancelButton.Text = "&Annulla";
            cancelButton.DialogResult = DialogResult.Cancel;
            //
            // MovimentoDialog
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(495, 438);
            Controls.Add(dateLabel);
            Controls.Add(contoLabel);
            Controls.Add(contoInput);
            Controls.Add(dateInput);
            Controls.Add(descriptionLabel);
            Controls.Add(descriptionInput);
            Controls.Add(amountLabel);
            Controls.Add(amountInput);
            Controls.Add(expenseCheck);
            Controls.Add(categoryLabel);
            Controls.Add(categoryInput);
            Controls.Add(confirmedCheck);
            Controls.Add(formulaLabel);
            Controls.Add(errorLabel);
            Controls.Add(saveButton);
            Controls.Add(cancelButton);
            Name = "MovimentoDialog";
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AcceptButton = saveButton;
            CancelButton = cancelButton;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            ((System.ComponentModel.ISupportInitialize)amountInput).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label dateLabel;
        private Label contoLabel;
        private ComboBox contoInput;
        private DateTimePicker dateInput;
        private Label descriptionLabel;
        private TextBox descriptionInput;
        private Label amountLabel;
        private NumericUpDown amountInput;
        private CheckBox expenseCheck;
        private Label categoryLabel;
        private ComboBox categoryInput;
        private CheckBox confirmedCheck;
        private Label formulaLabel;
        private Label errorLabel;
        private Button saveButton;
        private Button cancelButton;
    }
}


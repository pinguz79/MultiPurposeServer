namespace Finance.Desktop
{
    partial class PedaggioDialog
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
            dateLabel = new Label();
            dateInput = new DateTimePicker();
            categoryLabel = new Label();
            categoryInput = new ComboBox();
            entrataLabel = new Label();
            entrataInput = new ComboBox();
            uscitaLabel = new Label();
            uscitaInput = new ComboBox();
            otherStationsCheck = new CheckBox();
            descriptionLabel = new Label();
            descriptionInput = new TextBox();
            costLabel = new Label();
            amountLabel = new Label();
            confirmedCheck = new CheckBox();
            editAmountButton = new Button();
            errorLabel = new Label();
            saveButton = new Button();
            cancelButton = new Button();
            SuspendLayout();
            // 
            // dateLabel
            // 
            dateLabel.Location = new Point(18, 16);
            dateLabel.Name = "dateLabel";
            dateLabel.Size = new Size(100, 20);
            dateLabel.TabIndex = 0;
            dateLabel.Text = "&Data";
            // 
            // dateInput
            // 
            dateInput.Format = DateTimePickerFormat.Custom;
            dateInput.CustomFormat = "dd/MM/yyyy";
            dateInput.Location = new Point(18, 40);
            dateInput.Name = "dateInput";
            dateInput.Size = new Size(130, 23);
            dateInput.TabIndex = 1;
            dateInput.ValueChanged += DateChanged;
            // 
            // categoryLabel
            // 
            categoryLabel.Location = new Point(330, 16);
            categoryLabel.Name = "categoryLabel";
            categoryLabel.Size = new Size(200, 20);
            categoryLabel.TabIndex = 2;
            categoryLabel.Text = "&Categoria";
            // 
            // categoryInput
            // 
            categoryInput.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryInput.Location = new Point(330, 40);
            categoryInput.Name = "categoryInput";
            categoryInput.Size = new Size(270, 23);
            categoryInput.TabIndex = 3;
            // 
            // entrataLabel
            // 
            entrataLabel.Location = new Point(18, 82);
            entrataLabel.Name = "entrataLabel";
            entrataLabel.Size = new Size(265, 20);
            entrataLabel.TabIndex = 4;
            entrataLabel.Text = "&Entrata";
            // 
            // entrataInput
            // 
            entrataInput.DropDownStyle = ComboBoxStyle.DropDownList;
            entrataInput.Location = new Point(18, 106);
            entrataInput.Name = "entrataInput";
            entrataInput.Size = new Size(270, 23);
            entrataInput.TabIndex = 5;
            entrataInput.SelectedIndexChanged += EntrataChanged;
            // 
            // uscitaLabel
            // 
            uscitaLabel.Location = new Point(330, 82);
            uscitaLabel.Name = "uscitaLabel";
            uscitaLabel.Size = new Size(260, 20);
            uscitaLabel.TabIndex = 6;
            uscitaLabel.Text = "&Uscita";
            // 
            // uscitaInput
            // 
            uscitaInput.DropDownStyle = ComboBoxStyle.DropDownList;
            uscitaInput.Location = new Point(330, 106);
            uscitaInput.Name = "uscitaInput";
            uscitaInput.Size = new Size(270, 23);
            uscitaInput.TabIndex = 7;
            uscitaInput.SelectedIndexChanged += UscitaChanged;
            // 
            // otherStationsCheck
            // 
            otherStationsCheck.Location = new Point(330, 136);
            otherStationsCheck.Name = "otherStationsCheck";
            otherStationsCheck.Size = new Size(270, 24);
            otherStationsCheck.TabIndex = 8;
            otherStationsCheck.Text = "Mostra &altre stazioni";
            otherStationsCheck.CheckedChanged += OtherStationsChanged;
            // 
            // descriptionLabel
            // 
            descriptionLabel.Location = new Point(18, 168);
            descriptionLabel.Name = "descriptionLabel";
            descriptionLabel.Size = new Size(200, 20);
            descriptionLabel.TabIndex = 9;
            descriptionLabel.Text = "De&scrizione";
            // 
            // descriptionInput
            // 
            descriptionInput.Location = new Point(18, 191);
            descriptionInput.Name = "descriptionInput";
            descriptionInput.Size = new Size(582, 23);
            descriptionInput.TabIndex = 10;
            // 
            // costLabel
            // 
            costLabel.Location = new Point(18, 230);
            costLabel.Name = "costLabel";
            costLabel.Size = new Size(70, 23);
            costLabel.TabIndex = 11;
            costLabel.Text = "Importo:";
            // 
            // amountLabel
            // 
            amountLabel.Location = new Point(90, 230);
            amountLabel.Name = "amountLabel";
            amountLabel.Size = new Size(130, 23);
            amountLabel.TabIndex = 12;
            amountLabel.Text = "0,00 €";
            // 
            // confirmedCheck
            // 
            confirmedCheck.Location = new Point(230, 228);
            confirmedCheck.Name = "confirmedCheck";
            confirmedCheck.Size = new Size(140, 25);
            confirmedCheck.TabIndex = 13;
            confirmedCheck.Text = "Con&fermato";
            confirmedCheck.CheckedChanged += DateChanged;
            // 
            // editAmountButton
            // 
            editAmountButton.Location = new Point(400, 228);
            editAmountButton.Name = "editAmountButton";
            editAmountButton.Size = new Size(200, 27);
            editAmountButton.TabIndex = 14;
            editAmountButton.Text = "Modifica importo…";
            editAmountButton.Click += EditAmountClick;
            // 
            // errorLabel
            // 
            errorLabel.ForeColor = Color.Firebrick;
            errorLabel.Location = new Point(18, 264);
            errorLabel.Name = "errorLabel";
            errorLabel.Size = new Size(582, 42);
            errorLabel.TabIndex = 15;
            // 
            // saveButton
            // 
            saveButton.Enabled = false;
            saveButton.Location = new Point(414, 320);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(88, 28);
            saveButton.TabIndex = 16;
            saveButton.Text = "&Salva";
            saveButton.Click += SaveClick;
            // 
            // cancelButton
            // 
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(512, 320);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(88, 28);
            cancelButton.TabIndex = 17;
            cancelButton.Text = "A&nnulla";
            // 
            // PedaggioDialog
            // 
            AcceptButton = saveButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(620, 370);
            Controls.Add(cancelButton);
            Controls.Add(saveButton);
            Controls.Add(errorLabel);
            Controls.Add(editAmountButton);
            Controls.Add(confirmedCheck);
            Controls.Add(amountLabel);
            Controls.Add(costLabel);
            Controls.Add(descriptionInput);
            Controls.Add(descriptionLabel);
            Controls.Add(otherStationsCheck);
            Controls.Add(uscitaInput);
            Controls.Add(uscitaLabel);
            Controls.Add(entrataInput);
            Controls.Add(entrataLabel);
            Controls.Add(categoryInput);
            Controls.Add(categoryLabel);
            Controls.Add(dateInput);
            Controls.Add(dateLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PedaggioDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Nuovo pedaggio";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label dateLabel;
        private DateTimePicker dateInput;
        private Label categoryLabel;
        private ComboBox categoryInput;
        private Label entrataLabel;
        private ComboBox entrataInput;
        private Label uscitaLabel;
        private ComboBox uscitaInput;
        private CheckBox otherStationsCheck;
        private Label descriptionLabel;
        private TextBox descriptionInput;
        private Label costLabel;
        private Label amountLabel;
        private CheckBox confirmedCheck;
        private Button editAmountButton;
        private Label errorLabel;
        private Button saveButton;
        private Button cancelButton;
    }
}


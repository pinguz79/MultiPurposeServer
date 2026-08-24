namespace Finance.Desktop
{
    partial class NewContoDialog
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
            nameLabel = new Label();
            nameInput = new TextBox();
            nameErrorLabel = new Label();
            namePreviewLabel = new Label();
            namePreviewValue = new Label();
            displayNameLabel = new Label();
            displayNameInput = new TextBox();
            displayNameErrorLabel = new Label();
            initialBalanceLabel = new Label();
            initialBalanceInput = new NumericUpDown();
            initialBalanceErrorLabel = new Label();
            generalErrorLabel = new Label();
            createButton = new Button();
            cancelButton = new Button();
            ((System.ComponentModel.ISupportInitialize)initialBalanceInput).BeginInit();
            SuspendLayout();
            // 
            // nameLabel
            // 
            nameLabel.AutoSize = true;
            nameLabel.Location = new Point(24, 24);
            nameLabel.Name = "nameLabel";
            nameLabel.Size = new Size(40, 15);
            nameLabel.TabIndex = 0;
            nameLabel.Text = "&Nome";
            // 
            // nameInput
            // 
            nameInput.Location = new Point(24, 42);
            nameInput.Name = "nameInput";
            nameInput.Size = new Size(420, 23);
            nameInput.TabIndex = 1;
            nameInput.TextChanged += NameInputTextChanged;
            // 
            // nameErrorLabel
            // 
            nameErrorLabel.AutoSize = true;
            nameErrorLabel.ForeColor = Color.Firebrick;
            nameErrorLabel.Location = new Point(24, 68);
            nameErrorLabel.Name = "nameErrorLabel";
            nameErrorLabel.Size = new Size(0, 15);
            nameErrorLabel.TabIndex = 2;
            // 
            // namePreviewLabel
            // 
            namePreviewLabel.AutoSize = true;
            namePreviewLabel.ForeColor = SystemColors.GrayText;
            namePreviewLabel.Location = new Point(24, 88);
            namePreviewLabel.Name = "namePreviewLabel";
            namePreviewLabel.Size = new Size(85, 15);
            namePreviewLabel.TabIndex = 3;
            namePreviewLabel.Text = "Nome tecnico:";
            // 
            // namePreviewValue
            // 
            namePreviewValue.AutoSize = true;
            namePreviewValue.ForeColor = SystemColors.GrayText;
            namePreviewValue.Location = new Point(115, 88);
            namePreviewValue.Name = "namePreviewValue";
            namePreviewValue.Size = new Size(0, 15);
            namePreviewValue.TabIndex = 4;
            // 
            // displayNameLabel
            // 
            displayNameLabel.AutoSize = true;
            displayNameLabel.Location = new Point(24, 119);
            displayNameLabel.Name = "displayNameLabel";
            displayNameLabel.Size = new Size(105, 15);
            displayNameLabel.TabIndex = 5;
            displayNameLabel.Text = "Nome &visualizzato";
            // 
            // displayNameInput
            // 
            displayNameInput.Location = new Point(24, 137);
            displayNameInput.Name = "displayNameInput";
            displayNameInput.Size = new Size(420, 23);
            displayNameInput.TabIndex = 6;
            displayNameInput.TextChanged += DisplayNameInputTextChanged;
            // 
            // displayNameErrorLabel
            // 
            displayNameErrorLabel.AutoSize = true;
            displayNameErrorLabel.ForeColor = Color.Firebrick;
            displayNameErrorLabel.Location = new Point(24, 163);
            displayNameErrorLabel.Name = "displayNameErrorLabel";
            displayNameErrorLabel.Size = new Size(0, 15);
            displayNameErrorLabel.TabIndex = 7;
            // 
            // initialBalanceLabel
            // 
            initialBalanceLabel.AutoSize = true;
            initialBalanceLabel.Location = new Point(24, 194);
            initialBalanceLabel.Name = "initialBalanceLabel";
            initialBalanceLabel.Size = new Size(76, 15);
            initialBalanceLabel.TabIndex = 8;
            initialBalanceLabel.Text = "&Saldo iniziale";
            // 
            // initialBalanceInput
            // 
            initialBalanceInput.DecimalPlaces = 2;
            initialBalanceInput.Location = new Point(24, 212);
            initialBalanceInput.Name = "initialBalanceInput";
            initialBalanceInput.Size = new Size(180, 23);
            initialBalanceInput.TabIndex = 9;
            initialBalanceInput.TextAlign = HorizontalAlignment.Right;
            initialBalanceInput.ThousandsSeparator = true;
            initialBalanceInput.ValueChanged += InitialBalanceInputValueChanged;
            // 
            // initialBalanceErrorLabel
            // 
            initialBalanceErrorLabel.AutoSize = true;
            initialBalanceErrorLabel.ForeColor = Color.Firebrick;
            initialBalanceErrorLabel.Location = new Point(24, 238);
            initialBalanceErrorLabel.Name = "initialBalanceErrorLabel";
            initialBalanceErrorLabel.Size = new Size(0, 15);
            initialBalanceErrorLabel.TabIndex = 10;
            // 
            // generalErrorLabel
            // 
            generalErrorLabel.AutoEllipsis = true;
            generalErrorLabel.ForeColor = Color.Firebrick;
            generalErrorLabel.Location = new Point(24, 267);
            generalErrorLabel.Name = "generalErrorLabel";
            generalErrorLabel.Size = new Size(420, 44);
            generalErrorLabel.TabIndex = 11;
            // 
            // createButton
            // 
            createButton.Location = new Point(288, 326);
            createButton.Name = "createButton";
            createButton.Size = new Size(75, 28);
            createButton.TabIndex = 12;
            createButton.Text = "&Crea";
            createButton.UseVisualStyleBackColor = true;
            createButton.Click += CreateButtonClick;
            // 
            // cancelButton
            // 
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(369, 326);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 28);
            cancelButton.TabIndex = 13;
            cancelButton.Text = "&Annulla";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // NewContoDialog
            // 
            AcceptButton = createButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(468, 378);
            Controls.Add(cancelButton);
            Controls.Add(createButton);
            Controls.Add(generalErrorLabel);
            Controls.Add(initialBalanceErrorLabel);
            Controls.Add(initialBalanceInput);
            Controls.Add(initialBalanceLabel);
            Controls.Add(displayNameErrorLabel);
            Controls.Add(displayNameInput);
            Controls.Add(displayNameLabel);
            Controls.Add(namePreviewValue);
            Controls.Add(namePreviewLabel);
            Controls.Add(nameErrorLabel);
            Controls.Add(nameInput);
            Controls.Add(nameLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "NewContoDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Nuovo conto";
            ((System.ComponentModel.ISupportInitialize)initialBalanceInput).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label nameLabel;
        private TextBox nameInput;
        private Label nameErrorLabel;
        private Label namePreviewLabel;
        private Label namePreviewValue;
        private Label displayNameLabel;
        private TextBox displayNameInput;
        private Label displayNameErrorLabel;
        private Label initialBalanceLabel;
        private NumericUpDown initialBalanceInput;
        private Label initialBalanceErrorLabel;
        private Label generalErrorLabel;
        private Button createButton;
        private Button cancelButton;
    }
}

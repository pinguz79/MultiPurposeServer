namespace Finance.Desktop
{
    partial class CategoriaDialog
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
            nameTextBox = new TextBox();
            displayNameLabel = new Label();
            displayNameTextBox = new TextBox();
            saveButton = new Button();
            cancelButton = new Button();
            SuspendLayout();
            // 
            // nameLabel
            // 
            nameLabel.AutoSize = true;
            nameLabel.Location = new Point(18, 18);
            nameLabel.Name = "nameLabel";
            nameLabel.Size = new Size(40, 15);
            nameLabel.TabIndex = 0;
            nameLabel.Text = "&Nome";
            // 
            // nameTextBox
            // 
            nameTextBox.Location = new Point(18, 40);
            nameTextBox.Name = "nameTextBox";
            nameTextBox.Size = new Size(350, 23);
            nameTextBox.TabIndex = 1;
            nameTextBox.TextChanged += NameTextBoxTextChanged;
            // 
            // displayNameLabel
            // 
            displayNameLabel.AutoSize = true;
            displayNameLabel.Location = new Point(18, 78);
            displayNameLabel.Name = "displayNameLabel";
            displayNameLabel.Size = new Size(105, 15);
            displayNameLabel.TabIndex = 2;
            displayNameLabel.Text = "Nome &visualizzato";
            // 
            // displayNameTextBox
            // 
            displayNameTextBox.Location = new Point(18, 100);
            displayNameTextBox.Name = "displayNameTextBox";
            displayNameTextBox.Size = new Size(350, 23);
            displayNameTextBox.TabIndex = 3;
            displayNameTextBox.TextChanged += DisplayNameTextBoxTextChanged;
            // 
            // saveButton
            // 
            saveButton.Location = new Point(212, 148);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(75, 28);
            saveButton.TabIndex = 4;
            saveButton.Text = "&Salva";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += SaveButtonClick;
            // 
            // cancelButton
            // 
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(293, 148);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 28);
            cancelButton.TabIndex = 5;
            cancelButton.Text = "&Annulla";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // CategoriaDialog
            // 
            AcceptButton = saveButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(386, 194);
            Controls.Add(cancelButton);
            Controls.Add(saveButton);
            Controls.Add(displayNameTextBox);
            Controls.Add(displayNameLabel);
            Controls.Add(nameTextBox);
            Controls.Add(nameLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CategoriaDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Categoria";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label nameLabel;
        private TextBox nameTextBox;
        private Label displayNameLabel;
        private TextBox displayNameTextBox;
        private Button saveButton;
        private Button cancelButton;
    }
}

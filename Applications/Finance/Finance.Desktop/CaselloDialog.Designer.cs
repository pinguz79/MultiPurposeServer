namespace Finance.Desktop
{
    partial class CaselloDialog
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
            saveButton = new Button();
            cancelButton = new Button();
            SuspendLayout();
            // 
            // nameLabel
            // 
            nameLabel.Location = new Point(18, 16);
            nameLabel.Name = "nameLabel";
            nameLabel.Size = new Size(300, 20);
            nameLabel.TabIndex = 0;
            nameLabel.Text = "&Nome della stazione";
            // 
            // nameInput
            // 
            nameInput.Location = new Point(18, 40);
            nameInput.Name = "nameInput";
            nameInput.Size = new Size(342, 23);
            nameInput.TabIndex = 1;
            nameInput.TextChanged += NameChanged;
            // 
            // saveButton
            // 
            saveButton.DialogResult = DialogResult.OK;
            saveButton.Enabled = false;
            saveButton.Location = new Point(180, 95);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(86, 28);
            saveButton.TabIndex = 2;
            saveButton.Text = "&Aggiungi";
            // 
            // cancelButton
            // 
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(274, 95);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(86, 28);
            cancelButton.TabIndex = 3;
            cancelButton.Text = "&Annulla";
            // 
            // CaselloDialog
            // 
            AcceptButton = saveButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(380, 150);
            Controls.Add(cancelButton);
            Controls.Add(saveButton);
            Controls.Add(nameInput);
            Controls.Add(nameLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CaselloDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Nuova stazione";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label nameLabel;
        private TextBox nameInput;
        private Button saveButton;
        private Button cancelButton;
    }
}


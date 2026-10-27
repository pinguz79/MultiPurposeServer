namespace Skating.Desktop.Manifestations
{
    partial class NewManifestationDialog
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
            createButton = new Button();
            cancelButton = new Button();
            SuspendLayout();
            //
            // nameLabel
            //
            nameLabel.AutoSize = true;
            nameLabel.Location = new Point(16, 16);
            nameLabel.Name = "nameLabel";
            nameLabel.Text = "Nome della manifestazione";
            //
            // nameTextBox
            //
            nameTextBox.Location = new Point(16, 40);
            nameTextBox.Name = "nameTextBox";
            nameTextBox.Size = new Size(448, 23);
            nameTextBox.TabIndex = 0;
            //
            // createButton
            //
            createButton.Location = new Point(264, 88);
            createButton.Name = "createButton";
            createButton.Size = new Size(96, 28);
            createButton.TabIndex = 1;
            createButton.Text = "Crea...";
            createButton.Click += CreateButtonClick;
            //
            // cancelButton
            //
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(368, 88);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(96, 28);
            cancelButton.TabIndex = 2;
            cancelButton.Text = "Annulla";
            //
            // NewManifestationDialog
            //
            AcceptButton = createButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(480, 132);
            Controls.Add(nameLabel);
            Controls.Add(nameTextBox);
            Controls.Add(createButton);
            Controls.Add(cancelButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "NewManifestationDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Nuova manifestazione";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label nameLabel;
        private TextBox nameTextBox;
        private Button createButton;
        private Button cancelButton;
    }
}

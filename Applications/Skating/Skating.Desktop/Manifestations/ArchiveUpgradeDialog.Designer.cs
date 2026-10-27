namespace Skating.Desktop.Manifestations
{
    partial class ArchiveUpgradeDialog
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
            messageLabel = new Label();
            backupButton = new Button();
            skipBackupButton = new Button();
            cancelButton = new Button();
            SuspendLayout();
            //
            // messageLabel
            //
            messageLabel.Location = new Point(16, 16);
            messageLabel.Name = "messageLabel";
            messageLabel.Size = new Size(580, 120);
            messageLabel.Text = "L'archivio richiede un aggiornamento per i premi assoluti.\r\n\r\nCon backup: viene conservata una copia .before-v8-*.bak accanto all'originale.\r\n\r\nSenza backup: l'archivio viene aggiornato senza una copia preventiva recuperabile.";
            //
            // backupButton
            //
            backupButton.DialogResult = DialogResult.Yes;
            backupButton.Location = new Point(16, 152);
            backupButton.Name = "backupButton";
            backupButton.Size = new Size(190, 34);
            backupButton.TabIndex = 0;
            backupButton.Text = "Aggiorna con backup";
            //
            // skipBackupButton
            //
            skipBackupButton.DialogResult = DialogResult.No;
            skipBackupButton.Location = new Point(218, 152);
            skipBackupButton.Name = "skipBackupButton";
            skipBackupButton.Size = new Size(220, 34);
            skipBackupButton.TabIndex = 1;
            skipBackupButton.Text = "Continua senza backup";
            //
            // cancelButton
            //
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(450, 152);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(146, 34);
            cancelButton.TabIndex = 2;
            cancelButton.Text = "Annulla";
            //
            // ArchiveUpgradeDialog
            //
            AcceptButton = backupButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(612, 202);
            Controls.Add(messageLabel);
            Controls.Add(backupButton);
            Controls.Add(skipBackupButton);
            Controls.Add(cancelButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ArchiveUpgradeDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Aggiornamento archivio";
            ResumeLayout(false);
        }

        #endregion

        private Label messageLabel;
        private Button backupButton;
        private Button skipBackupButton;
        private Button cancelButton;
    }
}

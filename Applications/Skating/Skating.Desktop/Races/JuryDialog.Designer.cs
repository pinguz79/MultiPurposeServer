namespace Skating.Desktop.Races
{
    partial class JuryDialog
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
            choreographiesLabel = new Label();
            judgesLabel = new Label();
            hintLabel = new Label();
            choreographiesList = new ListBox();
            judgesList = new CheckedListBox();
            saveButton = new Button();
            defaultButton = new Button();
            cancelButton = new Button();
            SuspendLayout();
            choreographiesLabel.AutoSize = true;
            choreographiesLabel.Location = new Point(16, 16);
            choreographiesLabel.Text = "Coreografie partecipanti";
            choreographiesList.Location = new Point(16, 40);
            choreographiesList.Size = new Size(350, 340);
            choreographiesList.Name = "choreographiesList";
            choreographiesList.HorizontalScrollbar = true;
            choreographiesList.TabIndex = 0;
            judgesLabel.AutoSize = true;
            judgesLabel.Location = new Point(390, 16);
            judgesLabel.Text = "Giudici della gara (almeno tre)";
            judgesList.Location = new Point(390, 40);
            judgesList.Size = new Size(350, 340);
            judgesList.Name = "judgesList";
            judgesList.CheckOnClick = true;
            judgesList.HorizontalScrollbar = true;
            judgesList.TabIndex = 1;
            hintLabel.Location = new Point(16, 395);
            hintLabel.Size = new Size(724, 50);
            hintLabel.Text = "Salva giuria conserva una selezione esplicita. Usa tutti i giudici ripristina la giuria predefinita, includendo anche i giudici aggiunti in seguito.";
            saveButton.Location = new Point(16, 460);
            saveButton.Size = new Size(180, 32);
            saveButton.Name = "saveButton";
            saveButton.Text = "Salva giuria";
            saveButton.TabIndex = 2;
            saveButton.Click += SaveButtonClick;
            defaultButton.Location = new Point(210, 460);
            defaultButton.Size = new Size(210, 32);
            defaultButton.Name = "defaultButton";
            defaultButton.Text = "Usa tutti i giudici";
            defaultButton.TabIndex = 3;
            defaultButton.Click += DefaultButtonClick;
            cancelButton.Location = new Point(620, 460);
            cancelButton.Size = new Size(120, 32);
            cancelButton.Name = "cancelButton";
            cancelButton.Text = "Annulla";
            cancelButton.TabIndex = 4;
            cancelButton.DialogResult = DialogResult.Cancel;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(760, 510);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            CancelButton = cancelButton;
            Name = "JuryDialog";
            StartPosition = FormStartPosition.CenterParent;
            Controls.Add(choreographiesLabel);
            Controls.Add(judgesLabel);
            Controls.Add(hintLabel);
            Controls.Add(choreographiesList);
            Controls.Add(judgesList);
            Controls.Add(saveButton);
            Controls.Add(defaultButton);
            Controls.Add(cancelButton);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label choreographiesLabel;
        private Label judgesLabel;
        private Label hintLabel;
        private ListBox choreographiesList;
        private CheckedListBox judgesList;
        private Button saveButton;
        private Button defaultButton;
        private Button cancelButton;
    }
}

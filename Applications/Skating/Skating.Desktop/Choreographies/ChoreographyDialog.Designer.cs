namespace Skating.Desktop.Choreographies
{
    partial class ChoreographyDialog
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
            nameTextBox = new TextBox();
            schoolCombo = new ComboBox();
            disciplineCombo = new ComboBox();
            categoryCombo = new ComboBox();
            levelCombo = new ComboBox();
            compositionCombo = new ComboBox();
            minutesInput = new NumericUpDown();
            secondsInput = new NumericUpDown();
            athletesList = new CheckedListBox();
            athleteFilterTextBox = new TextBox();
            addAthleteButton = new Button();
            athleteCountLabel = new Label();
            hintLabel = new Label();
            saveButton = new Button();
            cancelButton = new Button();
            nameLabel = new Label();
            schoolLabel = new Label();
            disciplineLabel = new Label();
            categoryLabel = new Label();
            levelLabel = new Label();
            compositionLabel = new Label();
            durationLabel = new Label();
            athletesLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)minutesInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)secondsInput).BeginInit();
            SuspendLayout();
            //
            // nameLabel
            //
            nameLabel.AutoSize = true;
            nameLabel.Location = new Point(16, 16);
            nameLabel.Name = "nameLabel";
            nameLabel.Text = "Nome coreografia";
            //
            // schoolLabel
            //
            schoolLabel.AutoSize = true;
            schoolLabel.Location = new Point(16, 78);
            schoolLabel.Name = "schoolLabel";
            schoolLabel.Text = "Scuola rappresentata";
            //
            // disciplineLabel
            //
            disciplineLabel.AutoSize = true;
            disciplineLabel.Location = new Point(450, 78);
            disciplineLabel.Name = "disciplineLabel";
            disciplineLabel.Text = "Disciplina";
            //
            // categoryLabel
            //
            categoryLabel.AutoSize = true;
            categoryLabel.Location = new Point(16, 140);
            categoryLabel.Name = "categoryLabel";
            categoryLabel.Text = "Categoria";
            //
            // levelLabel
            //
            levelLabel.AutoSize = true;
            levelLabel.Location = new Point(450, 140);
            levelLabel.Name = "levelLabel";
            levelLabel.Text = "Livello";
            //
            // compositionLabel
            //
            compositionLabel.AutoSize = true;
            compositionLabel.Location = new Point(16, 202);
            compositionLabel.Name = "compositionLabel";
            compositionLabel.Text = "Composizione";
            //
            // durationLabel
            //
            durationLabel.AutoSize = true;
            durationLabel.Location = new Point(450, 202);
            durationLabel.Name = "durationLabel";
            durationLabel.Text = "Durata: minuti / secondi";
            //
            // athletesLabel
            //
            athletesLabel.AutoSize = true;
            athletesLabel.Location = new Point(16, 264);
            athletesLabel.Name = "athletesLabel";
            athletesLabel.Text = "Atleti partecipanti";
            nameTextBox.Location = new Point(16, 38);
            nameTextBox.Size = new Size(840, 23);
            nameTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            nameTextBox.Name = "nameTextBox";
            nameTextBox.TabIndex = 0;
            nameTextBox.TextChanged += InputChanged;
            schoolCombo.Location = new Point(16, 100);
            schoolCombo.Size = new Size(406, 23);
            schoolCombo.Name = "schoolCombo";
            schoolCombo.TabIndex = 1;
            schoolCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            schoolCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            schoolCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            schoolCombo.FormattingEnabled = true;
            schoolCombo.SelectedIndexChanged += InputChanged;
            disciplineCombo.Location = new Point(450, 100);
            disciplineCombo.Size = new Size(406, 23);
            disciplineCombo.Name = "disciplineCombo";
            disciplineCombo.TabIndex = 2;
            disciplineCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            disciplineCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            disciplineCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            disciplineCombo.FormattingEnabled = true;
            disciplineCombo.SelectedIndexChanged += InputChanged;
            categoryCombo.Location = new Point(16, 162);
            categoryCombo.Size = new Size(406, 23);
            categoryCombo.Name = "categoryCombo";
            categoryCombo.TabIndex = 3;
            categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            categoryCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            categoryCombo.FormattingEnabled = true;
            categoryCombo.SelectedIndexChanged += InputChanged;
            levelCombo.Location = new Point(450, 162);
            levelCombo.Size = new Size(406, 23);
            levelCombo.Name = "levelCombo";
            levelCombo.TabIndex = 4;
            levelCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            levelCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            levelCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            levelCombo.FormattingEnabled = true;
            levelCombo.SelectedIndexChanged += InputChanged;
            compositionCombo.Location = new Point(16, 224);
            compositionCombo.Size = new Size(406, 23);
            compositionCombo.Name = "compositionCombo";
            compositionCombo.TabIndex = 5;
            compositionCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            compositionCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            compositionCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            compositionCombo.FormattingEnabled = true;
            compositionCombo.SelectedIndexChanged += InputChanged;
            minutesInput.Location = new Point(450, 224);
            minutesInput.Size = new Size(110, 23);
            minutesInput.Maximum = 35791393;
            minutesInput.Name = "minutesInput";
            minutesInput.TabIndex = 6;
            minutesInput.ValueChanged += InputChanged;
            secondsInput.Location = new Point(580, 224);
            secondsInput.Size = new Size(110, 23);
            secondsInput.Maximum = 59;
            secondsInput.Name = "secondsInput";
            secondsInput.TabIndex = 7;
            secondsInput.ValueChanged += InputChanged;
            athleteFilterTextBox.Location = new Point(16, 286);
            athleteFilterTextBox.Size = new Size(660, 23);
            athleteFilterTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            athleteFilterTextBox.Name = "athleteFilterTextBox";
            athleteFilterTextBox.PlaceholderText = "Filtra per nome e/o cognome...";
            athleteFilterTextBox.TabIndex = 8;
            athleteFilterTextBox.TextChanged += AthleteFilterTextChanged;
            addAthleteButton.Location = new Point(692, 282);
            addAthleteButton.Size = new Size(164, 30);
            addAthleteButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            addAthleteButton.Name = "addAthleteButton";
            addAthleteButton.Text = "Aggiungi atleta...";
            addAthleteButton.TabIndex = 9;
            addAthleteButton.Click += AddAthleteButtonClick;
            athleteCountLabel.Location = new Point(16, 318);
            athleteCountLabel.Size = new Size(840, 22);
            athleteCountLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            athleteCountLabel.Name = "athleteCountLabel";
            athletesList.Location = new Point(16, 342);
            athletesList.Size = new Size(840, 218);
            athletesList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            athletesList.Name = "athletesList";
            athletesList.TabIndex = 10;
            athletesList.CheckOnClick = true;
            athletesList.HorizontalScrollbar = true;
            athletesList.ItemCheck += AthletesListItemCheck;
            hintLabel.Location = new Point(16, 572);
            hintLabel.Size = new Size(840, 48);
            hintLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            hintLabel.Name = "hintLabel";
            saveButton.Location = new Point(616, 632);
            saveButton.Size = new Size(116, 32);
            saveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            saveButton.Name = "saveButton";
            saveButton.TabIndex = 11;
            saveButton.Text = "Salva";
            saveButton.Click += SaveButtonClick;
            cancelButton.Location = new Point(740, 632);
            cancelButton.Size = new Size(116, 32);
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cancelButton.Name = "cancelButton";
            cancelButton.TabIndex = 12;
            cancelButton.Text = "Annulla";
            cancelButton.DialogResult = DialogResult.Cancel;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(876, 680);
            MinimumSize = new Size(892, 719);
            CancelButton = cancelButton;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Name = "ChoreographyDialog";
            StartPosition = FormStartPosition.CenterParent;
            Shown += ChoreographyDialogShown;
            FormClosing += ChoreographyDialogFormClosing;
            Controls.Add(nameTextBox);
            Controls.Add(schoolCombo);
            Controls.Add(disciplineCombo);
            Controls.Add(categoryCombo);
            Controls.Add(levelCombo);
            Controls.Add(compositionCombo);
            Controls.Add(minutesInput);
            Controls.Add(secondsInput);
            Controls.Add(athletesList);
            Controls.Add(athleteFilterTextBox);
            Controls.Add(addAthleteButton);
            Controls.Add(athleteCountLabel);
            Controls.Add(hintLabel);
            Controls.Add(saveButton);
            Controls.Add(cancelButton);
            Controls.Add(nameLabel);
            Controls.Add(schoolLabel);
            Controls.Add(disciplineLabel);
            Controls.Add(categoryLabel);
            Controls.Add(levelLabel);
            Controls.Add(compositionLabel);
            Controls.Add(durationLabel);
            Controls.Add(athletesLabel);
            ((System.ComponentModel.ISupportInitialize)minutesInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)secondsInput).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox nameTextBox;
        private ComboBox schoolCombo;
        private ComboBox disciplineCombo;
        private ComboBox categoryCombo;
        private ComboBox levelCombo;
        private ComboBox compositionCombo;
        private NumericUpDown minutesInput;
        private NumericUpDown secondsInput;
        private CheckedListBox athletesList;
        private TextBox athleteFilterTextBox;
        private Button addAthleteButton;
        private Label athleteCountLabel;
        private Label hintLabel;
        private Button saveButton;
        private Button cancelButton;
        private Label nameLabel;
        private Label schoolLabel;
        private Label disciplineLabel;
        private Label categoryLabel;
        private Label levelLabel;
        private Label compositionLabel;
        private Label durationLabel;
        private Label athletesLabel;
    }
}

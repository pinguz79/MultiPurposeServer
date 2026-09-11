namespace Finance.Desktop
{
    partial class PianificazioneDialog
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
            frequencyLabel = new Label();
            frequencyComboBox = new ComboBox();
            intervalLabel = new Label();
            intervalInput = new NumericUpDown();
            weekDayLabel = new Label();
            weekDayComboBox = new ComboBox();
            monthlyPanel = new Panel();
            weeklyPanel = new Panel();
            monthlyPanel.SuspendLayout();
            weeklyPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)intervalInput).BeginInit();
            contoLabel = new Label();
            contoComboBox = new ComboBox();
            descriptionLabel = new Label();
            descriptionTextBox = new TextBox();
            movimentoDescriptionLabel = new Label();
            movimentoDescriptionTextBox = new TextBox();
            formulaLabel = new Label();
            formulaTextBox = new TextBox();
            validFromLabel = new Label();
            validFromInput = new DateTimePicker();
            validToLabel = new Label();
            validToInput = new DateTimePicker();
            dayLabel = new Label();
            dayInput = new NumericUpDown();
            endOfMonthCheckBox = new CheckBox();
            categoryLabel = new Label();
            categoryComboBox = new ComboBox();
            previewGrid = new DataGridView();
            summaryLabel = new Label();
            errorLabel = new Label();
            createButton = new Button();
            cancelButton = new Button();
            ((System.ComponentModel.ISupportInitialize)dayInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)previewGrid).BeginInit();
            SuspendLayout();
            // 
            // contoLabel
            // 
            contoLabel.AutoSize = true;
            contoLabel.Location = new Point(18, 18);
            contoLabel.Name = "contoLabel";
            contoLabel.Size = new Size(41, 15);
            contoLabel.TabIndex = 0;
            contoLabel.Text = "&Conto";
            // 
            // contoComboBox
            // 
            contoComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            contoComboBox.FormattingEnabled = true;
            contoComboBox.Location = new Point(18, 40);
            contoComboBox.Name = "contoComboBox";
            contoComboBox.Size = new Size(270, 23);
            contoComboBox.TabIndex = 1;
            contoComboBox.SelectedValueChanged += InputValueChanged;
            // 
            // descriptionLabel
            // 
            descriptionLabel.AutoSize = true;
            descriptionLabel.Location = new Point(306, 18);
            descriptionLabel.Name = "descriptionLabel";
            descriptionLabel.Size = new Size(134, 15);
            descriptionLabel.TabIndex = 2;
            descriptionLabel.Text = "Descrizione &pianificazione";
            // 
            // descriptionTextBox
            // 
            descriptionTextBox.Location = new Point(306, 40);
            descriptionTextBox.Name = "descriptionTextBox";
            descriptionTextBox.Size = new Size(340, 23);
            descriptionTextBox.TabIndex = 3;
            descriptionTextBox.TextChanged += InputValueChanged;
            // 
            // movimentoDescriptionLabel
            // 
            movimentoDescriptionLabel.AutoSize = true;
            movimentoDescriptionLabel.Location = new Point(18, 78);
            movimentoDescriptionLabel.Name = "movimentoDescriptionLabel";
            movimentoDescriptionLabel.Size = new Size(130, 15);
            movimentoDescriptionLabel.TabIndex = 4;
            movimentoDescriptionLabel.Text = "Descrizione &movimento";
            // 
            // movimentoDescriptionTextBox
            // 
            movimentoDescriptionTextBox.Location = new Point(18, 100);
            movimentoDescriptionTextBox.Name = "movimentoDescriptionTextBox";
            movimentoDescriptionTextBox.Size = new Size(270, 23);
            movimentoDescriptionTextBox.TabIndex = 5;
            movimentoDescriptionTextBox.TextChanged += InputValueChanged;
            // 
            // formulaLabel
            // 
            formulaLabel.AutoSize = true;
            formulaLabel.Location = new Point(306, 78);
            formulaLabel.Name = "formulaLabel";
            formulaLabel.Size = new Size(50, 15);
            formulaLabel.TabIndex = 6;
            formulaLabel.Text = "&Formula";
            // 
            // formulaTextBox
            // 
            formulaTextBox.Location = new Point(306, 100);
            formulaTextBox.Name = "formulaTextBox";
            formulaTextBox.Size = new Size(340, 23);
            formulaTextBox.TabIndex = 7;
            formulaTextBox.TextChanged += InputValueChanged;
            // 
            // validFromLabel
            // 
            validFromLabel.AutoSize = true;
            validFromLabel.Location = new Point(18, 138);
            validFromLabel.Name = "validFromLabel";
            validFromLabel.Size = new Size(23, 15);
            validFromLabel.TabIndex = 8;
            validFromLabel.Text = "&Dal";
            // 
            // validFromInput
            // 
            validFromInput.Format = DateTimePickerFormat.Short;
            validFromInput.Location = new Point(18, 160);
            validFromInput.Name = "validFromInput";
            validFromInput.Size = new Size(128, 23);
            validFromInput.TabIndex = 9;
            validFromInput.ValueChanged += InputValueChanged;
            // 
            // validToLabel
            // 
            validToLabel.AutoSize = true;
            validToLabel.Location = new Point(164, 138);
            validToLabel.Name = "validToLabel";
            validToLabel.Size = new Size(17, 15);
            validToLabel.TabIndex = 10;
            validToLabel.Text = "&Al";
            // 
            // validToInput
            // 
            validToInput.Format = DateTimePickerFormat.Short;
            validToInput.Location = new Point(164, 160);
            validToInput.Name = "validToInput";
            validToInput.Size = new Size(124, 23);
            validToInput.TabIndex = 11;
            validToInput.ValueChanged += InputValueChanged;
            // 
            // dayLabel
            // 
            dayLabel.AutoSize = true;
            dayLabel.Location = new Point(0, 0);
            dayLabel.Name = "dayLabel";
            dayLabel.Size = new Size(43, 15);
            dayLabel.TabIndex = 12;
            dayLabel.Text = "&Giorno";
            // 
            // dayInput
            // 
            dayInput.Location = new Point(0, 22);
            dayInput.Maximum = 31;
            dayInput.Minimum = 1;
            dayInput.Name = "dayInput";
            dayInput.Size = new Size(70, 23);
            dayInput.TabIndex = 13;
            dayInput.Value = 1;
            dayInput.ValueChanged += InputValueChanged;
            // 
            // endOfMonthCheckBox
            // 
            endOfMonthCheckBox.AutoSize = true;
            endOfMonthCheckBox.Location = new Point(88, 24);
            endOfMonthCheckBox.Name = "endOfMonthCheckBox";
            endOfMonthCheckBox.Size = new Size(99, 19);
            endOfMonthCheckBox.TabIndex = 14;
            endOfMonthCheckBox.Text = "&Fine del mese";
            endOfMonthCheckBox.UseVisualStyleBackColor = true;
            endOfMonthCheckBox.CheckedChanged += EndOfMonthCheckedChanged;
            // 
            // categoryLabel
            // 
            categoryLabel.AutoSize = true;
            categoryLabel.Location = new Point(510, 138);
            categoryLabel.Name = "categoryLabel";
            categoryLabel.Size = new Size(58, 15);
            categoryLabel.TabIndex = 15;
            categoryLabel.Text = "&Categoria";
            // 
            // categoryComboBox
            // 
            categoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryComboBox.FormattingEnabled = true;
            categoryComboBox.Location = new Point(510, 160);
            categoryComboBox.Name = "categoryComboBox";
            categoryComboBox.Size = new Size(136, 23);
            categoryComboBox.TabIndex = 16;
            categoryComboBox.SelectedValueChanged += InputValueChanged;
            // 
            // frequencyLabel
            // 
            frequencyLabel.AutoSize = true;
            frequencyLabel.Location = new Point(306, 138);
            frequencyLabel.Name = "frequencyLabel";
            frequencyLabel.Text = "Ca&denza";
            frequencyLabel.TabIndex = 12;
            // 
            // frequencyComboBox
            // 
            frequencyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            frequencyComboBox.Items.AddRange(new object[] { "Mensile", "Settimanale" });
            frequencyComboBox.Location = new Point(306, 160);
            frequencyComboBox.Name = "frequencyComboBox";
            frequencyComboBox.Size = new Size(180, 23);
            frequencyComboBox.TabIndex = 13;
            frequencyComboBox.SelectedIndexChanged += FrequencyChanged;
            // 
            // intervalLabel
            // 
            intervalLabel.AutoSize = true;
            intervalLabel.Location = new Point(0, 0);
            intervalLabel.Name = "intervalLabel";
            intervalLabel.Text = "Ogni N &settimane";
            intervalLabel.TabIndex = 19;
            // 
            // intervalInput
            // 
            intervalInput.Location = new Point(0, 22);
            intervalInput.Minimum = 1;
            intervalInput.Maximum = 2147483647;
            intervalInput.Value = 1;
            intervalInput.Name = "intervalInput";
            intervalInput.Size = new Size(90, 23);
            intervalInput.TabIndex = 20;
            intervalInput.ValueChanged += InputValueChanged;
            // 
            // weekDayLabel
            // 
            weekDayLabel.AutoSize = true;
            weekDayLabel.Location = new Point(140, 0);
            weekDayLabel.Name = "weekDayLabel";
            weekDayLabel.Text = "Giorno della se&ttimana";
            weekDayLabel.TabIndex = 21;
            // 
            // weekDayComboBox
            // 
            weekDayComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            weekDayComboBox.Items.AddRange(new object[] { "Lunedì", "Martedì", "Mercoledì", "Giovedì", "Venerdì", "Sabato", "Domenica" });
            weekDayComboBox.Location = new Point(140, 22);
            weekDayComboBox.Name = "weekDayComboBox";
            weekDayComboBox.Size = new Size(160, 23);
            weekDayComboBox.TabIndex = 22;
            weekDayComboBox.SelectedIndexChanged += InputValueChanged;
            //
            // monthlyPanel
            //
            monthlyPanel.Controls.Add(dayLabel);
            monthlyPanel.Controls.Add(dayInput);
            monthlyPanel.Controls.Add(endOfMonthCheckBox);
            monthlyPanel.Location = new Point(18, 198);
            monthlyPanel.Name = "monthlyPanel";
            monthlyPanel.Size = new Size(628, 54);
            monthlyPanel.TabIndex = 17;
            //
            // weeklyPanel
            //
            weeklyPanel.Controls.Add(intervalLabel);
            weeklyPanel.Controls.Add(intervalInput);
            weeklyPanel.Controls.Add(weekDayLabel);
            weeklyPanel.Controls.Add(weekDayComboBox);
            weeklyPanel.Location = new Point(18, 198);
            weeklyPanel.Name = "weeklyPanel";
            weeklyPanel.Size = new Size(628, 54);
            weeklyPanel.TabIndex = 18;
            weeklyPanel.Visible = false;
            // 
            // previewGrid
            // 
            previewGrid.AllowUserToAddRows = false;
            previewGrid.AllowUserToDeleteRows = false;
            previewGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            previewGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            previewGrid.Location = new Point(18, 270);
            previewGrid.MultiSelect = false;
            previewGrid.Name = "previewGrid";
            previewGrid.ReadOnly = true;
            previewGrid.RowHeadersVisible = false;
            previewGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            previewGrid.Size = new Size(628, 270);
            previewGrid.TabIndex = 23;
            // 
            // summaryLabel
            // 
            summaryLabel.AutoSize = true;
            summaryLabel.Location = new Point(18, 554);
            summaryLabel.Name = "summaryLabel";
            summaryLabel.Size = new Size(74, 15);
            summaryLabel.TabIndex = 24;
            summaryLabel.Text = "0 occorrenze";
            // 
            // errorLabel
            // 
            errorLabel.ForeColor = Color.Firebrick;
            errorLabel.Location = new Point(18, 576);
            errorLabel.Name = "errorLabel";
            errorLabel.Size = new Size(460, 40);
            errorLabel.TabIndex = 25;
            // 
            // createButton
            // 
            createButton.Enabled = false;
            createButton.Location = new Point(490, 576);
            createButton.Name = "createButton";
            createButton.Size = new Size(75, 28);
            createButton.TabIndex = 26;
            createButton.Text = "&Crea";
            createButton.UseVisualStyleBackColor = true;
            createButton.Click += CreateButtonClick;
            // 
            // cancelButton
            // 
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(571, 576);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 28);
            cancelButton.TabIndex = 27;
            cancelButton.Text = "&Annulla";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // PianificazioneDialog
            // 
            AcceptButton = createButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(664, 626);
            Controls.Add(frequencyLabel);
            Controls.Add(frequencyComboBox);
            Controls.Add(monthlyPanel);
            Controls.Add(weeklyPanel);
            Controls.Add(cancelButton);
            Controls.Add(createButton);
            Controls.Add(categoryComboBox);
            Controls.Add(categoryLabel);
            Controls.Add(errorLabel);
            Controls.Add(summaryLabel);
            Controls.Add(previewGrid);
            Controls.Add(validToInput);
            Controls.Add(validToLabel);
            Controls.Add(validFromInput);
            Controls.Add(validFromLabel);
            Controls.Add(formulaTextBox);
            Controls.Add(formulaLabel);
            Controls.Add(movimentoDescriptionTextBox);
            Controls.Add(movimentoDescriptionLabel);
            Controls.Add(descriptionTextBox);
            Controls.Add(descriptionLabel);
            Controls.Add(contoComboBox);
            Controls.Add(contoLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PianificazioneDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Crea pianificazione";
            ((System.ComponentModel.ISupportInitialize)dayInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)previewGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)intervalInput).EndInit();
            monthlyPanel.ResumeLayout(false);
            monthlyPanel.PerformLayout();
            weeklyPanel.ResumeLayout(false);
            weeklyPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label frequencyLabel;
        private Panel monthlyPanel;
        private Panel weeklyPanel;
        private ComboBox frequencyComboBox;
        private Label intervalLabel;
        private NumericUpDown intervalInput;
        private Label weekDayLabel;
        private ComboBox weekDayComboBox;
        private Label contoLabel;
        private ComboBox contoComboBox;
        private Label descriptionLabel;
        private TextBox descriptionTextBox;
        private Label movimentoDescriptionLabel;
        private TextBox movimentoDescriptionTextBox;
        private Label formulaLabel;
        private TextBox formulaTextBox;
        private Label validFromLabel;
        private DateTimePicker validFromInput;
        private Label validToLabel;
        private DateTimePicker validToInput;
        private Label dayLabel;
        private NumericUpDown dayInput;
        private CheckBox endOfMonthCheckBox;
        private Label categoryLabel;
        private ComboBox categoryComboBox;
        private DataGridView previewGrid;
        private Label summaryLabel;
        private Label errorLabel;
        private Button createButton;
        private Button cancelButton;
    }
}

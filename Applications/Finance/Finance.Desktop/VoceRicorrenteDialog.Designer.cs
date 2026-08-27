namespace Finance.Desktop
{
    partial class VoceRicorrenteDialog
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
            definitionsGrid = new DataGridView();
            addButton = new Button();
            editButton = new Button();
            deleteButton = new Button();
            upButton = new Button();
            downButton = new Button();
            coveragePanel = new CoveragePanel();
            saveButton = new Button();
            cancelButton = new Button();
            ((System.ComponentModel.ISupportInitialize)definitionsGrid).BeginInit();
            SuspendLayout();
            // 
            // nameLabel
            // 
            nameLabel.AutoSize = true;
            nameLabel.Location = new Point(16, 16);
            nameLabel.Name = "nameLabel";
            nameLabel.Size = new Size(40, 15);
            nameLabel.TabIndex = 0;
            nameLabel.Text = "&Nome";
            // 
            // nameTextBox
            // 
            nameTextBox.Location = new Point(16, 38);
            nameTextBox.Name = "nameTextBox";
            nameTextBox.Size = new Size(360, 23);
            nameTextBox.TabIndex = 1;
            // 
            // definitionsGrid
            // 
            definitionsGrid.AllowUserToAddRows = false;
            definitionsGrid.AllowUserToDeleteRows = false;
            definitionsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            definitionsGrid.Location = new Point(16, 78);
            definitionsGrid.MultiSelect = false;
            definitionsGrid.Name = "definitionsGrid";
            definitionsGrid.ReadOnly = true;
            definitionsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            definitionsGrid.Size = new Size(720, 210);
            definitionsGrid.TabIndex = 2;
            // 
            // addButton
            // 
            addButton.Location = new Point(16, 298);
            addButton.Name = "addButton";
            addButton.Size = new Size(75, 23);
            addButton.TabIndex = 3;
            addButton.Text = "&Aggiungi...";
            addButton.UseVisualStyleBackColor = true;
            addButton.Click += AddButtonClick;
            // 
            // editButton
            // 
            editButton.Location = new Point(97, 298);
            editButton.Name = "editButton";
            editButton.Size = new Size(75, 23);
            editButton.TabIndex = 4;
            editButton.Text = "&Modifica...";
            editButton.UseVisualStyleBackColor = true;
            editButton.Click += EditButtonClick;
            // 
            // deleteButton
            // 
            deleteButton.Location = new Point(178, 298);
            deleteButton.Name = "deleteButton";
            deleteButton.Size = new Size(75, 23);
            deleteButton.TabIndex = 5;
            deleteButton.Text = "&Elimina";
            deleteButton.UseVisualStyleBackColor = true;
            deleteButton.Click += DeleteButtonClick;
            // 
            // upButton
            // 
            upButton.Location = new Point(574, 298);
            upButton.Name = "upButton";
            upButton.Size = new Size(75, 23);
            upButton.TabIndex = 6;
            upButton.Text = "↑";
            upButton.UseVisualStyleBackColor = true;
            upButton.Click += UpButtonClick;
            // 
            // downButton
            // 
            downButton.Location = new Point(655, 298);
            downButton.Name = "downButton";
            downButton.Size = new Size(75, 23);
            downButton.TabIndex = 7;
            downButton.Text = "↓";
            downButton.UseVisualStyleBackColor = true;
            downButton.Click += DownButtonClick;
            // 
            // coveragePanel
            // 
            coveragePanel.BackColor = Color.White;
            coveragePanel.BorderStyle = BorderStyle.FixedSingle;
            coveragePanel.Location = new Point(16, 338);
            coveragePanel.Name = "coveragePanel";
            coveragePanel.Size = new Size(720, 150);
            coveragePanel.TabIndex = 8;
            // 
            // saveButton
            // 
            saveButton.Location = new Point(580, 504);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(75, 28);
            saveButton.TabIndex = 9;
            saveButton.Text = "&Salva";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += SaveButtonClick;
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(661, 504);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 28);
            cancelButton.TabIndex = 10;
            cancelButton.Text = "&Annulla";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // VoceRicorrenteDialog
            // 
            AcceptButton = saveButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(752, 548);
            Controls.Add(cancelButton);
            Controls.Add(saveButton);
            Controls.Add(coveragePanel);
            Controls.Add(downButton);
            Controls.Add(upButton);
            Controls.Add(deleteButton);
            Controls.Add(editButton);
            Controls.Add(addButton);
            Controls.Add(definitionsGrid);
            Controls.Add(nameTextBox);
            Controls.Add(nameLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "VoceRicorrenteDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Voce ricorrente";
            ((System.ComponentModel.ISupportInitialize)definitionsGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label nameLabel;
        private TextBox nameTextBox;
        private DataGridView definitionsGrid;
        private Button addButton;
        private Button editButton;
        private Button deleteButton;
        private Button upButton;
        private Button downButton;
        private CoveragePanel coveragePanel;
        private Button saveButton;
        private Button cancelButton;
    }
}

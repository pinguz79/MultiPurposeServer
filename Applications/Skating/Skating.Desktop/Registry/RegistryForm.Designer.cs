namespace Skating.Desktop.Registry
{
    partial class RegistryForm
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
            layoutPanel = new TableLayoutPanel();
            hintLabel = new Label();
            entriesGrid = new DataGridView();
            nameColumn = new DataGridViewTextBoxColumn();
            detailColumn = new DataGridViewTextBoxColumn();
            inputPanel = new FlowLayoutPanel();
            nameLabel = new Label();
            nameTextBox = new TextBox();
            lastNameLabel = new Label();
            lastNameTextBox = new TextBox();
            proCheckBox = new CheckBox();
            absentCheckBox = new CheckBox();
            addButton = new Button();
            cancelEditButton = new Button();
            editButton = new Button();
            deleteButton = new Button();
            footerPanel = new FlowLayoutPanel();
            refreshButton = new Button();
            countLabel = new Label();
            statusLabel = new Label();
            layoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)entriesGrid).BeginInit();
            inputPanel.SuspendLayout();
            footerPanel.SuspendLayout();
            SuspendLayout();
            //
            // layoutPanel
            //
            layoutPanel.ColumnCount = 1;
            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutPanel.Controls.Add(hintLabel, 0, 0);
            layoutPanel.Controls.Add(entriesGrid, 0, 1);
            layoutPanel.Controls.Add(inputPanel, 0, 2);
            layoutPanel.Controls.Add(footerPanel, 0, 3);
            layoutPanel.Dock = DockStyle.Fill;
            layoutPanel.Name = "layoutPanel";
            layoutPanel.Padding = new Padding(12);
            layoutPanel.RowCount = 4;
            layoutPanel.RowStyles.Add(new RowStyle());
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutPanel.RowStyles.Add(new RowStyle());
            layoutPanel.RowStyles.Add(new RowStyle());
            layoutPanel.TabIndex = 0;
            //
            // hintLabel
            //
            hintLabel.AutoSize = true;
            hintLabel.Dock = DockStyle.Fill;
            hintLabel.Margin = new Padding(3, 3, 3, 12);
            hintLabel.Name = "hintLabel";
            //
            // entriesGrid
            //
            entriesGrid.AllowUserToAddRows = false;
            entriesGrid.AllowUserToDeleteRows = false;
            entriesGrid.AutoGenerateColumns = false;
            entriesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            entriesGrid.BackgroundColor = SystemColors.Window;
            entriesGrid.Columns.AddRange(new DataGridViewColumn[] { nameColumn, detailColumn });
            entriesGrid.Dock = DockStyle.Fill;
            entriesGrid.MultiSelect = false;
            entriesGrid.Name = "entriesGrid";
            entriesGrid.ReadOnly = true;
            entriesGrid.RowHeadersVisible = false;
            entriesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            entriesGrid.TabIndex = 1;
            entriesGrid.CellDoubleClick += EntriesGridCellDoubleClick;
            entriesGrid.SelectionChanged += EntriesGridSelectionChanged;
            nameColumn.DataPropertyName = "Name";
            nameColumn.HeaderText = "Nome";
            nameColumn.Name = "nameColumn";
            nameColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            detailColumn.DataPropertyName = "Detail";
            detailColumn.HeaderText = "Cognome";
            detailColumn.Name = "detailColumn";
            detailColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // inputPanel
            //
            inputPanel.AutoSize = true;
            inputPanel.Controls.Add(nameLabel);
            inputPanel.Controls.Add(nameTextBox);
            inputPanel.Controls.Add(lastNameLabel);
            inputPanel.Controls.Add(lastNameTextBox);
            inputPanel.Controls.Add(proCheckBox);
            inputPanel.Controls.Add(absentCheckBox);
            inputPanel.Controls.Add(addButton);
            inputPanel.Controls.Add(cancelEditButton);
            inputPanel.Dock = DockStyle.Fill;
            inputPanel.Name = "inputPanel";
            inputPanel.Padding = new Padding(0, 10, 0, 6);
            inputPanel.TabIndex = 0;
            nameLabel.AutoSize = true;
            nameLabel.Margin = new Padding(3, 7, 3, 3);
            nameLabel.Text = "Nome";
            nameTextBox.Name = "nameTextBox";
            nameTextBox.Size = new Size(190, 23);
            nameTextBox.TabIndex = 0;
            lastNameLabel.AutoSize = true;
            lastNameLabel.Margin = new Padding(3, 7, 3, 3);
            lastNameLabel.Text = "Cognome";
            lastNameTextBox.Name = "lastNameTextBox";
            lastNameTextBox.Size = new Size(190, 23);
            lastNameTextBox.TabIndex = 1;
            proCheckBox.AutoSize = true;
            proCheckBox.Name = "proCheckBox";
            absentCheckBox.Name = "absentCheckBox";
            absentCheckBox.AutoSize = true;
            absentCheckBox.Text = "Assente";
            absentCheckBox.TabIndex = 3;
            proCheckBox.TabIndex = 2;
            proCheckBox.Text = "Percorso PRO";
            addButton.AutoSize = true;
            addButton.Name = "addButton";
            addButton.TabIndex = 3;
            addButton.Text = "Aggiungi";
            addButton.Click += AddButtonClick;
            cancelEditButton.AutoSize = true;
            cancelEditButton.Name = "cancelEditButton";
            cancelEditButton.TabIndex = 4;
            cancelEditButton.Text = "Annulla modifica";
            cancelEditButton.Visible = false;
            cancelEditButton.Click += CancelEditButtonClick;
            //
            // footerPanel
            //
            footerPanel.AutoSize = true;
            footerPanel.Controls.Add(refreshButton);
            footerPanel.Controls.Add(editButton);
            footerPanel.Controls.Add(deleteButton);
            footerPanel.Controls.Add(countLabel);
            footerPanel.Controls.Add(statusLabel);
            footerPanel.Dock = DockStyle.Fill;
            footerPanel.Name = "footerPanel";
            footerPanel.TabIndex = 2;
            refreshButton.AutoSize = true;
            refreshButton.Name = "refreshButton";
            refreshButton.TabIndex = 0;
            refreshButton.Text = "Aggiorna elenco";
            refreshButton.Click += RefreshButtonClick;
            editButton.AutoSize = true;
            editButton.Enabled = false;
            editButton.Name = "editButton";
            editButton.TabIndex = 1;
            editButton.Text = "Modifica";
            editButton.Click += EditButtonClick;
            deleteButton.AutoSize = true;
            deleteButton.Enabled = false;
            deleteButton.Name = "deleteButton";
            deleteButton.TabIndex = 2;
            deleteButton.Text = "Elimina criterio";
            deleteButton.Click += DeleteButtonClick;
            countLabel.AutoSize = true;
            countLabel.Margin = new Padding(12, 7, 3, 3);
            countLabel.Name = "countLabel";
            statusLabel.AutoSize = true;
            statusLabel.Margin = new Padding(12, 7, 3, 3);
            statusLabel.Name = "statusLabel";
            //
            // RegistryForm
            //
            AcceptButton = addButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 540);
            Controls.Add(layoutPanel);
            MinimumSize = new Size(700, 400);
            Name = "RegistryForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Shown += RegistryFormShown;
            FormClosing += RegistryFormClosing;
            layoutPanel.ResumeLayout(false);
            layoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)entriesGrid).EndInit();
            inputPanel.ResumeLayout(false);
            inputPanel.PerformLayout();
            footerPanel.ResumeLayout(false);
            footerPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel layoutPanel;
        private Label hintLabel;
        private DataGridView entriesGrid;
        private DataGridViewTextBoxColumn nameColumn;
        private DataGridViewTextBoxColumn detailColumn;
        private FlowLayoutPanel inputPanel;
        private Label nameLabel;
        private TextBox nameTextBox;
        private Label lastNameLabel;
        private TextBox lastNameTextBox;
        private CheckBox proCheckBox;
        private CheckBox absentCheckBox;
        private Button addButton;
        private Button cancelEditButton;
        private Button editButton;
        private Button deleteButton;
        private FlowLayoutPanel footerPanel;
        private Button refreshButton;
        private Label countLabel;
        private Label statusLabel;
    }
}

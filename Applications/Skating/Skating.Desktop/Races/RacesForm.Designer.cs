namespace Skating.Desktop.Races
{
    partial class RacesForm
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
            entriesGrid = new DataGridView();
            actionsPanel = new FlowLayoutPanel();
            juryButton = new Button();
            refreshButton = new Button();
            summaryLabel = new Label();
            disciplineColumn = new DataGridViewTextBoxColumn();
            categoryColumn = new DataGridViewTextBoxColumn();
            levelColumn = new DataGridViewTextBoxColumn();
            countColumn = new DataGridViewTextBoxColumn();
            judgesColumn = new DataGridViewTextBoxColumn();
            juryColumn = new DataGridViewTextBoxColumn();
            statusColumn = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)entriesGrid).BeginInit();
            actionsPanel.SuspendLayout();
            SuspendLayout();
            entriesGrid.Dock = DockStyle.Fill;
            entriesGrid.Name = "entriesGrid";
            entriesGrid.AutoGenerateColumns = false;
            entriesGrid.ReadOnly = true;
            entriesGrid.AllowUserToAddRows = false;
            entriesGrid.AllowUserToDeleteRows = false;
            entriesGrid.MultiSelect = false;
            entriesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            entriesGrid.RowHeadersVisible = false;
            entriesGrid.TabIndex = 1;
            entriesGrid.CellDoubleClick += EntriesGridCellDoubleClick;
            entriesGrid.Columns.AddRange(new DataGridViewColumn[] { disciplineColumn, categoryColumn, levelColumn, countColumn, judgesColumn, juryColumn, statusColumn });
            disciplineColumn.Name = "disciplineColumn";
            disciplineColumn.HeaderText = "Disciplina";
            disciplineColumn.DataPropertyName = "Discipline";
            disciplineColumn.Width = 150;
            categoryColumn.Name = "categoryColumn";
            categoryColumn.HeaderText = "Categoria";
            categoryColumn.DataPropertyName = "Category";
            categoryColumn.Width = 120;
            levelColumn.Name = "levelColumn";
            levelColumn.HeaderText = "Livello";
            levelColumn.DataPropertyName = "Level";
            levelColumn.Width = 100;
            countColumn.Name = "countColumn";
            countColumn.HeaderText = "Coreografie";
            countColumn.DataPropertyName = "Choreographies";
            countColumn.Width = 90;
            judgesColumn.Name = "judgesColumn";
            judgesColumn.HeaderText = "Giudici";
            judgesColumn.DataPropertyName = "Judges";
            judgesColumn.Width = 70;
            juryColumn.Name = "juryColumn";
            juryColumn.HeaderText = "Giuria";
            juryColumn.DataPropertyName = "Jury";
            juryColumn.Width = 140;
            statusColumn.Name = "statusColumn";
            statusColumn.HeaderText = "Configurazione";
            statusColumn.DataPropertyName = "Status";
            statusColumn.Width = 230;
            actionsPanel.Dock = DockStyle.Top;
            actionsPanel.AutoSize = true;
            actionsPanel.Padding = new Padding(8);
            actionsPanel.Name = "actionsPanel";
            actionsPanel.TabIndex = 0;
            actionsPanel.Controls.Add(juryButton);
            actionsPanel.Controls.Add(refreshButton);
            juryButton.Name = "juryButton";
            juryButton.Text = "Partecipanti e giuria...";
            juryButton.AutoSize = true;
            juryButton.TabIndex = 0;
            juryButton.Click += JuryButtonClick;
            refreshButton.Name = "refreshButton";
            refreshButton.Text = "Aggiorna";
            refreshButton.AutoSize = true;
            refreshButton.TabIndex = 1;
            refreshButton.Click += RefreshButtonClick;
            summaryLabel.Dock = DockStyle.Bottom;
            summaryLabel.Height = 62;
            summaryLabel.Padding = new Padding(8);
            summaryLabel.Name = "summaryLabel";
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1040, 620);
            MinimumSize = new Size(850, 500);
            Controls.Add(entriesGrid);
            Controls.Add(actionsPanel);
            Controls.Add(summaryLabel);
            Name = "RacesForm";
            Text = "Gare";
            StartPosition = FormStartPosition.CenterParent;
            Shown += RacesFormShown;
            ((System.ComponentModel.ISupportInitialize)entriesGrid).EndInit();
            actionsPanel.ResumeLayout(false);
            actionsPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView entriesGrid;
        private FlowLayoutPanel actionsPanel;
        private Button juryButton;
        private Button refreshButton;
        private Label summaryLabel;
        private DataGridViewTextBoxColumn disciplineColumn;
        private DataGridViewTextBoxColumn categoryColumn;
        private DataGridViewTextBoxColumn levelColumn;
        private DataGridViewTextBoxColumn countColumn;
        private DataGridViewTextBoxColumn judgesColumn;
        private DataGridViewTextBoxColumn juryColumn;
        private DataGridViewTextBoxColumn statusColumn;
    }
}

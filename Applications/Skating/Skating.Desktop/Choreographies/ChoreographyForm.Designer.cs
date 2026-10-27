namespace Skating.Desktop.Choreographies
{
    partial class ChoreographyForm
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
            newButton = new Button();
            editButton = new Button();
            refreshButton = new Button();
            summaryLabel = new Label();
            nameColumn = new DataGridViewTextBoxColumn();
            schoolColumn = new DataGridViewTextBoxColumn();
            disciplineColumn = new DataGridViewTextBoxColumn();
            categoryColumn = new DataGridViewTextBoxColumn();
            levelColumn = new DataGridViewTextBoxColumn();
            compositionColumn = new DataGridViewTextBoxColumn();
            durationColumn = new DataGridViewTextBoxColumn();
            athletesColumn = new DataGridViewTextBoxColumn();
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
            entriesGrid.Columns.AddRange(new DataGridViewColumn[] { nameColumn, schoolColumn, disciplineColumn, categoryColumn, levelColumn, compositionColumn, durationColumn, athletesColumn });
            nameColumn.Name = "nameColumn";
            nameColumn.HeaderText = "Nome";
            nameColumn.DataPropertyName = "Name";
            nameColumn.Width = 140;
            schoolColumn.Name = "schoolColumn";
            schoolColumn.HeaderText = "Scuola";
            schoolColumn.DataPropertyName = "School";
            schoolColumn.Width = 140;
            disciplineColumn.Name = "disciplineColumn";
            disciplineColumn.HeaderText = "Disciplina";
            disciplineColumn.DataPropertyName = "Discipline";
            disciplineColumn.Width = 110;
            categoryColumn.Name = "categoryColumn";
            categoryColumn.HeaderText = "Categoria";
            categoryColumn.DataPropertyName = "Category";
            categoryColumn.Width = 90;
            levelColumn.Name = "levelColumn";
            levelColumn.HeaderText = "Livello";
            levelColumn.DataPropertyName = "Level";
            levelColumn.Width = 90;
            compositionColumn.Name = "compositionColumn";
            compositionColumn.HeaderText = "Composizione";
            compositionColumn.DataPropertyName = "Composition";
            compositionColumn.Width = 95;
            durationColumn.Name = "durationColumn";
            durationColumn.HeaderText = "Durata";
            durationColumn.DataPropertyName = "Duration";
            durationColumn.Width = 65;
            athletesColumn.Name = "athletesColumn";
            athletesColumn.HeaderText = "Atleti";
            athletesColumn.DataPropertyName = "Athletes";
            athletesColumn.Width = 250;
            actionsPanel.Dock = DockStyle.Top;
            actionsPanel.AutoSize = true;
            actionsPanel.Padding = new Padding(8);
            actionsPanel.Name = "actionsPanel";
            actionsPanel.TabIndex = 0;
            actionsPanel.Controls.Add(newButton);
            actionsPanel.Controls.Add(editButton);
            actionsPanel.Controls.Add(refreshButton);
            newButton.Name = "newButton";
            newButton.Text = "Nuova coreografia...";
            newButton.AutoSize = true;
            newButton.TabIndex = 0;
            newButton.Click += NewButtonClick;
            editButton.Name = "editButton";
            editButton.Text = "Modifica...";
            editButton.AutoSize = true;
            editButton.TabIndex = 1;
            editButton.Click += EditButtonClick;
            refreshButton.Name = "refreshButton";
            refreshButton.Text = "Aggiorna";
            refreshButton.AutoSize = true;
            refreshButton.TabIndex = 2;
            refreshButton.Click += RefreshButtonClick;
            summaryLabel.Dock = DockStyle.Bottom;
            summaryLabel.Height = 48;
            summaryLabel.Padding = new Padding(8);
            summaryLabel.Name = "summaryLabel";
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1120, 660);
            MinimumSize = new Size(850, 500);
            Controls.Add(entriesGrid);
            Controls.Add(actionsPanel);
            Controls.Add(summaryLabel);
            Name = "ChoreographyForm";
            Text = "Coreografie";
            StartPosition = FormStartPosition.CenterParent;
            Shown += ChoreographyFormShown;
            ((System.ComponentModel.ISupportInitialize)entriesGrid).EndInit();
            actionsPanel.ResumeLayout(false);
            actionsPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView entriesGrid;
        private FlowLayoutPanel actionsPanel;
        private Button newButton;
        private Button editButton;
        private Button refreshButton;
        private Label summaryLabel;
        private DataGridViewTextBoxColumn nameColumn;
        private DataGridViewTextBoxColumn schoolColumn;
        private DataGridViewTextBoxColumn disciplineColumn;
        private DataGridViewTextBoxColumn categoryColumn;
        private DataGridViewTextBoxColumn levelColumn;
        private DataGridViewTextBoxColumn compositionColumn;
        private DataGridViewTextBoxColumn durationColumn;
        private DataGridViewTextBoxColumn athletesColumn;
    }
}

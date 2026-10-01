namespace Finance.Desktop
{
    partial class FinanziamentoView
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            layout = new TableLayoutPanel();
            titleLabel = new Label();
            summaryLabel = new Label();
            warningLabel = new Label();
            actionsPanel = new FlowLayoutPanel();
            showPastCheckBox = new CheckBox();
            alignButton = new Button();
            deleteButton = new Button();
            refreshButton = new Button();
            grid = new DataGridView();
            layout.SuspendLayout();
            actionsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // layout
            // 
            layout.ColumnCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.Controls.Add(titleLabel, 0, 0);
            layout.Controls.Add(summaryLabel, 0, 1);
            layout.Controls.Add(warningLabel, 0, 2);
            layout.Controls.Add(actionsPanel, 0, 3);
            layout.Controls.Add(grid, 0, 4);
            layout.Dock = DockStyle.Fill;
            layout.Location = new Point(0, 0);
            layout.Name = "layout";
            layout.Padding = new Padding(12);
            layout.RowCount = 5;
            layout.RowStyles.Add(new RowStyle());
            layout.RowStyles.Add(new RowStyle());
            layout.RowStyles.Add(new RowStyle());
            layout.RowStyles.Add(new RowStyle());
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.Size = new Size(940, 580);
            layout.TabIndex = 0;
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Dock = DockStyle.Fill;
            titleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            titleLabel.Location = new Point(15, 12);
            titleLabel.Margin = new Padding(3, 0, 3, 12);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(910, 32);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Finanziamento";
            // 
            // summaryLabel
            // 
            summaryLabel.AutoSize = true;
            summaryLabel.Dock = DockStyle.Fill;
            summaryLabel.Location = new Point(15, 56);
            summaryLabel.Margin = new Padding(3, 0, 3, 12);
            summaryLabel.Name = "summaryLabel";
            summaryLabel.Size = new Size(910, 15);
            summaryLabel.TabIndex = 1;
            summaryLabel.Text = "Caricamento…";
            // 
            // warningLabel
            // 
            warningLabel.AutoSize = true;
            warningLabel.Dock = DockStyle.Fill;
            warningLabel.ForeColor = Color.DarkOrange;
            warningLabel.Location = new Point(15, 83);
            warningLabel.Name = "warningLabel";
            warningLabel.Size = new Size(910, 15);
            warningLabel.TabIndex = 2;
            // 
            // actionsPanel
            // 
            actionsPanel.AutoSize = true;
            actionsPanel.Controls.Add(showPastCheckBox);
            actionsPanel.Controls.Add(alignButton);
            actionsPanel.Controls.Add(deleteButton);
            actionsPanel.Controls.Add(refreshButton);
            actionsPanel.Dock = DockStyle.Fill;
            actionsPanel.Location = new Point(15, 101);
            actionsPanel.Name = "actionsPanel";
            actionsPanel.Size = new Size(910, 34);
            actionsPanel.TabIndex = 3;
            // 
            // showPastCheckBox
            // 
            showPastCheckBox.AutoSize = true;
            showPastCheckBox.Location = new Point(3, 8);
            showPastCheckBox.Margin = new Padding(3, 8, 12, 3);
            showPastCheckBox.Name = "showPastCheckBox";
            showPastCheckBox.Size = new Size(198, 19);
            showPastCheckBox.TabIndex = 0;
            showPastCheckBox.Text = "Mostra anche le rate &passate";
            showPastCheckBox.UseVisualStyleBackColor = true;
            showPastCheckBox.CheckedChanged += ShowPastCheckBoxCheckedChanged;
            // 
            // alignButton
            // 
            alignButton.AutoSize = true;
            alignButton.Enabled = false;
            alignButton.Location = new Point(216, 3);
            alignButton.Name = "alignButton";
            alignButton.Size = new Size(145, 28);
            alignButton.TabIndex = 1;
            alignButton.Text = "&Residuo verificato…";
            alignButton.UseVisualStyleBackColor = true;
            alignButton.Click += AlignButtonClick;
            // 
            // deleteButton
            // 
            deleteButton.AutoSize = true;
            deleteButton.Enabled = false;
            deleteButton.Location = new Point(367, 3);
            deleteButton.Name = "deleteButton";
            deleteButton.Size = new Size(175, 28);
            deleteButton.TabIndex = 2;
            deleteButton.Text = "&Elimina riallineamento…";
            deleteButton.UseVisualStyleBackColor = true;
            deleteButton.Click += DeleteButtonClick;
            // 
            // refreshButton
            // 
            refreshButton.AutoSize = true;
            refreshButton.Location = new Point(548, 3);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(90, 28);
            refreshButton.TabIndex = 3;
            refreshButton.Text = "&Aggiorna";
            refreshButton.UseVisualStyleBackColor = true;
            refreshButton.Click += RefreshButtonClick;
            // 
            // grid
            // 
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AutoGenerateColumns = false;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.Dock = DockStyle.Fill;
            grid.Location = new Point(15, 141);
            grid.MultiSelect = false;
            grid.Name = "grid";
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.Size = new Size(910, 424);
            grid.TabIndex = 4;
            grid.SelectionChanged += GridSelectionChanged;
            // 
            // FinanziamentoView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(layout);
            Name = "FinanziamentoView";
            Size = new Size(940, 580);
            layout.ResumeLayout(false);
            layout.PerformLayout();
            actionsPanel.ResumeLayout(false);
            actionsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel layout;
        private Label titleLabel;
        private Label summaryLabel;
        private Label warningLabel;
        private FlowLayoutPanel actionsPanel;
        private CheckBox showPastCheckBox;
        private Button alignButton;
        private Button deleteButton;
        private Button refreshButton;
        private DataGridView grid;
    }
}

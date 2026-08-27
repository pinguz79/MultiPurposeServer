namespace Finance.Desktop
{
    partial class RecurringEntriesView
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
            titleLabel = new Label();
            addButton = new Button();
            masterGrid = new DataGridView();
            detailCaption = new Label();
            detailGrid = new DataGridView();
            coveragePanel = new CoveragePanel();
            ((System.ComponentModel.ISupportInitialize)masterGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)detailGrid).BeginInit();
            SuspendLayout();
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            titleLabel.Location = new Point(16, 14);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(179, 32);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Voci ricorrenti";
            // 
            // addButton
            // 
            addButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            addButton.Location = new Point(790, 16);
            addButton.Name = "addButton";
            addButton.Size = new Size(150, 32);
            addButton.TabIndex = 1;
            addButton.Text = "Aggiungi voce...";
            addButton.UseVisualStyleBackColor = true;
            addButton.Click += AddButtonClick;
            // 
            // masterGrid
            // 
            masterGrid.AllowUserToAddRows = false;
            masterGrid.AllowUserToDeleteRows = false;
            masterGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            masterGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            masterGrid.Location = new Point(16, 62);
            masterGrid.MultiSelect = false;
            masterGrid.Name = "masterGrid";
            masterGrid.ReadOnly = true;
            masterGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            masterGrid.Size = new Size(924, 220);
            masterGrid.TabIndex = 2;
            masterGrid.SelectionChanged += MasterGridSelectionChanged;
            masterGrid.CellContentClick += MasterGridCellContentClick;
            // 
            // detailCaption
            // 
            detailCaption.AutoSize = true;
            detailCaption.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            detailCaption.Location = new Point(16, 298);
            detailCaption.Name = "detailCaption";
            detailCaption.Size = new Size(82, 20);
            detailCaption.TabIndex = 3;
            detailCaption.Text = "Definizioni";
            // 
            // detailGrid
            // 
            detailGrid.AllowUserToAddRows = false;
            detailGrid.AllowUserToDeleteRows = false;
            detailGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            detailGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            detailGrid.Location = new Point(16, 326);
            detailGrid.Name = "detailGrid";
            detailGrid.ReadOnly = true;
            detailGrid.Size = new Size(924, 150);
            detailGrid.TabIndex = 4;
            // 
            // coveragePanel
            // 
            coveragePanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            coveragePanel.BackColor = Color.White;
            coveragePanel.BorderStyle = BorderStyle.FixedSingle;
            coveragePanel.Location = new Point(16, 492);
            coveragePanel.Name = "coveragePanel";
            coveragePanel.Size = new Size(924, 125);
            coveragePanel.TabIndex = 5;
            // 
            // RecurringEntriesView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 246, 248);
            Controls.Add(coveragePanel);
            Controls.Add(detailGrid);
            Controls.Add(detailCaption);
            Controls.Add(masterGrid);
            Controls.Add(addButton);
            Controls.Add(titleLabel);
            Name = "RecurringEntriesView";
            Size = new Size(956, 637);
            ((System.ComponentModel.ISupportInitialize)masterGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)detailGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label titleLabel;
        private Button addButton;
        private DataGridView masterGrid;
        private Label detailCaption;
        private DataGridView detailGrid;
        private CoveragePanel coveragePanel;
    }
}

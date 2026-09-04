namespace Finance.Desktop
{
    partial class AccountParametersView
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
            accountLabel = new Label();
            accountComboBox = new ComboBox();
            addButton = new Button();
            configureCardButton = new Button();
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
            titleLabel.Size = new Size(196, 32);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Parametri conti";
            // 
            // accountLabel
            // 
            accountLabel.AutoSize = true;
            accountLabel.Location = new Point(250, 11);
            accountLabel.Name = "accountLabel";
            accountLabel.Size = new Size(39, 15);
            accountLabel.TabIndex = 1;
            accountLabel.Text = "&Conto";
            // 
            // accountComboBox
            // 
            accountComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            accountComboBox.FormattingEnabled = true;
            accountComboBox.Location = new Point(250, 29);
            accountComboBox.Name = "accountComboBox";
            accountComboBox.Size = new Size(220, 23);
            accountComboBox.TabIndex = 2;
            accountComboBox.SelectedIndexChanged += AccountComboBoxSelectedIndexChanged;
            // 
            // addButton
            // 
            addButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            addButton.Location = new Point(630, 16);
            addButton.Name = "addButton";
            addButton.Size = new Size(140, 32);
            addButton.TabIndex = 3;
            addButton.Text = "Aggiungi parametro...";
            addButton.UseVisualStyleBackColor = true;
            addButton.Click += AddButtonClick;
            // 
            // configureCardButton
            // 
            configureCardButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            configureCardButton.Location = new Point(780, 16);
            configureCardButton.Name = "configureCardButton";
            configureCardButton.Size = new Size(160, 32);
            configureCardButton.TabIndex = 4;
            configureCardButton.Text = "Configura carta a saldo...";
            configureCardButton.UseVisualStyleBackColor = true;
            configureCardButton.Click += ConfigureCardButtonClick;
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
            masterGrid.TabIndex = 5;
            masterGrid.CellContentClick += MasterGridCellContentClick;
            masterGrid.SelectionChanged += MasterGridSelectionChanged;
            // 
            // detailCaption
            // 
            detailCaption.AutoSize = true;
            detailCaption.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            detailCaption.Location = new Point(16, 298);
            detailCaption.Name = "detailCaption";
            detailCaption.Size = new Size(82, 20);
            detailCaption.TabIndex = 6;
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
            detailGrid.TabIndex = 7;
            detailGrid.CellContentClick += DetailGridCellContentClick;
            // 
            // coveragePanel
            // 
            coveragePanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            coveragePanel.BackColor = Color.White;
            coveragePanel.BorderStyle = BorderStyle.FixedSingle;
            coveragePanel.Location = new Point(16, 492);
            coveragePanel.Name = "coveragePanel";
            coveragePanel.Size = new Size(924, 125);
            coveragePanel.TabIndex = 8;
            // 
            // AccountParametersView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 246, 248);
            Controls.Add(coveragePanel);
            Controls.Add(detailGrid);
            Controls.Add(detailCaption);
            Controls.Add(masterGrid);
            Controls.Add(configureCardButton);
            Controls.Add(addButton);
            Controls.Add(accountComboBox);
            Controls.Add(accountLabel);
            Controls.Add(titleLabel);
            Name = "AccountParametersView";
            Size = new Size(956, 637);
            ((System.ComponentModel.ISupportInitialize)masterGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)detailGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label titleLabel;
        private Label accountLabel;
        private ComboBox accountComboBox;
        private Button addButton;
        private Button configureCardButton;
        private DataGridView masterGrid;
        private Label detailCaption;
        private DataGridView detailGrid;
        private CoveragePanel coveragePanel;
    }
}

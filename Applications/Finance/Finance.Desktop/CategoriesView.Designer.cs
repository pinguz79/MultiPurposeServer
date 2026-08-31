namespace Finance.Desktop
{
    partial class CategoriesView
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
            categoriesGrid = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)categoriesGrid).BeginInit();
            SuspendLayout();
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            titleLabel.Location = new Point(12, 12);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(125, 32);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Categorie";
            // 
            // addButton
            // 
            addButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            addButton.Location = new Point(704, 16);
            addButton.Name = "addButton";
            addButton.Size = new Size(132, 28);
            addButton.TabIndex = 1;
            addButton.Text = "&Nuova categoria...";
            addButton.UseVisualStyleBackColor = true;
            addButton.Click += AddButtonClick;
            // 
            // categoriesGrid
            // 
            categoriesGrid.AllowUserToAddRows = false;
            categoriesGrid.AllowUserToDeleteRows = false;
            categoriesGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            categoriesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            categoriesGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            categoriesGrid.Location = new Point(12, 58);
            categoriesGrid.MultiSelect = false;
            categoriesGrid.Name = "categoriesGrid";
            categoriesGrid.ReadOnly = true;
            categoriesGrid.RowHeadersVisible = false;
            categoriesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            categoriesGrid.Size = new Size(824, 476);
            categoriesGrid.TabIndex = 2;
            categoriesGrid.CellContentClick += CategoriesGridCellContentClick;
            // 
            // CategoriesView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(categoriesGrid);
            Controls.Add(addButton);
            Controls.Add(titleLabel);
            Name = "CategoriesView";
            Size = new Size(848, 546);
            ((System.ComponentModel.ISupportInitialize)categoriesGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label titleLabel;
        private Button addButton;
        private DataGridView categoriesGrid;
    }
}

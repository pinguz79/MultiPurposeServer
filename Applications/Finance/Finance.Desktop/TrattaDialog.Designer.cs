namespace Finance.Desktop
{
    partial class TrattaDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            hintLabel = new Label();
            grid = new DataGridView();
            addButton = new Button();
            editButton = new Button();
            deleteButton = new Button();
            upButton = new Button();
            downButton = new Button();
            saveButton = new Button();
            cancelButton = new Button();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            //
            // hintLabel
            //
            hintLabel.Name = "hintLabel";
            hintLabel.Text = "Tratta bidirezionale. Vince la prima eccezione valida; altrimenti si usa la base.\r\nI pedaggi già confermati non cambiano. Doppio click per modificare importo e validità.";
            hintLabel.Location = new Point(12, 12);
            hintLabel.Size = new Size(730, 40);
            hintLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            //
            // grid
            //
            grid.Name = "grid";
            grid.Location = new Point(12, 62);
            grid.Size = new Size(730, 270);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.TabIndex = 0;
            grid.CurrentCellChanged += SelectionChanged;
            grid.CellDoubleClick += GridDoubleClick;
            //
            // addButton
            //
            addButton.Name = "addButton";
            addButton.Text = "Nuova eccezione…";
            addButton.Location = new Point(12, 346);
            addButton.Size = new Size(135, 28);
            addButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            addButton.TabIndex = 1;
            addButton.Click += AddClick;
            //
            // editButton
            //
            editButton.Name = "editButton";
            editButton.Text = "Modifica…";
            editButton.Location = new Point(157, 346);
            editButton.Size = new Size(135, 28);
            editButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            editButton.TabIndex = 2;
            editButton.Click += EditClick;
            //
            // deleteButton
            //
            deleteButton.Name = "deleteButton";
            deleteButton.Text = "Elimina";
            deleteButton.Location = new Point(302, 346);
            deleteButton.Size = new Size(135, 28);
            deleteButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            deleteButton.TabIndex = 3;
            deleteButton.Click += DeleteClick;
            //
            // upButton
            //
            upButton.Name = "upButton";
            upButton.Text = "↑ Priorità";
            upButton.Location = new Point(447, 346);
            upButton.Size = new Size(135, 28);
            upButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            upButton.TabIndex = 4;
            upButton.Click += UpClick;
            //
            // downButton
            //
            downButton.Name = "downButton";
            downButton.Text = "↓ Priorità";
            downButton.Location = new Point(592, 346);
            downButton.Size = new Size(135, 28);
            downButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            downButton.TabIndex = 5;
            downButton.Click += DownClick;
            //
            // saveButton
            //
            saveButton.Name = "saveButton";
            saveButton.Text = "&Salva";
            saveButton.Location = new Point(546, 400);
            saveButton.Size = new Size(90, 28);
            saveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            saveButton.TabIndex = 6;
            saveButton.Click += SaveClick;
            //
            // cancelButton
            //
            cancelButton.Name = "cancelButton";
            cancelButton.Text = "&Annulla";
            cancelButton.Location = new Point(646, 400);
            cancelButton.Size = new Size(96, 28);
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cancelButton.TabIndex = 7;
            cancelButton.DialogResult = DialogResult.Cancel;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(cancelButton);
            Controls.Add(saveButton);
            Controls.Add(downButton);
            Controls.Add(upButton);
            Controls.Add(deleteButton);
            Controls.Add(editButton);
            Controls.Add(addButton);
            Controls.Add(grid);
            Controls.Add(hintLabel);
            ClientSize = new Size(754, 440);
            MinimumSize = new Size(770, 479);
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AcceptButton = saveButton;
            CancelButton = cancelButton;
            FormClosing += DialogClosing;
            Name = "TrattaDialog";
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label hintLabel;
        private DataGridView grid;
        private Button addButton;
        private Button editButton;
        private Button deleteButton;
        private Button upButton;
        private Button downButton;
        private Button saveButton;
        private Button cancelButton;
    }
}

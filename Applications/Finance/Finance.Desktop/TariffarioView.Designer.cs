namespace Finance.Desktop
{
    partial class TariffarioView
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
            titleLabel = new Label();
            filterInput = new TextBox();
            stationsCheckBox = new CheckBox();
            addStationButton = new Button();
            refreshButton = new Button();
            grid = new DataGridView();
            statusLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            //
            // titleLabel
            //
            titleLabel.Name = "titleLabel";
            titleLabel.Text = "Tariffario pedaggi";
            titleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(12, 12);
            //
            // filterInput
            //
            filterInput.Name = "filterInput";
            filterInput.PlaceholderText = "Cerca una stazione…";
            filterInput.Location = new Point(12, 60);
            filterInput.Size = new Size(260, 23);
            filterInput.TabIndex = 0;
            filterInput.TextChanged += FilterChanged;
            //
            // stationsCheckBox
            //
            stationsCheckBox.Name = "stationsCheckBox";
            stationsCheckBox.Text = "Gestisci stazioni";
            stationsCheckBox.AutoSize = true;
            stationsCheckBox.Location = new Point(290, 62);
            stationsCheckBox.TabIndex = 1;
            stationsCheckBox.CheckedChanged += FilterChanged;
            //
            // addStationButton
            //
            addStationButton.Name = "addStationButton";
            addStationButton.Text = "Nuova stazione…";
            addStationButton.Location = new Point(454, 57);
            addStationButton.Size = new Size(145, 28);
            addStationButton.TabIndex = 2;
            addStationButton.Click += AddStationClick;
            //
            // refreshButton
            //
            refreshButton.Name = "refreshButton";
            refreshButton.Text = "Aggiorna";
            refreshButton.Location = new Point(614, 57);
            refreshButton.Size = new Size(110, 28);
            refreshButton.TabIndex = 3;
            refreshButton.Click += RefreshClick;
            //
            // grid
            //
            grid.Name = "grid";
            grid.Location = new Point(12, 100);
            grid.Size = new Size(824, 390);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.TabIndex = 4;
            grid.CellContentClick += GridClick;
            grid.CellDoubleClick += GridDoubleClick;
            //
            // statusLabel
            //
            statusLabel.Name = "statusLabel";
            statusLabel.Text = "Caricamento…";
            statusLabel.Location = new Point(12, 506);
            statusLabel.Size = new Size(824, 28);
            statusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(statusLabel);
            Controls.Add(grid);
            Controls.Add(refreshButton);
            Controls.Add(addStationButton);
            Controls.Add(stationsCheckBox);
            Controls.Add(filterInput);
            Controls.Add(titleLabel);
            Size = new Size(848, 546);
            MinimumSize = new Size(740, 320);
            Name = "TariffarioView";
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label titleLabel;
        private TextBox filterInput;
        private CheckBox stationsCheckBox;
        private Button addStationButton;
        private Button refreshButton;
        private DataGridView grid;
        private Label statusLabel;
    }
}

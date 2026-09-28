namespace Finance.Desktop
{
    partial class MovimentiReviewDialog
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
            monthInput = new DateTimePicker();
            refreshButton = new Button();
            grid = new DataGridView();
            statusLabel = new Label();
            actionsPanel = new FlowLayoutPanel();
            newButton = new Button();
            editButton = new Button();
            deleteButton = new Button();
            confirmButton = new Button();
            closeButton = new Button();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            //
            // monthInput
            //
            monthInput.Location = new Point(16, 12);
            monthInput.Name = "monthInput";
            monthInput.Size = new Size(180, 25);
            monthInput.TabIndex = 0;
            monthInput.Format = DateTimePickerFormat.Custom;
            monthInput.CustomFormat = "MMMM yyyy";
            monthInput.ShowUpDown = true;
            //
            // refreshButton
            //
            refreshButton.Location = new Point(210, 10);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(100, 30);
            refreshButton.TabIndex = 1;
            refreshButton.Text = "&Aggiorna";
            refreshButton.Click += RefreshButtonClick;
            //
            // grid
            //
            grid.Location = new Point(16, 52);
            grid.Name = "grid";
            grid.Size = new Size(930, 360);
            grid.TabIndex = 2;
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            //
            // statusLabel
            //
            statusLabel.Location = new Point(16, 424);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(930, 40);
            statusLabel.TabIndex = 3;
            statusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            //
            // actionsPanel
            //
            actionsPanel.Location = new Point(16, 472);
            actionsPanel.Name = "actionsPanel";
            actionsPanel.Size = new Size(930, 42);
            actionsPanel.TabIndex = 4;
            actionsPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            actionsPanel.Controls.Add(newButton);
            actionsPanel.Controls.Add(editButton);
            actionsPanel.Controls.Add(deleteButton);
            actionsPanel.Controls.Add(confirmButton);
            actionsPanel.Controls.Add(closeButton);
            //
            // newButton
            //
            newButton.Location = new Point(0, 0);
            newButton.Name = "newButton";
            newButton.Size = new Size(120, 32);
            newButton.TabIndex = 5;
            newButton.Text = "&Nuovo…";
            newButton.Click += NewButtonClick;
            //
            // editButton
            //
            editButton.Location = new Point(0, 0);
            editButton.Name = "editButton";
            editButton.Size = new Size(155, 32);
            editButton.TabIndex = 6;
            editButton.Text = "&Modifica / sposta…";
            editButton.Click += EditButtonClick;
            //
            // deleteButton
            //
            deleteButton.Location = new Point(0, 0);
            deleteButton.Name = "deleteButton";
            deleteButton.Size = new Size(120, 32);
            deleteButton.TabIndex = 7;
            deleteButton.Text = "&Elimina";
            deleteButton.Click += DeleteButtonClick;
            //
            // confirmButton
            //
            confirmButton.Location = new Point(0, 0);
            confirmButton.Name = "confirmButton";
            confirmButton.Size = new Size(180, 32);
            confirmButton.TabIndex = 8;
            confirmButton.Text = "&Conferma selezionati";
            confirmButton.Click += ConfirmButtonClick;
            //
            // closeButton
            //
            closeButton.Location = new Point(0, 0);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(120, 32);
            closeButton.TabIndex = 9;
            closeButton.Text = "&Chiudi";
            closeButton.DialogResult = DialogResult.Cancel;
            //
            // MovimentiReviewDialog
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(962, 530);
            Controls.Add(monthInput);
            Controls.Add(refreshButton);
            Controls.Add(grid);
            Controls.Add(statusLabel);
            Controls.Add(actionsPanel);
            Name = "MovimentiReviewDialog";
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            ShowInTaskbar = false;
            CancelButton = closeButton;
            MinimumSize = new Size(850, 450);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DateTimePicker monthInput;
        private Button refreshButton;
        private DataGridView grid;
        private Label statusLabel;
        private FlowLayoutPanel actionsPanel;
        private Button newButton;
        private Button editButton;
        private Button deleteButton;
        private Button confirmButton;
        private Button closeButton;
    }
}


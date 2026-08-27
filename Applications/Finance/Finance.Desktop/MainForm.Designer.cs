namespace Finance.Desktop
{
    partial class MainForm
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
            menuStrip = new MenuStrip();
            contiMenuItem = new ToolStripMenuItem();
            newContoMenuItem = new ToolStripMenuItem();
            contiMenuSeparator = new ToolStripSeparator();
            accountsPanel = new FlowLayoutPanel();
            menuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new ToolStripItem[] { contiMenuItem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(984, 24);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip";
            // 
            // contiMenuItem
            // 
            contiMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newContoMenuItem, contiMenuSeparator });
            contiMenuItem.Name = "contiMenuItem";
            contiMenuItem.Size = new Size(49, 20);
            contiMenuItem.Text = "&Conti";
            // 
            // newContoMenuItem
            // 
            newContoMenuItem.Name = "newContoMenuItem";
            newContoMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            newContoMenuItem.Size = new Size(196, 22);
            newContoMenuItem.Text = "&Nuovo conto...";
            newContoMenuItem.Click += NewContoMenuItemClick;
            // 
            // contiMenuSeparator
            // 
            contiMenuSeparator.Name = "contiMenuSeparator";
            contiMenuSeparator.Size = new Size(193, 6);
            contiMenuSeparator.Visible = false;
            // 
            // accountsPanel
            // 
            accountsPanel.AutoScroll = true;
            accountsPanel.BackColor = Color.FromArgb(245, 246, 248);
            accountsPanel.Dock = DockStyle.Fill;
            accountsPanel.Location = new Point(0, 24);
            accountsPanel.Name = "accountsPanel";
            accountsPanel.Padding = new Padding(18);
            accountsPanel.Size = new Size(984, 637);
            accountsPanel.TabIndex = 1;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 661);
            Controls.Add(accountsPanel);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            MinimumSize = new Size(720, 480);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Finance";
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip;
        private ToolStripMenuItem contiMenuItem;
        private ToolStripMenuItem newContoMenuItem;
        private ToolStripSeparator contiMenuSeparator;
        private FlowLayoutPanel accountsPanel;
    }
}

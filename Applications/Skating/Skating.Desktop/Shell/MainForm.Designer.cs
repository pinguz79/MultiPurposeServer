namespace Skating.Desktop.Shell
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
            welcomeLabel = new Label();
            menuStrip = new MenuStrip();
            manifestationMenuItem = new ToolStripMenuItem();
            newMenuItem = new ToolStripMenuItem();
            openMenuItem = new ToolStripMenuItem();
            recentSeparator = new ToolStripSeparator();
            registryMenuItem = new ToolStripMenuItem();
            athletesMenuItem = new ToolStripMenuItem();
            disciplinesMenuItem = new ToolStripMenuItem();
            categoriesMenuItem = new ToolStripMenuItem();
            levelsMenuItem = new ToolStripMenuItem();
            schoolsMenuItem = new ToolStripMenuItem();
            choreographiesMenuItem = new ToolStripMenuItem();
            judgesMenuItem = new ToolStripMenuItem();
            criteriaMenuItem = new ToolStripMenuItem();
            racesMenuItem = new ToolStripMenuItem();
            scheduleMenuItem = new ToolStripMenuItem();
            menuStrip.SuspendLayout();
            SuspendLayout();
            //
            // welcomeLabel
            //
            welcomeLabel.Dock = DockStyle.Fill;
            welcomeLabel.Location = new Point(0, 0);
            welcomeLabel.Name = "welcomeLabel";
            welcomeLabel.Padding = new Padding(32);
            welcomeLabel.Size = new Size(1184, 761);
            welcomeLabel.TabIndex = 0;
            welcomeLabel.Text = "Skating System\r\n\r\nCreare o aprire una manifestazione dal menu Manifestazione.\r\n\r\nPrototipo: non usare ancora per una gara reale.\r\nAssolute e ripristino guidato dei backup non sono ancora disponibili.";
            welcomeLabel.TextAlign = ContentAlignment.MiddleCenter;
            //
            // menuStrip
            //
            menuStrip.Items.AddRange(new ToolStripItem[] { manifestationMenuItem, registryMenuItem, choreographiesMenuItem, racesMenuItem, scheduleMenuItem });
            menuStrip.Name = "menuStrip";
            menuStrip.TabIndex = 1;
            manifestationMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newMenuItem, openMenuItem, recentSeparator });
            manifestationMenuItem.DropDown.ShowItemToolTips = true;
            recentSeparator.Name = "recentSeparator";
            recentSeparator.Visible = false;
            manifestationMenuItem.Name = "manifestationMenuItem";
            manifestationMenuItem.Text = "&Manifestazione";
            newMenuItem.Name = "newMenuItem";
            newMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            newMenuItem.Text = "&Nuova...";
            newMenuItem.Click += NewManifestationClick;
            openMenuItem.Name = "openMenuItem";
            openMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            openMenuItem.Text = "&Apri...";
            openMenuItem.Click += OpenManifestationClick;
            registryMenuItem.DropDownItems.AddRange(new ToolStripItem[] { schoolsMenuItem, athletesMenuItem, disciplinesMenuItem, categoriesMenuItem, levelsMenuItem, judgesMenuItem, criteriaMenuItem });
            judgesMenuItem.Name = "judgesMenuItem";
            judgesMenuItem.Text = "&Giudici...";
            judgesMenuItem.Click += JudgesMenuItemClick;
            criteriaMenuItem.Name = "criteriaMenuItem";
            criteriaMenuItem.Text = "&Criteri di valutazione...";
            criteriaMenuItem.Click += CriteriaMenuItemClick;
            racesMenuItem.Name = "racesMenuItem";
            racesMenuItem.Text = "&Gare...";
            racesMenuItem.Enabled = false;
            racesMenuItem.Click += RacesMenuItemClick;
            scheduleMenuItem.Name = "scheduleMenuItem";
            scheduleMenuItem.Text = "&Scaletta e voti...";
            scheduleMenuItem.Enabled = false;
            scheduleMenuItem.Click += ScheduleMenuItemClick;
            schoolsMenuItem.Name = "schoolsMenuItem";
            schoolsMenuItem.Text = "&Scuole...";
            schoolsMenuItem.Click += SchoolsMenuItemClick;
            choreographiesMenuItem.Name = "choreographiesMenuItem";
            choreographiesMenuItem.Text = "&Coreografie...";
            choreographiesMenuItem.Enabled = false;
            choreographiesMenuItem.Click += ChoreographiesMenuItemClick;
            registryMenuItem.Enabled = false;
            registryMenuItem.Name = "registryMenuItem";
            registryMenuItem.Text = "&Anagrafiche";
            athletesMenuItem.Name = "athletesMenuItem";
            athletesMenuItem.Text = "&Atleti...";
            athletesMenuItem.Click += AthletesMenuItemClick;
            disciplinesMenuItem.Name = "disciplinesMenuItem";
            disciplinesMenuItem.Text = "&Discipline...";
            disciplinesMenuItem.Click += DisciplinesMenuItemClick;
            categoriesMenuItem.Name = "categoriesMenuItem";
            categoriesMenuItem.Text = "&Categorie...";
            categoriesMenuItem.Click += CategoriesMenuItemClick;
            levelsMenuItem.Name = "levelsMenuItem";
            levelsMenuItem.Text = "&Livelli...";
            levelsMenuItem.Click += LevelsMenuItemClick;
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 761);
            Controls.Add(welcomeLabel);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            MinimumSize = new Size(800, 600);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Skating System - Prototipo";
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label welcomeLabel;
        private MenuStrip menuStrip;
        private ToolStripMenuItem manifestationMenuItem;
        private ToolStripMenuItem newMenuItem;
        private ToolStripMenuItem openMenuItem;
        private ToolStripSeparator recentSeparator;
        private ToolStripMenuItem registryMenuItem;
        private ToolStripMenuItem athletesMenuItem;
        private ToolStripMenuItem disciplinesMenuItem;
        private ToolStripMenuItem categoriesMenuItem;
        private ToolStripMenuItem levelsMenuItem;
        private ToolStripMenuItem schoolsMenuItem;
        private ToolStripMenuItem choreographiesMenuItem;
        private ToolStripMenuItem judgesMenuItem;
        private ToolStripMenuItem criteriaMenuItem;
        private ToolStripMenuItem racesMenuItem;
        private ToolStripMenuItem scheduleMenuItem;
    }
}

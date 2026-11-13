namespace Skating.Desktop.Common
{
    internal static class OperatingModeAppearance
    {
        internal static Color Background(OperatingMode mode) => mode == OperatingMode.Registry ? Color.FromArgb(255, 239, 210) : Color.FromArgb(214, 235, 250);

        internal static string Caption(OperatingMode mode) => mode == OperatingMode.Registry ? "Modalità Anagrafica" : "Modalità Gara";

        internal static void Apply(Form form, OperatingMode mode)
        {
            form.BackColor = Background(mode);
            var banner = form.Controls.OfType<Label>().SingleOrDefault(label => label.Name == "operatingModeLabel");
            if (banner is null)
            {
                banner = new Label { Name = "operatingModeLabel", Dock = DockStyle.Bottom, Height = 32, Padding = new Padding(8), AutoEllipsis = true };
                form.Controls.Add(banner);
            }

            banner.BackColor = Background(mode);
            banner.ForeColor = Color.Black;
            banner.Text = Caption(mode);
        }
    }
}

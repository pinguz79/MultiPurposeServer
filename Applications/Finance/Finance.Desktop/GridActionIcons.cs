using System.Drawing.Imaging;
using System.Reflection;

namespace Finance.Desktop
{
    internal static class GridActionIcons
    {
        public static readonly Image ArrowDown = Load("arrow-down");
        public static readonly Image ArrowDownDisabled = Disable(ArrowDown);
        public static readonly Image ArrowUp = Load("arrow-up");
        public static readonly Image ArrowUpDisabled = Disable(ArrowUp);
        public static readonly Image CalendarAdd = Load("calendar-add");
        public static readonly Image Delete = Load("delete");
        public static readonly Image DeleteDisabled = Disable(Delete);
        public static readonly Image Edit = Load("edit");

        private static Image Disable(Image source)
        {
            var result = new Bitmap(source.Width, source.Height);
            using var graphics = Graphics.FromImage(result);
            using var attributes = new ImageAttributes();
            attributes.SetColorMatrix(new ColorMatrix { Matrix33 = 0.3F });
            graphics.DrawImage(source, new Rectangle(0, 0, result.Width, result.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
            return result;
        }

        private static Image Load(string name)
        {
            Assembly assembly = typeof(GridActionIcons).Assembly;
            using Stream stream = assembly.GetManifestResourceStream($"Finance.Desktop.Assets.Icons.{name}.png")
                ?? throw new InvalidOperationException($"Icona '{name}' non trovata.");
            using Image image = Image.FromStream(stream);
            return new Bitmap(image);
        }
    }
}

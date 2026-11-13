using System.Drawing.Printing;

namespace Skating.Desktop.Rankings
{
    internal sealed class RankingPrintDocument : PrintDocument
    {
        private readonly string _title;
        private readonly string _status;
        private readonly string[] _headers;
        private readonly string[][] _rows;
        private readonly float[] _weights;
        private readonly Font _font = new("Segoe UI", 10);
        private readonly Font _bold = new("Segoe UI", 10, FontStyle.Bold);
        private readonly Font _heading = new("Segoe UI", 16, FontStyle.Bold);
        private int _nextRow;
        private int _page;

        internal RankingPrintDocument(string title, string status, string[] headers, string[][] rows, float[] weights)
        {
            _title = title;
            _status = status;
            _headers = headers;
            _rows = rows;
            _weights = weights;
            DocumentName = title;
            DefaultPageSettings.Landscape = true;
            DefaultPageSettings.Margins = new Margins(40, 40, 40, 40);
        }

        protected override void OnBeginPrint(PrintEventArgs e)
        {
            _nextRow = 0;
            _page = 0;
            base.OnBeginPrint(e);
        }

        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            if (e.Graphics is null)
            {
                throw new InvalidOperationException("Impossibile preparare la pagina di stampa.");
            }

            e.HasMorePages = RenderPage(e.Graphics, e.MarginBounds);
            base.OnPrintPage(e);
        }

        internal bool RenderPage(Graphics graphics, Rectangle bounds)
        {
            _page++;
            var widths = _weights.Select(weight => bounds.Width * weight / _weights.Sum()).ToArray();
            var titleHeight = graphics.MeasureString(_title, _heading, bounds.Width).Height;
            graphics.DrawString(_title, _heading, Brushes.Black, new RectangleF(bounds.X, bounds.Y, bounds.Width, titleHeight));
            var y = bounds.Y + titleHeight + 8;
            var statusHeight = graphics.MeasureString(_status, _bold, bounds.Width).Height;
            graphics.DrawString(_status, _bold, Brushes.Black, new RectangleF(bounds.X, y, bounds.Width, statusHeight));
            y += statusHeight + 16;
            var headerHeight = RowHeight(graphics, _headers, widths, _bold);
            DrawRow(graphics, _headers, widths, bounds.X, y, headerHeight, _bold, true);
            y += headerHeight;
            var bottom = bounds.Bottom - 32;
            var pageCapacity = bottom - y;
            while (_nextRow < _rows.Length)
            {
                var row = _rows[_nextRow];
                var height = RowHeight(graphics, row, widths, _font);
                if (height > pageCapacity)
                {
                    throw new InvalidOperationException("Una riga è troppo lunga per la pagina. Ridurre il testo o scegliere un formato carta più grande.");
                }

                if (y + height > bottom)
                {
                    break;
                }

                DrawRow(graphics, row, widths, bounds.X, y, height, _font, false);
                y += height;
                _nextRow++;
            }

            if (_rows.Length == 0)
            {
                graphics.DrawString("Nessuna esibizione nella vista selezionata.", _font, Brushes.Black, bounds.X, y + 12);
            }

            graphics.DrawString($"Skating System · Pagina {_page}", _font, Brushes.Black, bounds.X, bounds.Bottom - 20);
            return _nextRow < _rows.Length;
        }

        private static float RowHeight(Graphics graphics, string[] cells, float[] widths, Font font) => Math.Max(30, cells.Select((text, index) => graphics.MeasureString(text, font, (int)Math.Max(1, widths[index] - 12)).Height + 12).Max());

        private static void DrawRow(Graphics graphics, string[] cells, float[] widths, float x, float y, float height, Font font, bool header)
        {
            for (var index = 0; index < cells.Length; index++)
            {
                var rectangle = new RectangleF(x, y, widths[index], height);
                if (header)
                {
                    graphics.FillRectangle(Brushes.Gainsboro, rectangle);
                }

                graphics.DrawRectangle(Pens.Gray, x, y, widths[index], height);
                graphics.DrawString(cells[index], font, Brushes.Black, new RectangleF(x + 6, y + 6, widths[index] - 12, height - 12));
                x += widths[index];
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _font.Dispose();
                _bold.Dispose();
                _heading.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

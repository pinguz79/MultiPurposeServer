using System.Drawing.Drawing2D;

using Finance.Desktop.Models;

namespace Finance.Desktop
{
    public sealed class CoveragePanel : Panel
    {
        private IReadOnlyList<VoceRicorrenteDefinition> _definitions = [];

        public void SetDefinitions(IReadOnlyList<VoceRicorrenteDefinition> definitions)
        {
            _definitions = definitions;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawString("Copertura temporale (non in scala)", Font, SystemBrushes.ControlText, 10, 8);

            if (_definitions.Count == 0)
            {
                return;
            }

            DateOnly[] boundaries = [.. _definitions.SelectMany(definition => new[] { definition.ValidFrom, definition.ValidTo?.AddDays(1) })
                .Where(date => date is not null).Select(date => date!.Value).Distinct().OrderBy(date => date)];

            for (var index = 0; index < _definitions.Count; index++)
            {
                VoceRicorrenteDefinition definition = _definitions[index];
                DrawDefinition(e.Graphics, definition, index, boundaries);
                e.Graphics.DrawString((index + 1).ToString(), Font, SystemBrushes.ControlText, 6, 34 + index * 24);
            }
        }

        private void DrawDefinition(Graphics graphics, VoceRicorrenteDefinition definition, int definitionIndex, DateOnly[] boundaries)
        {
            DateOnly?[] segments = [null, .. boundaries.Cast<DateOnly?>(), null];
            int top = 35 + definitionIndex * 24;

            for (var segmentIndex = 0; segmentIndex < segments.Length - 1; segmentIndex++)
            {
                DateOnly? segmentFrom = segments[segmentIndex];
                DateOnly? segmentTo = segments[segmentIndex + 1]?.AddDays(-1);
                if (!Intersects(definition, segmentFrom, segmentTo))
                {
                    continue;
                }

                int left = Map(segments[segmentIndex], boundaries, true);
                int right = Map(segments[segmentIndex + 1], boundaries, false);
                bool shadowed = _definitions.Take(definitionIndex).Any(previous => Covers(previous, segmentFrom, segmentTo));
                using Brush brush = shadowed
                    ? new HatchBrush(HatchStyle.ForwardDiagonal, Color.Gray, Color.WhiteSmoke)
                    : new SolidBrush(Color.FromArgb(73, 144, 206));
                graphics.FillRectangle(brush, left, top, Math.Max(3, right - left), 15);
            }
        }

        private int Map(DateOnly? date, DateOnly[] boundaries, bool from)
        {
            const int margin = 32;
            if (date is null)
            {
                return from ? margin : Width - margin;
            }

            int index = Array.IndexOf(boundaries, date.Value);
            return boundaries.Length < 2 ? Width / 2
                : margin + index * (Width - margin * 2) / (boundaries.Length - 1);
        }

        private static bool Covers(VoceRicorrenteDefinition definition, DateOnly? from, DateOnly? to)
            => (definition.ValidFrom is null || from is not null && definition.ValidFrom <= from)
                && (definition.ValidTo is null || to is not null && definition.ValidTo >= to);

        private static bool Intersects(VoceRicorrenteDefinition definition, DateOnly? from, DateOnly? to)
            => (definition.ValidTo is null || from is null || definition.ValidTo >= from)
                && (definition.ValidFrom is null || to is null || definition.ValidFrom <= to);
    }
}

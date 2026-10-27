using Skating.Core;

namespace Skating.Desktop.Scoring
{
    internal sealed class ScoreGrid : DataGridView
    {
        private readonly Font _headingFont = new("Segoe UI", 11, FontStyle.Bold);
        private readonly Font _detailFont = new("Segoe UI", 9);
        private readonly Font _voteFont = new("Segoe UI", 12);
        private readonly Font _nameFont = new("Segoe UI", 12, FontStyle.Bold);

        internal Dictionary<Guid, string> JudgeNames { get; } = [];
        internal Dictionary<Guid, ScoreRowVisual> RowVisuals { get; } = [];

        [System.ComponentModel.DefaultValue(false)]
        public bool ColorDiscipline { get; set; }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        internal Func<bool>? ValidatePendingEdit { get; set; }

        [System.ComponentModel.DefaultValue(false)]
        public bool EnterNextPerformance { get; set; }

        public ScoreGrid()
        {
            DoubleBuffered = true;
            BackgroundColor = Color.White;
            BorderStyle = BorderStyle.None;
            CellBorderStyle = DataGridViewCellBorderStyle.None;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            EnableHeadersVisualStyles = false;
            ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(237, 241, 246);
            ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 43, 57);
            ColumnHeadersDefaultCellStyle.Font = _headingFont;
            DefaultCellStyle.Font = _voteFont;
            DefaultCellStyle.SelectionBackColor = Color.White;
            DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 43, 57);
            AllowUserToResizeRows = false;
            AllowUserToOrderColumns = false;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            base.OnCellPainting(e);
            if (e.Graphics is not Graphics graphics || e.ColumnIndex < 0)
            {
                return;
            }

            var bounds = e.CellBounds;
            using var separator = new Pen(Color.FromArgb(222, 228, 235));
            if (e.RowIndex < 0)
            {
                e.PaintBackground(e.ClipBounds, false);
                var textBounds = bounds;
                if (Columns[e.ColumnIndex].Tag is ScoreColumn)
                {
                    textBounds.Y += bounds.Height / 2;
                    textBounds.Height /= 2;
                }

                TextRenderer.DrawText(graphics, Columns[e.ColumnIndex].HeaderText, _detailFont, textBounds, ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                graphics.DrawLine(separator, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
                e.Handled = true;
                return;
            }

            if (Rows[e.RowIndex].Tag is not Performance performance)
            {
                return;
            }

            graphics.FillRectangle(Brushes.White, bounds);
            if (Columns[e.ColumnIndex].Name == "Performance" && RowVisuals.TryGetValue(performance.ChoreographyId, out var visual))
            {
                DrawPerformance(graphics, bounds, visual);
            }
            else if (Columns[e.ColumnIndex].Tag is ScoreColumn scoreColumn)
            {
                var cell = Rows[e.RowIndex].Cells[e.ColumnIndex];
                var box = Rectangle.Inflate(bounds, -9, 0);
                box.Y += (box.Height - 38) / 2;
                box.Height = 38;
                if (!cell.ReadOnly)
                {
                    using var border = new Pen(CurrentCell == cell ? Color.FromArgb(43, 96, 166) : Color.FromArgb(190, 202, 217), CurrentCell == cell ? 2 : 1);
                    graphics.DrawRectangle(border, box);
                }

                var value = Convert.ToString(e.FormattedValue);
                TextRenderer.DrawText(graphics, cell.ReadOnly && string.IsNullOrEmpty(value) ? "—" : value, _voteFont, box,
                    cell.Tag is true ? Color.Silver : cell.ReadOnly ? Color.SlateGray : Color.FromArgb(30, 43, 57), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                var previous = Columns.Cast<DataGridViewColumn>().FirstOrDefault(column => column.DisplayIndex == Columns[e.ColumnIndex].DisplayIndex - 1);
                if (previous?.Tag is not ScoreColumn previousScore || previousScore.JudgeId != scoreColumn.JudgeId)
                {
                    graphics.DrawLine(separator, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom);
                }
            }
            else
            {
                var color = Columns[e.ColumnIndex].Name == "Status" ? performance.IsConfirmed ? Color.Teal : Color.DarkGoldenrod : Color.FromArgb(30, 43, 57);
                var value = Convert.ToString(e.FormattedValue);
                if (Columns[e.ColumnIndex].Name == "Action")
                {
                    var cell = Rows[e.RowIndex].Cells[e.ColumnIndex];
                    var enabled = cell.Tag is true;
                    var box = Rectangle.Inflate(bounds, -10, 0);
                    box.Y += (box.Height - 36) / 2;
                    box.Height = 36;
                    graphics.DrawRectangle(separator, box);
                    TextRenderer.DrawText(graphics, value, _detailFont, box, enabled ? color : Color.Silver, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    if (enabled && CurrentCell == cell && Focused)
                    {
                        ControlPaint.DrawFocusRectangle(graphics, Rectangle.Inflate(box, -3, -3));
                    }
                }
                else
                {
                    TextRenderer.DrawText(graphics, value, Columns[e.ColumnIndex].Name == "Total" ? _voteFont : _detailFont, bounds, color,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
                }
            }

            graphics.DrawLine(separator, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
            e.Handled = true;
        }

        private void DrawPerformance(Graphics graphics, Rectangle bounds, ScoreRowVisual visual)
        {
            if (ColorDiscipline)
            {
                using var background = new SolidBrush(visual.DisciplineColor);
                graphics.FillRectangle(background, new Rectangle(bounds.X + 42, bounds.Y, bounds.Width - 42, bounds.Height));
            }

            TextRenderer.DrawText(graphics, visual.Number.ToString(), _headingFont, new Rectangle(bounds.X, bounds.Y + 17, 42, 27), Color.FromArgb(30, 43, 57),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            var left = bounds.X + 52;
            var width = bounds.Width - 60;
            TextRenderer.DrawText(graphics, visual.Name, _nameFont, new Rectangle(left, bounds.Y + 17, width, 27), Color.FromArgb(30, 43, 57), TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(graphics, visual.School, _detailFont, new Rectangle(left, bounds.Y + 47, width, 22), Color.SlateGray, TextFormatFlags.EndEllipsis);
            var x = left;
            foreach (var (label, color) in new[] { (visual.Discipline, visual.DisciplineColor), (visual.Category, Color.FromArgb(255, 242, 206)), (visual.Level, Color.FromArgb(235, 229, 246)) })
            {
                var size = TextRenderer.MeasureText(label, _detailFont);
                var chip = new Rectangle(x, bounds.Y + 78, Math.Min(size.Width + 10, bounds.Right - x - 8), 25);
                if (chip.Width <= 0)
                {
                    break;
                }

                using var brush = new SolidBrush(color);
                graphics.FillRectangle(brush, chip);
                TextRenderer.DrawText(graphics, label, _detailFont, chip, Color.FromArgb(65, 75, 95), TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                x += chip.Width + 5;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var frozenRight = Columns.Cast<DataGridViewColumn>().Where(column => column.Frozen).Select(column => GetColumnDisplayRectangle(column.Index, false).Right).DefaultIfEmpty(0).Max();
            foreach (var group in Columns.Cast<DataGridViewColumn>().Where(column => column.Tag is ScoreColumn).GroupBy(column => (column.Tag as ScoreColumn)?.JudgeId ?? Guid.Empty))
            {
                var rectangles = group.Select(column => GetCellDisplayRectangle(column.Index, -1, false)).ToArray();
                var left = Math.Max(frozenRight, rectangles.Min(rectangle => rectangle.Left));
                var right = Math.Min(ClientSize.Width, rectangles.Max(rectangle => rectangle.Right));
                if (right <= left)
                {
                    continue;
                }

                var bounds = new Rectangle(left, 0, right - left, ColumnHeadersHeight / 2);
                using var background = new SolidBrush(ColumnHeadersDefaultCellStyle.BackColor);
                using var border = new Pen(Color.FromArgb(222, 228, 235));
                e.Graphics.FillRectangle(background, bounds);
                e.Graphics.DrawRectangle(border, bounds);
                TextRenderer.DrawText(e.Graphics, JudgeNames.GetValueOrDefault(group.Key), _headingFont, bounds, Color.FromArgb(30, 43, 57),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        internal bool TryFinishEdit()
        {
            // EndEdit conclude l'editor, ma non garantisce la validazione che avviene cambiando cella.
            return (ValidatePendingEdit?.Invoke() ?? true) && EndEdit();
        }

        protected override bool ProcessDialogKey(Keys keyData) => Navigate(keyData) || base.ProcessDialogKey(keyData);

        protected override bool ProcessDataGridViewKey(KeyEventArgs e) => Navigate(e.KeyData) || base.ProcessDataGridViewKey(e);

        protected override void OnCellClick(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && Columns[e.ColumnIndex].Name == "Action" && Rows[e.RowIndex].Cells[e.ColumnIndex].Tag is not true)
            {
                return;
            }

            base.OnCellClick(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            // L'attivazione da tastiera è già gestita sul key-down, una sola volta.
            if (CurrentCell?.OwningColumn?.Name == "Action" && e.KeyCode is Keys.Enter or Keys.Space)
            {
                e.Handled = true;
                return;
            }

            base.OnKeyUp(e);
        }

        private bool Navigate(Keys keyData)
        {
            var key = keyData & Keys.KeyCode;
            if (CurrentCell?.OwningColumn?.Name == "Action" && key is Keys.Enter or Keys.Space)
            {
                if (CurrentCell.Tag is true && TryFinishEdit())
                {
                    OnCellClick(new DataGridViewCellEventArgs(CurrentCell.ColumnIndex, CurrentCell.RowIndex));
                }

                return true;
            }

            if (key is not (Keys.Enter or Keys.Tab) || CurrentCell is null)
            {
                return false;
            }

            if (!TryFinishEdit())
            {
                return true;
            }

            var row = CurrentCell.RowIndex;
            var column = CurrentCell.ColumnIndex;
            var direction = keyData.HasFlag(Keys.Shift) ? -1 : 1;
            if (key == Keys.Enter && EnterNextPerformance)
            {
                for (var index = row + direction; index >= 0 && index < RowCount; index += direction)
                {
                    if (!Rows[index].Cells[column].ReadOnly)
                    {
                        MoveToCell(column, index);
                        break;
                    }
                }
            }
            else
            {
                var orderedColumns = Columns.Cast<DataGridViewColumn>().Where(entry => entry.Visible).OrderBy(entry => entry.DisplayIndex).ToList();
                var position = orderedColumns.FindIndex(entry => entry.Index == column);
                for (var index = row * orderedColumns.Count + position + direction; index >= 0 && index < RowCount * orderedColumns.Count; index += direction)
                {
                    var cell = Rows[index / orderedColumns.Count].Cells[orderedColumns[index % orderedColumns.Count].Index];
                    var enabledAction = key == Keys.Tab && cell.OwningColumn?.Name == "Action" && cell.Tag is true;
                    if (!cell.ReadOnly || enabledAction)
                    {
                        MoveToCell(cell.ColumnIndex, cell.RowIndex);
                        break;
                    }
                }
            }

            return true;
        }

        private void MoveToCell(int column, int row)
        {
            // Il setter CurrentCell lancia un'eccezione quando CellValidating annulla lo spostamento.
            if (SetCurrentCellAddressCore(column, row, true, true, false))
            {
                if (!Rows[row].Cells[column].ReadOnly)
                {
                    BeginEdit(true);
                }

                InvalidateCell(column, row);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _headingFont.Dispose();
                _detailFont.Dispose();
                _voteFont.Dispose();
                _nameFont.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

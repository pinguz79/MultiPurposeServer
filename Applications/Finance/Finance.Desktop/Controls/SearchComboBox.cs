using System.Globalization;

namespace Finance.Desktop.Controls
{
    /// <summary>Seleziona le voci esistenti tramite un prefisso digitato senza pause superiori a un secondo.</summary>
    public class SearchComboBox : ComboBox
    {
        private string _prefix = string.Empty;
        private long _lastKey;

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (DropDownStyle == ComboBoxStyle.DropDownList && (!char.IsControl(e.KeyChar) || e.KeyChar == '\b'))
            {
                Search(e.KeyChar, Environment.TickCount64);
                // Evita che la combo nativa selezioni nuovamente usando soltanto l'ultimo carattere.
                e.Handled = true;
            }
            base.OnKeyPress(e);
        }

        internal void Search(char character, long timestamp)
        {
            if (timestamp - _lastKey > 1000)
            {
                _prefix = string.Empty;
            }
            _lastKey = timestamp;
            _prefix = character == '\b' ? (_prefix.Length > 0 ? _prefix[..^1] : string.Empty) : _prefix + character;
            if (_prefix.Length == 0)
            {
                return;
            }
            for (int index = 0; index < Items.Count; index++)
            {
                if (CultureInfo.CurrentCulture.CompareInfo.IsPrefix(GetItemText(Items[index]) ?? string.Empty, _prefix, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace))
                {
                    if (SelectedIndex != index)
                    {
                        SelectedIndex = index;
                        OnSelectionChangeCommitted(EventArgs.Empty);
                    }
                    return;
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode is Keys.Up or Keys.Down or Keys.Home or Keys.End or Keys.PageUp or Keys.PageDown or Keys.Escape)
            {
                _prefix = string.Empty;
            }
            base.OnKeyDown(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            _prefix = string.Empty;
            base.OnLostFocus(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _prefix = string.Empty;
            base.OnMouseDown(e);
        }
    }
}

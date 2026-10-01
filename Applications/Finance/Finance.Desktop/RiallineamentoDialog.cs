namespace Finance.Desktop
{
    public partial class RiallineamentoDialog : Form
    {
        public decimal Principal => principalInput.Value;

        public RiallineamentoDialog(int number, DateOnly date, decimal principal)
        {
            InitializeComponent();
            referenceLabel.Text = $"Capitale residuo dopo la rata {number} del {date:dd/MM/yy}";
            principalInput.Value = principal;
            ActiveControl = principalInput;
        }
    }
}

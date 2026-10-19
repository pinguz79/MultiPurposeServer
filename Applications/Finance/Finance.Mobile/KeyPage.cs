namespace Finance.Mobile
{
    public sealed class KeyPage : ContentPage
    {
        private readonly TaskCompletionSource<string?> _completion = new();

        public Task<string?> Result => _completion.Task;

        public KeyPage()
        {
            Title = "Chiave API";
            var entry = new Entry { IsPassword = true, Placeholder = "Chiave API Finance" };
            var save = new Button { Text = "Salva connessione" };
            var cancel = new Button { Text = "Annulla" };
            save.Clicked += async (_, _) =>
            {
                _completion.TrySetResult(entry.Text ?? "");
                await Navigation.PopModalAsync();
            };
            cancel.Clicked += async (_, _) =>
            {
                _completion.TrySetResult(null);
                await Navigation.PopModalAsync();
            };
            var instructions = new Label { Text = "Inserisci la chiave API già utilizzata dal client desktop. Verrà conservata nella memoria protetta del telefono." };
            Content = new VerticalStackLayout { Padding = 24, Spacing = 16, Children = { instructions, entry, save, cancel } };
        }

        protected override void OnDisappearing()
        {
            _completion.TrySetResult(null);
            base.OnDisappearing();
        }
    }
}

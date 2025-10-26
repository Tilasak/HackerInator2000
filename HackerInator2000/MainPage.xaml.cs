using Microsoft.Maui.Controls;

namespace HackerInator2000;

public partial class MainPage : ContentPage
{
    private readonly List<ICipher> _ciphers = new()
{
    new CaesarCipher(),
    new DesCipher() 
};

    private class LastOperation
    {
        public ICipher Cipher { get; set; }
        public string KeyString { get; set; }
        public string OriginalText { get; set; }
        public string EncryptedText { get; set; }
    }

    private LastOperation? _lastEncryptOperation;

    public MainPage()
    {
        InitializeComponent();
        LoadMethods();

        if (ActionPicker.ItemsSource != null && ActionPicker.ItemsSource.Count > 0)
            ActionPicker.SelectedIndex = 0;

        if (MethodPicker.ItemsSource != null && MethodPicker.ItemsSource.Count > 0)
            MethodPicker.SelectedIndex = 0;
    }
    private void LoadMethods()
    {
        ActionPicker.ItemsSource = new List<string>
        {
            "Шифровать",
            "Дешифровать",
            "Взломать"
        };

        var methodNames = _ciphers.Select(c => c.Name).ToList();
        MethodPicker.ItemsSource = methodNames;
        MethodPicker.SelectedIndex = 0;

    }

    private void OnActionChanged(object sender, EventArgs e)
    {
        string? action = ActionPicker.SelectedItem as string;
        string? methodName = MethodPicker.SelectedItem as string;

        var cipher = _ciphers.FirstOrDefault(c => c.Name == methodName);

        bool isBreak = action == "Взломать";
        KeyInputLayout.IsVisible = !isBreak;

        
        if (cipher != null && !cipher.SupportsBreak && isBreak)
        {
         
        }

        QuickActionsLayout.IsVisible = false;
    }

    private async void OnExecuteClicked(object sender, EventArgs e)
    {
        string input = InputEditor.Text;
        if (string.IsNullOrWhiteSpace(input))
        {
            await DisplayAlert("Ошибка", "Введите текст!", "OK");
            return;
        }

        string? action = ActionPicker.SelectedItem as string;
        string? methodName = MethodPicker.SelectedItem as string;

        if (string.IsNullOrEmpty(action) || string.IsNullOrEmpty(methodName))
        {
            await DisplayAlert("Ошибка", "Выберите действие и метод!", "OK");
            return;
        }

        var cipher = _ciphers.First(c => c.Name == methodName);

        if (action == "Взломать" && !cipher.SupportsBreak)
        {
            await DisplayAlert("Ошибка", $"Метод '{cipher.Name}' не поддерживает взлом.", "OK");
            return;
        }

        if (action == "Взломать")
        {
            var results = cipher.Break(input);
            OutputEditor.Text = string.Join("\n", results);
            _lastEncryptOperation = null;
            QuickActionsLayout.IsVisible = false;
        }
        else
        {
            string key = KeyEntry.Text;
            if (string.IsNullOrWhiteSpace(key))
            {
                await DisplayAlert("Ошибка", "Введите ключ!", "OK");
                return;
            }

            try
            {
                string result = action == "Шифровать"
                    ? cipher.Encrypt(input, key)
                    : cipher.Decrypt(input, key);

                OutputEditor.Text = result;

                if (action == "Шифровать")
                {
                    _lastEncryptOperation = new LastOperation
                    {
                        Cipher = cipher,
                        KeyString = key, 
                        OriginalText = input,
                        EncryptedText = result
                    };
                    QuickActionsLayout.IsVisible = true;
                }
                else
                {
                    _lastEncryptOperation = null;
                    QuickActionsLayout.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка: {ex.Message}", "OK");
            }
        }
    }

    private async void OnDecryptResultClicked(object sender, EventArgs e)
    {
        if (_lastEncryptOperation == null) return;

        try
        {
            string decrypted = _lastEncryptOperation.Cipher.Decrypt(
                _lastEncryptOperation.EncryptedText,
                _lastEncryptOperation.KeyString 
            );
            OutputEditor.Text = decrypted;
            QuickActionsLayout.IsVisible = false;
            _lastEncryptOperation = null;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось дешифровать: {ex.Message}", "OK");
        }
    }

    private async void OnBreakResultClicked(object sender, EventArgs e)
    {
        if (_lastEncryptOperation == null) return;
        if (!_lastEncryptOperation.Cipher.SupportsBreak)
        {
            await DisplayAlert("Ошибка", "Этот метод нельзя взломать.", "OK");
            return;
        }

        try
        {
            var results = _lastEncryptOperation.Cipher.Break(_lastEncryptOperation.EncryptedText);
            OutputEditor.Text = string.Join("\n", results);
            QuickActionsLayout.IsVisible = false;
            _lastEncryptOperation = null;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось взломать: {ex.Message}", "OK");
        }
    }

    private void OnMethodChanged(object sender, EventArgs e)
    {
        string? methodName = MethodPicker.SelectedItem as string;
        var cipher = _ciphers.FirstOrDefault(c => c.Name == methodName);

        if (cipher != null)
        {
            if (cipher is CaesarCipher)
            {
                KeyEntry.Placeholder = "Число (1–32)";
            }
            else if (cipher is DesCipher)
            {
                KeyEntry.Placeholder = "Ключ (до 8 символов, например: secret12)";
            }
        }
    }
}
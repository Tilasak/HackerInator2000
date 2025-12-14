using Microsoft.Maui.Controls;
using System.Text;

namespace HackerInator2000;

public partial class MainPage : ContentPage
{
    private readonly List<ICipher> _ciphers = new()
    {
        new CaesarCipher(),
        new DesCipher(),
        new AdfgvxCipher(),
        new StreamCipher(),
        new HashCipher(),
        new ElGamalSignature(),
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

    private void UpdateElGamalUI()
    {
        string? action = ActionPicker.SelectedItem as string;

        if (action == "Подписать")
        {
            XLabel.IsVisible = true; XEditor.IsVisible = true;
            YLabel.IsVisible = false; YEditor.IsVisible = false;
            SigLabel.IsVisible = false; SigEditor.IsVisible = false;
        }
        else if (action == "Проверить подпись")
        {
            XLabel.IsVisible = false; XEditor.IsVisible = false;
            YLabel.IsVisible = true; YEditor.IsVisible = true;
            SigLabel.IsVisible = true; SigEditor.IsVisible = true;
        }

        QuickActionsLayout.IsVisible = false;
    }

    private void OnActionChanged(object sender, EventArgs e)
    {
        string? action = ActionPicker.SelectedItem as string;
        string? methodName = MethodPicker.SelectedItem as string;

        var cipher = _ciphers.FirstOrDefault(c => c.Name == methodName);

        bool isBreak = action == "Взломать";
        KeyInputLayout.IsVisible = !isBreak && !(cipher is ElGamalSignature);
        ElGamalParamsLayout.IsVisible = cipher is ElGamalSignature;

        if (cipher != null)
        {
            if (cipher is ElGamalSignature)
            {
                UpdateElGamalUI();
            }
            else if (!cipher.SupportsBreak && isBreak)
            {
            
            }
        }
    }

    private void OnMethodChanged(object sender, EventArgs e)
    {
        string? methodName = MethodPicker.SelectedItem as string;
        var cipher = _ciphers.FirstOrDefault(c => c.Name == methodName);

        if (cipher is ElGamalSignature)
        {
            ActionPicker.ItemsSource = new List<string>
            {
                "Подписать",
                "Проверить подпись"
            };
            ActionPicker.SelectedIndex = 0;
            KeyInputLayout.IsVisible = false;
            ElGamalParamsLayout.IsVisible = true;
            UpdateElGamalUI();
        }
        else
        {
            ActionPicker.ItemsSource = new List<string>
            {
                "Шифровать",
                "Дешифровать",
                "Взломать"
            };
            ActionPicker.SelectedIndex = Math.Min(MethodPicker.SelectedIndex, 2);
            KeyInputLayout.IsVisible = true;
            ElGamalParamsLayout.IsVisible = false;

            if (cipher != null)
            {
                KeyEntry.IsEnabled = true;
                KeyEntry.Placeholder = "Введите ключ";
                KeyEntry.Text = string.Empty;

                if (cipher is CaesarCipher)
                {
                    KeyEntry.Placeholder = "Число (1–32)";
                }
                else if (cipher is DesCipher)
                {
                    KeyEntry.Placeholder = "Ключ (до 8 символов, например: secret12)";
                }
                else if (cipher is AdfgvxCipher)
                {
                    KeyEntry.Placeholder = "Ключевое слово (латиница, например: secret)";
                    KeyEntry.Keyboard = Keyboard.Text;
                }
                else if (cipher is StreamCipher)
                {
                    KeyEntry.Placeholder = "Любая строка (например: secret)";
                    KeyEntry.Keyboard = Keyboard.Default;
                }
                else if (cipher is HashCipher)
                {
                    KeyEntry.Placeholder = "Ключ игнорируется";
                    KeyEntry.IsEnabled = false;
                    KeyEntry.Text = string.Empty;
                }
                else if (cipher is ElGamalSignature)
                {
                    KeyEntry.Placeholder = "Для подписи: p,g,x\nДля проверки: p,g,y,текст";
                    KeyEntry.Keyboard = Keyboard.Text;
                }
            }
        }
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

        if (cipher is ElGamalSignature elGamal)
        {
            try
            {
                if (action == "Подписать")
                {
                    string pStr = PEditor.Text.Trim(), gStr = GEditor.Text.Trim(), xStr = XEditor.Text.Trim();
                    if (string.IsNullOrEmpty(pStr) || string.IsNullOrEmpty(gStr) || string.IsNullOrEmpty(xStr))
                        throw new ArgumentException("Все поля (p, g, x) должны быть заполнены.");

                    string keyStr = $"{pStr},{gStr},{xStr}";
                    string result = elGamal.Encrypt(input, keyStr);
                    OutputEditor.Text = result;

                    _lastEncryptOperation = new LastOperation
                    {
                        Cipher = elGamal,
                        KeyString = keyStr,
                        OriginalText = input,
                        EncryptedText = result
                    };
                    QuickActionsLayout.IsVisible = false;
                }
                else if (action == "Проверить подпись")
                {
                    string pStr = PEditor.Text.Trim(), gStr = GEditor.Text.Trim(), yStr = YEditor.Text.Trim();
                    string signature = SigEditor.Text.Trim();

                    if (string.IsNullOrEmpty(pStr) || string.IsNullOrEmpty(gStr) || string.IsNullOrEmpty(yStr))
                        throw new ArgumentException("Поля p, g, y должны быть заполнены.");
                    if (string.IsNullOrEmpty(signature))
                        throw new ArgumentException("Подпись (r,s) должна быть указана.");

                    string publicKeyStr = $"{pStr},{gStr},{yStr}";
                    string result = elGamal.Decrypt(signature, $"{publicKeyStr},{input}");
                    OutputEditor.Text = result;
                    _lastEncryptOperation = null;
                    QuickActionsLayout.IsVisible = false;
                }
                else
                {
                    throw new InvalidOperationException("Неизвестное действие для Эль-Гамаля");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Эль-Гамаль: {ex.Message}", "OK");
            }
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

            if (!(cipher is HashCipher) && string.IsNullOrWhiteSpace(key))
            {
                await DisplayAlert("Ошибка", "Введите ключ!", "OK");
                return;
            }

            try
            {
                string result = action switch
                {
                    "Шифровать" => cipher.Encrypt(input, key),
                    "Дешифровать" => cipher.Decrypt(input, key),
                    _ => throw new InvalidOperationException("Неизвестное действие")
                };

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

    private void OnGenerateElGamalClicked(object sender, EventArgs e)
    {
        PEditor.Text = "2357";
        GEditor.Text = "2";
        XEditor.Text = "1751";
        YEditor.Text = "1185";
    }

    private async void OnElGamalHelpClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Как использовать Эль-Гамаля",
            "Этот метод реализует цифровую подпись.\n\n" +
            "🔹 Подписать:\n" +
            "   • p, g, x — параметры подписывающего.\n" +
            "   • Текст — в основное поле.\n" +
            "   • Результат: подпись вида \"r,s\".\n\n" +
            "🔹 Проверить подпись:\n" +
            "   • p, g, y — публичный ключ автора.\n" +
            "   • r,s — подпись.\n" +
            "   • Исходный текст — в основное поле.\n\n" +
            "Пример из учебника:\n" +
            "p=2357, g=2, x=1751 → y=1185.",
            "Понятно");
    }

    private long ModPow(long b, long e, long m)
    {
        if (m == 1) return 0;
        b %= m;
        if (b < 0) b += m;
        long result = 1;
        while (e > 0)
        {
            if ((e & 1) == 1)
                result = (result * b) % m;
            b = (b * b) % m;
            e >>= 1;
        }
        return result;
    }
}
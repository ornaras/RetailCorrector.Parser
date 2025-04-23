using System.ComponentModel;

namespace RetailCorrector
{
    public class Option(string key, string value, Predicate<string>? check = null) : INotifyPropertyChanged
    {
        public string Key { get; set; } = key;
        public string Value => _value;
        public string TextValue
        {
            get => _value;
            set
            {
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextValue)));
            }
        }
        public Predicate<string> Check { get; set; } = check ?? (_ => true);

        private string _value = value;

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class DateOption(string key, DateOnly value): 
        Option(key, value.ToString(DATE_FORMAT), CheckDate)
    {
        public DateOption(string key, DateTime value): this(key, DateOnly.FromDateTime(value)) { }
        public new DateOnly Value => DateOnly.ParseExact(TextValue, DATE_FORMAT, FORMAT_PROVIDER);

        private static bool CheckDate(string text) =>
            DateOnly.TryParseExact(text, DATE_FORMAT, FORMAT_PROVIDER, 0, out _);
    }


    public class VatinOption(string key, long value = 0): Option(key, $"{value:D12}", CheckVatin)
    {
        private static bool CheckVatin(string text) =>
            (text.Length == 10 || text.Length == 12) && long.TryParse(text, out _);
    }
}

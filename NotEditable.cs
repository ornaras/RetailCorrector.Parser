using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace RetailCorrector
{
    public partial class Parser : UserControl, INotifyPropertyChanged
    {
        public event Action<List<Receipt>>? OnSearched;
        public event Action? OnSearchBegin;

        public int CurrentProgress
        {
            get => _progress;
            set
            {
                if (_progress == value) return;
                _progress = value;
                OnPropertyChanged();
            }
        }
        public int MaxProgress
        {
            get => _maxProgress;
            set
            {
                if (_maxProgress == value) return;
                _maxProgress = value;
                OnPropertyChanged();
            }
        }

        private int _progress = 0;
        private int _maxProgress = 0;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Parser()
        {
            InitializeComponent();
        }

        private void Search(object sender, System.Windows.RoutedEventArgs e) =>
            new Thread(Parse) { IsBackground = true }.Start();

        private void CellEditEnded(object sender, DataGridCellEditEndingEventArgs e)
        {
            #pragma warning disable S1656
            var item = (Option)e.Row.Item;
            if (!item.Check(((TextBox)e.EditingElement).Text))
                item.TextValue = item.TextValue;
            #pragma warning restore S1656
        }
    }
}

using System.Collections.ObjectModel;

namespace RetailCorrector
{
    public partial class Parser
    {
        public ObservableCollection<Option> KVPairs { get; } = [
            // Настройки парсера
            ];  

        private void Search(object sender, System.Windows.RoutedEventArgs e)
        {
            var receipts = new List<Receipt>();
            // todo парсинг чеков
            OnSearched?.Invoke(receipts);
        }
    }    
}

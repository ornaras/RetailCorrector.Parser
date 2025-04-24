using System.Collections.ObjectModel;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.Http;
using System.Text.Json.Nodes;

namespace RetailCorrector
{
    public partial class Parser
    {
        public ObservableCollection<Option> KVPairs { get; } = [
            // Настройки парсера
            ];  

        private void Parse()
        {
            Dispatcher.Invoke(() => OnSearchBegin?.Invoke());
            var receipts = new List<Receipt>();
            // todo парсинг чеков
            Dispatcher.Invoke(() => OnSearched?.Invoke(receipts));
        }
    }    
}

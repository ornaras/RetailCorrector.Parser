using System.Collections.ObjectModel;

namespace RetailCorrector
{
    public partial class Parser
    {
        public ObservableCollection<Option> KVPairs { get; } = [
            // Настройки парсера
            ];  

        private List<Receipt> Parse(CancellationToken cancellationToken)
        {
            var receipts = new List<Receipt>();
            // todo парсинг чеков
            return receipts;
        }
    }    
}

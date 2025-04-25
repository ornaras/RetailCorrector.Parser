using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Windows;

namespace RetailCorrector
{
    public partial class Parser
    {
        public ObservableCollection<Option> KVPairs { get; } = [
            new DateOption("Начальная дата поиска", DateTime.Today),
            new DateOption("Конечная дата поиска", DateTime.Today),
            new VatinOption("ИНН"),
            new Option("Регистрационный номер", ""),
            new Option("Токен", ""),
            ];

        public DateTime FromDate => ((DateOption)KVPairs[0]).Value;
        public DateTime ToDate => ((DateOption)KVPairs[1]).Value;
        public string Vatin => KVPairs[2].Value;
        public string RegId => KVPairs[3].Value;
        public string Token => KVPairs[4].Value;

        private async Task<List<Receipt>> Parse(CancellationToken cancellationToken)
        {
            Dispatcher.Invoke(() => CurrentProgress = 0);
            var uri = $"https://ofd.ru/api/integration/v2/inn/{Vatin}/kkt/{RegId}/receipts-info";
            using var http = new HttpClient
            {
                BaseAddress = new Uri(uri)
            };
            var receipts = new List<Receipt>();
            Dispatcher.Invoke(() => MaxProgress = (int)(ToDate - FromDate).TotalDays + 1);
            
            for (var date = FromDate; date <= ToDate; date = date.AddDays(1))
            {
                if (cancellationToken.IsCancellationRequested) return [];
                LogInfo($"Получение чеков за {date:dd.MM.yyyy}");
                JsonNode? jsonobj = new JsonObject();
                var dText = date.ToString(DATE_FORMAT);
                var @params = $"?dateFrom={dText}T00:00:00&dateTo={dText}T23:59:59&AuthToken={Token}";
                int countTry = 0;
                do
                {
                    if (cancellationToken.IsCancellationRequested) return [];
                    countTry++;
                    using var req = new HttpRequestMessage(HttpMethod.Get, @params);
                    using var resp = await http.SendAsync(req);
                    if (cancellationToken.IsCancellationRequested) return [];
                    if (!resp.IsSuccessStatusCode)
                    {
                        LogError($"Не удалось получить чеки: {(int)resp.StatusCode}", null);
                        Thread.Sleep(countTry * 1000);
                        continue;
                    }
                    var stream = await resp.Content.ReadAsStringAsync();
                    if (cancellationToken.IsCancellationRequested) return [];
                    jsonobj = JsonNode.Parse(stream);
                    if (jsonobj?["Status"]?.GetValue<string>() == "Success")
                        break;
                    LogError($"Не удалось получить чеки\n{jsonobj?.ToJsonString()}", null);
                } while (countTry < 3);

                if (countTry == 3)
                {
                    MessageBox.Show($"Не удалось выгрузить чеки за {dText}!", "Проблема с чеками...");
                    continue;
                }
                var json = jsonobj!["Data"]!.AsArray()!;
                foreach (var item in json)
                {
                    if (cancellationToken.IsCancellationRequested) return [];
                    var payment = new Payment
                    {
                        Cash = item!["CashSumm"]!.GetValue<uint>(),
                        ECash = item!["ECashSumm"]!.GetValue<uint>(),
                        Pre = item!["PrepaidSumm"]!.GetValue<uint>(),
                        Post = item!["CreditSumm"]!.GetValue<uint>(),
                        Provision = item!["ProvisionSumm"]!.GetValue<uint>(),
                    };
                    var receipt = new Receipt
                    {
                        ActNumber = null,
                        Created = DateTime.ParseExact(
                            item!["DocDateTime"]!.GetValue<string>(),
                            "yyyy'-'MM'-'dd'T'HH':'mm':'ss", FORMAT_PROVIDER),
                        FiscalSign = item!["DecimalFiscalSign"]!.GetValue<string>(),
                        Items = new Position[item!["Depth"]!.GetValue<int>()],
                        RoundedSum = item!["TotalSumm"]!.GetValue<uint>(),
                        Payment = payment,
                        Operation = item!["OperationType"]!.GetValue<string>().ToLower() switch
                        {
                            "income" => Operation.Income,
                            "expense" => Operation.Outcome,
                            "refund income" => Operation.RefundIncome,
                            "refund expense" => Operation.RefundOutcome,
                            _ => throw new ArgumentOutOfRangeException("OperationType")
                        }
                    };
                    var positions = item["Items"]!.AsArray();
                    for (var i = 0; i < receipt.Items.Length; i++)
                    {
                        if (cancellationToken.IsCancellationRequested) return [];
                        var pos = positions[i]!;
                        receipt.Items[i] = new Position
                        {
                            Name = pos["Name"]!.GetValue<string>(),
                            Price = pos["Price"]!.GetValue<uint>(),
                            Quantity = (uint)(pos["Quantity"]!.GetValue<double>() * 1000),
                            TotalSum = pos["Total"]!.GetValue<uint>(),
                            MeasureUnit = (MeasureUnit)(int.Parse(pos["ProductUnitOfMeasure"]?.GetValue<string>() ?? "255")),
                            PayType = (PaymentType)pos["CalculationMethod"]!.GetValue<int>(),
                            PosType = (PositionType)pos["SubjectType"]!.GetValue<int>(),
                            TaxRate = (TaxRate)pos["NDS_Rate"]!.GetValue<int>()
                        };
                    }
                    receipts.Add(receipt);
                }
                Dispatcher.Invoke(() => CurrentProgress++);
            }
            return receipts;
        }
    }    
}

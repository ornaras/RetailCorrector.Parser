using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RetailCorrector
{
    public abstract class AbstractSourceModule(CancellationToken token):
        INotifyPropertyChanged
    {
        protected CancellationToken? _cancelToken = token;

        public event Action<bool, string, Exception?>? OnLog;
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action<string>? OnNotify;

        protected internal void Log(bool isError, string message, Exception? exception = null) =>
            OnLog?.Invoke(isError, message, exception);
        protected internal void Notify(string message) => OnNotify?.Invoke(message);
        protected internal void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public abstract Task<bool> OnLoad();
        public abstract Task<IEnumerable<Receipt>> Parse();
        public abstract Task OnUnload();
    }
}

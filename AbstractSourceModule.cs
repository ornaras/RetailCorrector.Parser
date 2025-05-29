using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RetailCorrector
{
    public abstract class AbstractSourceModule():
        INotifyPropertyChanged
    {
        public event Action<bool, string, Exception?>? OnLog;
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action<string>? OnNotify;
        public event Action<int>? ParseStarted;
        public event Action<int>? ProgressUpdated;

        protected internal void Log(bool isError, string message, Exception? exception = null) =>
            OnLog?.Invoke(isError, message, exception);
        protected internal void OnParseStarted(int maxProgress) => ParseStarted?.Invoke(maxProgress);
        protected internal void OnProgressUpdated(int value) => ProgressUpdated?.Invoke(value);
        protected internal void Notify(string message) => OnNotify?.Invoke(message);
        protected internal void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public abstract Task<bool> OnLoad();
        public abstract Task<IEnumerable<Receipt>> Parse(CancellationToken token);
        public abstract Task OnUnload();
    }
}

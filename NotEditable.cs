﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace RetailCorrector
{
    public delegate void ParserLogger(bool error, string text, Exception? exception);
    public partial class Parser : UserControl, INotifyPropertyChanged
    {
        public event Action<List<Receipt>>? OnSearched;
        public event Action? OnSearchBegin;
        private CancellationTokenSource cancelSource = new();
        public bool IsEnabledSearch => cancelSource.IsCancellationRequested;
        public bool IsEnabledCancel => !cancelSource.IsCancellationRequested;
        public event ParserLogger? Log;

        private void LogError(string text, Exception? exception = null) =>
            Log?.Invoke(true, text, exception);

        private void LogInfo(string text) => Log?.Invoke(false, text, null);

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
            cancelSource.Cancel();
            InitializeComponent();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            cancelSource.Cancel();
            OnPropertyChanged(nameof(IsEnabledSearch));
            OnPropertyChanged(nameof(IsEnabledCancel));
        }

        private async void Search(object sender, RoutedEventArgs e)
        {
            cancelSource = new CancellationTokenSource();
            OnPropertyChanged(nameof(IsEnabledSearch));
            OnPropertyChanged(nameof(IsEnabledCancel));
            Dispatcher.Invoke(() => CurrentProgress = 0);
            Dispatcher.Invoke(() => OnSearchBegin?.Invoke());
            var receipts = await Parse(cancelSource.Token);
            Dispatcher.Invoke(() => OnSearched?.Invoke(receipts));
        }

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
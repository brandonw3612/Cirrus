namespace Cirrus.Utilities;

/// <summary>
/// Status watcher for input signals in a certain time window.
/// </summary>
/// <typeparam name="TSignal">Type of the input signal.</typeparam>
public sealed class Sentinel<TSignal>
{
    public required TimeSpan Window { private get; init; }
    public required Func<TSignal, bool> StatusValidator { private get; init; }
    public required Func<TSignal, Task> NegativeReporter { private get; init; }
    public required Func<TSignal, Task> PositiveReporter { private get; init; }

    private TSignal? _signal;
    private readonly Timer _timer;
    private bool _isTimerOccupied;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public Sentinel()
    {
        _timer = new(TimerElapsedCallback, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    private async void TimerElapsedCallback(object? state)
    {
        await PositiveReporter(_signal!);
    }

    public async void Invoke(TSignal signal)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (StatusValidator(signal))
            {
                _signal = signal;
                if (_isTimerOccupied) return;
                _timer.Change(Window, Timeout.InfiniteTimeSpan);
                _isTimerOccupied = true;
            }
            else
            {
                if (!_isTimerOccupied) return;
                _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                _isTimerOccupied = false;
                await NegativeReporter(signal);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
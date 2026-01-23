
namespace pokemon_discord_bot
{
    public class InactivityTimer : IDisposable
    {

        private readonly Timer _timer;
        private readonly TimeSpan _timeoutPeriod;
        private readonly Func<Task> _onTimeoutAsync;

        public InactivityTimer(TimeSpan timeoutPeriod, Func<Task> onTimeoutAsync)
        {
            _timeoutPeriod = timeoutPeriod;
            _onTimeoutAsync = onTimeoutAsync;
            _timer = new Timer(OnTimerElapsed, null, _timeoutPeriod, Timeout.InfiniteTimeSpan);
        }

        private async void OnTimerElapsed(object? state)
        {
            try
            {
                await _onTimeoutAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Timer elapsed error: {ex.Message}");
            }
        }

        public void Reset()
        {
            _timer.Change(_timeoutPeriod, Timeout.InfiniteTimeSpan);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}

using pokemon_discord_bot.Example;
using System.Collections.Concurrent;

namespace pokemon_discord_bot.Services
{
    public class InteractionService
    {
        private ConcurrentDictionary<ulong, IViewInteractable> _interactionViews = new();
        private ConcurrentDictionary<ulong, InactivityTimer> _InteractionTimers = new();

        public void RegisterView(ulong messageId, IViewInteractable view, InactivityTimer viewTimer)
        {
            _interactionViews[messageId] = view;
            _InteractionTimers[messageId] = viewTimer;
        }

        public void UnregisterView(ulong messageId)
        {
            if (_interactionViews.TryRemove(messageId, out _))
            {
                if (_InteractionTimers.TryRemove(messageId, out var timer))
                {
                    timer.Dispose();
                }
            }
        }

        public IViewInteractable? TryGetView(ulong messageId)
        {
            _interactionViews.TryGetValue(messageId, out var view);
            return view;
            
        }

        public InactivityTimer? TryGetViewTimer(ulong messageId)
        {
            _InteractionTimers.TryGetValue(messageId, out var timer);
            return timer;
        }
    }
}

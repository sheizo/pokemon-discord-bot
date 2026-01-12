using pokemon_discord_bot.Example;
using System.Collections.Concurrent;

namespace pokemon_discord_bot.Services
{
    public class InteractionService
    {
        private ConcurrentDictionary<ulong, IViewInteractable> _interactionViews = new();

        public void RegisterView(ulong messageId, IViewInteractable view)
        {
            _interactionViews[messageId] = view;
        }

        public void UnregisterView(ulong messageId)
        {
            _interactionViews.TryRemove(messageId, out _);
        }

        public IViewInteractable? TryGetView(ulong messageId)
        {
            if (_interactionViews.TryGetValue(messageId, out var view))
            {
                return view;
            }
            return null;
        }
    }
}

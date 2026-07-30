using System;
using System.Collections.Generic;

namespace AdeebTask.Core.Events
{
    public sealed class EventBus : IEventBus
    {
        private static class Channel<T> where T : struct
        {
            public static readonly List<Action<T>> Handlers = new List<Action<T>>(8);
        }

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            if (!Channel<T>.Handlers.Contains(handler))
                Channel<T>.Handlers.Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            Channel<T>.Handlers.Remove(handler);
        }

        public void Publish<T>(T evt) where T : struct
        {
            var handlers = Channel<T>.Handlers;
            // Iterate backwards to allow safe unsubscription during event handling
            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                handlers[i].Invoke(evt);
            }
        }
    }
}

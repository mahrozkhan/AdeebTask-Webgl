using System;
using System.Collections.Generic;
using UnityEngine;
using AdeebTask.Core;
using AdeebTask.Core.Events;
using AdeebTask.UI.Screens;

namespace AdeebTask.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private List<AppScreen> _preRegisteredScreens = new List<AppScreen>();

        private readonly Dictionary<Type, AppScreen> _screens = new Dictionary<Type, AppScreen>();
        private AppScreen _currentScreen;

        public void Initialize()
        {
            foreach (var screen in _preRegisteredScreens)
            {
                if (screen != null)
                {
                    _screens[screen.GetType()] = screen;
                    screen.Initialize(); // Initialize event bindings before hiding
                    screen.Hide(); // Ensure all are hidden initially
                }
            }

            var eventBus = ServiceLocator.Get<IEventBus>();
            if (eventBus != null)
            {
                eventBus.Subscribe<GlobalLoadingEvent>(HandleGlobalLoading);
            }
        }

        private void OnDestroy()
        {
            if (ServiceLocator.TryGet<IEventBus>(out var eventBus))
            {
                eventBus.Unsubscribe<GlobalLoadingEvent>(HandleGlobalLoading);
            }
        }

        private void HandleGlobalLoading(GlobalLoadingEvent evt)
        {
            var initScreen = GetScreen<InitScreen>();
            if (initScreen != null)
            {
                if (evt.Show)
                {
                    initScreen.Show();
                    initScreen.transform.SetAsLastSibling();
                    initScreen.UpdateProgress(evt.Progress, evt.Message);
                }
                else
                {
                    // Only hide it if it isn't the currently active screen (e.g. during startup)
                    if (_currentScreen != initScreen)
                    {
                        initScreen.Hide();
                    }
                }
            }
        }

        public T GetScreen<T>() where T : AppScreen
        {
            if (_screens.TryGetValue(typeof(T), out var screen))
            {
                return (T)screen;
            }
            Debug.LogError($"[UIManager] Screen {typeof(T).Name} not found!");
            return null;
        }

        public T Show<T>() where T : AppScreen
        {
            if (_currentScreen != null)
            {
                _currentScreen.Hide();
            }

            var nextScreen = GetScreen<T>();
            if (nextScreen != null)
            {
                _currentScreen = nextScreen;
                nextScreen.Show();
                return nextScreen;
            }
            return null;
        }

        public T Show<T, TData>(TData data) where T : AppScreen<TData>
        {
            if (_currentScreen != null)
            {
                _currentScreen.Hide();
            }

            var nextScreen = GetScreen<T>();
            if (nextScreen != null)
            {
                _currentScreen = nextScreen;
                nextScreen.Show(data);
                return nextScreen;
            }
            return null;
        }
    }
}

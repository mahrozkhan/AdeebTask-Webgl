using System;
using System.Collections.Generic;
using UnityEngine;

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

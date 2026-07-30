using System;
using System.Collections.Generic;
using AdeebTask.Models;
using UnityEngine;
using UnityEngine.UI;
using AdeebTask.Core;
using AdeebTask.Core.Events;

namespace AdeebTask.UI.Screens
{
    public class CardListScreen : AppScreen
    {
        private IEventBus _eventBus;

        [SerializeField] private Button _createNewButton;
        [SerializeField] private Transform _cardsContainer;
        [SerializeField] private ProjectCardView _cardPrefab;

        private List<ProjectCardView> _cardPool = new List<ProjectCardView>();

        private void Awake()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            if (_createNewButton != null)
            {
                _createNewButton.onClick.AddListener(OnCreateButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (_createNewButton != null)
            {
                _createNewButton.onClick.RemoveListener(OnCreateButtonClicked);
            }
            
            foreach (var card in _cardPool)
            {
                if (card != null) card.OnCardClicked -= OnProjectCardClicked;
            }
        }

        public void DisplayProjects(List<ProjectCardData> projects)
        {
            if (_cardPrefab == null || _cardsContainer == null)
            {
                Debug.LogWarning("[CardListScreen] CardPrefab or CardsContainer is missing in Inspector.");
                return;
            }

            // Object Pooling: Reuse existing cards, create new if needed, hide excess
            for (int i = 0; i < Mathf.Max(projects.Count, _cardPool.Count); i++)
            {
                if (i < projects.Count)
                {
                    ProjectCardView card;
                    if (i < _cardPool.Count)
                    {
                        card = _cardPool[i]; // Reuse
                    }
                    else
                    {
                        // Instantiate new and add to pool
                        card = Instantiate(_cardPrefab, _cardsContainer);
                        card.OnCardClicked += OnProjectCardClicked;
                        _cardPool.Add(card);
                    }

                    card.gameObject.SetActive(true);
                    card.Setup(projects[i]);
                }
                else
                {
                    // Hide unused cards in the pool
                    if (_cardPool[i] != null)
                    {
                        _cardPool[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        private void OnCreateButtonClicked()
        {
            _eventBus.Publish(new CreateNewProjectRequestedEvent());
        }

        private void OnProjectCardClicked(string projectId)
        {
            _eventBus.Publish(new OpenProjectRequestedEvent(projectId));
        }
    }
}

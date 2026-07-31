using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using ContentDiscovery.Models;
using ContentDiscovery.Events;
using AdeebTask.Core; 
using AdeebTask.Core.Events; 

namespace ContentDiscovery.Services
{
    public class FirebaseContentService : MonoBehaviour, IDataContentService
    {
        [SerializeField] private string firebaseDatabaseUrl = "https://adeebtask-default-rtdb.asia-southeast1.firebasedatabase.app/StoryLibary.json";//"https://adeebtask-default-rtdb.asia-southeast1.firebasedatabase.app/";


        private IEventBus _eventBus;
        private static readonly Regex CoverInfoRegex = new Regex(@"\""([a-zA-Z0-9_-]+)CoverInfo\""\s*:\s*\""([^\""]+)\""", RegexOptions.Compiled);

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
        }

        public async UniTask<bool> FetchDataAsync()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(firebaseDatabaseUrl))
            {
                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                {
                    await UniTask.Yield();
                }

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    _eventBus?.Publish(new LibraryDataLoadErrorEvent($"Failed to load data: {webRequest.error}"));
                    return false;
                }

                string jsonResponse = webRequest.downloadHandler.text;
                return await ParseDataAsync(jsonResponse);
            }
        }

        private async UniTask<bool> ParseDataAsync(string jsonResponse)
        {
            try
            {
                MatchCollection matches = CoverInfoRegex.Matches(jsonResponse);
                int processedCount = 0;
                var items = new List<ContentItem>(matches.Count);

                foreach (Match match in matches)
                {
                    string id = match.Groups[1].Value;
                    string rawCoverInfo = match.Groups[2].Value;

                    ContentItem item = CoverInfoParser.Parse(id, rawCoverInfo);

                    if (item.ParseSucceeded)
                    {
                        items.Add(item);
                        processedCount++;
                    }

                    if (processedCount % 50 == 0)
                    {
                        await UniTask.Yield();
                    }
                }
                
                // Completely decoupled from Trie Search Engine
                _eventBus?.Publish(new RawContentFetchedEvent(items));
                _eventBus?.Publish(new LibraryDataLoadedEvent());
                return true;
            }
            catch (Exception ex)
            {
                _eventBus?.Publish(new LibraryDataLoadErrorEvent($"Error parsing data: {ex.Message}"));
                return false;
            }
        }
    }
}

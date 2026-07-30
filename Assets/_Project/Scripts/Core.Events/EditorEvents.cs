namespace AdeebTask.Core.Events
{
    public readonly struct BackgroundSelectedEvent 
    { 
        public readonly string AddressableKey; 
        public BackgroundSelectedEvent(string addressableKey) => AddressableKey = addressableKey;
    }
    
    public readonly struct AssetSelectedEvent 
    { 
        public readonly string AddressableKey; 
        public AssetSelectedEvent(string addressableKey) => AddressableKey = addressableKey;
    }
}

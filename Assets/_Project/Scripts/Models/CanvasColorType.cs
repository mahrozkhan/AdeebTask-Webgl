namespace AdeebTask.Models
{
    public enum CanvasColorType
    {
        White,
        LightGray,
        DarkGray,
        Black,
        BlueprintBlue
    }

    public static class CanvasColorTypeExtensions
    {
        public static UnityEngine.Color ToColor(this CanvasColorType type)
        {
            switch (type)
            {
                case CanvasColorType.White: return UnityEngine.Color.white;
                case CanvasColorType.LightGray: return new UnityEngine.Color(0.8f, 0.8f, 0.8f);
                case CanvasColorType.DarkGray: return new UnityEngine.Color(0.2f, 0.2f, 0.2f);
                case CanvasColorType.Black: return UnityEngine.Color.black;
                case CanvasColorType.BlueprintBlue: return new UnityEngine.Color(0.1f, 0.3f, 0.6f);
                default: return UnityEngine.Color.white;
            }
        }
    }
}

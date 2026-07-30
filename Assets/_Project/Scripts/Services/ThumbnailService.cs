using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

namespace AdeebTask.Services
{
    public class ThumbnailService : MonoBehaviour
    {
        public async UniTask<string> CaptureThumbnailBase64Async(Camera targetCamera, int width = 512, int height = 512)
        {
            // Wait for end of frame to ensure rendering is complete
            await UniTask.WaitForEndOfFrame(this);
            
            RenderTexture rt = new RenderTexture(width, height, 24);
            targetCamera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            
            targetCamera.Render();
            
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();
            
            targetCamera.targetTexture = null;
            RenderTexture.active = null; 
            Destroy(rt);
            
            byte[] bytes = screenShot.EncodeToPNG();
            Destroy(screenShot);
            
            return Convert.ToBase64String(bytes);
        }
    }
}

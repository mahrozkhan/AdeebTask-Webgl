using UnityEngine;

namespace AdeebTask.Core
{
    [RequireComponent(typeof(Camera))]
    public class CameraScaler : MonoBehaviour
    {
        [Tooltip("The target resolution width")]
        public float targetWidth = 1920f;
        
        [Tooltip("The target resolution height")]
        public float targetHeight = 1080f;
        
        [Tooltip("The baseline orthographic size when aspect ratio matches perfectly")]
        public float baseOrthographicSize = 5f;

        private Camera _camera;

        private void Start()
        {
            _camera = GetComponent<Camera>();
            AdjustCamera();
        }

        private void Update()
        {
            // In a production app, you might only run this when Screen.width/height changes
            AdjustCamera();
        }

        private void AdjustCamera()
        {
            if (_camera == null || !_camera.orthographic) return;

            float targetAspect = targetWidth / targetHeight;
            float windowAspect = (float)Screen.width / (float)Screen.height;
            float scaleHeight = windowAspect / targetAspect;

            if (scaleHeight < 1.0f) 
            {
                // Screen is narrower than our target (e.g. 4:3 or Portrait)
                // We increase the orthographic size to "zoom out" and fit the content horizontally
                _camera.orthographicSize = baseOrthographicSize / scaleHeight;
            }
            else 
            {
                // Screen is wider than our target (e.g. Ultrawide)
                // We lock the height, and let the sides show extra space
                _camera.orthographicSize = baseOrthographicSize;
            }
        }
    }
}

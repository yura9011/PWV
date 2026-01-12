using UnityEngine;
using UnityEngine.Video;

namespace EtherDomes.UI
{
    /// <summary>
    /// Dibuja el fondo de pantalla para todos los menús.
    /// Puede usar video o imagen estática.
    /// </summary>
    public class MenuBackground : MonoBehaviour
    {
        // Cambiar a true para usar video, false para imagen estática
        private const bool USE_VIDEO = false;
        
        private VideoPlayer _videoPlayer;
        private RenderTexture _renderTexture;
        private Texture2D _backgroundTexture;
        private bool _videoReady = false;
        
        private void Start()
        {
            if (USE_VIDEO)
            {
                SetupVideoPlayer();
            }
            else
            {
                LoadStaticBackground();
            }
        }
        
        private void LoadStaticBackground()
        {
            _backgroundTexture = Resources.Load<Texture2D>("Wallpapers/fondobosquewallpaper");
            
            if (_backgroundTexture == null)
            {
                UnityEngine.Debug.LogWarning("[MenuBackground] No se encontró la imagen, usando gradiente");
                _backgroundTexture = CreateGradientTexture();
            }
            else
            {
                UnityEngine.Debug.Log("[MenuBackground] Imagen de fondo cargada");
            }
        }
        
        private void SetupVideoPlayer()
        {
            // Crear RenderTexture para el video
            _renderTexture = new RenderTexture(1920, 1080, 0);
            _renderTexture.Create();
            
            // Crear VideoPlayer
            _videoPlayer = gameObject.AddComponent<VideoPlayer>();
            _videoPlayer.playOnAwake = false;
            _videoPlayer.isLooping = true;
            _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            _videoPlayer.targetTexture = _renderTexture;
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            
            // Cargar video desde Resources
            VideoClip videoClip = Resources.Load<VideoClip>("Wallpapers/fondobosquewallpaperliver");
            
            if (videoClip != null)
            {
                _videoPlayer.clip = videoClip;
                _videoPlayer.prepareCompleted += OnVideoPrepared;
                _videoPlayer.Prepare();
                UnityEngine.Debug.Log("[MenuBackground] Preparando video de fondo...");
            }
            else
            {
                UnityEngine.Debug.LogWarning("[MenuBackground] No se encontró el video, usando imagen estática");
                LoadStaticBackground();
            }
        }
        
        private void OnVideoPrepared(VideoPlayer source)
        {
            _videoReady = true;
            _videoPlayer.Play();
            UnityEngine.Debug.Log("[MenuBackground] Video de fondo listo");
        }
        
        private Texture2D CreateGradientTexture()
        {
            int width = 2;
            int height = 256;
            Texture2D texture = new Texture2D(width, height);
            
            Color topColor = new Color(0.05f, 0.05f, 0.15f);
            Color bottomColor = new Color(0.1f, 0.15f, 0.2f);
            
            for (int y = 0; y < height; y++)
            {
                float t = (float)y / height;
                Color color = Color.Lerp(bottomColor, topColor, t);
                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
            
            texture.Apply();
            return texture;
        }
        
        private void Update()
        {
            if (!USE_VIDEO || MenuNavigator.Instance == null) return;
            
            bool inMenu = MenuNavigator.Instance.CurrentMenu != MenuType.None;
            
            if (_videoPlayer != null && _videoReady)
            {
                if (inMenu && !_videoPlayer.isPlaying)
                    _videoPlayer.Play();
                else if (!inMenu && _videoPlayer.isPlaying)
                    _videoPlayer.Pause();
            }
        }
        
        private void OnGUI()
        {
            if (MenuNavigator.Instance == null || MenuNavigator.Instance.CurrentMenu == MenuType.None)
                return;
            
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            
            if (USE_VIDEO && _videoReady && _renderTexture != null)
            {
                GUI.DrawTexture(screenRect, _renderTexture, ScaleMode.ScaleAndCrop);
            }
            else if (_backgroundTexture != null)
            {
                GUI.DrawTexture(screenRect, _backgroundTexture, ScaleMode.ScaleAndCrop);
            }
        }
        
        private void OnDestroy()
        {
            if (_videoPlayer != null)
                _videoPlayer.Stop();
            
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
            }
        }
    }
}

using System.Collections;
using UnityEngine;

namespace EtherDomes.UI
{
    /// <summary>
    /// Reproduce música de fondo en loop mientras se está en los menús
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class MenuMusic : MonoBehaviour
    {
        [SerializeField] private float _startDelay = 2f;
        
        private AudioSource _audioSource;
        private AudioClip _menuMusic;
        private Coroutine _delayedPlayCoroutine;
        
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Configurar AudioSource
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
            _audioSource.volume = 0.5f;
        }
        
        private void Start()
        {
            // Cargar música desde Resources
            _menuMusic = Resources.Load<AudioClip>("Music/Mainmenutrackmusic");
            
            if (_menuMusic == null)
            {
                UnityEngine.Debug.LogWarning("[MenuMusic] No se encontró la música en Resources/Music/Mainmenutrackmusic");
                return;
            }
            
            _audioSource.clip = _menuMusic;
            
            // Suscribirse a cambios de menú
            if (MenuNavigator.Instance != null)
            {
                MenuNavigator.Instance.OnMenuChanged += OnMenuChanged;
                
                // Iniciar música con delay si estamos en menú principal
                if (MenuNavigator.Instance.CurrentMenu == MenuType.Principal)
                {
                    PlayMusicWithDelay();
                }
                else if (MenuNavigator.Instance.CurrentMenu != MenuType.None)
                {
                    PlayMusic();
                }
            }
        }
        
        private void OnDestroy()
        {
            if (MenuNavigator.Instance != null)
            {
                MenuNavigator.Instance.OnMenuChanged -= OnMenuChanged;
            }
        }
        
        private void OnMenuChanged(MenuType newMenu)
        {
            if (newMenu == MenuType.None)
            {
                // Salimos del menú, detener música
                StopMusic();
            }
            else if (newMenu == MenuType.Principal && !_audioSource.isPlaying)
            {
                // Entramos al menú principal, reproducir con delay
                PlayMusicWithDelay();
            }
            else if (!_audioSource.isPlaying)
            {
                // Entramos a otro menú, reproducir inmediatamente
                PlayMusic();
            }
        }
        
        private void PlayMusicWithDelay()
        {
            if (_delayedPlayCoroutine != null)
            {
                StopCoroutine(_delayedPlayCoroutine);
            }
            _delayedPlayCoroutine = StartCoroutine(PlayMusicAfterDelay());
        }
        
        private IEnumerator PlayMusicAfterDelay()
        {
            yield return new WaitForSeconds(_startDelay);
            PlayMusic();
            _delayedPlayCoroutine = null;
        }
        
        private void PlayMusic()
        {
            if (_menuMusic != null && !_audioSource.isPlaying)
            {
                _audioSource.Play();
                UnityEngine.Debug.Log("[MenuMusic] Reproduciendo música del menú");
            }
        }
        
        private void StopMusic()
        {
            if (_delayedPlayCoroutine != null)
            {
                StopCoroutine(_delayedPlayCoroutine);
                _delayedPlayCoroutine = null;
            }
            
            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
                UnityEngine.Debug.Log("[MenuMusic] Música detenida");
            }
        }
        
        /// <summary>
        /// Ajusta el volumen de la música (0-1)
        /// </summary>
        public void SetVolume(float volume)
        {
            _audioSource.volume = Mathf.Clamp01(volume);
        }
    }
}

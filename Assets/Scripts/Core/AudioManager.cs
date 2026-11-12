using UnityEngine;

namespace LLG
{
    /// <summary>
    /// Plays the background music for the whole game and remembers the volume.
    /// The track is Assets/Resources/Audio/lofi_village.wav, made by Tools/audio/generate_music.py.
    /// To use a different song, drop a file with the same name into that folder.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        static AudioManager instance;
        AudioSource music;
        float fadeIn;

        public static float Volume
        {
            get => PlayerPrefs.GetFloat("musicVolume", 0.5f);
            set { PlayerPrefs.SetFloat("musicVolume", Mathf.Clamp01(value)); Apply(); }
        }

        public static bool Muted
        {
            get => PlayerPrefs.GetInt("musicMuted", 0) == 1;
            set { PlayerPrefs.SetInt("musicMuted", value ? 1 : 0); Apply(); }
        }

        public static void Ensure()
        {
            if (instance != null) return;
            var go = new GameObject("Audio");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<AudioManager>();
            instance.music = go.AddComponent<AudioSource>();
            instance.music.clip = Resources.Load<AudioClip>("Audio/lofi_village");
            instance.music.loop = true;
            instance.music.ignoreListenerPause = true;
            instance.music.volume = 0;
            if (instance.music.clip != null) instance.music.Play();
            else Debug.LogWarning("Music not found at Resources/Audio/lofi_village");
        }

        static void Apply()
        {
            PlayerPrefs.Save();
            if (instance != null && instance.fadeIn >= 1) instance.music.volume = Muted ? 0 : Volume;
        }

        void Update()
        {
            if (fadeIn >= 1) return;
            fadeIn = Mathf.Min(1, fadeIn + Time.unscaledDeltaTime / 3f);   // fade the music in over 3 seconds
            music.volume = (Muted ? 0 : Volume) * fadeIn;
        }
    }

    /// <summary>Things that start once when the game launches, whatever screen it opens on.</summary>
    public static class GameBoot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            AudioManager.Ensure();
            var cursor = Resources.Load<Texture2D>("Art/Cursor/cursor");
            if (cursor != null) Cursor.SetCursor(cursor, new Vector2(1, 1), CursorMode.Auto);
        }
    }
}

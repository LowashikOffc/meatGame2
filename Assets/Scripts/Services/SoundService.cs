using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sounds
{
    [HideInInspector]
    public string _name;
    public SoundBase _ID;
    public AudioClip _clip;
}

[System.Serializable]
public class SoundPoolSettings
{
    [Header("Pool Settings")]
    public int _maxPoolSize = 20;
    public int _initialPoolSize = 5;
    public bool _autoExpand = true;
}

public class SoundService : MonoBehaviour
{
    public static SoundService Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
        }
        else Destroy(this);
    }

    [SerializeField] private Sounds[] _sounds;
    [SerializeField] private SoundPoolSettings _poolSettings = new SoundPoolSettings();

    private Queue<AudioSource> _audioPool = new Queue<AudioSource>();
    private List<AudioSource> _activeSources = new List<AudioSource>();
    private GameObject _poolContainer;

    private void OnValidate()
    {
        foreach (Sounds s in _sounds)
        {
            if (s._clip != null)
                s._name = s._ID.ToString() + " : " + s._clip.name;
            else
                s._name = s._ID.ToString() + " : NULL";
        }
    }

    #region Pool Initialization

    private void InitializePool()
    {
        if (_poolContainer == null)
        {
            _poolContainer = new GameObject("SoundPool");
            _poolContainer.transform.parent = transform;
        }

        // Создаем начальное количество источников
        for (int i = 0; i < _poolSettings._initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }

        Debug.Log($"SoundPool initialized with {_poolSettings._initialPoolSize} sources");
    }

    private AudioSource CreateNewAudioSource()
    {
        if (_audioPool.Count >= _poolSettings._maxPoolSize && !_poolSettings._autoExpand)
        {
            Debug.LogWarning($"SoundPool reached max size: {_poolSettings._maxPoolSize}");
            return null;
        }

        GameObject audioObject = new GameObject($"SoundSource_{_audioPool.Count + _activeSources.Count}");
        audioObject.transform.parent = _poolContainer.transform;
        audioObject.SetActive(false);

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.dopplerLevel = 0;
        source.spatialBlend = 1;

        SoundCollision collision = audioObject.AddComponent<SoundCollision>();

        _audioPool.Enqueue(source);

        return source;
    }

    #endregion

    #region Public Methods

    public void PlaySound(SoundBase soundID, Vector3 position, float volume)
    {
        AudioClip newClip = GetClipByID(soundID);
        if (newClip == null) return;

        AudioSource source = GetAudioSource();
        if (source == null) return;

        // Настраиваем источник
        source.clip = newClip;
        source.volume = Mathf.Clamp01(volume);

        SoundCollision col = source.GetComponent<SoundCollision>();
        col.SetVolume(volume);

        if (position == Vector3.zero)
        {
            source.spatialBlend = 0;
            source.transform.position = Vector3.zero;
        }
        else
        {
            source.spatialBlend = 1;
            source.transform.position = position;
        }

        // Активируем и играем
        source.gameObject.SetActive(true);
        source.Play();

        // Добавляем в активные
        _activeSources.Add(source);

        // Запускаем корутину на возврат в пул
        StartCoroutine(ReturnToPoolAfterPlay(source, newClip.length));
        //Debug.Log(source.clip.name);
    }

    public void PlaySound(SoundBase soundID, float volume)
    {
        PlaySound(soundID, Vector3.zero, volume);
    }

    public void PlaySound(SoundBase soundID)
    {
        PlaySound(soundID, Vector3.zero, 1f);
    }

    public void StopAllSounds()
    {
        foreach (AudioSource source in _activeSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
                ReturnToPool(source);
            }
        }
        _activeSources.Clear();
    }

    public void StopSound(SoundBase soundID)
    {
        List<AudioSource> toRemove = new List<AudioSource>();

        foreach (AudioSource source in _activeSources)
        {
            if (source != null && source.isPlaying && source.clip != null)
            {
                Sounds sound = GetSoundByID(soundID);
                if (sound != null && source.clip == sound._clip)
                {
                    source.Stop();
                    ReturnToPool(source);
                    toRemove.Add(source);
                }
            }
        }

        foreach (AudioSource source in toRemove)
        {
            _activeSources.Remove(source);
        }
    }

    #endregion

    #region Pool Management

    private AudioSource GetAudioSource()
    {
        AudioSource source = null;

        // Пытаемся взять из пула
        if (_audioPool.Count > 0)
        {
            source = _audioPool.Dequeue();
        }
        else
        {
            // Если пул пуст, создаем новый
            source = CreateNewAudioSource();

            // Если не удалось создать (достигнут максимум), берем самый старый активный
            if (source == null && _activeSources.Count > 0)
            {
                source = _activeSources[0];
                _activeSources.RemoveAt(0);

                if (source.isPlaying)
                    source.Stop();

                source.gameObject.SetActive(false);
                _audioPool.Enqueue(source);
                source = _audioPool.Dequeue();
            }
        }

        return source;
    }

    private void ReturnToPool(AudioSource source)
    {
        if (source == null) return;

        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);

        // Удаляем из активных
        if (_activeSources.Contains(source))
            _activeSources.Remove(source);

        // Возвращаем в пул, если есть место
        if (_audioPool.Count < _poolSettings._maxPoolSize)
        {
            _audioPool.Enqueue(source);
        }
        else
        {
            // Если пул переполнен, удаляем источник
            Destroy(source.gameObject);
        }
    }

    private IEnumerator ReturnToPoolAfterPlay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (source != null)
        {
            ReturnToPool(source);
        }
    }

    #endregion

    #region Helpers

    private AudioClip GetClipByID(SoundBase soundID)
    {
        foreach (Sounds s in _sounds)
        {
            if (s._ID == soundID)
            {
                return s._clip;
            }
        }
        Debug.LogWarning($"Sound with ID {soundID} not found!");
        return null;
    }

    private Sounds GetSoundByID(SoundBase soundID)
    {
        foreach (Sounds s in _sounds)
        {
            if (s._ID == soundID)
            {
                return s;
            }
        }
        return null;
    }

    #endregion

    #region Editor Methods

    [ContextMenu("Clear Pool")]
    private void ClearPool()
    {
        StopAllSounds();

        // Очищаем пул
        while (_audioPool.Count > 0)
        {
            AudioSource source = _audioPool.Dequeue();
            if (source != null)
                Destroy(source.gameObject);
        }

        _activeSources.Clear();
        InitializePool();
        Debug.Log("SoundPool cleared and reinitialized");
    }

    [ContextMenu("Print Pool Status")]
    private void PrintPoolStatus()
    {
        Debug.Log($"=== SoundPool Status ===");
        Debug.Log($"Pool size: {_audioPool.Count}");
        Debug.Log($"Active sources: {_activeSources.Count}");
        Debug.Log($"Max pool size: {_poolSettings._maxPoolSize}");
        Debug.Log($"Total sources: {_audioPool.Count + _activeSources.Count}");
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        StopAllSounds();

        // Очищаем пул
        while (_audioPool.Count > 0)
        {
            AudioSource source = _audioPool.Dequeue();
            if (source != null)
                Destroy(source.gameObject);
        }
    }

    #endregion
}

public enum SoundBase
{
    Step1,
    Step2,
    Step3,
    Step4,
    MudStep1,
    MudStep2,
    MudStep3,
    MudStep4,

    Jump,
    Landing,

    Metal,
    QuietMetal,
    Wood,
    QuietWood,

    Climb1,
    Climb2,
    Climb3,
    Climb4,

    ElectricConnect1,
    ElectricConnect2,
    ElectricConnect3,
    Teleport,

    WoodStep1,
    WoodStep2,
    WoodStep3,
    WoodStep4,

    MetalStep1,
    MetalStep2,
    MetalStep3,
    MetalStep4,
}
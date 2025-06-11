using Unity.Mathematics;
using UnityEngine;


public class VolumeHandler : MonoBehaviour
{
    private static VolumeHandler Instance;
    private void Awake()
    {
        Instance = this;
    }


    [SerializeField] private AudioSource[] sources;
    [SerializeField] private float[] baseVolumes;

    [Range(0, 100)]
    [SerializeField] private float volume;
    public static float Volume => Instance.volume;


    private void Start()
    {
        FindSources();
    }


    public void UpdateMasterVolume(float change)
    {
        volume = math.clamp(volume + change, 0, 100);

        UpdateSources(volume * 0.01f);
    }

    private void UpdateSources(float newVolume01)
    {
        for (int i = 0; i < sources.Length; i++)
        {
            sources[i].volume = newVolume01 * baseVolumes[i];
        }
    }

    [ContextMenu("Find Sources")]
    private void FindSources()
    {
        sources = FindObjectsOfType<AudioSource>(true);
        baseVolumes = new float[sources.Length];

        for (int i = 0; i < sources.Length; i++)
        {
            baseVolumes[i] = sources[i].volume;
        }
    }


#if UNITY_EDITOR

    [ContextMenu("Force Update Sources")]
    private void ForceUpdateSources()
    {
        for (int i = 0; i < sources.Length; i++)
        {
            sources[i].volume = volume * baseVolumes[i] * 0.01f;
        }
    }
#endif
}

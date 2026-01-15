using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Diagnostics;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [UnityEngine.Range(0f, 2f)]
    [SerializeField] private float _masterVolume = 1f;
    [SerializeField] private SoundColection _soundColection;

    [SerializeField] private AudioMixerGroup _sfxMixerGroup;
    [SerializeField] private AudioMixerGroup _musicMixerGroup;

    private AudioSource _currentMusic;

    #region Unity Methods
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }


    private void Start()
    {
    }

    #endregion

    #region Sound Methods
    private void PlayRandomSound(SoundData[] sounds)
    {
        if (sounds != null && sounds.Length > 0)
        {
            SoundData soundData = sounds[Random.Range(0, sounds.Length)];
            SoundToPlay(soundData);
        }
    }

    private void PlaySound(SoundData sound)
    {
        SoundToPlay(sound);
    }

    private void SoundToPlay(SoundData soundData)
    {
        AudioClip clip = soundData.Clip;
        float pitch = soundData.Pitch;
        float volume = soundData.Volume * _masterVolume;
        bool loop = soundData.Loop;
        AudioMixerGroup audioMixerGroup;

        pitch = RandomizePitch(soundData, pitch);
        audioMixerGroup = DetermineAudioMixerGroup(soundData);

        PlaySound(clip, pitch, volume, loop, audioMixerGroup);
    }

    private float RandomizePitch(SoundData soundData, float pitch)
    {
        if (soundData.RandomizePitch)
        {
            float randomPitchModifier = Random.Range(-soundData.RandomPitchRangeModifier, soundData.RandomPitchRangeModifier);
            pitch = soundData.Pitch + randomPitchModifier;
        }

        return pitch;
    }

    private AudioMixerGroup DetermineAudioMixerGroup(SoundData soundData)
    {
        AudioMixerGroup audioMixerGroup;
        switch (soundData.AudioType)
        {
            case SoundData.AudioTypes.SFX:
                audioMixerGroup = _sfxMixerGroup;
                break;
            case SoundData.AudioTypes.Music:
                audioMixerGroup = _musicMixerGroup;
                break;
            default:
                audioMixerGroup = null;
                break;
        }

        return audioMixerGroup;
    }

    private void PlaySound(AudioClip clip, float pitch, float volume, bool loop, AudioMixerGroup audioMixerGroup)
    {
        GameObject soundObject = new GameObject("Temp Audio Source");
        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.outputAudioMixerGroup = audioMixerGroup;
        audioSource.Play();

        if (!loop) Destroy(soundObject, clip.length);
        DetermineMusic(audioMixerGroup, audioSource);
    }

    private void DetermineMusic(AudioMixerGroup audioMixerGroup, AudioSource audioSource)
    {
        if (audioMixerGroup == _musicMixerGroup)
        {
            if (_currentMusic != null)
            {
                _currentMusic.Stop();
            }

            _currentMusic = audioSource;
        }
    }
    #endregion

    #region SFX
    public void Click()
    {
        PlayRandomSound(_soundColection.ButtonPress);
    }

    public void CandyLand()
    {
        PlayRandomSound(_soundColection.CandyLand);
    }

    public void CreateChocolate()
    {
        PlayRandomSound(_soundColection.CreateChocolate);
    }

    public void Combo()
    {
        PlayRandomSound(_soundColection.Combo);
    }

    public void Exclaimations(int i)
    {
        PlaySound(_soundColection.Exclaimations[i]);
    }

    public void LineBlast()
    {
        PlayRandomSound(_soundColection.LineBlast);
    }

    public void Swap()
    {
        PlayRandomSound(_soundColection.Swap);
    }

    public void WrapCandy()
    {
        PlayRandomSound(_soundColection.WrapCandy);
    }

    public void WrongSwap()
    {
        PlayRandomSound(_soundColection.WrongSwap);
    }

    public void Crack()
    {
        PlayRandomSound(_soundColection.Crack);
    }

    public void Heal()
    {
        PlayRandomSound(_soundColection.Heal);
    }
    public void DestroyBoost()
    {
        PlayRandomSound(_soundColection.DestroyBoost);
    }

    public void ComboSound(int s)
    {
        PlaySound(_soundColection.Combo[s]);
    }
    #endregion

    #region Music
    public void MusicGameplay()
    {
        PlayRandomSound(_soundColection.MusicGameplay);
    }

    public void Bomb()
    {
        PlayRandomSound(_soundColection.Bomb);
    }

    public void ColorBomb()
    {
        PlayRandomSound(_soundColection.ColorBomb);
    }
    #endregion

}

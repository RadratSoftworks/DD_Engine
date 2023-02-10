using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneAudioController : MonoBehaviour
{
    public AudioSource normalSoundSource;
    public AudioSource bgmSoundSource;

    public void Play(string audioFileName, string playType)
    {
        AudioClip clip = SoundManager.Instance.GetAudioClip(audioFileName);
        if (clip == null)
        {
            return;
        }
        if (playType.Equals("normal", StringComparison.OrdinalIgnoreCase))
        {
            normalSoundSource.PlayOneShot(clip);
        } else
        {
            if (clip == bgmSoundSource.clip)
            {
                bgmSoundSource.Play();
            }
            else
            {
                bgmSoundSource.Stop();
                bgmSoundSource.clip = clip;
                bgmSoundSource.loop = true;

                bgmSoundSource.Play();
            }
        }
    }

    public void StopNormal()
    {
        normalSoundSource.Stop();
    }

    public void StopAll()
    {
        normalSoundSource.Stop();

        if (bgmSoundSource != null)
        {
            bgmSoundSource.Stop();
        }
    }
}
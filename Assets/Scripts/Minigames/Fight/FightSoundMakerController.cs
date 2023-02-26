using UnityEngine;

namespace DDEngine.Minigame.Fight
{
    public class FightSoundMakerController : MonoBehaviour
    {
        private AudioSource fxAudioSource;

        private AudioClip missSoundClip;
        private AudioClip hitSoundClip;
        private AudioClip hitHardSoundClip;

        private void Awake()
        {
            fxAudioSource = GetComponent<AudioSource>();
        }

        public void Setup(FightSoundInfo soundInfo)
        {
            missSoundClip = SoundManager.Instance.GetAudioClip(soundInfo.MissSoundPath);
            hitSoundClip = SoundManager.Instance.GetAudioClip(soundInfo.HitSoundPath);
            hitHardSoundClip = SoundManager.Instance.GetAudioClip(soundInfo.HitHardSoundPath);
        }

        public void PlayBasedOnAttackResult(FightAttackResult result, FightPunchType punchType)
        {
            switch (result)
            {
                case FightAttackResult.Miss:
                    if (missSoundClip != null)
                    {
                        fxAudioSource.PlayOneShot(missSoundClip);
                    }
                    break;

                case FightAttackResult.DealtDamage:
                case FightAttackResult.KnockedOut:
                    if (punchType == FightPunchType.StrongPunch)
                    {
                        if (hitHardSoundClip != null)
                        {
                            fxAudioSource.PlayOneShot(hitHardSoundClip);
                        }
                    }
                    else
                    {
                        if (hitSoundClip != null)
                        {
                            fxAudioSource.PlayOneShot(hitSoundClip);
                        }
                    }

                    break;

                default:
                    break;
            }
        }
    }
}
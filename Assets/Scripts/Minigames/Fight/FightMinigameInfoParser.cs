using System;
using UnityEngine;

public static class FightMinigameInfoParser
{
    private static bool ParseSound(MinigameVariable soundVar, ref FightSoundInfo soundInfo)
    {
        if (!soundVar.TryGetValue("hit", out soundInfo.HitSoundPath))
        {
            return false;
        }

        if (!soundVar.TryGetValue("hithard", out soundInfo.HitHardSoundPath))
        {
            Debug.LogWarning("Hit hard sound info not found!");
        }

        if (!soundVar.TryGetValue("miss", out soundInfo.MissSoundPath))
        {
            return false;
        }

        return true;
    }

    private static bool ParseAttackInfo(MinigameVariable attack, ref FightAttackTypeInfo attackTypeInfo)
    {
        if (attack.Value is string)
        {
            attackTypeInfo.AnimationPath = attack.Value as string;
        } else
        {
            return false;
        }

        if (!attack.TryGetValue("hittime", out attackTypeInfo.HitTimes))
        {
            return false;
        }

        if (!attack.TryGetValue("hitpower", out attackTypeInfo.HitPower))
        {
            Debug.LogWarning("Unable to get hit power of attack, assign default value");
            attackTypeInfo.HitPower = 10;
        }

        return true;
    }

    private static bool ParsePlayer(MinigameVariable player, ref FightPlayerInfo playerInfo)
    {
        if (!player.ConvertToVector2(out playerInfo.Position))
        {
            return false;
        }

        if (!player.TryGetValue("idle", out playerInfo.IdleAnimPath))
        {
            return false;
        }

        if (!player.TryGetValue("dodge", out playerInfo.DodgeAnimPath))
        {
            return false;
        }

        if (!player.TryGetValue("dodgeend", out playerInfo.DodgeRevertAnimPath))
        {
            return false;
        }

        if (!player.TryGetValue("gettinghit", out playerInfo.GetHitAnimPath))
        {
            return false;
        }

        if (!player.TryGetValue("gameover", out playerInfo.GameOverAnimPath))
        {
            return false;
        }

        if (!player.TryGetValue("knockedout", out playerInfo.KnockedOutAnimPath))
        {
            return false;
        }

        if (player.TryGetValue("jab", out MinigameVariable jabVar))
        {
            if (!ParseAttackInfo(jabVar, ref playerInfo.Jab))
            {
                return false;
            }
        }

        if (player.TryGetValue("punch", out MinigameVariable punchVar))
        {
            if (!ParseAttackInfo(punchVar, ref playerInfo.Punch))
            {
                return false;
            }
        }

        if (player.TryGetValue("strongpunch", out MinigameVariable strongPunch))
        {
            if (!ParseAttackInfo(strongPunch, ref playerInfo.StrongPunch))
            {
                return false;
            }
        }


        if (!player.TryGetValue("sound", out MinigameVariable soundInfoVar)) {
            Debug.LogWarning("Can't find player audio info!");
        } else
        {
            return ParseSound(soundInfoVar, ref playerInfo.SoundInfo);
        }

        return true;
    }

    private static bool ParseOpponent(MinigameVariable opponent, ref FightOpponentInfo opponentInfo)
    {
        if (!opponent.ConvertToVector2(out opponentInfo.Position))
        {
            return false;
        }

        if (!opponent.TryGetValue("idle", out opponentInfo.BodyIdleAnimPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("block", out opponentInfo.BodyBlockAnimPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("punch", out opponentInfo.PunchAnimPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("punch2", out opponentInfo.PunchEffectAnimPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("punch", out MinigameVariable punchVar) || !punchVar.TryGetValue("hittime", out opponentInfo.PunchHitTime))
        {
            return false;
        }

        if (!opponent.TryGetValue("punchprep", out opponentInfo.BodyPunchPrepAnimPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("confused", out opponentInfo.BodyConfusedAnimPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("groggy", out opponentInfo.BodyGroggyAnimPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("gettinghit", out opponentInfo.GettingHitAnimPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("gameover", out opponentInfo.GameOverAnimPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("gameover2", out opponentInfo.GameOver2AnimPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("knockedout", out opponentInfo.KnockedOutAnimPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("head", out opponentInfo.HeadAnimPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("head_confused", out opponentInfo.HeadConfusedAnimPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("head_confused2", out opponentInfo.HeadConfusedAnimEffectPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("left_hand", out opponentInfo.LeftHandAnimPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("left_block", out opponentInfo.LeftHandBlockAnimPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("right_hand", out opponentInfo.RightHandAnimPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("right_block", out opponentInfo.RightHandAnimBlockPath))
        {
            return false;
        }

        if (!opponent.TryGetValue("sound", out MinigameVariable soundInfoVar))
        {
            Debug.LogWarning("Can't find player audio info!");
        }
        else
        {
            return ParseSound(soundInfoVar, ref opponentInfo.SoundInfo);
        }

        return true;
    }

    public static FightMinigameInfo Parse(MinigameVariable root)
    {
        FightMinigameInfo info = new FightMinigameInfo();
        if (!root.TryGetValue("gamewon", out info.FileWonScript))
        {
            return null;
        }

        if (!root.TryGetValue("gamelost", out info.FileLoseScript))
        {
            return null;
        }

        if (!root.TryGetValue("bg", out info.BackgroundPicture))
        {
            Debug.Log("Failed to retrieve the fight's background picture path!");
        }

        if (!root.TryGetValue("bgsound", out info.BackgroundSoundPath)) {
            Debug.Log("Failed to retrieve the fight's background sound path!");
        }

        if (!root.TryGetValue("player", out MinigameVariable playerVar) || !root.TryGetValue("opponent", out MinigameVariable opponentVar))
        {
            Debug.LogErrorFormat("Failed to get player/opponent info variable!");
            return null;
        }

        if (!ParsePlayer(playerVar, ref info.PlayerInfo))
        {
            return null;
        }

        if (!ParseOpponent(opponentVar, ref info.OpponentInfo))
        {
            return null;
        }

        return info;
    }
}

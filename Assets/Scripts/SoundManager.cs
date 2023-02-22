using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

public class SoundManager : GameBaseAssetManager<AudioClip>
{
    // https://github.com/dpwe/dpwelib/blob/master/ulaw.c
    // Thank you!
    private static readonly short[] ULawDecodeTable = {
        -32124, -31100, -30076, -29052, -28028, -27004, -25980, -24956,
        -23932, -22908, -21884, -20860, -19836, -18812, -17788, -16764,
        -15996, -15484, -14972, -14460, -13948, -13436, -12924, -12412,
        -11900, -11388, -10876, -10364,  -9852,  -9340,  -8828,  -8316,
         -7932,  -7676,  -7420,  -7164,  -6908,  -6652,  -6396,  -6140,
         -5884,  -5628,  -5372,  -5116,  -4860,  -4604,  -4348,  -4092,
         -3900,  -3772,  -3644,  -3516,  -3388,  -3260,  -3132,  -3004,
         -2876,  -2748,  -2620,  -2492,  -2364,  -2236,  -2108,  -1980,
         -1884,  -1820,  -1756,  -1692,  -1628,  -1564,  -1500,  -1436,
         -1372,  -1308,  -1244,  -1180,  -1116,  -1052,   -988,   -924,
          -876,   -844,   -812,   -780,   -748,   -716,   -684,   -652,
          -620,   -588,   -556,   -524,   -492,   -460,   -428,   -396,
          -372,   -356,   -340,   -324,   -308,   -292,   -276,   -260,
          -244,   -228,   -212,   -196,   -180,   -164,   -148,   -132,
          -120,   -112,   -104,    -96,    -88,    -80,    -72,    -64,
           -56,    -48,    -40,    -32,    -24,    -16,     -8,      0,
         32124,  31100,  30076,  29052,  28028,  27004,  25980,  24956,
         23932,  22908,  21884,  20860,  19836,  18812,  17788,  16764,
         15996,  15484,  14972,  14460,  13948,  13436,  12924,  12412,
         11900,  11388,  10876,  10364,   9852,   9340,   8828,   8316,
          7932,   7676,   7420,   7164,   6908,   6652,   6396,   6140,
          5884,   5628,   5372,   5116,   4860,   4604,   4348,   4092,
          3900,   3772,   3644,   3516,   3388,   3260,   3132,   3004,
          2876,   2748,   2620,   2492,   2364,   2236,   2108,   1980,
          1884,   1820,   1756,   1692,   1628,   1564,   1500,   1436,
          1372,   1308,   1244,   1180,   1116,   1052,    988,    924,
           876,    844,    812,    780,    748,    716,    684,    652,
           620,    588,    556,    524,    492,    460,    428,    396,
           372,    356,    340,    324,    308,    292,    276,    260,
           244,    228,    212,    196,    180,    164,    148,    132,
           120,    112,    104,     96,     88,     80,     72,     64,
        56,     48,     40,     32,     24,     16,      8,      0
    };

    public static SoundManager Instance;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    private static float[] ULawToPcm(byte[] ulawBuffer)
    {
        float[] pcmBuffer = new float[ulawBuffer.Length];
        for (int i = 0; i < ulawBuffer.Length; i++)
        {
            short value = ULawDecodeTable[ulawBuffer[i]];
            pcmBuffer[i] = (value >= 0) ? (value / 32767.0f) : (value / 32768.0f);
        }
        return pcmBuffer;
    }

    public AudioClip GetAudioClip(string path)
    {
        if (path == null)
        {
            return null;
        }
        string extension = Path.GetExtension(path);
        if ((extension == "") || (extension.Equals(".wav", StringComparison.OrdinalIgnoreCase)))
        {
            path = Path.ChangeExtension(path, ".ul");
        }

        AudioClip cached = GetFromCache(path);
        if (cached != null)
        {
            return cached;
        }

        ResourceFile generalResources = ResourceManager.Instance.GeneralResources;
        if (!generalResources.Exists(path))
        {
            Debug.LogError("Sound file: " + path + " does not exist!");
            return null;
        }

        ResourceInfo soundResourceInfo = generalResources.Resources[path];
        byte[] ulaw = generalResources.ReadResourceData(soundResourceInfo);

        if (ulaw == null)
        {
            Debug.LogError("Can't read sound file: " + path);
            return null;
        }

        AudioClip result = AudioClip.Create(path, ulaw.Length, Constants.SoundChannelCount,
            Constants.SoundFrequency, false);

        result.SetData(ULawToPcm(ulaw), 0);
        AddToCache(path, result);

        return result;
    }
}

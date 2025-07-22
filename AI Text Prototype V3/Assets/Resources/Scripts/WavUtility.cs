using UnityEngine;
using System;
using System.IO;

public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] wavFile, string clipName = "clip")
    {
        int channels = BitConverter.ToInt16(wavFile, 22);
        int sampleRate = BitConverter.ToInt32(wavFile, 24);
        int pos = 44; // WAV header size

        int sampleCount = (wavFile.Length - pos) / 2; // 2 bytes per sample (16-bit audio)
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(wavFile, pos);
            samples[i] = sample / 32768f;
            pos += 2;
        }

        AudioClip audioClip = AudioClip.Create(clipName, sampleCount, channels, sampleRate, false);
        audioClip.SetData(samples, 0);
        return audioClip;
    }
}
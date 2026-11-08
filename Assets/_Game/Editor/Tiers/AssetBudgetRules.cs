using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MGD.Samples.Editor
{
    /// <summary>The import settings of one texture that the budget rules look at.</summary>
    public struct TextureFacts
    {
        public bool HasAndroidOverride;
        public TextureImporterFormat Format;
        public int MaxSize;
        public bool Mipmaps;
        public bool IsSprite;
        public bool IsNormalMap;
    }

    /// <summary>The import settings of one audio clip that the budget rules look at.</summary>
    public struct AudioFacts
    {
        public float LengthSeconds;
        public AudioClipLoadType LoadType;
        public AudioCompressionFormat Format;
        public bool ForceToMono;
    }

    /// <summary>
    /// The Week 9 Lab A Part C import rules as checks: each returns one line per
    /// broken rule, empty when the asset is inside the budget. Kept apart from
    /// the menu item so the rules can be tested without assets.
    /// </summary>
    public static class AssetBudgetRules
    {
        /// <summary>Clips this long or longer are music: streamed, not held in memory.</summary>
        public const float MusicSeconds = 30f;

        /// <summary>Clips shorter than this are short sound effects: decompressed once, played often.</summary>
        public const float ShortSfxSeconds = 1f;

        public static List<string> CheckTexture(TextureFacts facts)
        {
            var problems = new List<string>();
            if (!facts.HasAndroidOverride)
            {
                problems.Add("no Android override: tick Override for Android in the Inspector's Android tab");
            }

            TextureImporterFormat expected = facts.IsSprite || facts.IsNormalMap
                ? TextureImporterFormat.ASTC_4x4
                : TextureImporterFormat.ASTC_6x6;
            if (facts.Format != expected)
            {
                problems.Add($"format {facts.Format}, expected {expected}");
            }

            if (facts.MaxSize > 2048)
            {
                problems.Add($"max size {facts.MaxSize}: 2048 is the ceiling, for atlases and one hero texture");
            }
            else if (facts.MaxSize > 1024)
            {
                problems.Add($"max size {facts.MaxSize}: 1024 unless this is an atlas or the lane's hero texture");
            }

            if (facts.IsSprite && facts.Mipmaps)
            {
                problems.Add("mipmaps on a sprite: turn Generate Mipmaps off");
            }
            else if (!facts.IsSprite && !facts.Mipmaps)
            {
                problems.Add("no mipmaps on a texture drawn scaled: turn Generate Mipmaps on");
            }

            return problems;
        }

        public static List<string> CheckAudio(AudioFacts facts)
        {
            var problems = new List<string>();
            if (facts.LengthSeconds >= MusicSeconds)
            {
                if (facts.LoadType != AudioClipLoadType.Streaming || facts.Format != AudioCompressionFormat.Vorbis)
                {
                    problems.Add(FormattableString.Invariant($"music ({facts.LengthSeconds:0} s) is {facts.LoadType} {facts.Format}: use Streaming, Vorbis"));
                }
            }
            else if (facts.LengthSeconds < ShortSfxSeconds)
            {
                bool pcmOrAdpcm = facts.Format == AudioCompressionFormat.PCM || facts.Format == AudioCompressionFormat.ADPCM;
                if (facts.LoadType != AudioClipLoadType.DecompressOnLoad || !pcmOrAdpcm)
                {
                    problems.Add(FormattableString.Invariant($"short sound ({facts.LengthSeconds:0.0} s) is {facts.LoadType} {facts.Format}: use Decompress On Load, PCM or ADPCM"));
                }
            }
            else if (facts.LoadType != AudioClipLoadType.CompressedInMemory || facts.Format != AudioCompressionFormat.Vorbis)
            {
                problems.Add(FormattableString.Invariant($"sound ({facts.LengthSeconds:0.0} s) is {facts.LoadType} {facts.Format}: use Compressed In Memory, Vorbis"));
            }

            if (!facts.ForceToMono)
            {
                problems.Add("Force To Mono is off: turn it on unless the stereo is audible");
            }

            return problems;
        }
    }
}

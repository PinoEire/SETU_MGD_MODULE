using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MGD.Samples.Editor
{
    public class AssetBudgetRulesTests
    {
        static TextureFacts GoodWorldTexture() => new TextureFacts
        {
            HasAndroidOverride = true,
            Format = TextureImporterFormat.ASTC_6x6,
            MaxSize = 1024,
            Mipmaps = true,
            IsSprite = false,
            IsNormalMap = false
        };

        static TextureFacts GoodSprite() => new TextureFacts
        {
            HasAndroidOverride = true,
            Format = TextureImporterFormat.ASTC_4x4,
            MaxSize = 1024,
            Mipmaps = false,
            IsSprite = true,
            IsNormalMap = false
        };

        [Test]
        public void Texture_FollowingTheRules_HasNoProblems()
        {
            CollectionAssert.IsEmpty(AssetBudgetRules.CheckTexture(GoodWorldTexture()));
            CollectionAssert.IsEmpty(AssetBudgetRules.CheckTexture(GoodSprite()));
        }

        [Test]
        public void Texture_WithoutAnAndroidOverride_IsReported()
        {
            TextureFacts facts = GoodWorldTexture();
            facts.HasAndroidOverride = false;

            StringAssert.Contains("no Android override", AssetBudgetRules.CheckTexture(facts)[0]);
        }

        [Test]
        public void WorldTexture_ExpectsAstc6x6()
        {
            TextureFacts facts = GoodWorldTexture();
            facts.Format = TextureImporterFormat.RGBA32;

            StringAssert.Contains("ASTC_6x6", AssetBudgetRules.CheckTexture(facts)[0]);
        }

        [Test]
        public void SpritesAndNormalMaps_ExpectAstc4x4()
        {
            TextureFacts sprite = GoodSprite();
            sprite.Format = TextureImporterFormat.ASTC_6x6;
            TextureFacts normal = GoodWorldTexture();
            normal.IsNormalMap = true;

            StringAssert.Contains("ASTC_4x4", AssetBudgetRules.CheckTexture(sprite)[0]);
            StringAssert.Contains("ASTC_4x4", AssetBudgetRules.CheckTexture(normal)[0]);
        }

        [TestCase(2048, "1024 unless")]
        [TestCase(4096, "2048 is the ceiling")]
        public void Texture_OverTheSizeBudget_IsReported(int maxSize, string expected)
        {
            TextureFacts facts = GoodWorldTexture();
            facts.MaxSize = maxSize;

            StringAssert.Contains(expected, AssetBudgetRules.CheckTexture(facts)[0]);
        }

        [Test]
        public void Sprite_WithMipmaps_IsReported()
        {
            TextureFacts facts = GoodSprite();
            facts.Mipmaps = true;

            StringAssert.Contains("mipmaps on a sprite", AssetBudgetRules.CheckTexture(facts)[0]);
        }

        [Test]
        public void WorldTexture_WithoutMipmaps_IsReported()
        {
            TextureFacts facts = GoodWorldTexture();
            facts.Mipmaps = false;

            StringAssert.Contains("no mipmaps", AssetBudgetRules.CheckTexture(facts)[0]);
        }

        [Test]
        public void Texture_BreakingSeveralRules_ReportsEach()
        {
            var facts = new TextureFacts
            {
                HasAndroidOverride = false,
                Format = TextureImporterFormat.RGBA32,
                MaxSize = 4096,
                Mipmaps = true,
                IsSprite = true
            };

            Assert.AreEqual(4, AssetBudgetRules.CheckTexture(facts).Count);
        }

        [TestCase(45f, AudioClipLoadType.Streaming, AudioCompressionFormat.Vorbis)]
        [TestCase(0.4f, AudioClipLoadType.DecompressOnLoad, AudioCompressionFormat.PCM)]
        [TestCase(0.4f, AudioClipLoadType.DecompressOnLoad, AudioCompressionFormat.ADPCM)]
        [TestCase(4f, AudioClipLoadType.CompressedInMemory, AudioCompressionFormat.Vorbis)]
        public void Audio_FollowingTheRules_HasNoProblems(float seconds, AudioClipLoadType load, AudioCompressionFormat format)
        {
            var facts = new AudioFacts { LengthSeconds = seconds, LoadType = load, Format = format, ForceToMono = true };

            CollectionAssert.IsEmpty(AssetBudgetRules.CheckAudio(facts));
        }

        [Test]
        public void MaxSize_Of1024_IsInsideTheBudget()
        {
            TextureFacts facts = GoodWorldTexture();
            facts.MaxSize = 1024;

            CollectionAssert.IsEmpty(AssetBudgetRules.CheckTexture(facts));
        }

        [TestCase(30f, AudioClipLoadType.Streaming)]
        [TestCase(29.9f, AudioClipLoadType.CompressedInMemory)]
        [TestCase(1f, AudioClipLoadType.CompressedInMemory)]
        public void AudioLengthBoundaries_PickTheRule(float seconds, AudioClipLoadType expected)
        {
            // 30 s and longer is music; from 1 s up to 30 s is a longer effect.
            var facts = new AudioFacts
            {
                LengthSeconds = seconds,
                LoadType = expected,
                Format = AudioCompressionFormat.Vorbis,
                ForceToMono = true
            };

            CollectionAssert.IsEmpty(AssetBudgetRules.CheckAudio(facts));
        }

        [Test]
        public void Music_NotStreamed_IsReported()
        {
            var facts = new AudioFacts
            {
                LengthSeconds = 120f,
                LoadType = AudioClipLoadType.DecompressOnLoad,
                Format = AudioCompressionFormat.Vorbis,
                ForceToMono = true
            };

            StringAssert.Contains("Streaming", AssetBudgetRules.CheckAudio(facts)[0]);
        }

        [Test]
        public void ShortSfx_CompressedWithVorbis_IsReported()
        {
            var facts = new AudioFacts
            {
                LengthSeconds = 0.3f,
                LoadType = AudioClipLoadType.DecompressOnLoad,
                Format = AudioCompressionFormat.Vorbis,
                ForceToMono = true
            };

            StringAssert.Contains("PCM or ADPCM", AssetBudgetRules.CheckAudio(facts)[0]);
        }

        [Test]
        public void Audio_InStereo_IsReported()
        {
            var facts = new AudioFacts
            {
                LengthSeconds = 4f,
                LoadType = AudioClipLoadType.CompressedInMemory,
                Format = AudioCompressionFormat.Vorbis,
                ForceToMono = false
            };

            StringAssert.Contains("Force To Mono", AssetBudgetRules.CheckAudio(facts)[0]);
        }
    }
}

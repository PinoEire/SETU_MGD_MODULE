using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MGD.Samples.Editor
{
    // Guards what MGD Samples > Set Up Quality Tiers leaves in the project: a render
    // scale left at 0.8 by an interrupted Play session, or a renamed or missing
    // level, fails here before it is committed.
    public class QualityTiersSetupTests
    {
        [Test]
        public void QualityLevels_AreLowMidHighInOrder()
        {
            Assert.IsTrue(DeviceTier.LevelsMatch(QualitySettings.names),
                "Run MGD Samples > Set Up Quality Tiers; levels are " + string.Join(", ", QualitySettings.names));
        }

        [TestCase(Tier.Low, 1)]
        [TestCase(Tier.Mid, 2)]
        [TestCase(Tier.High, 4)]
        public void EachLevel_HasItsOwnUrpAssetAtTheTierScaleAndMsaa(Tier tier, int msaa)
        {
            var asset = QualitySettings.GetRenderPipelineAssetAt((int)tier) as UniversalRenderPipelineAsset;

            Assert.IsNotNull(asset, $"{tier} has no URP asset");
            Assert.AreEqual(DeviceTier.RenderScaleFor(tier), asset.renderScale, 1e-5f, $"{asset.name} render scale");
            Assert.AreEqual(msaa, asset.msaaSampleCount, $"{asset.name} MSAA");
        }

        [Test]
        public void EachLevel_HasADifferentAsset()
        {
            Object low = QualitySettings.GetRenderPipelineAssetAt(0);
            Object mid = QualitySettings.GetRenderPipelineAssetAt(1);
            Object high = QualitySettings.GetRenderPipelineAssetAt(2);

            Assert.AreNotSame(low, mid);
            Assert.AreNotSame(mid, high);
            Assert.AreNotSame(low, high);
        }
    }
}

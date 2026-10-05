using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace MGD.Samples.Editor
{
    // EditMode never runs Awake, OnEnable or Update, so the tests call Prewarm,
    // ResetState and Advance themselves. A scene object stands in for the prefab:
    // Instantiate copies it the same way.
    public class SpawnPoolTests
    {
        GameObject _template;
        SpawnPool _pool;

        [SetUp]
        public void SetUp()
        {
            _template = new GameObject("Template", typeof(SpriteRenderer), typeof(Pooled));
            _pool = new GameObject("Pool").AddComponent<SpawnPool>();
        }

        [TearDown]
        public void TearDown()
        {
            // Pooled items are children of the pool, so they go with it.
            Object.DestroyImmediate(_pool.gameObject);
            Object.DestroyImmediate(_template);
        }

        static void Configure(SpawnPool pool, Pooled prefab, int size)
        {
            var props = new SerializedObject(pool);
            props.FindProperty("prefab").objectReferenceValue = prefab;
            props.FindProperty("size").intValue = size;
            props.ApplyModifiedPropertiesWithoutUndo();
        }

        SpawnPool Prewarmed(int size)
        {
            Configure(_pool, _template.GetComponent<Pooled>(), size);
            _pool.Prewarm();
            return _pool;
        }

        [Test]
        public void Prewarm_CreatesSizeInactiveChildren()
        {
            SpawnPool pool = Prewarmed(4);

            Assert.AreEqual(4, pool.transform.childCount);
            foreach (Transform child in pool.transform)
            {
                Assert.IsFalse(child.gameObject.activeSelf, child.name);
            }

            Assert.AreEqual(4, pool.CreatedCount);
            Assert.AreEqual(4, pool.FreeCount);
            Assert.AreEqual(0, pool.ActiveCount);
        }

        [Test]
        public void Prewarm_Twice_CreatesNothingMore()
        {
            SpawnPool pool = Prewarmed(4);
            pool.Prewarm();

            Assert.AreEqual(4, pool.CreatedCount);
            Assert.AreEqual(4, pool.transform.childCount);
        }

        [Test]
        public void Spawn_WithFreeItem_ActivatesItWithoutCreating()
        {
            SpawnPool pool = Prewarmed(2);

            Pooled item = pool.Spawn(new Vector2(1f, 2f));

            Assert.IsTrue(item.gameObject.activeSelf);
            Assert.AreEqual(new Vector3(1f, 2f, 0f), item.transform.position);
            Assert.AreSame(pool, item.Pool);
            Assert.AreEqual(2, pool.CreatedCount);
            Assert.AreEqual(1, pool.FreeCount);
            Assert.AreEqual(1, pool.ActiveCount);
        }

        [Test]
        public void Spawn_AfterRelease_ReusesTheSameItem()
        {
            SpawnPool pool = Prewarmed(2);
            Pooled first = pool.Spawn(Vector2.zero);
            pool.Release(first);

            Pooled second = pool.Spawn(Vector2.zero);

            Assert.AreSame(first, second);
            Assert.AreEqual(2, pool.CreatedCount);
        }

        [Test]
        public void Spawn_WhenEmpty_CreatesAndWarnsOnce()
        {
            SpawnPool pool = Prewarmed(1);
            LogAssert.Expect(LogType.Warning, new Regex(@"\[SpawnPool\].*Raise size"));

            pool.Spawn(Vector2.zero);
            Pooled extra = pool.Spawn(Vector2.zero);
            pool.Spawn(Vector2.zero);

            Assert.IsTrue(extra.gameObject.activeSelf);
            Assert.AreEqual(3, pool.CreatedCount);
            Assert.AreEqual(3, pool.ActiveCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Release_DeactivatesAndReturnsToTheStack()
        {
            SpawnPool pool = Prewarmed(2);
            Pooled item = pool.Spawn(Vector2.zero);

            pool.Release(item);

            Assert.IsFalse(item.gameObject.activeSelf);
            Assert.AreEqual(2, pool.FreeCount);
            Assert.AreEqual(0, pool.ActiveCount);
        }

        [Test]
        public void Release_Twice_IsIgnoredAndWarns()
        {
            SpawnPool pool = Prewarmed(2);
            Pooled item = pool.Spawn(Vector2.zero);
            pool.Release(item);
            LogAssert.Expect(LogType.Warning, new Regex(@"\[SpawnPool\].*already released"));

            pool.Release(item);

            Assert.AreEqual(2, pool.FreeCount);
            Assert.AreEqual(0, pool.ActiveCount);
        }

        [Test]
        public void Release_ItemFromAnotherPool_IsRefused()
        {
            SpawnPool pool = Prewarmed(2);
            var other = new GameObject("Other Pool").AddComponent<SpawnPool>();
            try
            {
                Configure(other, _template.GetComponent<Pooled>(), 1);
                other.Prewarm();
                Pooled foreign = other.Spawn(Vector2.zero);
                LogAssert.Expect(LogType.Warning, new Regex(@"\[SpawnPool\].*another pool"));

                pool.Release(foreign);

                Assert.IsTrue(foreign.gameObject.activeSelf);
                Assert.AreEqual(2, pool.FreeCount);
                Assert.AreEqual(0, other.FreeCount);
            }
            finally
            {
                Object.DestroyImmediate(other.gameObject);
            }
        }

        [Test]
        public void Prewarm_WithoutPrefab_LogsErrorAndSpawnReturnsNull()
        {
            Configure(_pool, null, 4);
            LogAssert.Expect(LogType.Error, new Regex(@"\[SpawnPool\].*prefab"));

            _pool.Prewarm();

            Assert.IsNull(_pool.Spawn(Vector2.zero));
            Assert.AreEqual(0, _pool.CreatedCount);
        }

        [Test]
        public void ResetState_RestoresAgeAndColour()
        {
            Pooled item = Prewarmed(1).Spawn(Vector2.zero);
            item.Advance(1f);
            var renderer = item.GetComponent<SpriteRenderer>();
            Assert.Less(renderer.color.a, 1f, "Advance should fade the item.");

            item.ResetState();

            Assert.AreEqual(0f, item.Age);
            Assert.AreEqual(1f, renderer.color.a, 0.0001f);
        }

        [Test]
        public void Advance_PastLifetime_ReleasesToItsPool()
        {
            SpawnPool pool = Prewarmed(1);
            Pooled item = pool.Spawn(Vector2.zero);

            item.Advance(10f);

            Assert.IsFalse(item.gameObject.activeSelf);
            Assert.AreEqual(1, pool.FreeCount);
        }
    }

    public class WaveTimerTests
    {
        [Test]
        public void ClampInterval_BelowFloor_ReturnsMinInterval()
        {
            Assert.AreEqual(WaveTimer.MinInterval, WaveTimer.ClampInterval(0.01f));
            Assert.AreEqual(WaveTimer.MinInterval, WaveTimer.ClampInterval(0f));
        }

        [Test]
        public void ClampInterval_AboveCeiling_ReturnsMaxInterval()
        {
            Assert.AreEqual(WaveTimer.MaxInterval, WaveTimer.ClampInterval(10f));
        }

        [Test]
        public void ClampInterval_FasterThreeTimesFromDefault_ReachesTheFloorExactly()
        {
            float interval = 0.5f;
            for (int i = 0; i < 3; i++)
            {
                interval = WaveTimer.ClampInterval(interval * 0.5f);
            }

            Assert.AreEqual(0.0625f, interval);
            Assert.AreEqual(WaveTimer.MinInterval, WaveTimer.ClampInterval(interval * 0.5f));
        }

        [Test]
        public void SpawnArea_KeepsTheBottomShareAndTheInsetClear()
        {
            var go = new GameObject("Camera", typeof(Camera));
            try
            {
                var cam = go.GetComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 5f;
                cam.aspect = 0.5f;
                go.transform.position = new Vector3(0f, 0f, -10f);

                Rect area = WaveTimer.SpawnArea(cam, 0.4f, 0.5f);

                Assert.AreEqual(-2f, area.xMin, 0.0001f);
                Assert.AreEqual(2f, area.xMax, 0.0001f);
                Assert.AreEqual(-0.5f, area.yMin, 0.0001f);
                Assert.AreEqual(4.5f, area.yMax, 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}

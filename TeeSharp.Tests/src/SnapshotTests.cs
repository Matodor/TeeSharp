using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Tests
{
    [TestClass]
    public class SnapshotTests
    {
        [TestMethod]
        public void TestSize()
        {
            Assert.AreEqual(88, SnapshotItemsInfo.GetSize<SnapshotCharacter>());
        }

        //[TestMethod]
        public void TestDeserialization()
        {
            var actual = BaseSnapshotItem.FromArray<SnapshotCharacter>(new int[]
            {
                94128,
                111,
                222,
                333,
                444,
                555,
                666,
                777,
                888,
                1,
                999,
                1111,
                2222,
                3333,
                4444,
                5555,
                6666,
                7777,
                5,
                1,
                8888,
                10,
            });

            var expected = new SnapshotCharacter()
            {
                Tick = 94128,
                X = 111,
                Y = 222,
                VelX = 333,
                VelY = 444,

                Angle = 555,
                Direction = 666,

                Jumped = 777,
                HookedPlayer = 888,
                HookState = HookState.RetractStart,
                HookTick = 999,

                HookX = 1111,
                HookY = 2222,
                HookDx = 3333,
                HookDy = 4444,
                Health = 5555,
                Armor = 6666,
                AmmoCount = 7777,
                Weapon = Weapon.Ninja,
                Emote = Emote.Pain,
                AttackTick = 8888,
                TriggeredEvents = CoreEvents.HookAttachGround | CoreEvents.AirJump,
            };
        }

        [TestMethod]
        public void TestSerialization()
        {
            var item = new SnapshotCharacter()
            {
                Tick = 94128,
                X = 111,
                Y = 222,
                VelX = 333,
                VelY = 444,

                Angle = 555,
                Direction = 666,

                Jumped = 777,
                HookedPlayer = 888,
                HookState = HookState.RetractStart,
                HookTick = 999,

                HookX = 1111,
                HookY = 2222,
                HookDx = 3333,
                HookDy = 4444,
                Health = 5555,
                Armor = 6666,
                AmmoCount = 7777,
                Weapon = Weapon.Ninja,
                Emote = Emote.Pain,
                AttackTick = 8888,
                TriggeredEvents = CoreEvents.HookAttachGround | CoreEvents.AirJump,
            };

            var actual = item.ToArray();
            var expected = new int[]
            {
                94128,
                111,
                222,
                333,
                444,
                555,
                666,
                777,
                888,
                1,
                999,
                1111,
                2222,
                3333,
                4444,
                5555,
                6666,
                7777,
                5,
                1,
                8888,
                10,
            };
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
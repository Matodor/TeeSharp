using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Server.Game.Entities
{
    public class Pickup : Entity<Pickup>
    {
        public override float ProximityRadius { get; protected set; } = 14f;

        public readonly Common.Enums.Pickup Type;

        protected virtual int SpawnTick { get; set; }

        public Pickup(Common.Enums.Pickup type) : base(idsCount: 1)
        {
            SpawnTick = -1;
            Type = type;
            Reseted += OnReseted;
        }

        private void OnReseted(Entity pickup)
        {
            if (ServerData.Pickups[Type].SpawnDelay > 0)
                SpawnTick = Server.Tick + Server.TickSpeed * ServerData.Pickups[Type].SpawnDelay;
            else
                SpawnTick = -1;
        }

        public override void Tick()
        {
            base.Tick();

            if (SpawnTick > 0)
            {
                if (Server.Tick >= SpawnTick)
                {
                    SpawnTick = -1;

                    if (Type == Common.Enums.Pickup.Grenade ||
                        Type == Common.Enums.Pickup.Shotgun ||
                        Type == Common.Enums.Pickup.Laser)
                    {
                        GameContext.CreateSound(Position, Sound.WeaponSpawn);
                    }
                }
                else return;
            }

            var character = BaseCharacter.Entities.Closest(Position, 20f, null);
            if (character == null || !character.IsAlive)
                return;

            void Picked(Sound sound)
            {
                GameContext.CreateSound(Position, sound);
                Console.Print(OutputLevel.Debug, "game", $"pickup player='{character.Player.ClientId}:{Server.ClientName(character.Player.ClientId)}' item={Type}");
                var respawnTime = ServerData.Pickups[Type].RespawnTime;
                if (respawnTime >= 0)
                    SpawnTick = Server.Tick + Server.TickSpeed * respawnTime;
            }

            switch (Type)
            {
                case Common.Enums.Pickup.Health:
                    if (character.IncreaseHealth(1))
                    {
                        Picked(Sound.PickupHealth);
                    }
                    break;

                case Common.Enums.Pickup.Armor:
                    if (character.IncreaseArmor(1))
                    {
                        Picked(Sound.PickupArmor);
                    }
                    break;

                case Common.Enums.Pickup.Grenade:
                    if (character.GiveWeapon(Weapon.Grenade, ServerData.Weapons[Weapon.Grenade].MaxAmmo))
                    {
                        Picked(Sound.PickupGrenade);
                        GameContext.SendWeaponPickup(character.Player.ClientId, Weapon.Grenade);
                    }
                    break;

                case Common.Enums.Pickup.Shotgun:
                    if (character.GiveWeapon(Weapon.Shotgun, ServerData.Weapons[Weapon.Shotgun].MaxAmmo))
                    {
                        Picked(Sound.PickupShotgun);
                        GameContext.SendWeaponPickup(character.Player.ClientId, Weapon.Shotgun);
                    }
                    break;

                case Common.Enums.Pickup.Laser:
                    if (character.GiveWeapon(Weapon.Laser, ServerData.Weapons[Weapon.Laser].MaxAmmo))
                    {
                        Picked(Sound.PickupShotgun);
                        GameContext.SendWeaponPickup(character.Player.ClientId, Weapon.Laser);
                    }
                    break;

                case Common.Enums.Pickup.Ninja:
                    Picked((Sound) (-1));
                    character.GiveNinja();
                    character.SetEmote(Emote.Angry,
                        Server.Tick + Server.TickSpeed * ServerData.Weapons.Ninja.Duration / 1000);

                    foreach (var entity in BaseCharacter.Entities)
                    {
                        if (entity == character)
                            continue;

                        entity.SetEmote(Emote.Surprise, Server.Tick + Server.TickSpeed);
                    }
                    break;
            }
        }

        public override void TickPaused()
        {
            base.TickPaused();

            if (SpawnTick != -1)
                SpawnTick++;
        }

        public override void OnSnapshot(int snappingClient)
        {
            if (SpawnTick != -1 || NetworkClipped(snappingClient))
                return;

            var pickup = Server.SnapshotItem<SnapshotPickup>(IDs[0]);
            if (pickup == null)
                return;

            pickup.X = (int) Position.x;
            pickup.Y = (int) Position.y;
            pickup.Pickup = Type;
        }
    }
}
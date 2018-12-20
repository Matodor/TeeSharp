using System;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Server.Game.Entities
{
    public class Pickup : Entity<Pickup>
    {
        public override float ProximityRadius { get; protected set; } = 14f;

        private int _spawnTick;
        private readonly Powerup _powerup;
        private readonly Weapon _weapon;

        public Pickup(Powerup powerup, Weapon weapon) : base(1)
        {
            _powerup = powerup;
            _weapon = weapon;

            Reset();
        }

        public override void Reset()
        {
            if (ServerData.Data.Pickups[(int) _powerup].SpawnDelay > 0)
                _spawnTick = Server.Tick + Server.TickSpeed * ServerData.Data.Pickups[(int) _powerup].SpawnDelay;
            else
                _spawnTick = -1;
        }

        public override void Tick()
        {
            if (_spawnTick > 0)
            {
                if (Server.Tick > _spawnTick)
                {
                    _spawnTick = -1;

                    if (_powerup == Powerup.Weapon)
                        GameContext.CreateSound(Position, Sound.WeaponSpawn);
                }
                else return;
            }

            var character = GameWorld.ClosestCharacter(Position, 20f, null);
            if (character == null || !character.IsAlive)
                return;

            var respawnTime = -1;
            switch (_powerup)
            {
                case Powerup.Health:
                    if (character.IncreaseHealth(1))
                    {
                        GameContext.CreateSound(Position, Sound.PickupHealth);
                        respawnTime = ServerData.Data.Pickups[(int) _powerup].RespawnTime;
                    }
                    break;

                case Powerup.Armor:
                    if (character.IncreaseArmor(1))
                    {
                        GameContext.CreateSound(Position, Sound.PickupArmor);
                        respawnTime = ServerData.Data.Pickups[(int) _powerup].RespawnTime;
                    }
                    break;

                case Powerup.Weapon:
                    if (_weapon >= 0 && _weapon < Weapon.NumWeapons)
                    {
                        if (character.GiveWeapon(_weapon, 10))
                        {
                            respawnTime = ServerData.Data.Pickups[(int) _powerup].RespawnTime;

                            if (_weapon == Weapon.Grenade)
                                GameContext.CreateSound(Position, Sound.PickupGrenade);
                            else if (_weapon == Weapon.Shotgun)
                                GameContext.CreateSound(Position, Sound.PickupShotgun);
                            else if (_weapon == Weapon.Laser)
                                GameContext.CreateSound(Position, Sound.PickupShotgun);

                            GameContext.SendWeaponPickup(character.Player.ClientId, _weapon);
                        }
                    }
                    break;

                case Powerup.Ninja:
                    character.GiveNinja();
                    respawnTime = ServerData.Data.Pickups[(int)_powerup].RespawnTime;

                    foreach (var chr in GameWorld.GetEntities<Character>())
                    {
                        if (chr != character)
                            chr.SetEmote(Emote.Surprise, Server.Tick + Server.TickSpeed);
                    }

                    character.SetEmote(Emote.Angry, Server.Tick + 1200 * Server.TickSpeed / 1000);
                    break;
            }

            if (respawnTime >= 0)
            {
                GameContext.Console.Print(OutputLevel.DEBUG, "game", $"pickup player='{character.Player.ClientId}:{character.Player.Name}' item={_powerup}:{_weapon}");
                _spawnTick = Server.Tick + Server.TickSpeed * respawnTime;
            }
        }

        public override void TickPaused()
        {
            if (_spawnTick != -1)
                _spawnTick++;
        }

        public override void OnSnapshot(int snappingClient)
        {
            if (_spawnTick != -1 || NetworkClipped(snappingClient))
                return;

            var pickup = Server.SnapObject<SnapObj_Pickup>(IDs[0]);
            if (pickup == null)
                return;

            pickup.Position = Position;
            pickup.Powerup = _powerup;
            pickup.Weapon = _weapon;
        }
    }
}
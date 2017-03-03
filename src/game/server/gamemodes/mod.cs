namespace Teecsharp
{
    // you can subclass GAMECONTROLLER_CTF, GAMECONTROLLER_TDM etc if you want
    // todo a modification with their base as well.
    public class CGameControllerMOD : IGameController
    {
        public CGameControllerMOD(CGameContext pGameServer) : base(pGameServer)
        {
            // Exchange this to a string that identifies your game mode.
            // DM, TDM and CTF are reserved for teeworlds original modes.
            m_pGameType = "MOD";

            //m_GameFlags = GAMEFLAG_TEAMS; // GAMEFLAG_TEAMS makes it a two-team gamemode
        }

        public override void Tick()
        {
            // this is the main part of the gamemode, this function is run every tick

            base.Tick();
        }
        // add more virtual functions here if you wish
    }
}

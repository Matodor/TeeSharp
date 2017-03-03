namespace Teecsharp
{
    class CGameControllerDM : IGameController
    {
        public CGameControllerDM(CGameContext pGameServer) : base(pGameServer)
        {
            m_pGameType = "DM";
        }
    }
}

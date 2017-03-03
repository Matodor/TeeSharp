using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public abstract class IServer : IInterface
    {
        public struct CClientInfo
        {
            public string m_pName;
            public int m_Latency;
            public bool m_CustClt;
            public int m_ClientVersion;
        };

        public int Tick() { return m_CurrentGameTick; }
        public int TickSpeed() { return m_TickSpeed; }

        public abstract int MaxClients();
        public abstract string ClientName(int ClientID);
        public abstract string ClientClan(int ClientID);
        public abstract int ClientCountry(int ClientID);
        public abstract bool ClientIngame(int ClientID);
        public abstract bool GetClientInfo(int ClientID, out CClientInfo pInfo);
        public abstract bool GetClientAddr(int ClientID, out string s, int Size = 48);
        public abstract bool GetClientAddr(int ClientID, out NETADDR pAddr);

        public abstract bool SendMsg(CMsgPacker pMsg, int Flags, int ClientID);

        public abstract void SetClientName(int ClientID, string pName);
        public abstract void SetClientClan(int ClientID, string pClan);
        public abstract void SetClientCountry(int ClientID, int Country);
        public abstract void SetClientScore(int ClientID, int Score);

        public abstract int SnapNewID();
        public abstract void SnapFreeID(int ID);
        public abstract T SnapNetObj<T>(int Type, int ID) where T : CNet_Common, new();
        public abstract object SnapEvent(Type Type, int TypeID, int ID);

        public const int
            RCON_CID_SERV = -1,
            RCON_CID_VOTE = -2;

        public abstract bool IsAuthed(int ClientID);
        public abstract void Kick(int ClientID, string pReason, int from);

        public abstract int GetIdMap(int ClientID);
        public abstract void SetCustClt(int ClientID);
        public int[] IdMap = new int[MAX_CLIENTS * VANILLA_MAX_CLIENTS];

        protected int m_CurrentGameTick;
        protected int m_TickSpeed;

        protected const int
            MAX_CLIENTS = (int)Consts.MAX_CLIENTS,
            VANILLA_MAX_CLIENTS = (int)Consts.VANILLA_MAX_CLIENTS;

        protected IServer()
        {
        }

        public bool SendPackMsg<T>(T pMsg, int Flags, int ClientID) where T : CNetMsgBase, new()
        {
            bool result = false;
            
            if (ClientID == -1)
            {
                for (int i = 0; i < MAX_CLIENTS; i++)
                    if (ClientIngame(i))
                    {
                        T copy = new T();
                        pMsg.Write(copy);
                        result = SendPackMsgBody(copy, Flags, i);
                    }
            }
            else
            {
                T copy = new T();
                pMsg.Write(copy);
                result = SendPackMsgBody(copy, Flags, ClientID);
            }
            return result;
        }

        private bool SendPackMsgBody<T>(T pMsg, int Flags, int ClientID) where T : CNetMsgBase
        {
            if (pMsg.GetType() == typeof(CNetMsg_Sv_Emoticon))
                return SendPackMsgTranslate(pMsg as CNetMsg_Sv_Emoticon, Flags, ClientID);
            if (pMsg.GetType() == typeof(CNetMsg_Sv_Chat))
                return SendPackMsgTranslate(pMsg as CNetMsg_Sv_Chat, Flags, ClientID);
            if (pMsg.GetType() == typeof(CNetMsg_Sv_KillMsg))
                return SendPackMsgTranslate(pMsg as CNetMsg_Sv_KillMsg, Flags, ClientID);
            return SendPackMsgTranslate(pMsg, Flags, ClientID);
        }

        public bool SendPackMsgTranslate(CNetMsg_Sv_Emoticon pMsg, int Flags, int ClientID)
        {
            return Translate(ref pMsg.m_ClientID, ClientID) && SendPackMsgOne(pMsg, Flags, ClientID);
        }

        public bool SendPackMsgTranslate(CNetMsg_Sv_Chat pMsg, int Flags, int ClientID)
        {
            if (pMsg.m_ClientID >= 0 && !Translate(ref pMsg.m_ClientID, ClientID))
            {
                string buf = string.Format("{0}: {1}", ClientName(pMsg.m_ClientID), pMsg.m_pMessage);
                pMsg.m_pMessage = buf;
                pMsg.m_ClientID = VANILLA_MAX_CLIENTS - 1;
            }
            return SendPackMsgOne(pMsg, Flags, ClientID);
        }

        public bool SendPackMsgTranslate(CNetMsg_Sv_KillMsg pMsg, int Flags, int ClientID)
        {
            if (!Translate(ref pMsg.m_Victim, ClientID)) return false;
            if (!Translate(ref pMsg.m_Killer, ClientID)) pMsg.m_Killer = pMsg.m_Victim;
            return SendPackMsgOne(pMsg, Flags, ClientID);
        }

        public bool SendPackMsgTranslate(CNetMsgBase pMsg, int Flags, int ClientID)
        {
            return SendPackMsgOne(pMsg, Flags, ClientID);
        }

        public bool SendPackMsgOne(CNetMsgBase pMsg, int Flags, int ClientID)
        {
            CMsgPacker Packer = new CMsgPacker(pMsg.MsgID());
            if (pMsg.Pack(Packer))
                return false;
            return SendMsg(Packer, Flags, ClientID);
        }

        public bool Translate(ref int target, int client)
        {
            CClientInfo info;
            GetClientInfo(client, out info);

            if (info.m_ClientVersion >= (int)Consts.VERSION_DDNET_OLD)
                return true;

            int map = GetIdMap(client);
            bool found = false;
            for (int i = 0; i < VANILLA_MAX_CLIENTS; i++)
            {
                if (target == ((CServer)this).IdMap[map + i])
                {
                    target = i;
                    found = true;
                    break;
                }
            }
            return found;
        }

        public bool ReverseTranslate(ref int target, int client)
        {
            CClientInfo info;
            GetClientInfo(client, out info);
            if (info.m_ClientVersion >= (int)Consts.VERSION_DDNET_OLD)
                return true;

            int map = GetIdMap(client);
            if (((CServer)this).IdMap[map + target] == -1)
                return false;
            target = ((CServer)this).IdMap[map + target];
            return true;
        }
    }


    public abstract class IGameServer : IInterface
    {
        public abstract void OnInit();
        public abstract void OnConsoleInit();
        public abstract void OnShutdown();

        public abstract void OnTick();
        public abstract void OnPreSnap();
        public abstract void OnSnap(int ClientID);
        public abstract void OnPostSnap();

        public abstract void OnMessage(int MsgID, CUnpacker pUnpacker, int ClientID);

        public abstract void OnClientConnected(int ClientID);
        public abstract void OnClientEnter(int ClientID);
        public abstract void OnClientDrop(int ClientID, string pReason);
        public abstract void OnClientDirectInput(int ClientID, int[] pInput);
        public abstract void OnClientPredictedInput(int ClientID, int[] pInput);

        public abstract bool IsClientReady(int ClientID);
        public abstract bool IsClientPlayer(int ClientID);

        public abstract string GameType();
        public abstract string Version();
        public abstract string NetVersion();
    }
}

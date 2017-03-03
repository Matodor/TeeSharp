using System;
using Teecsharp;

namespace Teecsharp
{
    public class CLayers
    {
        private int m_GroupsNum;
        private int m_GroupsStart;
        private int m_LayersNum;
        private int m_LayersStart;
        private CMapItemGroup m_pGameGroup;
        private CMapItemLayerTilemap m_pGameLayer;
        private IMap m_pMap;

        public CLayers()
        {
            m_GroupsNum = 0;
            m_GroupsStart = 0;
            m_LayersNum = 0;
            m_LayersStart = 0;
            m_pGameGroup = null;
            m_pGameLayer = null;
            m_pMap = null;
        }

        public void Init(IKernel pKernel)
        {
            m_pMap = pKernel.RequestInterface<IMap>();
            m_pMap.GetType((int)MapItems.MAPITEMTYPE_GROUP, ref m_GroupsStart, ref m_GroupsNum);
            m_pMap.GetType((int)MapItems.MAPITEMTYPE_LAYER, ref m_LayersStart, ref m_LayersNum);

            for (int g = 0; g < NumGroups(); g++)
            {
                CMapItemGroup pGroup = GetGroup(g);
                for (int l = 0; l < pGroup.m_NumLayers; l++)
                {
                    CMapItemLayer pLayer = GetLayer(pGroup.m_StartLayer + l);

                    if (pLayer.m_Type == (int)MapItems.LAYERTYPE_TILES)
                    {
                        int t = -1;
                        int i = -1;
                        CMapItemLayerTilemap pTilemap = m_pMap.GetItem<CMapItemLayerTilemap>(m_LayersStart + pGroup.m_StartLayer + l, ref t, ref i);

                        if ((pTilemap.m_Flags & (int)MapItems.TILESLAYERFLAG_GAME) != 0)
                        {
                            m_pGameLayer = pTilemap;
                            m_pGameGroup = pGroup;

                            // make sure the game group has standard settings
                            m_pGameGroup.m_OffsetX = 0;
                            m_pGameGroup.m_OffsetY = 0;
                            m_pGameGroup.m_ParallaxX = 100;
                            m_pGameGroup.m_ParallaxY = 100;

                            if (m_pGameGroup.m_Version >= 2)
                            {
                                m_pGameGroup.m_UseClipping = 0;
                                m_pGameGroup.m_ClipX = 0;
                                m_pGameGroup.m_ClipY = 0;
                                m_pGameGroup.m_ClipW = 0;
                                m_pGameGroup.m_ClipH = 0;
                            }

                            break;
                        }
                    }
                }
            }
        }

        public int NumGroups()
        {
            return m_GroupsNum;
        }

        public IMap Map()
        {
            return m_pMap;
        }

        public CMapItemGroup GameGroup()
        {
            return m_pGameGroup;
        }

        public CMapItemLayerTilemap GameLayer()
        {
            return m_pGameLayer;
        }

        public CMapItemGroup GetGroup(int Index)
        {
            int t = -1;
            int i = -1;
            return m_pMap.GetItem<CMapItemGroup>(m_GroupsStart + Index, ref t, ref i);
        }

        public CMapItemLayer GetLayer(int Index)
        {
            int t = -1;
            int i = -1;
            return m_pMap.GetItem<CMapItemLayer>(m_LayersStart + Index, ref t, ref i);
        }
    }
}

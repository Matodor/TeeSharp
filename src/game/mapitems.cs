namespace Teecsharp
{
    public enum MapItems
    {
        LAYERTYPE_INVALID = 0,
        LAYERTYPE_GAME,
        LAYERTYPE_TILES,
        LAYERTYPE_QUADS,

        MAPITEMTYPE_VERSION = 0,
        MAPITEMTYPE_INFO,
        MAPITEMTYPE_IMAGE,
        MAPITEMTYPE_ENVELOPE,
        MAPITEMTYPE_GROUP,
        MAPITEMTYPE_LAYER,
        MAPITEMTYPE_ENVPOINTS,


        CURVETYPE_STEP = 0,
        CURVETYPE_LINEAR,
        CURVETYPE_SLOW,
        CURVETYPE_FAST,
        CURVETYPE_SMOOTH,
        NUM_CURVETYPES,

        // game layer tiles
        ENTITY_NULL = 0,
        ENTITY_SPAWN,
        ENTITY_SPAWN_RED,
        ENTITY_SPAWN_BLUE,
        ENTITY_FLAGSTAND_RED,
        ENTITY_FLAGSTAND_BLUE,
        ENTITY_ARMOR_1,
        ENTITY_HEALTH_1,
        ENTITY_WEAPON_SHOTGUN,
        ENTITY_WEAPON_GRENADE,
        ENTITY_POWERUP_NINJA,
        ENTITY_WEAPON_RIFLE,
        NUM_ENTITIES,

        // GAME TILES
        TILE_AIR = 0,
        TILE_SOLID,
        TILE_DEATH,
        TILE_NOHOOK,
       
        TILEFLAG_VFLIP = 1,
        TILEFLAG_HFLIP = 2,
        TILEFLAG_OPAQUE = 4,
        TILEFLAG_ROTATE = 8,

        LAYERFLAG_DETAIL = 1,
        TILESLAYERFLAG_GAME = 1,
        
        ENTITY_COUNT = 255,
        ENTITY_OFFSET = 255 - 16 * 4,
    }
    
    public class CPoint
    {
        public int x, y; // 22.10 fixed point
    }
    
    public class CColor
    {
        public int r, g, b, a;
    }
    
    class CQuad
    {
        public readonly CPoint[] m_aPoints = new CPoint[5];
        public readonly CColor[] m_aColors = new CColor[4];
        public readonly CPoint[] m_aTexcoords = new CPoint[4];

        public int m_PosEnv;
        public int m_PosEnvOffset;

        public int m_ColorEnv;
        public int m_ColorEnvOffset;
    }

    
    public class CTile
    {
        public byte m_Index;
        public byte m_Flags;
        public byte m_Skip;
        public byte m_Reserved;
    }

    
    struct CMapItemInfo
    {
        public int m_Version;
        public int m_Author;
        public int m_MapVersion;
        public int m_Credits;
        public int m_License;
    }

    
    class CMapItemImage
    {
        public int m_Version;
        public int m_Width;
        public int m_Height;
        public int m_External;
        public int m_ImageName;
        public int m_ImageData;
    }

 
    public class CMapItemGroup
    {
        public int m_Version;
        public int m_OffsetX;
        public int m_OffsetY;
        public int m_ParallaxX;
        public int m_ParallaxY;

        public int m_StartLayer;
        public int m_NumLayers;

        public int m_UseClipping;
        public int m_ClipX;
        public int m_ClipY;
        public int m_ClipW;
        public int m_ClipH;
        public readonly int[] m_aName = new int[3];
    }

    
    public class CMapItemLayer
    {
        public int m_Version;
        public int m_Type;
        public int m_Flags;
    }

    
    public class CMapItemLayerTilemap
    {
        public int m_VersionItemLayer;
        public int m_TypeItemLayer;
        public int m_FlagsItemLayer;
        /*****************************/
        public int m_Version;
        public int m_Width;
        public int m_Height;
        public int m_Flags;

        // Color
        public int r;
        public int g;
        public int b;
        public int a;

        public int m_ColorEnv;
        public int m_ColorEnvOffset;

        public int m_Image;
        public int m_Data;

        public readonly int[] m_aName = new int[3];
    }

    
    class CMapItemLayerQuads
    {
        public CMapItemLayer m_Layer = new CMapItemLayer();
        public int m_Version;

        public int m_NumQuads;
        public int m_Data;
        public int m_Image;

        public readonly int[] m_aName = new int[3];
    }

    
    class CMapItemVersion
    {
        public int m_Version;
    }

    class CEnvPoint
    {
        public int m_Time; // in ms
        public int m_Curvetype;
        public int[] m_aValues = new int[4]; // 1-4 depending on envelope (22.10 fixed point)

        public static bool operator >(CEnvPoint This, CEnvPoint Other)
        {
            return This.m_Time > Other.m_Time;
        }

        public static bool operator <(CEnvPoint This, CEnvPoint Other)
        {
            return This.m_Time < Other.m_Time;
        }
    }

    
    public class CMapItemEnvelope_v1
    {
        public int m_Version;
        public int m_Channels;
        public int m_StartPoint;
        public int m_NumPoints;
        public readonly int[] m_aName = new int[8];
    }

    
    public class CMapItemEnvelope : CMapItemEnvelope_v1
    {
        public int m_Synchronized;
    }
}

namespace TeeSharp.Common.Enums
{
    public enum NetworkMessages
    {
        NULL = 0,
        CL_INFO = 1,
        
        // sent by server
        SV_MAP_CHANGE,      // sent when client should switch map
        SV_MAP_DATA,        // map transfer, contains a chunk of the map file
        SV_CON_READY,       // connection is ready, client should send start info
        SV_SNAP,            // normal snapshot, multiple parts
        SV_SNAPEMPTY,       // empty snapshot
        SV_SNAPSINGLE,      // ?
        SV_SNAPSMALL,       //
        SV_INPUT_TIMING,    // reports how off the input was
        SV_RCON_AUTH_STATUS,// result of the authentication
        SV_RCON_LINE,       // line that should be printed to the remote console

        AUTH_CHALLANGE,  //
        AUTH_RESULT,     //

        // sent by client
        CL_READY,           //
        CL_ENTERGAME,
        CL_INPUT,           // contains the inputdata from the client
        CL_RCON_CMD,        //
        CL_RCON_AUTH,       //
        CL_REQUEST_MAP_DATA,//

        CL_AUTH_START,      //
        CL_AUTH_RESPONSE,   //

        // sent by both
        PING,
        PING_REPLY,
        ERROR,

        RCON_CMD_ADD,
        RCON_CMD_REM,
    }
}
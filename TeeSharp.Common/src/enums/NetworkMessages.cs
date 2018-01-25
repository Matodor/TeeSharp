namespace TeeSharp.Common.Enums
{
    public enum NetworkMessages
    {
        NULL = 0,
        INFO = 1,
        
        // sent by server
        MAP_CHANGE,      // sent when client should switch map
        MAP_DATA,        // map transfer, contains a chunk of the map file
        CON_READY,       // connection is ready, client should send start info
        SNAP,            // normal snapshot, multiple parts
        SNAPEMPTY,       // empty snapshot
        SNAPSINGLE,      // ?
        SNAPSMALL,       //
        INPUT_TIMING,     // reports how off the input was
        RCON_AUTH_STATUS,// result of the authentication
        RCON_LINE,       // line that should be printed to the remote console

        AUTH_CHALLANGE,  //
        AUTH_RESULT,     //

        // sent by client
        READY,           //
        ENTERGAME,
        INPUT,           // contains the inputdata from the client
        RCON_CMD,        //
        RCON_AUTH,       //
        REQUEST_MAP_DATA,//

        AUTH_START,      //
        AUTH_RESPONSE,   //

        // sent by both
        PING,
        PING_REPLY,
        ERROR,

        RCON_CMD_ADD,
        RCON_CMD_REM,
    }
}
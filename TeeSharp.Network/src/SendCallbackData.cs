namespace TeeSharp.Network
{
    public delegate void SendCallback(int trackID, object context);

    public class SendCallbackData
    {
        public SendCallback Callback { get; set; }
        public object Context { get; set; }
        public int TrackID { get; set; }
    }
}
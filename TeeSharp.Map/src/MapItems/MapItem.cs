namespace TeeSharp.Map;

public struct MapItem<T> where T : struct, IDataFileItem
{
    public DataFileItem Info;
    public T Item;
}
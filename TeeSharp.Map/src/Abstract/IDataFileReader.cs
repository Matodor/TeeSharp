using System.IO;

namespace TeeSharp.Map.Abstract;

public interface IDataFileReader
{
    public DataFile Read(string path);
    public DataFile Read(Stream stream);
}

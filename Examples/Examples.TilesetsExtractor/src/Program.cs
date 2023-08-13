using TeeSharp.Map;
using TeeSharp.Map.Concrete;
using System.Security.Cryptography;
using TeeSharp.Map.MapItems;

namespace Examples.TilesetsExtractor;

internal class Program
{
    private const string MapsPath = "C:\\Projects\\Teeworlds\\maps";
    private const string SaveDirectory = "C:\\Projects\\Teeworlds\\tilesets";

    static void Main(string[] args)
    {
        Console.WriteLine("Starting...");

        var maps = Directory.GetFiles(MapsPath, "*.map");
        var processed = 0;

        foreach (var mapPath in maps)
        {
            ExtractFrom(mapPath);

            processed++;
            Console.WriteLine($"Processed: {processed}/{maps.Length}");
        }

        Console.WriteLine("Finished!");
    }

    private static void ExtractFrom(string mapPath)
    {
        DataFile dataFile;

        try
        {
            dataFile = DefaultDataFileReader.Instance.Read(mapPath);
        }
        catch (Exception e)
        {
            return;
        }

        if (!dataFile.HasItemType(MapItemType.Image))
            return;

        Directory.CreateDirectory("images");

        foreach (var mapImage in dataFile.GetItems<MapItemImage>(MapItemType.Image))
        {
            if (mapImage.Item.IsExternal)
                continue;

            try
            {
                var imageName = dataFile.GetDataAsString(mapImage.Item.DataIndexName);
                var data = dataFile.GetDataAsRaw(mapImage.Item.DataIndexImage);
                var hash = Convert.ToHexString(SHA256.HashData(data));
                var savePath = Path.Combine(SaveDirectory, hash + ".png");

                if (hash == "E51A5EBD423747F7C731F76CD918FB0609F2597871A2D258C5E2C6068295311A")
                {
                    Console.WriteLine("AAAA");
                }

                if (File.Exists(savePath))
                    continue;

                using var image = PictureFromArgb(mapImage.Item.Width, mapImage.Item.Height, data);
                image.Save(savePath);
                File.AppendAllText(Path.Combine(SaveDirectory, "files.txt"), $"{hash} {imageName}");
            }
            catch (Exception)
            {
                Console.WriteLine(mapPath);
            }

            // var picture = PictureFromArgb(mapImage.Item.Width, mapImage.Item.Height, data);
            // var path = Path.Combine(Environment.CurrentDirectory, "images", $"{imageName}.png");
            //
            // picture.Save(path, ImageFormat.Png);
            // dataFile.UnloadData(mapImage.Item.DataIndexImage);
        }
    }

    private static Image PictureFromArgb(int width, int height, ReadOnlySpan<byte> data)
    {
        return Image.LoadPixelData<Rgba32>(data, width, height);

        // new Rgba32()
        // var image = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        //
        // for (var x = 0; x < width; x++)
        // {
        //     for (var y = 0; y < height; y++)
        //     {
        //         var position = (y * width + x) * 4;
        //         var color = Color.FromArgb(
        //             data[position + 3],
        //             data[position + 0],
        //             data[position + 1],
        //             data[position + 2]
        //         );
        //
        //         image.SetPixel(x, y, color);
        //     }
        // }
        //
        // return image;
    }
}

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using TeeSharp.Map;
using TeeSharp.Map.Abstract;
using TeeSharp.Map.Concrete;
using TeeSharp.Map.MapItems;

namespace Examples.Map;

internal static class Program
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct MapItemInfoSettings : IDataFileItem
    {
        public MapItemInfo Base;
        public int DataIndexSettings;
    }

    private const string MapName = "Gold Mine";
    private const string MapPath = $"maps/{MapName}.map";

    private static void Main(string[] args)
    {
        DataFile dataFile;

        try
        {
            dataFile = DefaultDataFileReader.Instance.Read(MapPath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        Console.WriteLine($"Loaded, map: {MapName}");

        ShowVersion(dataFile);
        ShowImages(dataFile);
        ShowInfo(dataFile);
        ShowEnvelopes(dataFile);
        ShowGroups(dataFile);

        Console.WriteLine("Press any key to close...");
        Console.ReadKey();
    }

    private static void ShowGroups(DataFile dataFile)
    {
        if (dataFile.HasItemType(MapItemType.Group))
        {
            foreach (var (groupInfo, groupItem) in dataFile.GetItems<MapItemGroup>(MapItemType.Group))
            {
                Console.WriteLine($"[{groupInfo.Id}] MapItemGroup version: {groupItem.ItemVersion}");
                Console.WriteLine($"[{groupInfo.Id}] MapItemGroup Name: {groupItem.Name}");
                Console.WriteLine($"[{groupInfo.Id}] MapItemGroup OffsetX: {groupItem.OffsetX}");
                Console.WriteLine($"[{groupInfo.Id}] MapItemGroup OffsetY: {groupItem.OffsetY}");
                Console.WriteLine($"[{groupInfo.Id}] MapItemGroup ParallaxX: {groupItem.ParallaxX}");
                Console.WriteLine($"[{groupInfo.Id}] MapItemGroup ParallaxY: {groupItem.ParallaxY}");
                Console.WriteLine($"[{groupInfo.Id}] MapItemGroup StartLayer: {groupItem.StartLayer}");
                Console.WriteLine($"[{groupInfo.Id}] MapItemGroup NumberOfLayers: {groupItem.NumberOfLayers}");
                Console.WriteLine($"[{groupInfo.Id}] MapItemGroup UseClipping: {groupItem.UseClipping}");
                Console.WriteLine($"[{groupInfo.Id}] MapItemGroup ClipX: {groupItem.ClipX}");
                Console.WriteLine($"[{groupInfo.Id}] MapItemGroup ClipY: {groupItem.ClipY}");
                Console.WriteLine($"[{groupInfo.Id}] MapItemGroup ClipW: {groupItem.ClipWidth}");
                Console.WriteLine($"[{groupInfo.Id}] MapItemGroup ClipH: {groupItem.ClipHeight}");
                Console.WriteLine("--------------------------------------");
            }
        }
    }

    private static void ShowEnvelopes(DataFile dataFile)
    {
        if (dataFile.HasItemType(MapItemType.Envelope))
        {
            foreach (var (envelopeInfo, envelopeItem) in dataFile.GetItems<MapItemEnvelope>(MapItemType.Envelope))
            {
                Console.WriteLine($"[{envelopeInfo.Id}] MapItemEnvelope version: {envelopeItem.ItemVersion}");
                Console.WriteLine($"[{envelopeInfo.Id}] MapItemEnvelope Channels: {envelopeItem.Channels}");
                Console.WriteLine($"[{envelopeInfo.Id}] MapItemEnvelope StartPoint: {envelopeItem.StartPoint}");
                Console.WriteLine($"[{envelopeInfo.Id}] MapItemEnvelope NumberOfPoints: {envelopeItem.NumberOfPoints}");
                Console.WriteLine($"[{envelopeInfo.Id}] MapItemEnvelope PointsCount: {envelopeItem.Name}");
                Console.WriteLine($"[{envelopeInfo.Id}] MapItemEnvelope IsSynchronized: {envelopeItem.IsSynchronized}");
                Console.WriteLine("--------------------------------------");
            }
        }
    }

    private static void ShowInfo(DataFile dataFile)
    {
        if (dataFile.HasItemType(MapItemType.Info))
        {
            foreach (var (info, item) in dataFile.GetItems<MapItemInfoSettings>(MapItemType.Info))
            {
                Console.WriteLine($"[{info.Id}] MapItemInfo version: {item.Base.ItemVersion}");

                if (item.Base.DataIndexAuthor > -1)
                {
                    var author = dataFile.GetDataAsString(item.Base.DataIndexAuthor);
                    Console.WriteLine($"[{info.Id}] MapItemInfo author: {author}");
                }

                if (item.Base.DataIndexVersion > -1)
                {
                    var version = dataFile.GetDataAsString(item.Base.DataIndexVersion);
                    Console.WriteLine($"[{info.Id}] MapItemInfo version: {version}");
                }

                if (item.Base.DataIndexCredits > -1)
                {
                    var credits = dataFile.GetDataAsString(item.Base.DataIndexCredits);
                    Console.WriteLine($"[{info.Id}] MapItemInfo credits: {credits}");
                }

                if (item.Base.DataIndexLicense > -1)
                {
                    var license = dataFile.GetDataAsString(item.Base.DataIndexLicense);
                    Console.WriteLine($"[{info.Id}] MapItemInfo license: {license}");
                }

                Console.WriteLine("--------------------------------------");
            }
        }
    }

    private static void ShowImages(DataFile dataFile)
    {
        Directory.CreateDirectory("images");

        if (dataFile.HasItemType(MapItemType.Image))
        {
            foreach (var mapImage in dataFile.GetItems<MapItemImage>(MapItemType.Image))
            {
                var imageName = dataFile.GetDataAsString(mapImage.Item.DataIndexName);
                Console.WriteLine($"[{mapImage.Info.Id}] Image: {imageName}");
                Console.WriteLine("--------------------------------------");

                if (!mapImage.Item.IsExternal)
                {
                    var data = dataFile.GetDataAsRaw(mapImage.Item.DataIndexImage);
                    var picture = PictureFromArgb(mapImage.Item.Width, mapImage.Item.Height, data);
                    var path = Path.Combine(Environment.CurrentDirectory, "images", $"{imageName}.png");

                    picture.Save(path, ImageFormat.Png);
                }
            }
        }

        Image PictureFromArgb(int width, int height, ReadOnlySpan<byte> data)
        {
            var image = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var position = (y * width + x) * 4;
                    var color = Color.FromArgb(
                        data[position + 3],
                        data[position + 0],
                        data[position + 1],
                        data[position + 2]
                    );

                    image.SetPixel(x, y, color);
                }
            }

            return image;
        }
    }

    private static void ShowVersion(DataFile dataFile)
    {
        if (dataFile.HasItemType(MapItemType.Version))
        {
            foreach (var mapVersion in dataFile.GetItems<MapItemVersion>(MapItemType.Version))
            {
                Console.WriteLine($"[{mapVersion.Info.Id}] MapItemVersion version: {mapVersion.Item.Version}");
                Console.WriteLine("--------------------------------------");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using TeeSharp.Map;

namespace Examples.Map;

internal static class Program
{
    private const string MapName = "Gold Mine";

    private static void Main(string[] args)
    {
        using (var stream = File.OpenRead($"maps/{MapName}.map"))
        {
            if (stream == null)
            {
            }
            else
            {
                if (DataFileReader.Read(stream, out var error, out var dataFile))
                {
                    Console.WriteLine($"Loaded, map: {MapName}");

                    ShowVersion(dataFile);
                    ShowImages(dataFile);
                    ShowInfo(dataFile);
                    ShowEnvelopes(dataFile);
                    ShowGroups(dataFile);
                }
                else
                {
                    Console.WriteLine($"Loading error: {error}");
                }
            }
        }

        Console.ReadKey();
    }

    private static void ShowGroups(DataFile dataFile)
    {
        if (dataFile.HasItemType((int) MapItemType.Group))
        {
            foreach (var group in dataFile.GetItems<MapItemGroup>((int) MapItemType.Group))
            {
                Console.WriteLine($"[{group.Info.Id}] MapItemGroup version: {group.Item.ItemVersion}");
                Console.WriteLine($"[{group.Info.Id}] MapItemGroup Name: {group.Item.Name}");
                Console.WriteLine($"[{group.Info.Id}] MapItemGroup OffsetX: {group.Item.OffsetX}");
                Console.WriteLine($"[{group.Info.Id}] MapItemGroup OffsetY: {group.Item.OffsetY}");
                Console.WriteLine($"[{group.Info.Id}] MapItemGroup ParallaxX: {group.Item.ParallaxX}");
                Console.WriteLine($"[{group.Info.Id}] MapItemGroup ParallaxY: {group.Item.ParallaxY}");
                Console.WriteLine($"[{group.Info.Id}] MapItemGroup StartLayer: {group.Item.StartLayer}");
                Console.WriteLine($"[{group.Info.Id}] MapItemGroup NumLayers: {group.Item.LayersCount}");
                Console.WriteLine($"[{group.Info.Id}] MapItemGroup UseClipping: {group.Item.UseClipping}");
                Console.WriteLine($"[{group.Info.Id}] MapItemGroup ClipX: {group.Item.ClipX}");
                Console.WriteLine($"[{group.Info.Id}] MapItemGroup ClipY: {group.Item.ClipY}");
                Console.WriteLine($"[{group.Info.Id}] MapItemGroup ClipW: {group.Item.ClipWidth}");
                Console.WriteLine($"[{group.Info.Id}] MapItemGroup ClipH: {group.Item.ClipHeight}");
                Console.WriteLine("--------------------------------------");
            }
        }
    }

    private static void ShowEnvelopes(DataFile dataFile)
    {
        if (dataFile.HasItemType((int) MapItemType.Envelope))
        {
            foreach (var envelope in dataFile.GetItems<MapItemEnvelope>((int) MapItemType.Envelope))
            {
                Console.WriteLine($"[{envelope.Info.Id}] MapItemEnvelope version: {envelope.Item.ItemVersion}");
                Console.WriteLine($"[{envelope.Info.Id}] MapItemEnvelope Channels: {envelope.Item.Channels}");
                Console.WriteLine($"[{envelope.Info.Id}] MapItemEnvelope StartPoint: {envelope.Item.StartPoint}");
                Console.WriteLine($"[{envelope.Info.Id}] MapItemEnvelope PointsCount: {envelope.Item.PointsCount}");
                Console.WriteLine($"[{envelope.Info.Id}] MapItemEnvelope PointsCount: {envelope.Item.Name}");
                Console.WriteLine($"[{envelope.Info.Id}] MapItemEnvelope IsSynchronized: {envelope.Item.IsSynchronized}");
                Console.WriteLine("--------------------------------------");
            }
        }
    }

    private static void ShowInfo(DataFile dataFile)
    {
        if (dataFile.HasItemType((int) MapItemType.Info))
        {
            foreach (var mapInfo in dataFile.GetItems<MapItemInfo>((int) MapItemType.Info))
            {
                Console.WriteLine($"[{mapInfo.Info.Id}] MapItemInfo version: {mapInfo.Item.ItemVersion}");

                if (mapInfo.Item.DataIndexAuthor > -1)
                {
                    var author = dataFile.GetDataAsString(mapInfo.Item.DataIndexAuthor);
                    Console.WriteLine($"[{mapInfo.Info.Id}] MapItemInfo author: {author}");
                }

                if (mapInfo.Item.DataIndexVersion > -1)
                {
                    var version = dataFile.GetDataAsString(mapInfo.Item.DataIndexVersion);
                    Console.WriteLine($"[{mapInfo.Info.Id}] MapItemInfo version: {version}");
                }

                if (mapInfo.Item.DataIndexCredits > -1)
                {
                    var credits = dataFile.GetDataAsString(mapInfo.Item.DataIndexCredits);
                    Console.WriteLine($"[{mapInfo.Info.Id}] MapItemInfo credits: {credits}");
                }

                if (mapInfo.Item.DataIndexLicense > -1)
                {
                    var license = dataFile.GetDataAsString(mapInfo.Item.DataIndexLicense);
                    Console.WriteLine($"[{mapInfo.Info.Id}] MapItemInfo license: {license}");
                }
                    
                Console.WriteLine("--------------------------------------");
            }
        }
    }

    private static void ShowImages(DataFile dataFile)
    {
        Directory.CreateDirectory("images");
            
        if (dataFile.HasItemType((int) MapItemType.Image))
        {
            foreach (var mapImage in dataFile.GetItems<MapItemImage>((int) MapItemType.Image))
            {
                var imageName = dataFile.GetDataAsString(mapImage.Item.DataIndexName);
                Console.WriteLine($"[{mapImage.Info.Id}] Image: {imageName}");
                Console.WriteLine("--------------------------------------");

                if (!mapImage.Item.IsExternal)
                {
                    var data = dataFile.GetDataAsRaw(mapImage.Item.DateIndexImage);
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
        if (dataFile.HasItemType((int) MapItemType.Version))
        {
            foreach (var mapVersion in dataFile.GetItems<MapItemVersion>((int) MapItemType.Version))
            {
                Console.WriteLine($"[{mapVersion.Info.Id}] MapItemVersion version: {mapVersion.Item.Version}");
                Console.WriteLine("--------------------------------------");
            }
        }
    }
}
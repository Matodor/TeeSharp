using System;
using System.IO;
using TeeSharp.Map;

namespace Examples.Map
{
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
                    }
                    else
                    {
                        Console.WriteLine($"Loading error: {error}");
                    }
                }
            }

            Console.ReadKey();
        }

        private static void ShowInfo(DataFile dataFile)
        {
            if (dataFile.HasItemType((int) MapItem.Info))
            {
                foreach (var item in dataFile.GetItems<MapItemInfo>((int) MapItem.Info))
                {
                    var itemVersion = item.ItemVersion;
                    Console.WriteLine($"MapItemInfo version: {itemVersion}");

                    if (item.DataIndexAuthor > -1)
                    {
                        var author = dataFile.GetDataAsString(item.DataIndexAuthor);
                        Console.WriteLine($"Map author: {author}");
                    }

                    if (item.DataIndexVersion > -1)
                    {
                        var version = dataFile.GetDataAsString(item.DataIndexVersion);
                        Console.WriteLine($"Map version: {version}");
                    }

                    if (item.DataIndexCredits > -1)
                    {
                        var credits = dataFile.GetDataAsString(item.DataIndexCredits);
                        Console.WriteLine($"Map credits: {credits}");
                    }

                    if (item.DataIndexLicense > -1)
                    {
                        var license = dataFile.GetDataAsString(item.DataIndexLicense);
                        Console.WriteLine($"Map license: {license}");
                    }
                }
                            
                Console.WriteLine("--------------------------------------");
            }
        }

        private static void ShowImages(DataFile dataFile)
        {
            if (dataFile.HasItemType((int) MapItem.Image))
            {
                foreach (var item in dataFile.GetItems<MapItemImage>((int) MapItem.Image))
                {
                    var imageName = dataFile.GetDataAsString(item.DataIndexName);
                    Console.WriteLine($"Image: {imageName}");
                }
                            
                Console.WriteLine("--------------------------------------");
            }
        }

        private static void ShowVersion(DataFile dataFile)
        {
            if (dataFile.HasItemType((int) MapItem.Version))
            {
                foreach (var item in dataFile.GetItems<MapItemVersion>((int) MapItem.Version))
                    Console.WriteLine($"Map version: {item.Version}");
                            
                Console.WriteLine("--------------------------------------");
            }
        }
    }
}
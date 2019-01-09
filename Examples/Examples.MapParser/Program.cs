using System;
using System.IO;
using TeeSharp.Core;
using TeeSharp.Map;
using TeeSharp.Map.MapItems;

namespace Examples.MapParser
{
    internal class Program
    {
        private const string MAP_NAME = "Kobra 4";

        static void Main(string[] args)
        {
            using (var stream = File.OpenRead($"maps/{MAP_NAME}.map"))
            {
                if (stream == null)
                {
                    Debug.Error("map", $"could not open map='{MAP_NAME}'");
                }
                else
                {
                    var mapContainer = MapContainer.Load(stream, out var error);
                    if (mapContainer == null)
                    {
                        Debug.Error("map", $"error with load map='{MAP_NAME}' ({error})");
                    }
                    else
                    {
                        Debug.Log("map", $"successful load map='{MAP_NAME}' ({error})");
                        ShowMapInfo(mapContainer);
                        ExportImages(mapContainer);
                        Debug.Log("map", "succesful parsed");
                    }
                }
            }

            Console.ReadLine();
        }

        private static void ExportImages(MapContainer mapContainer)
        {
            mapContainer.GetType(MapItemTypes.Image, out var imagesStart, out var imagesNum);
            Debug.Log("map", imagesNum > 0 ? "images:" : "images not found");

            for (var i = 0; i < imagesNum; i++)
            {
                var image = mapContainer.GetItem<MapItemImage>(imagesStart + i, out _, out _);
                var imageName = mapContainer.GetData<string>(image.ImageName);

                Debug.Log("map", "   " + string.Join(';', new string[]
                {
                    $"name={imageName}",
                    $"width={image.Width}",
                    $"height={image.Height}",
                    $"external={image.External}"
                }));

                //var imageData = mapContainer.GetData<byte[]>(image.ImageData);
                //var format = Image.DetectFormat(imageData);

                //using (var image32 = Image.Load<Rgba32>(imageData))
                //{
                //    image32.Save($"{image.ImageName}.png");
                //}

                mapContainer.UnloadData(image.ImageName);
            }
        }

        private static void ShowMapInfo(MapContainer mapContainer)
        {
            Debug.Log("map", $"map size={mapContainer.Size} bytes");
            Debug.Log("map", $"map crc={mapContainer.CRC}");
        }
    }
}

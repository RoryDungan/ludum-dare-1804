using System;
using System.IO;
using UnityEngine;

public class PhotoSaveManager : Singleton<PhotoSaveManager>
{
    private const string PhotosDir = "Photos";
    private const string ThumbnailsDir = "Thumbnails";

    private readonly PhotosCollection photoCollection;

    public PhotoSaveManager()
    {
        photoCollection = new PhotosCollection(
            Path.Combine(Application.persistentDataPath, "photos.json")
        );
    }

    public string SavePhoto(RenderTexture photo)
    {
        var texture = new Texture2D(photo.width, photo.height, TextureFormat.ARGB32, false);
        RenderTexture.active = photo;
        texture.ReadPixels(new Rect(0, 0, photo.width, photo.height), 0, 0);
        texture.Apply();

        var id = Guid.NewGuid().ToString();

        var photoDest = Path.Combine(Application.persistentDataPath, PhotosDir);
        Directory.CreateDirectory(photoDest);
        File.WriteAllBytes(
            Path.Combine(photoDest, id + ".png"),
            texture.EncodeToPNG()
        );

        // Save thumbnail at quarter scale
        var thumbnail = ResizeTexture(texture, 0.2f);
        var thumbDest = Path.Combine(Application.persistentDataPath, ThumbnailsDir);
        Directory.CreateDirectory(thumbDest);
        File.WriteAllBytes(
            Path.Combine(thumbDest, id + ".png"),
            thumbnail.EncodeToPNG()
        );

        photoCollection.AddPhoto(id);

        return id;
    }

    /// <summary>
    /// Resize a texture to a specified scale. Taken from this blog post: 
    /// http://blog.collectivemass.com/2014/03/resizing-textures-in-unity/
    /// </summary>
    public static Texture2D ResizeTexture(Texture2D pSource, float pScale){
     
        //*** Variables
        int i;
     
        //*** Get All the source pixels
        Color[] aSourceColor = pSource.GetPixels(0);
        Vector2 vSourceSize = new Vector2(pSource.width, pSource.height);
     
        //*** Calculate New Size
        float xWidth = Mathf.RoundToInt((float)pSource.width * pScale);                     
        float xHeight = Mathf.RoundToInt((float)pSource.height * pScale);
     
        //*** Make New
        Texture2D oNewTex = new Texture2D((int)xWidth, (int)xHeight, TextureFormat.RGBA32, false);
     
        //*** Make destination array
        int xLength = (int)xWidth * (int)xHeight;
        Color[] aColor = new Color[xLength];
     
        Vector2 vPixelSize = new Vector2(vSourceSize.x / xWidth, vSourceSize.y / xHeight);
     
        //*** Loop through destination pixels and process
        Vector2 vCenter = new Vector2();
        for(i=0; i<xLength; i++){
     
            //*** Figure out x&y
            float xX = (float)i % xWidth;
            float xY = Mathf.Floor((float)i / xWidth);
     
            //*** Calculate Center
            vCenter.x = (xX / xWidth) * vSourceSize.x;
            vCenter.y = (xY / xHeight) * vSourceSize.y;

            //*** Average
 
            //*** Calculate grid around point
            int xXFrom = (int)Mathf.Max(Mathf.Floor(vCenter.x - (vPixelSize.x * 0.5f)), 0);
            int xXTo = (int)Mathf.Min(Mathf.Ceil(vCenter.x + (vPixelSize.x * 0.5f)), vSourceSize.x);
            int xYFrom = (int)Mathf.Max(Mathf.Floor(vCenter.y - (vPixelSize.y * 0.5f)), 0);
            int xYTo = (int)Mathf.Min(Mathf.Ceil(vCenter.y + (vPixelSize.y * 0.5f)), vSourceSize.y);
 
            //*** Loop and accumulate
            Vector4 oColorTotal = new Vector4();
            Color oColorTemp = new Color();
            float xGridCount = 0;
            for(int iy = xYFrom; iy < xYTo; iy++){
                for(int ix = xXFrom; ix < xXTo; ix++){
 
                    //*** Get Color
                    oColorTemp += aSourceColor[(int)(((float)iy * vSourceSize.x) + ix)];
 
                    //*** Sum
                    xGridCount++;
                }
            }
 
            //*** Average Color
            aColor[i] = oColorTemp / (float)xGridCount;
        }
     
        //*** Set Pixels
        oNewTex.SetPixels(aColor);
        oNewTex.Apply();
     
        //*** Return
        return oNewTex;
    }
}

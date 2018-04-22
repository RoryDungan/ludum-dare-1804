using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PhotosCollection
{
    /// <summary>
    /// Location of the collection file.
    /// </summary>
    private readonly string collectionFilePath;

    /// <summary>
    /// Cache of photo IDs.
    /// </summary>
    private readonly IList<string> photoIds;

    public PhotosCollection(string collectionPath)
    {
        if (!File.Exists(collectionPath))
        {
            photoIds = new List<string>();
        }
        else
        {
            var collectionFile = File.ReadAllText(collectionPath);
            photoIds = JsonUtility.FromJson<List<string>>(collectionFile);
        }

        collectionFilePath = collectionPath;
    }

    public bool AddPhoto(string id)
    {
        if (photoIds.Contains(id))
        {
            return false;
        }

        photoIds.Add(id);
        UpdateCollectionFile();

        return true;
    }

    public bool RemovePhoto(string id)
    {
        var ret = photoIds.Remove(id);
        if (ret)
        {
            UpdateCollectionFile();
        }
        return ret;
    }

    public IEnumerable<string> GetPhotoIds()
    {
        return photoIds;
    }

    private void UpdateCollectionFile()
    {
        File.WriteAllText(collectionFilePath, JsonUtility.ToJson(photoIds));
    }
}

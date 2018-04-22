using System;
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
    private PhotoCollectionData data;

    [Serializable]
    private struct PhotoCollectionData
    {
        public List<string> photoIds;
    }

    public PhotosCollection(string collectionPath)
    {
        if (File.Exists(collectionPath))
        {
            var collectionFile = File.ReadAllText(collectionPath);
            data = JsonUtility.FromJson<PhotoCollectionData>(collectionFile);
        }
        else
        {
            data.photoIds = new List<string>();
        }

        collectionFilePath = collectionPath;
    }

    public bool AddPhoto(string id)
    {
        if (data.photoIds.Contains(id))
        {
            return false;
        }

        data.photoIds.Add(id);
        UpdateCollectionFile();

        return true;
    }

    public bool RemovePhoto(string id)
    {
        var ret = data.photoIds.Remove(id);
        if (ret)
        {
            UpdateCollectionFile();
        }
        return ret;
    }

    public IEnumerable<string> GetPhotoIds()
    {
        return data.photoIds;
    }

    private void UpdateCollectionFile()
    {
        File.WriteAllText(collectionFilePath, JsonUtility.ToJson(data));
    }
}

using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class FinalScreenUI : MonoBehaviour 
{
    private GameObject template;
    private Transform imageParent;

    private void Awake()
    {
        template = GetComponentsInChildren<Transform>(true)
            .Select(t => t.gameObject)
            .FirstOrDefault(go => go.name == "ImageTemplate"); 
        Assert.IsNotNull(template);
        imageParent = template.transform.parent;

        template.SetActive(false);
    }

    public void ClearImages()
    {
        foreach (Transform child in imageParent)
        {
            if (child.gameObject == template)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    public void LoadAndDisplayImage(string id)
    {
        var newTexture = PhotoSaveManager.Instance.LoadPhotoThumbnail(id);

        var newImageGO = Instantiate(template);
        newImageGO.transform.SetParent(imageParent);
        newImageGO.SetActive(true);
        newImageGO.name = id;

        newImageGO.GetComponentInChildren<RawImage>(true)
            .texture = newTexture;
    }
}

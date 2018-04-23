using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/// <summary>
/// Updates the UI text with the number of photos taken.
/// </summary>
public class PhotosRemainingUI : MonoBehaviour
{
    private GameManager gm;

    private Text text;

    // Use this for initialization
    void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        Assert.IsNotNull(gm);

        text = GetComponentInChildren<Text>(true);
        Assert.IsNotNull(text);
    }
    
    // Update is called once per frame
    void Update ()
    {
        text.text = gm.PhotosTaken + "/" + gm.TotalPhotos;
    }
}

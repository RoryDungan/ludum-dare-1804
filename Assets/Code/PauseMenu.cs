using Assets;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    private bool paused = false;

    // Update is called once per frame
    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused)
            {
                Unpause();
            }
            else
            {
                Pause();
            }
        }
    }

    void Pause()
    {
        paused = true;
        PauseManager.Instance.Pause();

        SetPauseMenuVisible(paused);
    }

    void Unpause()
    {
        paused = false;
        PauseManager.Instance.Unpause();

        SetPauseMenuVisible(paused);
    }

    void SetPauseMenuVisible(bool visible)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(visible);
        }
    }

    public void ContinueClicked()
    {
        Unpause();
    }

    public void QuitClicked()
    {
        Application.Quit();
    }
}

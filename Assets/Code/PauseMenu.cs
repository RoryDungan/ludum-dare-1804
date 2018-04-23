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
        Time.timeScale = 0f;

        SetPauseMenuVisible(paused);
    }

    void Unpause()
    {
        paused = false;
        Time.timeScale = 1f;

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

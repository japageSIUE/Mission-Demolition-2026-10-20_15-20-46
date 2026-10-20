using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class TryAgain : MonoBehaviour
{
    public Button tryAgainButton;
    MissionDemolition md;

    void Start()
    {
        // finding the button in the hierarchy
        Button btn = tryAgainButton.GetComponent<Button>();
        btn.onClick.AddListener(ResetGame);
    }

    void ResetGame()
    {
        SceneManager.LoadScene("_Scene_0");
    }
}

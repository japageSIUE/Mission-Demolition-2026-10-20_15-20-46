using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public enum GameMode
{
    idle,
    playing,
    levelEnd
}
public class MissionDemolition : MonoBehaviour
{
    static private MissionDemolition S; // a private Singleton

    [Header("Inscribed")]
    public Text uitLevel; // the UIText_Level text
    public Text uitShots; // the UIText_Shots text
    public Vector3 castlePos; // the place to put castles
    public GameObject[] castles; // an array of the castles
    // stuff for the game over screen
    public Canvas parentCanvas;
    public GameObject gameOverPrefab; // this shows up in the inspector 
        // and needs assigned
    private GameObject currentGameOver; // this stores the instantiated
        // gameOverPrefab and needs to be destroyed


    [Header("Dynamic")]
    public int level; // the current level
    public int levelMax; // the number of levels
    public int shotsTaken;
    public GameObject castle; // the current castle
    public GameMode mode = GameMode.idle;
    public string showing = "Show Slingshot"; // FollowCam mode

    // Start is called before the first frame update
    void Start()
    {
        S = this; // define the Singleton

        level = 0; 
        shotsTaken = 0;
        levelMax = castles.Length;
        StartLevel();
    }

    void StartLevel()
    {
        // get rid of the old castle if one exists
        if(castle != null)
        {
            Destroy(castle);
        }

        // destroy old projectiles if they exist (the method is not yet written)
        Projectile.DESTROY_PROJECTILES(); 

        // instantiate the new castle
        castle = Instantiate<GameObject>(castles[level]);
        castle.transform.position = castlePos;

        // reset the goal
        Goal.goalMet = false;

        UpdateGUI();

        mode = GameMode.playing;

        // zoom out to show both
        FollowCam.SWITCH_VIEW(FollowCam.eView.both);
    }

    void UpdateGUI()
    {
        // show the data in the GUITexts (levelMax - 1 cause I want the
        // "thx 4 playing castle" to not feel like a playable level [even though it is])
        uitLevel.text = "Level: " + (level + 1) + " of " + (levelMax - 1);
        uitShots.text = "Shots Taken: " + shotsTaken;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGUI();

        // check for level end
        if((mode == GameMode.playing) && Goal.goalMet)
        {
            // change mode to stop checking for level end
            mode = GameMode.levelEnd;
            // zoom out to show both
            FollowCam.SWITCH_VIEW(FollowCam.eView.both);
            // start the next level in 2 seconds
            Invoke("NextLevel", 2f);
        }
    }

    void NextLevel()
    {
        level++;
        // for the cool game over
        if(level == levelMax - 1)
        {
            // level = 0;
            // shotsTaken = 0;
            InstantiateGameOver();
        }
        if(level == levelMax)
        {
            ResetGame();
        }

        StartLevel();
    }

    void InstantiateGameOver()
    {
        // instantiate the gameOverPrefab and store it in currentGameOver
        currentGameOver = Instantiate(gameOverPrefab);
        // parent to the canvas so it renders correctly
        currentGameOver.transform.SetParent(parentCanvas.transform, false);
    }

    void ResetGame()
    {
        SceneManager.LoadScene("_Scene_0");
    }

    // static method that allows code anywhere to increment shotsTaken
    static public void SHOT_FIRED()
    {
        S.shotsTaken++;
    }

    // static method that allows code anywhere to get a reference to S.castle
    static public GameObject GET_CASTLE()
    {
        return S.castle;
    }
}

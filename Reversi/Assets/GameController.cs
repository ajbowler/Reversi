using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
    public Board board;
    public Piece[] pieces;
    public GameObject mainMenu;
    public Dropdown colorSelector;
    public Dropdown difficultySelector;
    public EventSystem eventSystem;

    private int difficulty;
    private string playerColor;

    void Start()
    {

    }

    void Update()
    {
        GameObject currentSelection = eventSystem.currentSelectedGameObject;
        if (currentSelection == GameObject.Find("Play Button"))
        {
            mainMenu.SetActive(false);
            difficulty = difficultySelector.value + 1;
            if (colorSelector.value == 0) playerColor = "black";
            else playerColor = "white";
            Debug.Log(playerColor);
        }
    }

    void DeterminePlacementCoordinates()
    {

    }
}

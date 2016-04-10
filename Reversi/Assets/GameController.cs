using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour {
    public Board board;
    public Piece[] pieces;
    public GameObject mainMenu;
    public EventSystem eventSystem;

    void Start()
    {

    }

    void Update()
    {
        GameObject currentSelection = eventSystem.currentSelectedGameObject;
        if (currentSelection == GameObject.Find("Play Button"))
        {
            mainMenu.SetActive(false);
        }
    }

    void DeterminePlacementCoordinates()
    {

    }
}

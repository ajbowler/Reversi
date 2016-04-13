using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public enum Player
{
    Black,
    White,
    Nobody
};

public class GameController : MonoBehaviour {
    public Board board;
    public GameObject mainMenu;
    public Dropdown colorSelector;
    public Dropdown difficultySelector;
    public EventSystem eventSystem;
    public Player human;
    public Player ai;

    private int difficulty;
    private Player ply;

    void Start()
    {
        ply = Player.Black;
    }

    void Update()
    {
        if (mainMenu.activeSelf) UseMenu();
        else StartCoroutine(PlayGame());
    }

    IEnumerator PlayGame()
    {
        if (board.legalMoves.Count == 0)
            Debug.Log("Game Over");
        else
        {
            UpdateSquareDisplays();
            //if (ply == human)
            //{
            //    if (Input.GetMouseButtonUp(0))
            //    {
            //        int clickedSquare = GetClickedSquare();
            //        if (clickedSquare > -1 && board.squares[clickedSquare].isLegalMove)
            //            MakeMove(clickedSquare);
            //    }
            //    while (ply == human) yield return null;
            //}

            // TESTING PIECE PLACEMENT AND FLIPPING
            if (Input.GetMouseButtonUp(0))
            {
                int clickedSquare = GetClickedSquare();
                if (clickedSquare > -1 && board.squares[clickedSquare].isLegalMove)
                    MakeMove(board.squares[clickedSquare]);
            }
            while (ply == human) yield return null;

            //else yield return StartCoroutine(AITurn());
        }
    }

    IEnumerator AITurn()
    {
        while (ply == ai) yield return null; // TODO
    }

    void UseMenu()
    {
        GameObject currentSelection = eventSystem.currentSelectedGameObject;
        board.gameObject.SetActive(false);
        if (Input.GetKeyUp(KeyCode.Return))
        {
            mainMenu.SetActive(false);
            difficulty = difficultySelector.value + 1;
            if (colorSelector.value == 0)
            {
                human = Player.Black;
                ai = Player.White;
            }
            else
            {
                human = Player.White;
                ai = Player.Black;
            }
            board.SetInitialBoard();
            SetPly(Player.Black);
        }
    }

    void MakeMove(Square square)
    {
        if (square.isLegalMove)
        {
            board.CaptureTile(ply, square);
            FlankPieces(ply, square);
        }
        if (ply == human) SetPly(ai);
        else SetPly(human);
    }

    void SetPly(Player player)
    {
        ply = player;
        UpdateLegalMoves(ply);
    }

    public void FlankPieces(Player currentPlayer, Square square)
    {
        List<int> flankedPieces = new List<int>();
        flankedPieces.AddRange(AddFlankedPieces(currentPlayer, square, -1));
        flankedPieces.AddRange(AddFlankedPieces(currentPlayer, square, 1));
        flankedPieces.AddRange(AddFlankedPieces(currentPlayer, square, -8));
        flankedPieces.AddRange(AddFlankedPieces(currentPlayer, square, 8));
        flankedPieces.AddRange(AddFlankedPieces(currentPlayer, square, -9));
        flankedPieces.AddRange(AddFlankedPieces(currentPlayer, square, 9));
        flankedPieces.AddRange(AddFlankedPieces(currentPlayer, square, -7));
        flankedPieces.AddRange(AddFlankedPieces(currentPlayer, square, 7));

        foreach (int piecePos in flankedPieces)
            board.FlipPiece(currentPlayer, board.squares[piecePos]);
    }

    public void UpdateLegalMoves(Player currentPlayer)
    {
        if (board.legalMoves.Count > 0)
            ResetLegalMoves();
        foreach (Square square in board.squares)
        {
            if (square.player == currentPlayer)
            {
                AddLegalMove(GetLegalMoveOnPath(currentPlayer, square, -1));
                AddLegalMove(GetLegalMoveOnPath(currentPlayer, square, 1));
                AddLegalMove(GetLegalMoveOnPath(currentPlayer, square, -8));
                AddLegalMove(GetLegalMoveOnPath(currentPlayer, square, 8));
                AddLegalMove(GetLegalMoveOnPath(currentPlayer, square, -9));
                AddLegalMove(GetLegalMoveOnPath(currentPlayer, square, 9));
                AddLegalMove(GetLegalMoveOnPath(currentPlayer, square, -7));
                AddLegalMove(GetLegalMoveOnPath(currentPlayer, square, 7));
            }
        }
    }

    public void UpdateSquareDisplays()
    {
        foreach (Square s in board.squares)
        {
            if (s.isLegalMove)
                s.gameObject.GetComponent<MeshRenderer>().enabled = true;
            else
                s.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private List<int> AddFlankedPieces(Player currentPlayer, Square square, int direction)
    {
        List<int> flankedPieces = new List<int>();

        for (int i = square.position + direction; i >= 0 && i <= 63; i += direction)
        {
            if (board.squares[i].player != currentPlayer && board.squares[i].player != Player.Nobody)
                flankedPieces.Add(i);
            else if (board.squares[i].player == currentPlayer)
                break; // we have flanked everything we can in this direction
            else if (board.squares[i].player == Player.Nobody)
            {
                flankedPieces.Clear();
                break; // nothing here can be flanked
            }
        }

        return flankedPieces;
    }

    private int GetLegalMoveOnPath(Player player, Square square, int direction)
    {
        bool flankablesExist = false;

        for (int i = square.position + direction; i >= 0 && i <= 63; i += direction)
        {
            // TODO establish rules for rows and columns
            if (IsPastBoardEdge(square, board.squares[i], direction))
                return -1; // we can't go this way
            if (board.squares[i].player != player && board.squares[i].player != Player.Nobody)
                flankablesExist = true;
            else if (board.squares[i].player == Player.Nobody && flankablesExist)
                return i;
            else return -1; // we can't go this way
        }

        return -1; // shouldn't be reached unless there is an indexing error
    }

    private bool IsPastBoardEdge(Square start, Square end, int direction)
    {
        if (direction > 0)
            return (start.row - end.row) > 0;
        else
            return (start.row - end.row) < 0;
    }

    private void AddLegalMove(int position)
    {
        if (position > -1)
        {
            board.legalMoves.Add(position);
            board.squares[position].isLegalMove = true;
        }
    }

    private int GetClickedSquare()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            Square s = hit.transform.gameObject.GetComponent<Square>();
            return s.position;
        }
        else return -1;
    }

    private void ResetLegalMoves()
    {
        foreach (Square s in board.squares)
        {
            if (s.isLegalMove)
                s.isLegalMove = false;
        }

        board.legalMoves.Clear();
    }
}

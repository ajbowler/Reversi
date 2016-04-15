using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public enum Player
{
    Black,
    White,
    Nobody
};

public class MinimaxPair<First, Second>
{
    public First first { get; set; }
    public Second second { get; set; }

    public MinimaxPair(First first, Second second)
    {
        this.first = first;
        this.second = second;
    }
}

public class GameController : MonoBehaviour {
    public Board board;
    public GameObject mainMenu;
    public GameObject gameOverMenu;
    public Text blackScoreText;
    public Text whiteScoreText;
    public Text winnerText;
    public Dropdown colorSelector;
    public Dropdown difficultySelector;
    public Player human;
    public Player ai;

    private int difficulty;
    private int blackScore;
    private int whiteScore;
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
            GameOver();
        else
        {
            UpdateSquareDisplays();
            UpdateScore();
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
            blackScoreText.gameObject.SetActive(true);
            whiteScoreText.gameObject.SetActive(true);
            SetPly(Player.Black);
        }
    }

    void GameOver()
    {
        board.gameObject.SetActive(false);
        board.Reset();

        if (blackScore > whiteScore)
            winnerText.text = "Black Wins!";
        else if (whiteScore > blackScore)
            winnerText.text = "White Wins!";
        else
            winnerText.text = "Tie!";

        gameOverMenu.gameObject.SetActive(true);
        if (Input.GetKeyUp(KeyCode.R))
        {
            mainMenu.gameObject.SetActive(true);
            gameOverMenu.gameObject.SetActive(false);
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

    void UpdateScore()
    {
        blackScore = 0;
        whiteScore = 0;
        foreach (Square square in board.squares)
        {
            if (square.player == Player.Black)
                blackScore++;
            if (square.player == Player.White)
                whiteScore++;
        }

        blackScoreText.text = "Black: " + blackScore;
        whiteScoreText.text = "White: " + whiteScore;
    }

    MinimaxPair<List<Square>, double> Minimax(Board board, Player currentPlayer, int depth)
    {
        if (board.legalMoves.Count == 0 || depth == difficulty)
            return new MinimaxPair<List<Square>, double>(board.Evaluate(currentPlayer), Double.MaxValue);

        List<Square> bestMove = new List<Square>();
        double bestScore;
        if (currentPlayer == ai)
            bestScore = -Mathf.Infinity;
        else
            bestScore = Mathf.Infinity;

        foreach (Square legalMove in board.legalMoves)
        {

        }

        return null; // TODO
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
            else if (board.squares[i].player == Player.Nobody || IsPastBoardEdge(square, board.squares[i], direction))
            {
                flankedPieces.Clear();
                break; // nothing here can be flanked
            }
        }

        return flankedPieces;
    }

    private Square GetLegalMoveOnPath(Player player, Square square, int direction)
    {
        bool flankablesExist = false;

        for (int i = square.position + direction; i >= 0 && i <= 63; i += direction)
        {
            if (IsPastBoardEdge(square, board.squares[i], direction))
                return null; // we can't go this way
            if (board.squares[i].player != player && board.squares[i].player != Player.Nobody)
                flankablesExist = true;
            else if (board.squares[i].player == Player.Nobody && flankablesExist)
                return board.squares[i];
            else return null; // we can't go this way
        }

        return null; // shouldn't be reached unless there is an indexing error
    }

    private bool IsPastBoardEdge(Square start, Square end, int direction)
    {
        if (direction > 0) return (start.row - end.row) > 0;
        else return (start.row - end.row) < 0;
    }

    private void AddLegalMove(Square square)
    {
        if (square != null)
        {
            board.legalMoves.Add(square);
            board.squares[square.position].isLegalMove = true;
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

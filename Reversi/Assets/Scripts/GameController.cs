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
    public First bestMove { get; set; }
    public Second bestScore { get; set; }

    public MinimaxPair(First bestMove, Second bestScore)
    {
        this.bestMove = bestMove;
        this.bestScore = bestScore;
    }

    public MinimaxPair()
    {

    }
}

public class GameController : MonoBehaviour
{
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
            if (ply == human) yield return StartCoroutine(HumanTurn());
            else yield return StartCoroutine(AITurn());
        }
    }

    IEnumerator HumanTurn()
    {
        if (Input.GetMouseButtonUp(0))
        {
            int clickedSquare = GetClickedSquare();
            if (clickedSquare > -1 && board.squares[clickedSquare].isLegalMove)
            {
                MakeMove(board, ply, board.squares[clickedSquare], true);
                if (ply == human) SetPly(ai);
                else SetPly(human);
            }
        }
        while (ply == human) yield return null;
    }

    IEnumerator AITurn()
    {
        MinimaxPair<Square[], double> moveToMake = Minimax(board.squares, difficulty, true);
        Square[] nextBoard = moveToMake.bestMove;

        int movePos = GetNextMove(board.squares, nextBoard);
        MakeMove(board, ply, board.squares[movePos], true);

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

    void MakeMove(Board board, Player ply, Square square, bool isTrueBoard)
    {
        if (square.isLegalMove)
        {
            board.CaptureTile(ply, square, isTrueBoard);
            FlankPieces(board, ply, square, isTrueBoard);
        }
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

    MinimaxPair<Square[], double> Minimax(Square[] squares, int depth, bool maximizingPlayer)
    {
        List<Square> legalMoves = GetLegalMoves(squares);
        if (legalMoves.Count == 0 || depth == 0)
            return new MinimaxPair<Square[], double>(squares, Evaluate(squares));

        MinimaxPair<Square[], double> bestBoard = new MinimaxPair<Square[], double>();

        if (maximizingPlayer)
        {
            bestBoard.bestScore = Mathf.NegativeInfinity;
            foreach (Square legalMove in legalMoves)
            {
                Board newBoard = CopyBoard();
                MakeMove(newBoard, ply, legalMove, false);
                MinimaxPair<Square[], double> nextMove = Minimax(newBoard.squares, depth - 1, false);
                if (nextMove.bestScore > bestBoard.bestScore)
                {
                    bestBoard.bestMove = nextMove.bestMove;
                    bestBoard.bestScore = nextMove.bestScore;
                }
            }
            return bestBoard;
        }
        else
        {
            bestBoard.bestScore = Mathf.Infinity;
            foreach (Square legalMove in legalMoves)
            {
                Board newBoard = CopyBoard();
                MakeMove(newBoard, ply, legalMove, false);
                MinimaxPair<Square[], double> nextMove = Minimax(newBoard.squares, depth - 1, true);
                if (nextMove.bestScore < bestBoard.bestScore)
                {
                    bestBoard.bestMove = nextMove.bestMove;
                    bestBoard.bestScore = nextMove.bestScore;
                }
            }
            return bestBoard;
        }
    }

    public void FlankPieces(Board board, Player currentPlayer, Square square, bool isTrueBoard)
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

        if (isTrueBoard)
        {
            foreach (int piecePos in flankedPieces)
                board.FlipPiece(currentPlayer, board.pieces[piecePos]);
        }
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

    private Board CopyBoard()
    {
        Board newBoard = gameObject.AddComponent<Board>();
        newBoard.gameController = this;
        newBoard.legalMoves = new List<Square>();
        newBoard.squares = (Square[])board.squares.Clone();
        for (int i = 0; i < 64; i++)
        {
            if (newBoard.squares[i].isLegalMove)
                newBoard.legalMoves.Add(newBoard.squares[i]);
        }
        return newBoard;
    }

    private List<Square> GetLegalMoves(Square[] squares)
    {
        List<Square> legalMoves = new List<Square>();
        foreach (Square square in squares)
            if (square.isLegalMove) legalMoves.Add(CopySquare(square));
        return legalMoves;
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

    private Square CopySquare(Square s)
    {
        Square square = gameObject.AddComponent<Square>();
        square.column = s.column;
        square.isLegalMove = s.isLegalMove;
        square.player = s.player;
        square.position = s.position;
        square.row = s.row;
        return square;
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
            if (s != null) return s.position;
            else return -1;
        }
        else return -1;
    }

    private double Evaluate(Square[] board)
    {
        double score = 0.0f;
        score += GetCornerScore(board);
        score += GetEdgeScore(board);
        score += GetPieceCountScore(board);

        return score;
    }

    private double GetCornerScore(Square[] board)
    {
        int[] corners = new int[4] { 0, 7, 56, 63 };
        double score = 0.0f;
        foreach (int corner in corners)
        {
            if (board[corner].player == ai)
                score++;
            else if (board[corner].player == human)
                score--;
        }

        return score * 20;
    }

    private double GetEdgeScore(Square[] board)
    {
        double score = 0.0f;
        for (int i = 0; i < 64; i++)
        {
            if (i % 8 == 0 || i % 8 == 7 || i / 8 == 0 || i / 8 == 7)
            {
                if (board[i].player == ai)
                    score++;
                else if (board[i].player == human)
                    score--;
            }
        }

        return score * 10;
    }

    private double GetPieceCountScore(Square[] board)
    {
        double score = 0.0f;

        for (int i = 0; i < 64; i++)
        {
            if (board[i].player == ai)
                score++;
            else if (board[i].player == human)
                score--;
        }

        return score;
    }

    private int GetNextMove(Square[] currentBoard, Square[] nextBoard)
    {
        for (int i = 0; i < 64; i++)
        {
            if (currentBoard[i].player == Player.Nobody && nextBoard[i].player == ai)
                return i;
        }

        Debug.Log("ERROR");
        return -1;
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

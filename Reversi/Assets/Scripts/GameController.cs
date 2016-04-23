using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

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
    public Square[] squares;
    public Dictionary<int, Piece> pieces;
    public Piece piecePrefab;
    public Square squarePrefab;
    public GameObject mainMenu;
    public GameObject gameOverMenu;
    public Text blackScoreText;
    public Text whiteScoreText;
    public Text winnerText;
    public Dropdown colorSelector;
    public Dropdown difficultySelector;
    public Player human;
    public Player ai;
    public Player[] playerMap;

    private int difficulty;
    private int blackScore;
    private int whiteScore;
    private Player ply;

    void Start()
    {
        ply = Player.Black;
        squares = new Square[64];
        playerMap = new Player[64];
        pieces = new Dictionary<int, Piece>();
    }

    void Update()
    {
        if (mainMenu.activeSelf) UseMenu();
        else StartCoroutine(PlayGame());
    }

    IEnumerator PlayGame()
    {
        if (!HasLegalMoves())
            GameOver();
        else
        {
            UpdatePieces();
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
            if (clickedSquare > -1 && squares[clickedSquare].isLegalMove)
            {
                MakeMove(playerMap, ply, clickedSquare);
                SetPly(ai);
            }
        }
        while (ply == human) yield return null;
    }

    IEnumerator AITurn()
    {
        //Player[] boardMap = GetBoardPlayerMap(board.squares);
        //MinimaxPair<Player[], double> moveToMake = Minimax(boardMap, difficulty, true);
        //Player[] nextBoard = moveToMake.bestMove;

        //int movePos = GetNextMove(board.squares, nextBoard);
        //MakeMove(board, ply, board.squares[movePos]);
        //SetPly(human);

        //while (ply == ai) yield return null; // TODO

        if (Input.GetMouseButtonUp(0))
        {
            int clickedSquare = GetClickedSquare();
            if (clickedSquare > -1 && squares[clickedSquare].isLegalMove)
            {
                MakeMove(playerMap, ply, clickedSquare);
                SetPly(human);
            }
        }
        while (ply == ai) yield return null;
    }

    public void UseMenu()
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
            SetInitialBoard();
            blackScoreText.gameObject.SetActive(true);
            whiteScoreText.gameObject.SetActive(true);
            SetPly(Player.Black);
        }
    }

    public void SetInitialBoard()
    {
        board.gameObject.SetActive(true);
        for (int i = 0; i< 64; i++)
        {
            if (i == 27 || i == 36) InitializeTile(i, Player.Black);
            else if (i == 28 || i == 35) InitializeTile(i, Player.White);
            else InitializeTile(i, Player.Nobody);
        }
    }


    public void GameOver()
    {
        Reset();
        board.gameObject.SetActive(false);
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

    public void MakeMove(Player[] playerMap, Player ply, int position)
    {
        if (squares[position].isLegalMove)
        {
            CaptureTile(playerMap, ply, position);
            FlankPieces(playerMap, ply, position);
        }
    }

    public void SetPly(Player player)
    {
        ply = player;
        UpdateLegalMoves(playerMap, ply);
    }

    public void UpdateScore()
    {
        blackScore = 0;
        whiteScore = 0;
        foreach (Player player in playerMap)
        {
            if (player == Player.Black)
                blackScore++;
            else if (player == Player.White)
                whiteScore++;
        }

        blackScoreText.text = "Black: " + blackScore;
        whiteScoreText.text = "White: " + whiteScore;
    }

    public void UpdatePieces()
    {
        for (int i = 0; i < 64; i++)
        {
            if (playerMap[i] != Player.Nobody)
            {
                Piece existingPiece;
                if (!pieces.TryGetValue(i, out existingPiece))
                    PlacePiece(i);
                else
                {
                    if (pieces[i].player != playerMap[i])
                        FlipPiece(playerMap[i], pieces[i]);
                }
            }
        }
    }

    //MinimaxPair<Player[], double> Minimax(Player[] boardMap, int depth, bool maximizingPlayer)
    //{
    //    List<Square> legalMoves = GetLegalMoves(boardMap);
    //    if (legalMoves.Count == 0 || depth == 0)
    //        return new MinimaxPair<Player[], double>(boardMap, Evaluate(squares));

    //    MinimaxPair<Player[], double> bestBoard = new MinimaxPair<Player[], double>();

    //    if (maximizingPlayer)
    //    {
    //        bestBoard.bestScore = Mathf.NegativeInfinity;
    //        foreach (Square legalMove in legalMoves)
    //        {
    //            Board newBoard = CopyBoard();
    //            MakeMove(newBoard, ply, legalMove, false);
    //            Player[] newBoardMap = GetBoardPlayerMap(newBoard.squares);
    //            MinimaxPair<Player[], double> nextMove = Minimax(newBoardMap, newBoard.squares, depth - 1, false);
    //            if (nextMove.bestScore > bestBoard.bestScore)
    //            {
    //                bestBoard.bestMove = nextMove.bestMove;
    //                bestBoard.bestScore = nextMove.bestScore;
    //            }
    //        }
    //        return bestBoard;
    //    }
    //    else
    //    {
    //        bestBoard.bestScore = Mathf.Infinity;
    //        foreach (Square legalMove in legalMoves)
    //        {
    //            Board newBoard = CopyBoard();
    //            MakeMove(newBoard, ply, legalMove, false);
    //            Player[] newBoardMap = GetBoardPlayerMap(newBoard.squares);
    //            MinimaxPair<Player[], double> nextMove = Minimax(newBoardMap, newBoard.squares, depth - 1, true);
    //            if (nextMove.bestScore < bestBoard.bestScore)
    //            {
    //                bestBoard.bestMove = nextMove.bestMove;
    //                bestBoard.bestScore = nextMove.bestScore;
    //            }
    //        }
    //        return bestBoard;
    //    }
    //}

    public void UpdateSquareDisplays()
    {
        foreach (Square s in squares)
        {
            if (s.isLegalMove)
                s.gameObject.GetComponent<MeshRenderer>().enabled = true;
            else
                s.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void CaptureTile(Player[] map, Player player, int position)
    {
        squares[position].isLegalMove = false;
        map[position] = player;
    }

    public void FlankPieces(Player[] map, Player currentPlayer, int position)
    {
        List<int> flankedPieces = new List<int>();
        flankedPieces.AddRange(AddFlankedPieces(map, currentPlayer, position, -1));
        flankedPieces.AddRange(AddFlankedPieces(map, currentPlayer, position, 1));
        flankedPieces.AddRange(AddFlankedPieces(map, currentPlayer, position, -8));
        flankedPieces.AddRange(AddFlankedPieces(map, currentPlayer, position, 8));
        flankedPieces.AddRange(AddFlankedPieces(map, currentPlayer, position, -9));
        flankedPieces.AddRange(AddFlankedPieces(map, currentPlayer, position, 9));
        flankedPieces.AddRange(AddFlankedPieces(map, currentPlayer, position, -7));
        flankedPieces.AddRange(AddFlankedPieces(map, currentPlayer, position, 7));

        for (int i = 0; i < flankedPieces.Count; i++)
            CaptureTile(map, currentPlayer, flankedPieces[i]);
    }

    public void Reset()
    {
        squares = new Square[64];
        playerMap = new Player[64];
        pieces.Clear();
        GameObject[] pieceObjects = GameObject.FindGameObjectsWithTag("Piece");
        GameObject[] squareObjects = GameObject.FindGameObjectsWithTag("Square");
        for (int i = 0; i < pieceObjects.Length; i++)
            Destroy(pieceObjects[i]);
        for (int i = 0; i < squareObjects.Length; i++)
            Destroy(squareObjects[i]);
    }

    public void FlipPiece(Player player, Piece piece)
    {
        piece.player = player;
        piece.gameObject.transform.Rotate(180f, 0f, 0f);
    }

    public void PlacePiece(int position)
    {
        Vector3 piecePosition = squares[position].transform.position;
        piecePosition.y = 1f;
        Piece newPiece;
        if (playerMap[squares[position].position] == Player.White)
            newPiece = Instantiate(piecePrefab, piecePosition, Quaternion.identity) as Piece;
        else
            newPiece = Instantiate(piecePrefab, piecePosition, Quaternion.AngleAxis(180f, Vector3.right)) as Piece;
        newPiece.player = playerMap[squares[position].position];
        pieces.Add(squares[position].position, newPiece);
    }

    public void InitializeTile(int position, Player player)
    {
        playerMap[position] = player;

        Vector3 squarePos = DeterminePlacementCoordinates(position);
        squarePos.y += .2f;
        Square s = Instantiate(squarePrefab, squarePos, Quaternion.identity) as Square;
        s.gameObject.GetComponent<MeshRenderer>().enabled = false;
        s.position = position;
        s.isLegalMove = false;
        squares[position] = s;
        if (player != Player.Nobody)
            PlacePiece(position);
    }

    public void UpdateLegalMoves(Player[] map, Player currentPlayer)
    {
        ResetLegalMoves();
        for (int i = 0; i < 64; i++)
        {
            if (map[i] == currentPlayer)
            {
                AddLegalMove(map, currentPlayer, i, -1);
                AddLegalMove(map, currentPlayer, i, 1);
                AddLegalMove(map, currentPlayer, i, -8);
                AddLegalMove(map, currentPlayer, i, 8);
                AddLegalMove(map, currentPlayer, i, -9);
                AddLegalMove(map, currentPlayer, i, 9);
                AddLegalMove(map, currentPlayer, i, -7);
                AddLegalMove(map, currentPlayer, i, 7);
            }
        }
    }

    private void AddLegalMove(Player[] map, Player player, int position, int direction)
    {
        bool flankablesExist = false;

        for (int i = position + direction; i >= 0 && i <= 63; i += direction)
        {
            if (IsPastBoardEdge(position, position, direction))
                return; // we can't go this way
            if (map[i] != player && map[i] != Player.Nobody)
                flankablesExist = true;
            else if (map[i] == Player.Nobody && flankablesExist)
            {
                squares[i].isLegalMove = true;
                return;
            }
            else return; // we can't go this way
        }
    }

    public void ResetLegalMoves()
    {
        foreach (Square s in squares)
            if (s.isLegalMove)
                s.isLegalMove = false;
    }

    public bool HasLegalMoves()
    {
        for (int i = 0; i < 64; i++)
        {
            if (squares[i] != null)
            {
                if (squares[i].isLegalMove)
                    return true;
            }
        }
        return false;
    }

    private Vector3 DeterminePlacementCoordinates(int position)
    {
        int row = position / 8;
        int col = position % 8;

        float x = row - 3.5f;
        float z = col - 3.5f;

        return new Vector3(x, 1f, z);
    }

    private List<int> AddFlankedPieces(Player[] playerMap, Player currentPlayer, int position, int direction)
    {
        List<int> flankedPieces = new List<int>();

        for (int i = position + direction; i >= 0 && i <= 63; i += direction)
        {
            if (playerMap[i] != currentPlayer && playerMap[i] != Player.Nobody)
                flankedPieces.Add(i);
            else if (playerMap[i] == currentPlayer)
                break; // we have flanked everything we can in this direction
            else if (playerMap[i] == Player.Nobody || IsPastBoardEdge(position, i, direction))
            {
                flankedPieces.Clear();
                break; // nothing here can be flanked
            }
        }

        return flankedPieces;
    }

    private bool IsPastBoardEdge(int start, int end, int direction)
    {
        if (direction > 0) return ((start % 8) - (end % 8)) > 0;
        else return ((start % 8) - (end % 8)) < 0;
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

    private double Evaluate(Player[] boardMap)
    {
        double score = 0.0f;
        score += GetCornerScore(boardMap);
        score += GetEdgeScore(boardMap);
        score += GetPieceCountScore(boardMap);

        return score;
    }

    private double GetCornerScore(Player[] boardMap)
    {
        int[] corners = new int[4] { 0, 7, 56, 63 };
        double score = 0.0f;
        foreach (int corner in corners)
        {
            if (boardMap[corner] == ai)
                score++;
            else if (boardMap[corner] == human)
                score--;
        }

        return score * 20;
    }

    private double GetEdgeScore(Player[] boardMap)
    {
        double score = 0.0f;
        for (int i = 0; i < 64; i++)
        {
            if (i % 8 == 0 || i % 8 == 7 || i / 8 == 0 || i / 8 == 7)
            {
                if (boardMap[i] == ai)
                    score++;
                else if (boardMap[i] == human)
                    score--;
            }
        }

        return score * 10;
    }

    private double GetPieceCountScore(Player[] boardMap)
    {
        double score = 0.0f;

        for (int i = 0; i < 64; i++)
        {
            if (boardMap[i] == ai)
                score++;
            else if (boardMap[i] == human)
                score--;
        }

        return score;
    }

    private int GetNextMove(Player[] currentBoardMap, Player[] nextBoardMap)
    {
        for (int i = 0; i < 64; i++)
            if (currentBoardMap[i] == Player.Nobody && nextBoardMap[i] == ai)
                return i;

        Debug.Log("Error in Minimax. Could not find next move.");
        return -1;
    }
}

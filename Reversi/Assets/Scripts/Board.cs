using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {
    public GameController gameController;
    public Square square;
    public Piece piece;
    public List<Square> squares;
    public List<Piece> pieces;

    private List<int> legalMoves;

    void Start()
    {
        squares = new List<Square>();
        pieces = new List<Piece>();
        legalMoves = new List<int>();
    }

    public void SetInitialBoard()
    {
        for (int i = 0; i < 64; i++)
        {
            if (i == 27 || i == 36) InitializeTile(i, Player.Black);
            else if (i == 28 || i == 35) InitializeTile(i, Player.White);
            else InitializeTile(i, Player.Nobody);
        }
        InitializeTile(27, Player.Black);
        InitializeTile(28, Player.White);
        InitializeTile(35, Player.White);
        InitializeTile(36, Player.Black);
    }

    Vector3 DeterminePlacementCoordinates(int position)
    {
        int row = position / 8;
        int col = position % 8;

        float x = row - 3.5f;
        float z = col - 3.5f;

        return new Vector3(x, 1f, z);
    }

    void PlacePiece(Square s)
    {
        if (s.player == Player.White)
            Instantiate(piece, s.transform.position, Quaternion.identity);
        else
            Instantiate(piece, s.transform.position, Quaternion.AngleAxis(180f, Vector3.right));
        pieces.Add(piece);
    }

    void InitializeTile(int position, Player player)
    {
        Vector3 squarePos = DeterminePlacementCoordinates(position);
        squarePos.y += .2f;
        Square s = Instantiate(square, squarePos, Quaternion.identity) as Square;
        s.gameObject.GetComponent<MeshRenderer>().enabled = false;
        s.player = player;
        s.position = position;
        s.isLegalMove = false;
        if (player != Player.Nobody)
            PlacePiece(s);
        squares.Add(s);
    }

    public bool UpdateLegalMoves(Player currentPlayer)
    {
        List<int> indicesToFlip = new List<int>();
        foreach (Square s in squares)
        {
            if (s.player == currentPlayer)
            {
                CheckPath(indicesToFlip, currentPlayer, s.position, -1);
                CheckPath(indicesToFlip, currentPlayer, s.position, 1);
                CheckPath(indicesToFlip, currentPlayer, s.position, -8);
                CheckPath(indicesToFlip, currentPlayer, s.position, 8);
                CheckPath(indicesToFlip, currentPlayer, s.position, -9);
                CheckPath(indicesToFlip, currentPlayer, s.position, 9);
                CheckPath(indicesToFlip, currentPlayer, s.position, -7);
                CheckPath(indicesToFlip, currentPlayer, s.position, 7);
            }
        }

        return legalMoves.Count > 0;
    }

    public void UpdateSquares()
    {
        foreach (Square s in squares)
        {
            if (s.isLegalMove)
                s.gameObject.GetComponent<MeshRenderer>().enabled = true;
            else
                s.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private void CheckPath(List<int> indicesToFlip, Player player, int startingPosition, int difference)
    {
        bool flankablesExist = false;
        List<int> flippedTiles = new List<int>();

        for (int i = startingPosition + difference; i >= 0 && i <= 63; i += difference)
        {
            if (squares[i].player != player && squares[i].player != Player.Nobody)
            {
                flankablesExist = true;
                flippedTiles.Add(i);
            }
            else if (squares[i].player == Player.Nobody && flankablesExist)
            {
                flippedTiles.Add(i);
                legalMoves.Add(i);
                squares[i].isLegalMove = true;
                break;
            }
            else break; // we can't go this way
        }
    }
}

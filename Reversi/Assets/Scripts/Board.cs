using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {
    public GameController gameController;
    public Square squarePrefab;
    public Piece piecePrefab;
    public List<Square> squares;
    public List<Piece> pieces;
    public List<int> legalMoves;

    void Start()
    {
        squares = new List<Square>();
        pieces = new List<Piece>();
        legalMoves = new List<int>();
    }

    public void SetInitialBoard()
    {
        this.gameObject.SetActive(true);
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

    public void CaptureTile(Player player, int position)
    {
        squares[position].isLegalMove = false;
        squares[position].player = player;
        PlacePiece(squares[position]);
    }

    public void FlipPiece(Player player, int position)
    {
        CaptureTile(player, position);
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
        Vector3 position = s.transform.position;
        position.y = 1f;
        if (s.player == Player.White)
            Instantiate(piecePrefab, s.transform.position, Quaternion.identity);
        else
            Instantiate(piecePrefab, s.transform.position, Quaternion.AngleAxis(180f, Vector3.right));
        pieces.Add(piecePrefab);
    }

    void InitializeTile(int position, Player player)
    {
        Vector3 squarePos = DeterminePlacementCoordinates(position);
        squarePos.y += .2f;
        Square s = Instantiate(squarePrefab, squarePos, Quaternion.identity) as Square;
        s.gameObject.GetComponent<MeshRenderer>().enabled = false;
        s.player = player;
        s.position = position;
        s.isLegalMove = false;
        if (player != Player.Nobody)
            PlacePiece(s);
        squares.Add(s);
    }
}

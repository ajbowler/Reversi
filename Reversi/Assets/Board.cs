using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {
    public GameController gameController;
    public Square square;
    public Piece piece;
    public List<Square> squares = new List<Square>();
    public List<Piece> pieces = new List<Piece>();

    public void SetInitialBoard()
    {
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
        s.gameObject.GetComponent<MeshRenderer>().enabled = false;
        pieces.Add(piece);
    }

    void InitializeTile(int position, Player player)
    {
        Square s = Instantiate(square, DeterminePlacementCoordinates(position), Quaternion.identity) as Square;
        s.player = player;
        if (player != Player.Nobody)
            PlacePiece(s);
        else
        {
            // TODO
        }
        squares.Add(s);
    }
}

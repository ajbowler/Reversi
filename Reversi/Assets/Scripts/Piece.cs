using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour
{
    public Player player;
    public float flipSpeed;
    private Quaternion flipTo;

    /// <summary>
    /// Rotate the piece towards its proper rotation, depending on the player that owns it.
    /// </summary>
    void Update()
    {
        if (player == Player.Black) flipTo = Quaternion.AngleAxis(180, Vector3.right);
        else if (player == Player.White) flipTo = Quaternion.AngleAxis(0, Vector3.right);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, flipTo, flipSpeed);
    }
}

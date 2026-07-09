using UnityEngine;

public class HighlightTile : MonoBehaviour
{
    // Always reflects this tile's current world position, even if the
    // environment/grid has rotated since the tile was created. Previously
    // this was a plain field set once at creation time, which went stale
    // as soon as the environment rotated, causing clicks to resolve to
    // where the tile used to be instead of where it currently is.
    public Vector3 worldPosition => transform.position;
    public bool isMove;
    public bool isAttack;
}
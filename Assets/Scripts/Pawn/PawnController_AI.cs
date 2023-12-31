using System.Collections.Generic;
using UnityEngine;

public class PawnController_AI : MonoBehaviour
{
    [System.NonSerialized] public Vector3 originalPosition;
    float smooth = 200;
    private Vector3 targetPosition;
    private Vector3 offset;

    public int my_pawn_number = 0; // é©ï™é©êgÇÃãÓÇÃêîéö

    List<string> cellTags;

    [System.NonSerialized] public Collider2D cell_collider;


    private void Start()
    {
        cellTags = CellObserver.instance.AssignCellTags(cellTags);
        originalPosition = transform.position;

    }


    public void BeDestroyedAndSetOriginalPos()
    {
        transform.position = originalPosition;
    }

    public int GetPawnOwnNumber()
    {
        return my_pawn_number;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        cell_collider = collision.GetComponent<BoxCollider2D>();

    }
}
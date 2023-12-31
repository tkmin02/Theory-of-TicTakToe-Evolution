using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static CellObserver;

// OnMouseDown OnMouseDrag OnMouseUp などはゲームオブジェクト対象,  ImageなどのUIには使えない


public class PawnController_Player : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    [System.NonSerialized] public Vector3 originalPosition;
    float smooth = 200;
    private Vector3 targetPosition;
    private Vector3 offset;

    public int my_pawn_number = 0;// 自分自身の駒の数字

    List<string> cellTags;

    static List<Image> playersHands;
    static List<Image> playersDeck;

    Collider2D cell_collider;

    private void Start()
    {
        cellTags = CellObserver.instance.AssignCellTags(cellTags);

        originalPosition = transform.position;
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CellObserver.isHumanMarked)

            offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, transform.position.z));
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CellObserver.isHumanMarked)
        {
            Vector3 newPosition = new Vector3(eventData.position.x, eventData.position.y, transform.position.z);
            targetPosition = Camera.main.ScreenToWorldPoint(newPosition) + offset;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, smooth * Time.deltaTime);

        }
    }


    // ドラッグ中のプレイヤーの駒を離したときの処理
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!CellObserver.isHumanMarked)
        {
            // セルと駒のColliderを取得
            Collider2D[] hitCollider = Physics2D.OverlapCircleAll(gameObject.transform.position, 0.00001f);


            for (int i = 0; i < hitCollider.Length; i++)
            {

                // AIの駒、セルの当たり判定を取得
                bool isCellCollided = hitCollider[i].tag.Equals("Cell");
                bool isAIPawnCollided = hitCollider[i].tag.Equals("AIPawn");



                // プレイヤーの駒とAIの駒とセルが当たった時（つまり既に置かれている駒を上書きしようとするとき　
                if (isAIPawnCollided && isCellCollided)
                {
                    if (CellObserver.cellInfo[i].OwnerAndPawnNum.Item1 != CellObserver.CELL_OWNER.Empty)
                    {

                        // Aiの駒、セルのオブジェクトを取得
                        var ai_pawn = hitCollider[i].gameObject.GetComponent<PawnController_AI>();
                        cell_collider = hitCollider[i].GetComponent<BoxCollider2D>();


                        // プレイヤーの駒の数字が、AIの駒の数字より大きければ
                        if (my_pawn_number > ai_pawn.my_pawn_number)
                        {
                            // AIの駒を無効化
                            ai_pawn.enabled = false;


                            // プレイヤーの駒をセルへ新しく配置する
                            transform.position = cell_collider.bounds.center;


                            for (int j = 0; j < CellObserver.cellInfo.Length; j++)
                            {

                                if (cellInfo[j].OwnerAndPawnNum.Item1 != CELL_OWNER.Empty && cellInfo[j].OwnerAndPawnNum.Item2 != CELL_OWNER_PAWN_NUM.None)
                                {
                                    Assign_CellOwnerAndPawnNumber(j);
                                    break;
                                }
                            }
                        }

                        // プレイヤーの駒の数字が、AIの駒の数字と等しいもしくはそれ以下であれば
                        else if (my_pawn_number <= ai_pawn.my_pawn_number)
                        {
                            // 上書きは無効。駒は初期位置へ戻す
                            transform.position = originalPosition;
                        }
                    }
                }
                // プレイヤーの駒とセルが当たった時（セルには駒が置かれていない）
                else if (isCellCollided)
                {
                    CellObserver.instance.UpdateCellState();

                    cell_collider = hitCollider[i].GetComponent<BoxCollider2D>();
                    
                    // 駒を置いたセルのインデックスを取得
                    int target_index = GetPuttedPawnCellIndex(cell_collider);

                    for (int j = 0; j < CellObserver.cellInfo.Length; j++)
                    {

                        if (cellInfo[j].OwnerAndPawnNum.Item1 == CELL_OWNER.Empty && cellInfo[j].OwnerAndPawnNum.Item2 == CELL_OWNER_PAWN_NUM.None)
                        {
                            Assign_CellOwnerAndPawnNumber(target_index);

                            CellObserver.isHumanMarked = true;

                            GameManager.didHumanMove = true;

                            break;
                        }
                    }
                }
                else
                {
                    transform.position = originalPosition;
                }

            }

        }

    }

    public void BeDestroyedAndSetOriginalPos()
    {
        transform.position = originalPosition;
    }


    public static List<Image> GetPlayersHands()
    {
        return playersHands;
    }


    public static void SetPlayersHands(List<Image> hand)
    {
        List<Image> copy = new List<Image>(hand);
        playersHands = copy;
    }



    public static List<Image> GetPlayersDeck()
    {

        return playersDeck;
    }



    public static void SetPlayersDeck(List<Image> hand)
    {
        List<Image> copy = new List<Image>(hand);
        playersDeck = copy;
    }



    /// <summary>
    /// 駒を置いたセルのインデックス。0～8
    /// </summary>
    /// <param name="collider"></param>
    /// <returns></returns>
    public int GetPuttedPawnCellIndex(Collider2D collider)
    {

        if(collider != null)
        {
            if (collider.gameObject.name.Equals("Cell(0,0)"))
            {
                return 0;
            }
            if (collider.gameObject.name.Equals("Cell(0,1)"))
            {
                return 1;
            }
            if (collider.gameObject.name.Equals("Cell(0,2)"))
            {
                return 2;
            }
            if (collider.gameObject.name.Equals("Cell(1,0)"))
            {
                return 3;
            }
            if (collider.gameObject.name.Equals("Cell(1,1)"))
            {
                return 4;
            }
            if (collider.gameObject.name.Equals("Cell(1,2)"))
            {
                return 5;
            }
            if (collider.gameObject.name.Equals("Cell(2,0)"))
            {
                return 6;
            }
            if (collider.gameObject.name.Equals("Cell(2,1)"))
            {
                return 7;
            }
            if (collider.gameObject.name.Equals("Cell(2,2)"))
            {
                return 8;
            }
        }

        return -1;
    }



    private void Assign_CellOwnerAndPawnNumber(int target_index)
    {
        // ↓ プロパティやインデクサーから値型を取得すると、そのオブジェクトのコピーが返されエラーになる
        //cellInfo[j].OwnerAndPawnNum.Item1 = CELL_OWNER.PlayerPawn;


        transform.position = cell_collider.bounds.center;

        // 配置が成功したかどうかを確認
        bool isPositionedAtCenter = transform.position == cell_collider.bounds.center;

        if (!isPositionedAtCenter) return;


        CellInfo tempInfo = cellInfo[target_index];

        if (GetComponent<Pawn1_Function>() != null)
        {
            tempInfo.OwnerAndPawnNum = new ValueTuple<CELL_OWNER, CELL_OWNER_PAWN_NUM>(CELL_OWNER.PlayerPawn, CELL_OWNER_PAWN_NUM.One);

            cellInfo[target_index].OwnerAndPawnNum = tempInfo.OwnerAndPawnNum;

        }
        else if (GetComponent<Pawn2_Function>() != null)
        {
            tempInfo.OwnerAndPawnNum = new ValueTuple<CELL_OWNER, CELL_OWNER_PAWN_NUM>(CELL_OWNER.PlayerPawn, CELL_OWNER_PAWN_NUM.Two);

            cellInfo[target_index].OwnerAndPawnNum = tempInfo.OwnerAndPawnNum;

        }
        else if (GetComponent<Pawn3_Function>() != null)
        {
            tempInfo.OwnerAndPawnNum = new ValueTuple<CELL_OWNER, CELL_OWNER_PAWN_NUM>(CELL_OWNER.PlayerPawn, CELL_OWNER_PAWN_NUM.Three);

            cellInfo[target_index].OwnerAndPawnNum = tempInfo.OwnerAndPawnNum;

        }
        else if (GetComponent<Pawn4_Function>() != null)
        {
            tempInfo.OwnerAndPawnNum = new ValueTuple<CELL_OWNER, CELL_OWNER_PAWN_NUM>(CELL_OWNER.PlayerPawn, CELL_OWNER_PAWN_NUM.Four);

            cellInfo[target_index].OwnerAndPawnNum = tempInfo.OwnerAndPawnNum;

        }
        else if (GetComponent<Pawn5_Function>() != null)
        {
            tempInfo.OwnerAndPawnNum = new ValueTuple<CELL_OWNER, CELL_OWNER_PAWN_NUM>(CELL_OWNER.PlayerPawn, CELL_OWNER_PAWN_NUM.Five);

            cellInfo[target_index].OwnerAndPawnNum = tempInfo.OwnerAndPawnNum;

        }
        else if (GetComponent<Pawn6_Function>() != null)
        {
            tempInfo.OwnerAndPawnNum = new ValueTuple<CELL_OWNER, CELL_OWNER_PAWN_NUM>(CELL_OWNER.PlayerPawn, CELL_OWNER_PAWN_NUM.Six);

            cellInfo[target_index].OwnerAndPawnNum = tempInfo.OwnerAndPawnNum;

        }
        else if (GetComponent<Pawn7_Function>() != null)
        {
            tempInfo.OwnerAndPawnNum = new ValueTuple<CELL_OWNER, CELL_OWNER_PAWN_NUM>(CELL_OWNER.PlayerPawn, CELL_OWNER_PAWN_NUM.Seven);

            cellInfo[target_index].OwnerAndPawnNum = tempInfo.OwnerAndPawnNum;

        }
        else if (GetComponent<Pawn8_Function>() != null)
        {
            tempInfo.OwnerAndPawnNum = new ValueTuple<CELL_OWNER, CELL_OWNER_PAWN_NUM>(CELL_OWNER.PlayerPawn, CELL_OWNER_PAWN_NUM.Eight);

            cellInfo[target_index].OwnerAndPawnNum = tempInfo.OwnerAndPawnNum;

        }
        else if (GetComponent<Pawn9_Function>() != null)
        {
            tempInfo.OwnerAndPawnNum = new ValueTuple<CELL_OWNER, CELL_OWNER_PAWN_NUM>(CELL_OWNER.PlayerPawn, CELL_OWNER_PAWN_NUM.Nine);

            cellInfo[target_index].OwnerAndPawnNum = tempInfo.OwnerAndPawnNum;

        }
        else if (GetComponent<Pawn10_Function>() != null)
        {
            tempInfo.OwnerAndPawnNum = new ValueTuple<CELL_OWNER, CELL_OWNER_PAWN_NUM>(CELL_OWNER.PlayerPawn, CELL_OWNER_PAWN_NUM.Ten);

            cellInfo[target_index].OwnerAndPawnNum = tempInfo.OwnerAndPawnNum;
        }

        if (isPositionedAtCenter)
        {
        }
    }


    public int GetPawnOwnNumber()
    {
        return my_pawn_number;
    }

}




// これでも動かせる
//public void DragHandler(BaseEventData eventData)
//{
//    PointerEventData pointerEventData = eventData as PointerEventData;

//    Vector2 pos;
//    RectTransformUtility.ScreenPointToLocalPointInRectangle(
//        (RectTransform)canvas.transform,
//        pointerEventData.position,
//        canvas.worldCamera,
//        out pos);

//    transform.position = canvas.transform.TransformPoint(pos);
//}
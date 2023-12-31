using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CellObserver : MonoBehaviour
{

    public enum CELL_OWNER
    {
        Empty,

        PlayerPawn,

        AIPawn
    };


    public enum CELL_OWNER_PAWN_NUM
    {
        None,
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
    }

    public struct CellInfo
    {
        public ValueTuple<CELL_OWNER, CELL_OWNER_PAWN_NUM> OwnerAndPawnNum { get; set; }
    }

    public static CellInfo[] cellInfo;

    GameObject[] cells = null;

    public static CellObserver instance;

    PawnController_Player pawnCtrl_player;
    PawnController_AI pawnCtrl_ai;


    public static int cell_all_count;

    public static bool isHumanMarked { get; set; } // プレイヤーが駒を置いたかどうか
    public static bool isAIMarked { get; set; } // AIが駒を置いたかどうか


    private void Awake()
    {
        if (instance == null) instance = this;

        isHumanMarked = false;

        // 子オブジェクトの数をカウント
        cell_all_count = transform.childCount;

        cells = new GameObject[cell_all_count];

        for (int i = 0; i < cell_all_count; i++)
        {
            cells[i] = transform.GetChild(i).gameObject;
        }


        cellInfo = new CellInfo[cell_all_count];

        for (int i = 0; i < cell_all_count; i++)
        {
            cellInfo[i] = new CellInfo();
        }
    }


    /// <summary>
    /// // GetMatrix_CellInfo()
    /// // 9つのセルの状態を構造体で取得
    /// </summary>
    /// <returns></returns>
    public CellInfo[] GetMatrix_CellInfo()
    {
        return cellInfo;
    }

    public List<string> AssignCellTags(List<string> cellTags)
    {
        cellTags = cellTags ?? new List<string> { "Cell_00", "Cell_01", "Cell_02", "Cell_10", "Cell_11", "Cell_12", "Cell_20", "Cell_21", "Cell_22" };
        return cellTags;
    }



    // 現在のセルの状態を取得
    public static CELL_OWNER[] CloneCell(CELL_OWNER[] cells)
    {
        CELL_OWNER[] clone = new CELL_OWNER[cells.Length];

        for (int i = 0; i < cell_all_count; i++)
        {
            clone[i] = cells[i];
        }

        return clone;
    }


    // PlayerとAIで共有する関数
    public void UpdateCellState()
    {
        Collider2D[] hitCollider = new Collider2D[cell_all_count];


        for (int i = 0; i < cell_all_count; i++)
        {
            // 子オブジェクト（9つのセル）のコライダーを取得
            hitCollider[i] = cells[i].GetComponent<Collider2D>();


            if (hitCollider[i] != null && hitCollider[i].CompareTag("PlayerPawn"))
            {
                pawnCtrl_player = hitCollider[i].GetComponent<PawnController_Player>();

                cellInfo[i].OwnerAndPawnNum = (CELL_OWNER.PlayerPawn, (CELL_OWNER_PAWN_NUM)pawnCtrl_player.my_pawn_number);
            }
            else if (hitCollider[i] != null && hitCollider[i].CompareTag("AIPawn"))
            {
                pawnCtrl_ai = hitCollider[i].GetComponent<PawnController_AI>();

                cellInfo[i].OwnerAndPawnNum = (CELL_OWNER.AIPawn, (CELL_OWNER_PAWN_NUM)pawnCtrl_ai.my_pawn_number);
            }
            else
            {
                cellInfo[i].OwnerAndPawnNum = (CELL_OWNER.Empty, CELL_OWNER_PAWN_NUM.None);
            }

            
        }
    }

}

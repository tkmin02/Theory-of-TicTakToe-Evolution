using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static CellObserver;


public class MiniMaxAI
{
    static List<Image> myHands;
    static List<Image> myDeck;

    [HideInInspector] bool make_ai_thought_duration;


    int empty_cell_count = 0;
    int recursion_score = 0;
    int current_checking_index;
    int best_choice_index;


    public void DoAIMove()
    {

        // 最新のセルの状態を取得
        CellInfo[] cell_ary_1dimension = CellObserver.instance.GetMatrix_CellInfo();


        int score_result = MiniMax(cell_ary_1dimension, 3, true);

        if (score_result == 100)
        {
            Debug.Log("Human Win");
            return;
        }
        else if (score_result == -100)
        {
            Debug.Log("AI Win");
            return;

        }
        else if (score_result == 0)
        {
            Debug.Log("Tie");
            return;
        }
        else if (score_result == 50)
        {

            CELL_OWNER[] cellOwner = new CELL_OWNER[cell_ary_1dimension.Length];

            for (int i = 0; i < cellOwner.Length; i++)
            {
                // 各セルの所有者とその駒の番号をオブジェクトへ代入
                cellOwner[i] = cell_ary_1dimension[i].OwnerAndPawnNum.Item1;
            }


            cellOwner[best_choice_index] = CELL_OWNER.AIPawn;

            var cellInfo = cell_ary_1dimension[best_choice_index];
            cellInfo.OwnerAndPawnNum = new ValueTuple<CELL_OWNER, CELL_OWNER_PAWN_NUM>(cellOwner[best_choice_index], cellInfo.OwnerAndPawnNum.Item2);
            cell_ary_1dimension[best_choice_index] = cellInfo;
        }

    }







    public int MiniMax(CellInfo[] cells, int depth, bool isMaximizingHuman)
    {

        CELL_OWNER[] cellOwner = new CELL_OWNER[cells.Length];
        CELL_OWNER_PAWN_NUM[] cellPawnNum = new CELL_OWNER_PAWN_NUM[cells.Length];

        //自分の手札を取得
        myHands = GetHands();
        myDeck = GetDeck();


        int bestScore = isMaximizingHuman ? int.MinValue : int.MaxValue;
        int current_best_choice_index = -1;
        bool endRecursion = false;

        int stateScore = CheckGameState(cells);
        if (stateScore != 50 || depth == 0)
        {
            endRecursion = true;
            // 勝ち、負け、引き分け、または最大深さに達した場合にスコアを返す
            return stateScore;
        }


        //　セルの空白が8つあれば
        if (empty_cell_count == 8)
        {
            CheckCanShortCut_HasEmptyCell8(cellOwner, cellPawnNum, isMaximizingHuman);
        }

        for (int i = 0; i < cells.Length; i++)
        {

            // 各セルの所有者とその駒の番号をオブジェクトへ代入
            cellOwner[i] = cells[i].OwnerAndPawnNum.Item1;
            cellPawnNum[i] = cells[i].OwnerAndPawnNum.Item2;


            // セルが空いている、もしくはプレイヤーの駒が置いてある場合
            if (cellOwner[i] == CELL_OWNER.Empty)
            {

                cellOwner[i] = isMaximizingHuman ? CELL_OWNER.PlayerPawn : CELL_OWNER.AIPawn;

                int current_score = MiniMax(cells, depth - 1, !isMaximizingHuman);


                if (empty_cell_count > 0)
                {

                    isMaximizingHuman = !isMaximizingHuman;
                    current_score = recursion_score;
                    isMaximizingHuman = !isMaximizingHuman;

                }

                // プレイヤーのターンで評価値が最小、またはAIのターンで評価値が最大
                if ((isMaximizingHuman && current_score > bestScore) || (!isMaximizingHuman && current_score < bestScore))
                {
                    bestScore = current_score;
                    current_best_choice_index = current_checking_index;
                }


                empty_cell_count++;
            }

            current_checking_index++;

            if (endRecursion) break;

        }

        recursion_score = bestScore;
        best_choice_index = current_best_choice_index;

        return bestScore;
    }


    /// <summary>
    /// セルが全部埋まってたら引き分け
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static bool CheckIsCellFullOfPawn(CellObserver.CELL_OWNER[] cell)
    {
        foreach (var c in cell)
        {
            if (c == CellObserver.CELL_OWNER.Empty) return false;
        }
        return true;
    }


    public static bool CheckIsGameWin(CellObserver.CELL_OWNER[] cell, CellObserver.CELL_OWNER cellOwner)
    {
        // 勝利条件　8パターン
        int[,] winPatterns =
        {
        { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 },  // 横
        { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 },  // 縦
        { 0, 4, 8 }, { 2, 4, 6 }                // 斜め
        };


        // need at least 5 moves before x hits three in a row
        //if (moves < 5)     return false




        for (int i = 0; i < winPatterns.GetUpperBound(0); i++)
        {
            if
            (cell[winPatterns[i, 0]] == cellOwner &&
                cell[winPatterns[i, 1]] == cellOwner &&
                cell[winPatterns[i, 2]] == cellOwner)
            {
                return true;
            }
        }
        return false;
    }


    private int CheckGameState(CellInfo[] info)
    {

        if (info != null)
        {
            CELL_OWNER[] cellOwner = new CELL_OWNER[info.Length];

            for (int i = 0; i < info.Length; i++)
            {
                cellOwner[i] = info[i].OwnerAndPawnNum.Item1;
            }

            bool isPlayerWin = CheckIsGameWin(cellOwner, CellObserver.CELL_OWNER.PlayerPawn);
            bool isAIWin = CheckIsGameWin(cellOwner, CellObserver.CELL_OWNER.AIPawn);
            bool isTie = CheckIsCellFullOfPawn(cellOwner);

            // プレイヤーが勝利、もしくは引き分けならゲーム終了
            if (isPlayerWin) return 100;
            if (isAIWin) return -100;
            if (isTie) return 0;

        }

        return 50;
    }



    /// <summary>
    /// 該当のセルが空いている場合は trueを返す
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public bool IsValidMove(CELL_OWNER[] owner, int x)
    {
        if (owner[x] == CELL_OWNER.Empty)
            return true;
        else return false;
    }


    /// <summary>
    ///  ゲーム開始後のAIの最初のターン、またはセルの空白が8つあるとき、毎回必ずこの処理が走る           
    /// </summary>
    /// <param name="cellOwner"></param>
    /// <param name="cellPawnNum"></param>
    /// <param name="empty_cell_count"></param>
    /// <returns></returns>
    private void CheckCanShortCut_HasEmptyCell8(CELL_OWNER[] cellOwner, CELL_OWNER_PAWN_NUM[] cellPawnNum, bool isMaximizing)
    {

        // プレイヤーが中央(1, 1)に駒を置いた場合
        //・4つの角のうちのどこかをランダムで選択
        //・駒の数字がAIの手札の駒より小さければ、その駒を破壊してその場所を奪う
        if (cellOwner[4] == CELL_OWNER.PlayerPawn)
        {

            int[] pawn_num = new int[myHands.Count];

            for (int i = 0; i < myHands.Count; i++)
            {

                if (myHands[i].GetComponent<PawnController_AI>() == null) continue;

                for (int j = 0; j < myHands.Count; j++)
                {
                    pawn_num[j] = myHands[j].GetComponent<PawnController_AI>().my_pawn_number;

                    // 手札内に、置かれた駒の数字より大きい駒が1つでもあれば続行
                    if ((int)cellPawnNum[4] < pawn_num[j])
                    {
                        for (int k = 0; k < SpawnPlayersPawn.hands.Count; k++)
                        {
                            if (SpawnPlayersPawn.hands[k] != null)
                            {

                                var player_pawns = SpawnPlayersPawn.hands[k].GetComponent<PawnController_Player>();

                                if (player_pawns == null || player_pawns.my_pawn_number != (int)cellPawnNum[4]) continue;

                                Image target_pawn = SpawnPlayersPawn.hands[k].GetComponent<Image>();

                                // プレイヤーの駒を手札から削除
                                SpawnPlayersPawn.hands.Remove(target_pawn);


                                Image randomPawn = SpawnPlayersPawn.deck[UnityEngine.Random.Range(0, SpawnPlayersPawn.deck.Count)];

                                // 手札へ新たな駒を追加
                                SpawnPlayersPawn.hands.Add(randomPawn);

                                // 先程手札へ追加した駒を山札から削除
                                SpawnPlayersPawn.deck.Remove(randomPawn);

                                // 削除したプレイヤーの駒を山札の末尾へ追加
                                SpawnPlayersPawn.deck.Add(target_pawn);

                                // ターンを変える
                                isMaximizing = !isMaximizing;

                                GameManager.didAIMove = true;
                                GameManager.didHumanMove = false;
                            }
                        }

                    }
                    else
                    {
                        // 4つの角のうち1つをランダムに選択し、ターンチェンジ
                        SetRandomCornerIndex();
                        isMaximizing = !isMaximizing;

                        GameManager.didAIMove = true;
                        GameManager.didHumanMove = false;
                    }
                }
            }
        }
        // プレイヤーが中央(1, 1)以外に駒を置いた場合
        else
        {
            int[] pawn_num = new int[myHands.Count];

            for (int i = 0; i < myHands.Count; i++)
            {

                if (myHands[i].GetComponent<PawnController_AI>() == null) continue;

                for (int j = 0; j < myHands.Count; j++)
                {
                    pawn_num[j] = myHands[j].GetComponent<PawnController_AI>().my_pawn_number;

                    // プレイヤーが駒を置いたセルのインデックスを取得
                    int playerPawnPosition = Array.IndexOf(cellOwner, CELL_OWNER.PlayerPawn);


                    // 手札内に、置かれた駒の数字より大きい駒が1つでもあれば上書き
                    if ((int)cellPawnNum[playerPawnPosition] < pawn_num[j])
                    {
                        for (int k = 0; k < SpawnPlayersPawn.hands.Count; k++)
                        {
                            if (SpawnPlayersPawn.hands[k] != null)
                            {
                                // プレイヤーの駒の更新----------------------------------------------------------------

                                var player_pawns = SpawnPlayersPawn.hands[k].GetComponent<PawnController_Player>();

                                if (player_pawns == null || player_pawns.my_pawn_number != (int)cellPawnNum[playerPawnPosition]) continue;

                                Image target_pawn = SpawnPlayersPawn.hands[k].GetComponent<Image>();

                                Transform newTransform = target_pawn.transform;



                                // プレイヤーの駒を手札から削除
                                SpawnPlayersPawn.hands.Remove(target_pawn);

                                Image randomPawn = SpawnPlayersPawn.deck[UnityEngine.Random.Range(0, SpawnPlayersPawn.deck.Count)];

                                // 手札へ新たな駒を追加
                                SpawnPlayersPawn.hands.Add(randomPawn);

                                // 先程手札へ追加した駒を山札から削除
                                SpawnPlayersPawn.deck.Remove(randomPawn);

                                // 削除したプレイヤーの駒を山札の末尾へ追加
                                SpawnPlayersPawn.deck.Add(target_pawn);


                                player_pawns.transform.position = player_pawns.originalPosition;

                                //  AIの駒の更新----------------------------------------------------------------


                                PawnController_AI pawnControllerAI = SpawnAisPawn.hands[k].GetComponent<PawnController_AI>();
                                // セルのColliderを取得
                                var hitCollider = Physics2D.OverlapCircle(pawnControllerAI.transform.position, 0.00001f);


                                // セルの当たり判定を取得
                                bool isCellCollided = hitCollider.tag.Equals("Cell");

                                if (isCellCollided)
                                {

                                    CellObserver.instance.UpdateCellState();

                                    pawnControllerAI.transform.position = newTransform.position;

                                    isMaximizing = !isMaximizing;

                                    GameManager.didAIMove = true;
                                    GameManager.didHumanMove = false;
                                }
                            }
                        }
                    }
                    //  手札内に、置かれた駒の数字より大きい駒が1つもなければ中央に駒を配置する
                    else
                    {
                        // 手札の内どの駒を中央に配置するかを決定する処理
                        int[] num_ary = new int[SpawnAisPawn.hands.Count];

                        for (int k = 0; k < SpawnAisPawn.hands.Count; k++)
                        {
                            if (SpawnAisPawn.hands[k] != null)
                            {
                                PawnController_AI pawnController_AI = SpawnAisPawn.hands[k].GetComponent<PawnController_AI>();
                                num_ary[k] = pawnController_AI.my_pawn_number;
                            }
                        }

                        int max_pawn_num = num_ary.Max();


                        for (int l = 0; l < SpawnAisPawn.hands.Count; l++)
                        {
                            if (!CellObserver.isAIMarked)
                            {
                                // 手札の中で数字が最大の駒を選択
                                if (SpawnAisPawn.hands[l] != null && num_ary[l] == max_pawn_num)
                                {

                                    PawnController_AI ai_hands = SpawnAisPawn.hands[l].GetComponent<PawnController_AI>();

                                    // 先に駒を配置する
                                    ai_hands.gameObject.transform.position = ai_hands.originalPosition;


                                    // セルのColliderを取得
                                    var cell_collider = Physics2D.OverlapCircle(ai_hands.transform.position, 0.00001f);


                                    // AIの駒、セルの当たり判定を取得
                                    bool isCellCollided = cell_collider.tag.Equals("Cell");

                                    if (isCellCollided)
                                    {

                                        CellObserver.instance.UpdateCellState();


                                        // 駒を置いたセルのインデックスを取得
                                        int target_index = GetPuttedPawnCellIndex(cell_collider);

                                        for (int m = 0; m < CellObserver.cellInfo.Length; m++)
                                        {

                                            if (cellInfo[m].OwnerAndPawnNum.Item1 == CELL_OWNER.Empty && cellInfo[m].OwnerAndPawnNum.Item2 == CELL_OWNER_PAWN_NUM.None)
                                            {
                                                Assign_CellOwnerAndPawnNumber(target_index, cell_collider, ai_hands);

                                                CellObserver.isHumanMarked = true;

                                                GameManager.didAIMove = true;
                                                GameManager.didHumanMove = false;

                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            best_choice_index = 4;
                        }
                    }
                }
            }
        }
    }



    public int GetPuttedPawnCellIndex(Collider2D collider)
    {

        if (collider != null)
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


    private void Assign_CellOwnerAndPawnNumber(int target_index, Collider2D cell_collider, PawnController_AI pawnControllerAI)
    {

        CellInfo tempInfo = cellInfo[target_index];

        Dictionary<Type, CELL_OWNER_PAWN_NUM> pawnTypeToNum = new Dictionary<Type, CELL_OWNER_PAWN_NUM>()
        {
            {typeof(Pawn1_Function), CELL_OWNER_PAWN_NUM.One },
            {typeof(Pawn2_Function), CELL_OWNER_PAWN_NUM.Two },
            {typeof(Pawn3_Function), CELL_OWNER_PAWN_NUM.Three },
            {typeof(Pawn4_Function), CELL_OWNER_PAWN_NUM.Four },
            {typeof(Pawn5_Function), CELL_OWNER_PAWN_NUM.Five},
            {typeof(Pawn6_Function), CELL_OWNER_PAWN_NUM.Six },
            {typeof(Pawn7_Function), CELL_OWNER_PAWN_NUM.Seven},
            {typeof(Pawn8_Function), CELL_OWNER_PAWN_NUM.Eight },
            {typeof(Pawn9_Function), CELL_OWNER_PAWN_NUM.Nine },
            {typeof(Pawn10_Function), CELL_OWNER_PAWN_NUM.Ten},

        };


        foreach (var entry in pawnTypeToNum)
        {
            if (pawnControllerAI.GetComponent(entry.Key) != null)
            {
                tempInfo.OwnerAndPawnNum = new ValueTuple<CELL_OWNER, CELL_OWNER_PAWN_NUM>(CELL_OWNER.PlayerPawn, entry.Value);
                cellInfo[target_index].OwnerAndPawnNum = tempInfo.OwnerAndPawnNum;
                break;
            }
        }

        pawnControllerAI.transform.position = cell_collider.bounds.center;
    }


    private void SetRandomCornerIndex()
    {
        best_choice_index = (int)UnityEngine.Mathf.Floor(UnityEngine.Random.Range(0, 4));

        if (best_choice_index == 1) best_choice_index = 6;
        else if (best_choice_index == 3) best_choice_index = 8;
    }



    static CellInfo[] MakeGridMove(CellInfo player_move, int position)
    {
        CellInfo[] newMatrix = CellObserver.instance.GetMatrix_CellInfo();

        newMatrix[position] = player_move;
        return newMatrix;
    }


    //static CellInfo SwitchPawn(CellInfo pawn)
    //{
    //    var owner = pawn.OwnerAndPawnNum.Item1;
    //    var number = pawn.OwnerAndPawnNum.Item2;



    //    if (pawn == pawn.X)
    //    {

    //        return pawn.O;
    //    }
    //    else return pawn.X;
    //}





    List<Image> GetHands()
    {

        return myHands;
    }


    public static void SetHands(List<Image> hand)
    {
        List<Image> copy = new List<Image>(hand);
        myHands = copy;
    }


    List<Image> GetDeck()
    {
        return myDeck;
    }

    public static void SetDeck(List<Image> deck)
    {
        List<Image> copy = new List<Image>(deck);
        myDeck = copy;
    }

}
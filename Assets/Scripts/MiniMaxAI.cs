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

        // �ŐV�̃Z���̏�Ԃ��擾
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
                // �e�Z���̏��L�҂Ƃ��̋�̔ԍ����I�u�W�F�N�g�֑��
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

        //�����̎�D���擾
        myHands = GetHands();
        myDeck = GetDeck();


        int bestScore = isMaximizingHuman ? int.MinValue : int.MaxValue;
        int current_best_choice_index = -1;
        bool endRecursion = false;

        int stateScore = CheckGameState(cells);
        if (stateScore != 50 || depth == 0)
        {
            endRecursion = true;
            // �����A�����A���������A�܂��͍ő�[���ɒB�����ꍇ�ɃX�R�A��Ԃ�
            return stateScore;
        }


        //�@�Z���̋󔒂�8�����
        if (empty_cell_count == 8)
        {
            CheckCanShortCut_HasEmptyCell8(cellOwner, cellPawnNum, isMaximizingHuman);
        }

        for (int i = 0; i < cells.Length; i++)
        {

            // �e�Z���̏��L�҂Ƃ��̋�̔ԍ����I�u�W�F�N�g�֑��
            cellOwner[i] = cells[i].OwnerAndPawnNum.Item1;
            cellPawnNum[i] = cells[i].OwnerAndPawnNum.Item2;


            // �Z�����󂢂Ă���A�������̓v���C���[�̋�u���Ă���ꍇ
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

                // �v���C���[�̃^�[���ŕ]���l���ŏ��A�܂���AI�̃^�[���ŕ]���l���ő�
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
    /// �Z�����S�����܂��Ă����������
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
        // ���������@8�p�^�[��
        int[,] winPatterns =
        {
        { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 },  // ��
        { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 },  // �c
        { 0, 4, 8 }, { 2, 4, 6 }                // �΂�
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

            // �v���C���[�������A�������͈��������Ȃ�Q�[���I��
            if (isPlayerWin) return 100;
            if (isAIWin) return -100;
            if (isTie) return 0;

        }

        return 50;
    }



    /// <summary>
    /// �Y���̃Z�����󂢂Ă���ꍇ�� true��Ԃ�
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
    ///  �Q�[���J�n���AI�̍ŏ��̃^�[���A�܂��̓Z���̋󔒂�8����Ƃ��A����K�����̏���������           
    /// </summary>
    /// <param name="cellOwner"></param>
    /// <param name="cellPawnNum"></param>
    /// <param name="empty_cell_count"></param>
    /// <returns></returns>
    private void CheckCanShortCut_HasEmptyCell8(CELL_OWNER[] cellOwner, CELL_OWNER_PAWN_NUM[] cellPawnNum, bool isMaximizing)
    {

        // �v���C���[������(1, 1)�ɋ��u�����ꍇ
        //�E4�̊p�̂����̂ǂ����������_���őI��
        //�E��̐�����AI�̎�D�̋��菬������΁A���̋��j�󂵂Ă��̏ꏊ��D��
        if (cellOwner[4] == CELL_OWNER.PlayerPawn)
        {

            int[] pawn_num = new int[myHands.Count];

            for (int i = 0; i < myHands.Count; i++)
            {

                if (myHands[i].GetComponent<PawnController_AI>() == null) continue;

                for (int j = 0; j < myHands.Count; j++)
                {
                    pawn_num[j] = myHands[j].GetComponent<PawnController_AI>().my_pawn_number;

                    // ��D���ɁA�u���ꂽ��̐������傫���1�ł�����Α��s
                    if ((int)cellPawnNum[4] < pawn_num[j])
                    {
                        for (int k = 0; k < SpawnPlayersPawn.hands.Count; k++)
                        {
                            if (SpawnPlayersPawn.hands[k] != null)
                            {

                                var player_pawns = SpawnPlayersPawn.hands[k].GetComponent<PawnController_Player>();

                                if (player_pawns == null || player_pawns.my_pawn_number != (int)cellPawnNum[4]) continue;

                                Image target_pawn = SpawnPlayersPawn.hands[k].GetComponent<Image>();

                                // �v���C���[�̋����D����폜
                                SpawnPlayersPawn.hands.Remove(target_pawn);


                                Image randomPawn = SpawnPlayersPawn.deck[UnityEngine.Random.Range(0, SpawnPlayersPawn.deck.Count)];

                                // ��D�֐V���ȋ��ǉ�
                                SpawnPlayersPawn.hands.Add(randomPawn);

                                // �����D�֒ǉ���������R�D����폜
                                SpawnPlayersPawn.deck.Remove(randomPawn);

                                // �폜�����v���C���[�̋���R�D�̖����֒ǉ�
                                SpawnPlayersPawn.deck.Add(target_pawn);

                                // �^�[����ς���
                                isMaximizing = !isMaximizing;

                                GameManager.didAIMove = true;
                                GameManager.didHumanMove = false;
                            }
                        }

                    }
                    else
                    {
                        // 4�̊p�̂���1�������_���ɑI�����A�^�[���`�F���W
                        SetRandomCornerIndex();
                        isMaximizing = !isMaximizing;

                        GameManager.didAIMove = true;
                        GameManager.didHumanMove = false;
                    }
                }
            }
        }
        // �v���C���[������(1, 1)�ȊO�ɋ��u�����ꍇ
        else
        {
            int[] pawn_num = new int[myHands.Count];

            for (int i = 0; i < myHands.Count; i++)
            {

                if (myHands[i].GetComponent<PawnController_AI>() == null) continue;

                for (int j = 0; j < myHands.Count; j++)
                {
                    pawn_num[j] = myHands[j].GetComponent<PawnController_AI>().my_pawn_number;

                    // �v���C���[�����u�����Z���̃C���f�b�N�X���擾
                    int playerPawnPosition = Array.IndexOf(cellOwner, CELL_OWNER.PlayerPawn);


                    // ��D���ɁA�u���ꂽ��̐������傫���1�ł�����Ώ㏑��
                    if ((int)cellPawnNum[playerPawnPosition] < pawn_num[j])
                    {
                        for (int k = 0; k < SpawnPlayersPawn.hands.Count; k++)
                        {
                            if (SpawnPlayersPawn.hands[k] != null)
                            {
                                // �v���C���[�̋�̍X�V----------------------------------------------------------------

                                var player_pawns = SpawnPlayersPawn.hands[k].GetComponent<PawnController_Player>();

                                if (player_pawns == null || player_pawns.my_pawn_number != (int)cellPawnNum[playerPawnPosition]) continue;

                                Image target_pawn = SpawnPlayersPawn.hands[k].GetComponent<Image>();

                                Transform newTransform = target_pawn.transform;



                                // �v���C���[�̋����D����폜
                                SpawnPlayersPawn.hands.Remove(target_pawn);

                                Image randomPawn = SpawnPlayersPawn.deck[UnityEngine.Random.Range(0, SpawnPlayersPawn.deck.Count)];

                                // ��D�֐V���ȋ��ǉ�
                                SpawnPlayersPawn.hands.Add(randomPawn);

                                // �����D�֒ǉ���������R�D����폜
                                SpawnPlayersPawn.deck.Remove(randomPawn);

                                // �폜�����v���C���[�̋���R�D�̖����֒ǉ�
                                SpawnPlayersPawn.deck.Add(target_pawn);


                                player_pawns.transform.position = player_pawns.originalPosition;

                                //  AI�̋�̍X�V----------------------------------------------------------------


                                PawnController_AI pawnControllerAI = SpawnAisPawn.hands[k].GetComponent<PawnController_AI>();
                                // �Z����Collider���擾
                                var hitCollider = Physics2D.OverlapCircle(pawnControllerAI.transform.position, 0.00001f);


                                // �Z���̓����蔻����擾
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
                    //  ��D���ɁA�u���ꂽ��̐������傫���1���Ȃ���Β����ɋ��z�u����
                    else
                    {
                        // ��D�̓��ǂ̋�𒆉��ɔz�u���邩�����肷�鏈��
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
                                // ��D�̒��Ő������ő�̋��I��
                                if (SpawnAisPawn.hands[l] != null && num_ary[l] == max_pawn_num)
                                {

                                    PawnController_AI ai_hands = SpawnAisPawn.hands[l].GetComponent<PawnController_AI>();

                                    // ��ɋ��z�u����
                                    ai_hands.gameObject.transform.position = ai_hands.originalPosition;


                                    // �Z����Collider���擾
                                    var cell_collider = Physics2D.OverlapCircle(ai_hands.transform.position, 0.00001f);


                                    // AI�̋�A�Z���̓����蔻����擾
                                    bool isCellCollided = cell_collider.tag.Equals("Cell");

                                    if (isCellCollided)
                                    {

                                        CellObserver.instance.UpdateCellState();


                                        // ���u�����Z���̃C���f�b�N�X���擾
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
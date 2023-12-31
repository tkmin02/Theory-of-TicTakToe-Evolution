using System;
using System.Collections.Generic;
using UnityEngine;
using static CellObserver;


public class GameManager : MonoBehaviour
{
    // �Q�[���̏��s�̔���A�X�R�A�̌v�Z�A�Q�[���I�[�o�[��Q�[���N���A�̏�Ԃ̊Ǘ�

    //// �Q�[���I�u�W�F�N�g�̎q�Q�[���I�u�W�F�N�g���擾�ł���@�iIEnumerable�������Ă���� foreach���g����j
    //foreach (Transform t in transform)

    // �N���X MiniMax��GameManager���Ŏg�p����


    // �Z��-------------------------------
    [SerializeField] GameObject cell_00;
    [SerializeField] GameObject cell_01;
    [SerializeField] GameObject cell_02;
    [SerializeField] GameObject cell_10;
    [SerializeField] GameObject cell_11;
    [SerializeField] GameObject cell_12;
    [SerializeField] GameObject cell_20;
    [SerializeField] GameObject cell_21;
    [SerializeField] GameObject cell_22;

    static int turn_counter;

    [System.NonSerialized] public static bool didHumanMove = false;
    [System.NonSerialized] public static bool didAIMove = false;


    bool isGameOver = false;



    void Start()
    {
        cell_00.SetActive(true);
        cell_01.SetActive(true);
        cell_02.SetActive(true);
        cell_10.SetActive(true);
        cell_11.SetActive(true);
        cell_12.SetActive(true);
        cell_20.SetActive(true);
        cell_21.SetActive(true);
        cell_22.SetActive(true);

        List<string> pawnNames_player = new List<string>
        { "Pawn1_Player", "Pawn2_Player", "Pawn3_Player", "Pawn4_Player", "Pawn5_Player", "Pawn6_Player", "Pawn7_Player", "Pawn8_Player", "Pawn9_Player", "Pawn10_Player" };
    }






    void Update()
    {
        if (!isGameOver) StartCoroutine(Game());
    }



    /// <summary>
    //�@Game�֐�
    //�@1. �v���C���[���s��
    //�@2. ���s����
    //�@3. AI���s��
    //�@4. ���s����
    //�@5. �v���C���[�̃^�[��

    /// </summary>
    IEnumerator<WaitForSeconds> Game()
    {


        if (didHumanMove)
        {
            //CheckGameState();
        }
        // �v���C���[���s�����A�Q�[�����I�����ĂȂ����
        if (!isGameOver && didHumanMove)
        {
            yield return new WaitForSeconds(2);

            // AI�̃^�[���ƂȂ�
            didAIMove = false;
            CellObserver.isAIMarked = false;

            MiniMaxAI miniMaxAI = new MiniMaxAI();
            miniMaxAI.DoAIMove();

        }

        if (didAIMove)
        {
            //CheckGameState();
        }
        // AI���s�����A�Q�[�����I�����ĂȂ����
        if (!isGameOver && didAIMove)
        {
            yield return new WaitForSeconds(2);

            turn_counter++;
            // �l�Ԃ̃^�[���ƂȂ�
            didHumanMove = false;
            CellObserver.isHumanMarked = false;
        }

    }





    public void GameReset()
    {

        CellInfo[] matrix = CellObserver.cellInfo;

        for (int i = 0; i < 3; ++i)
        {
            for (int j = 0; j < 3; ++j)
            {
                //matrix[i, j].ResetAllCell();
            }
        }
        isGameOver = false;
        didHumanMove = false;
    }

}
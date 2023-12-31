using System;
using System.Collections.Generic;
using System.Linq;
using static TicTacToe_AIstudy.Program;

//�@foreach�͂Q�����z��ɂ��Ή����Ă���̂ŏ��s����ɂ��g����

// out �͏o�͐�p�Ȃ̂œǂݍ��݂��ł��Ȃ��B�������͕K�v�Ȃ�
// ref �͏������݂��ł���

// in  �͓ǂݎ���p�Q�Ɠn���Ȃ̂ŁA�������݂��ł��Ȃ��i���S�j
namespace TicTacToe_AIstudy
{
    class Program
    {
        static void Main(string[] args)
        {
            Board board = new Board();
            // �Ֆʂ̏�����
            board.instance = board; // �C���X�^���X����

            board.instance.Q_TABLE = new double[board.state_size, board.action_size];�@// Q table�������� 
            // AI�̃C���X�^���X�쐬
            RandomAI ai1 = new RandomAI();
            RandomAI ai2 = new RandomAI();

            // ���[�v�񐔂�ݒ�
            board.instance.loopTimes_AIStudy = 100;

            // ���ȑ΋ǂ��J�n
            board.instance.Loop_AIStudy(ai1, ai2);

            // Q table���w�K
            board.instance.LearnQ();
        }
    }

    public class Board
    {
        public Board instance;

        public const int Size = 3;
        // �Ֆ�
        public int[,] cells;

        // �Ֆʂ̏�ԁi ���ɋ�u����Ă��邩�A�u����Ă��Ȃ��� �j
        private const int EMPTY = 0;
        private const int CIRCLE = 1;  // ����
        private const int CROSS = -1;  // ����
        private int[,] _cellsState;

        // ���݂̎��
        private int turn; // AI��2�l�A-1��1�ŕ���

        // ���s�̂��ꂼ��̕�V
        private const int LOSE = -1, DRAW = 0, WIN = 1, CONTINUE = 100;

        // ���ȑ΋ǂ̉�
        public int loopTimes_AIStudy;

        // ���s�̃J�E���g
        int winCount = 0, loseCount = 0, drawCount = 0;

        int result;

        public List<Board> GetBoardFinalResult { get; private set; }

        // ��Ԑ��ƍs�������`
        public int state_size = (int)Math.Pow(3, 9); // 3��ނ̏��
        public int action_size = 9; // �Ֆʂ�9�}�X

        public double[,] Q_TABLE;  // 3^9 = 19683�̔Ֆʂ̃p�^�[�������݂���

        // �w�K���Ɗ�������ݒ�
        double alpha = 0.1;  // �w�K��
        double gamma = 0.9; // ������

        int[,] bingoTable = new int[8, 3] // �c�A���A�΂߂������S�p�^�[��
        {

    {0, 1, 2},�@//��
    {3, 4, 5},  //��
    {6, 7, 8},  //��
    {0, 3, 6},  //�c
    {1, 4, 7},  //�c
    {2, 5, 8},  //�c
    {0, 4, 8},  //�΂�
    {2, 4, 6}   //�΂�

        };

        int GetStateNumber(int[,] cells_state)
        {
            int state = 0;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    state += cells_state[i, j] * (int)Math.Pow(3, i * Size + j);
                }
            }
            return state;
        }

        void UpdateQ(int state, int action, int reward, int next_state)
        {
            //Q_TABLE[state, action] =
            //    Q_TABLE[state, action] +
            //    alpha *
            //    (reward + gamma * Math.Max(Q_TABLE[next_state,action]) - Q_TABLE[state, action]);
        }

        public void LearnQ()
        {
            // �w�K�񐔂������[�v����
            for (int i = 0; i < instance.loopTimes_AIStudy; i++)
            {
                // �Ֆʂ�����������
                int[,] cells_state = new int[Size, Size];
                List<(int, int)> empties = new List<(int, int)>();
                for (int x = 0; x < Size; x++)
                {
                    for (int y = 0; y < Size; y++)
                    {
                        cells_state[x, y] = EMPTY;
                        empties.Add((x, y));
                    }
                    turn = 1;
                }


                // �Q�[�����I���܂Ń��[�v����
                while (true)
                {
                    // ���݂̏�Ԕԍ����擾����
                    int state = GetStateNumber(cells_state);


                    // �^�[���ɉ����čs�������߂�
                    (int, int) action = new();

                    // �s���ɉ����ĔՖʂ��X�V����
                    int x;
                    int y;

                    if (turn == 1) // �����̃^�[��
                    {
                        action = SelectNextMove_AI(); // AI�̃��W�b�N�ɏ]���čs����I������
                        x = action.Item1;
                        y = action.Item2;
                    }
                    else // ����̃^�[��
                    {
                        do
                        {
                            // �����_���ɋ󂢂Ă���}�X�ڂ�I������
                            action = (UnityEngine.Random.Range(0, Size), UnityEngine.Random.Range(0, Size));
                        }
                        // �󂢂Ă���}�X�ڂ�I������܂ŌJ��Ԃ�
                        while (cells_state[action.Item1, action.Item2] != EMPTY);
                        x = action.Item1;
                        y = action.Item2;
                    }

                    if (turn == 1)
                    {
                        cells_state[x, y] = CIRCLE; // �����́��Ń}�X�ڂ𖄂߂�
                    }
                    else
                    {
                        cells_state[x, y] = CROSS; // ����́~�Ń}�X�ڂ𖄂߂�
                    }

                    // �󂢂Ă���}�X�ڂ���I�������}�X�ڂ��폜����
                    empties.RemoveAt(cells_state[x, y]);

                    // ���̏�Ԕԍ����擾����
                    int next_state = GetStateNumber(cells_state);

                    // ���s������s��
                    int result = JudgeResult(x, y, turn);

                    // ��V��Q table�̍X�V���s��
                    int reward;
                    if (result == WIN) // �����̏���
                    {
                        reward = winCount; // ��V��+1
                        UpdateQ(state, action.Item1 * Size + action.Item2, reward, next_state); // Q table���X�V����
                        break; // �Q�[���I��
                    }
                    else if (result == LOSE) // ����̏���
                    {
                        reward = loseCount; // ��V��-1
                        UpdateQ(state, action.Item1 * Size + action.Item2, reward, next_state); // Q table���X�V����
                        break; // �Q�[���I��
                    }
                    else if (result == DRAW) // ��������
                    {
                        reward = drawCount; // ��V��0
                        UpdateQ(state, action.Item1 * Size + action.Item2, reward, next_state); // Q table���X�V����
                        break; // �Q�[���I��
                    }
                    else // �Q�[�����s
                    {
                        reward = 0; // ��V��0
                        UpdateQ(state, action.Item1 * Size + action.Item2, reward, next_state); // Q table���X�V����
                        turn = 1 - turn; // �^�[������シ��
                    }
                }
            }
        }

        public (int, int) SelectNextMove_AI()   // �����_���Ȏ��I��
        {
            var state = 0;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    state += cells[i, j] * (int)Math.Pow(3, i * Size + j);
                }
            }


            // Q table ����ő�� Q �l�����s����T��
            var maxQ = double.NegativeInfinity;
            var bestAction = (-1, -1);
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (_cellsState[i, j] == EMPTY) // ��̃}�X�ڂɑ΂���
                    {
                        var action = i * Size + j; // �s���ԍ����v�Z
                        var row = state / Size; // �s�ԍ����v�Z
                        var col = state % Size; // ��ԍ����v�Z
                        var Q_table = _cellsState[row,col]; // Q �l���擾
                        if (Q_table > maxQ) // Q �l���ő�l���X�V������
                        {
                            maxQ = Q_table; // �ő�l���X�V
                            bestAction = (i, j); // �œK�ȍs�����L�^
                        }
                    }
                }
            }
            return bestAction;
        }

        public Board Clone() // �Ֆʂ̏�Ԃ��R�s�[���Ԃ�
        {
            Board copy = new Board();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    copy.cells[i, j] = this.cells[i, j];
                }
            }
            copy.turn = this.turn; // �ǂ���̃^�[�������R�s�[
            return copy;
        }

        public int GetTurn() { return turn; }

        public List<(int, int)> GetEmptyCells()
        {
            List<(int, int)> emptyCells = new List<(int, int)>();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (cells[i, j] == EMPTY) emptyCells.Add((i, j));
                }
            }
            return emptyCells;
        }

        public void PutPawn_AIStudy(int x, int y)
        {

            if (cells[x, y] != EMPTY)
            { throw new Exception("Already exists Pawn on that position"); }

            // �ՖʂɎ��łA�Y���̃}�X�ڂ́u�P�� -�P�v
            cells[x, y] = turn;
            // �^�[���̐؂�ւ�
            turn = -turn;
        }

        public void Loop_AIStudy(RandomAI ai1, RandomAI ai2)
        {
            for (int game = 0; game < loopTimes_AIStudy; game++)
            {
                // �Ֆʂ̏�����
                Board board = new Board();


                // ���݂̎�Ԃ�AI������
                RandomAI ai = board.GetTurn() == 1 ? ai1 : ai2;

                // AI�����I��
                var move = ai.SelectNextMove_Randomly(board);

                // �ՖʂɎ��ł�
                board.PutPawn_AIStudy(move.Item1, move.Item2);

                // �^�[�����
                turn = -turn;

                // �Ĕ���
                result = JudgeResult(move.Item1, move.Item2, turn);

                // �����ɔՖʂ�ǉ�
                AddJudgeHistory(board);

                // ���ȑ΋ǂ��ĊJ
                Loop_AIStudy(ai1, ai2);
            }
        }

        public int JudgeResult(int row, int col, int turn)  // ���s����
        {
            // �Ō�ɑł�����̈ʒu�ɑΉ����钼����T��
            for (int i = 0; i < Size; i++)
            {
                // �����̐擪�̈ʒu
                int head = bingoTable[i, 0];

                // �����ɍŌ�ɑł�����̈ʒu���܂܂�Ă��邩
                bool contains = false;
                for (int j = 0; j < 3; j++)
                {
                    if (bingoTable[i, j] == row * Size + col)
                    {
                        contains = true;
                        break;
                    }
                }

                // �܂܂�Ă��Ȃ���Ύ��̒����𒲂ׂ�
                if (!contains) continue;

                // �܂܂�Ă���΁A���̒����������Ă��邩���ׂ�
                bool aligned = true;
                for (int j = 0; j < 3; j++)
                {
                    if (_cellsState[head / Size, head % Size] != _cellsState[bingoTable[i, j] / Size, bingoTable[i, j] % Size])
                    {
                        aligned = false;
                        break;
                    }
                }

                // �����Ă���΁A���̃^�[���̃v���C���[�������Ɣ��肷��
                if (aligned)
                {
                    if (turn == 1) return WIN;
                    else return LOSE;
                }
            }

            // �����������Q�[�����s�����肷��
            if (_cellsState.Cast<int>().Count(x => x == EMPTY) == 0) return DRAW;
            else return CONTINUE;
        }

        void AddJudgeHistory(Board board)
        {
            List<Board> history = new List<Board>();

            // �Ō�̔Ֆʂ𗚗��ɒǉ�
            history.Add(board.Clone());

            int allJudgeCount = winCount + loseCount + drawCount;

            if (allJudgeCount == loopTimes_AIStudy)
            {
                GetBoardFinalResult = history;  // �ŏI���ʂ��v���p�e�B�ɑ��
                return;
            }
        }
    }
    // �����_��AI�N���X
    public class RandomAI
    {
        public (int, int) SelectNextMove_Randomly(Board board)   // �����_���Ȏ��I��
        {
            var emptyCells = board.GetEmptyCells();
            var random = UnityEngine.Random.Range(0, emptyCells.Count); // ��̃}�X�ڂ̑���
            var index = random;
            return emptyCells[index];
        }
    }
}
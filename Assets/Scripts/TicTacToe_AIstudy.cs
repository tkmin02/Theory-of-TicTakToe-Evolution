using System;
using System.Collections.Generic;
using System.Linq;
using static TicTacToe_AIstudy.Program;

//　foreachは２次元配列にも対応しているので勝敗判定にも使える

// out は出力専用なので読み込みができない。初期化は必要ない
// ref は書き込みができる

// in  は読み取り専用参照渡しなので、書き込みができない（安全）
namespace TicTacToe_AIstudy
{
    class Program
    {
        static void Main(string[] args)
        {
            Board board = new Board();
            // 盤面の初期化
            board.instance = board; // インスタンスを代入

            board.instance.Q_TABLE = new double[board.state_size, board.action_size];　// Q tableを初期化 
            // AIのインスタンス作成
            RandomAI ai1 = new RandomAI();
            RandomAI ai2 = new RandomAI();

            // ループ回数を設定
            board.instance.loopTimes_AIStudy = 100;

            // 自己対局を開始
            board.instance.Loop_AIStudy(ai1, ai2);

            // Q tableを学習
            board.instance.LearnQ();
        }
    }

    public class Board
    {
        public Board instance;

        public const int Size = 3;
        // 盤面
        public int[,] cells;

        // 盤面の状態（ 既に駒が置かれているか、置かれていないか ）
        private const int EMPTY = 0;
        private const int CIRCLE = 1;  // 自分
        private const int CROSS = -1;  // 相手
        private int[,] _cellsState;

        // 現在の手番
        private int turn; // AIは2人、-1と1で分類

        // 勝敗のそれぞれの報酬
        private const int LOSE = -1, DRAW = 0, WIN = 1, CONTINUE = 100;

        // 自己対局の回数
        public int loopTimes_AIStudy;

        // 勝敗のカウント
        int winCount = 0, loseCount = 0, drawCount = 0;

        int result;

        public List<Board> GetBoardFinalResult { get; private set; }

        // 状態数と行動数を定義
        public int state_size = (int)Math.Pow(3, 9); // 3種類の状態
        public int action_size = 9; // 盤面は9マス

        public double[,] Q_TABLE;  // 3^9 = 19683個の盤面のパターンが存在する

        // 学習率と割引率を設定
        double alpha = 0.1;  // 学習率
        double gamma = 0.9; // 割引率

        int[,] bingoTable = new int[8, 3] // 縦、横、斜めが揃う全パターン
        {

    {0, 1, 2},　//横
    {3, 4, 5},  //横
    {6, 7, 8},  //横
    {0, 3, 6},  //縦
    {1, 4, 7},  //縦
    {2, 5, 8},  //縦
    {0, 4, 8},  //斜め
    {2, 4, 6}   //斜め

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
            // 学習回数だけループする
            for (int i = 0; i < instance.loopTimes_AIStudy; i++)
            {
                // 盤面を初期化する
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


                // ゲームが終わるまでループする
                while (true)
                {
                    // 現在の状態番号を取得する
                    int state = GetStateNumber(cells_state);


                    // ターンに応じて行動を決める
                    (int, int) action = new();

                    // 行動に応じて盤面を更新する
                    int x;
                    int y;

                    if (turn == 1) // 自分のターン
                    {
                        action = SelectNextMove_AI(); // AIのロジックに従って行動を選択する
                        x = action.Item1;
                        y = action.Item2;
                    }
                    else // 相手のターン
                    {
                        do
                        {
                            // ランダムに空いているマス目を選択する
                            action = (UnityEngine.Random.Range(0, Size), UnityEngine.Random.Range(0, Size));
                        }
                        // 空いているマス目を選択するまで繰り返す
                        while (cells_state[action.Item1, action.Item2] != EMPTY);
                        x = action.Item1;
                        y = action.Item2;
                    }

                    if (turn == 1)
                    {
                        cells_state[x, y] = CIRCLE; // 自分は◯でマス目を埋める
                    }
                    else
                    {
                        cells_state[x, y] = CROSS; // 相手は×でマス目を埋める
                    }

                    // 空いているマス目から選択したマス目を削除する
                    empties.RemoveAt(cells_state[x, y]);

                    // 次の状態番号を取得する
                    int next_state = GetStateNumber(cells_state);

                    // 勝敗判定を行う
                    int result = JudgeResult(x, y, turn);

                    // 報酬とQ tableの更新を行う
                    int reward;
                    if (result == WIN) // 自分の勝ち
                    {
                        reward = winCount; // 報酬は+1
                        UpdateQ(state, action.Item1 * Size + action.Item2, reward, next_state); // Q tableを更新する
                        break; // ゲーム終了
                    }
                    else if (result == LOSE) // 相手の勝ち
                    {
                        reward = loseCount; // 報酬は-1
                        UpdateQ(state, action.Item1 * Size + action.Item2, reward, next_state); // Q tableを更新する
                        break; // ゲーム終了
                    }
                    else if (result == DRAW) // 引き分け
                    {
                        reward = drawCount; // 報酬は0
                        UpdateQ(state, action.Item1 * Size + action.Item2, reward, next_state); // Q tableを更新する
                        break; // ゲーム終了
                    }
                    else // ゲーム続行
                    {
                        reward = 0; // 報酬は0
                        UpdateQ(state, action.Item1 * Size + action.Item2, reward, next_state); // Q tableを更新する
                        turn = 1 - turn; // ターンを交代する
                    }
                }
            }
        }

        public (int, int) SelectNextMove_AI()   // ランダムな手を選択
        {
            var state = 0;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    state += cells[i, j] * (int)Math.Pow(3, i * Size + j);
                }
            }


            // Q table から最大の Q 値を持つ行動を探す
            var maxQ = double.NegativeInfinity;
            var bestAction = (-1, -1);
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (_cellsState[i, j] == EMPTY) // 空のマス目に対して
                    {
                        var action = i * Size + j; // 行動番号を計算
                        var row = state / Size; // 行番号を計算
                        var col = state % Size; // 列番号を計算
                        var Q_table = _cellsState[row,col]; // Q 値を取得
                        if (Q_table > maxQ) // Q 値が最大値を更新したら
                        {
                            maxQ = Q_table; // 最大値を更新
                            bestAction = (i, j); // 最適な行動を記録
                        }
                    }
                }
            }
            return bestAction;
        }

        public Board Clone() // 盤面の状態をコピーし返す
        {
            Board copy = new Board();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    copy.cells[i, j] = this.cells[i, j];
                }
            }
            copy.turn = this.turn; // どちらのターンかもコピー
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

            // 盤面に手を打つ、該当のマス目は「１か -１」
            cells[x, y] = turn;
            // ターンの切り替え
            turn = -turn;
        }

        public void Loop_AIStudy(RandomAI ai1, RandomAI ai2)
        {
            for (int game = 0; game < loopTimes_AIStudy; game++)
            {
                // 盤面の初期化
                Board board = new Board();


                // 現在の手番のAIを決定
                RandomAI ai = board.GetTurn() == 1 ? ai1 : ai2;

                // AIが手を選択
                var move = ai.SelectNextMove_Randomly(board);

                // 盤面に手を打つ
                board.PutPawn_AIStudy(move.Item1, move.Item2);

                // ターン交代
                turn = -turn;

                // 再判定
                result = JudgeResult(move.Item1, move.Item2, turn);

                // 履歴に盤面を追加
                AddJudgeHistory(board);

                // 自己対局を再開
                Loop_AIStudy(ai1, ai2);
            }
        }

        public int JudgeResult(int row, int col, int turn)  // 勝敗判定
        {
            // 最後に打った手の位置に対応する直線を探す
            for (int i = 0; i < Size; i++)
            {
                // 直線の先頭の位置
                int head = bingoTable[i, 0];

                // 直線に最後に打った手の位置が含まれているか
                bool contains = false;
                for (int j = 0; j < 3; j++)
                {
                    if (bingoTable[i, j] == row * Size + col)
                    {
                        contains = true;
                        break;
                    }
                }

                // 含まれていなければ次の直線を調べる
                if (!contains) continue;

                // 含まれていれば、その直線が揃っているか調べる
                bool aligned = true;
                for (int j = 0; j < 3; j++)
                {
                    if (_cellsState[head / Size, head % Size] != _cellsState[bingoTable[i, j] / Size, bingoTable[i, j] % Size])
                    {
                        aligned = false;
                        break;
                    }
                }

                // 揃っていれば、そのターンのプレイヤーが勝ちと判定する
                if (aligned)
                {
                    if (turn == 1) return WIN;
                    else return LOSE;
                }
            }

            // 引き分けかゲーム続行か判定する
            if (_cellsState.Cast<int>().Count(x => x == EMPTY) == 0) return DRAW;
            else return CONTINUE;
        }

        void AddJudgeHistory(Board board)
        {
            List<Board> history = new List<Board>();

            // 最後の盤面を履歴に追加
            history.Add(board.Clone());

            int allJudgeCount = winCount + loseCount + drawCount;

            if (allJudgeCount == loopTimes_AIStudy)
            {
                GetBoardFinalResult = history;  // 最終結果をプロパティに代入
                return;
            }
        }
    }
    // ランダムAIクラス
    public class RandomAI
    {
        public (int, int) SelectNextMove_Randomly(Board board)   // ランダムな手を選択
        {
            var emptyCells = board.GetEmptyCells();
            var random = UnityEngine.Random.Range(0, emptyCells.Count); // 空のマス目の総数
            var index = random;
            return emptyCells[index];
        }
    }
}
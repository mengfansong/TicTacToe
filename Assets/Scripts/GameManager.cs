using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//先手X、后手O   玩家的选边
public enum Role
{
    X,
    O
}

//棋格状态  每个格子
public enum CellState
{
    Empty,
    X,
    O
}

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    public GameObject cellPrefab;      // 格子Prefab
    public Transform cellsParent;      // 格子的容器
    public GameObject boardGO;  //棋盘GO


    #region 格子图片
    private const float CELL_PIXEL_SIZE = 144f;
    private const float LINE_THICKNESS = 12f;
    private const float PPU = 60f;

    private float CellSize => CELL_PIXEL_SIZE / PPU;
    private float CellSpacing => (CELL_PIXEL_SIZE + LINE_THICKNESS) / PPU;
    #endregion

    public Role playerRole;       // 玩家选择的棋子类型 X、O
    public Role aiRole;    //AI的棋子类型

    //达成3子的连线
    public LineRenderer winLineRenderer;

    //棋子的列表
    private Cell[,] cells = new Cell[3, 3];
    //棋盘的数据
    private CellState[,] board = new CellState[3, 3];

    //玩家是否可以操作
    public bool isPlayerTurn;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UIManager.Instance.ShowMenu();  //进入菜单面板
    }


    //开始新游戏  选边按钮的点击事件 先手X 后手O
    public void StartNewGame(string role)
    {
        winLineRenderer.enabled = false;    //隐藏掉3子的连线
        //禁用玩家输入
        isPlayerTurn = false;

        //设置玩家和电脑的棋子类型
        switch (role)
        {
            case "X" :
                playerRole = Role.X;
                aiRole = Role.O;
                break;
            case "O":
                playerRole = Role.O;
                aiRole = Role.X;
                break;
            default:
                Debug.LogError("按钮选边设置错误");
                break;                
        }          

        //隐藏开始菜单UI
        UIManager.Instance.HideMenu();

        //绘制棋盘棋子
        DrawBoard();

        GoFirstTurn(playerRole == Role.X);

    }

    private void GoFirstTurn(bool player)
    {        
        if (player)
        {
            //玩家下第一步
            GoPlayerTurn();
        }
        else
        {
            //电脑下第一步            
            GoAiTurn();
        }
    }   

    //绘制棋盘
    public void DrawBoard()
    {
        
        boardGO.SetActive(true);

        if (cellPrefab == null || cellsParent == null)
        {
            Debug.LogError("没拖格子");
            return;
        }

        // 清空容器
        foreach (Transform child in cellsParent)
        {
            Destroy(child.gameObject);
        }

        // 生成3行3列格子
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                // 计算位置（左上角为第0行0列）
                float x = (col - 1) * CellSpacing;
                float y = (1 - row) * CellSpacing;
                Vector3 position = new Vector3(x, y, 0f);

                // 实例化棋子格子
                GameObject cellGO = Instantiate(cellPrefab, position, Quaternion.identity, cellsParent);
                cellGO.name = $"Cell_{row}_{col}"; // Cell_0_0 - Cell_2_2

                // 对棋子进行初始化
                Cell cell = cellGO.GetComponent<Cell>();
                cell.Init(new Vector2Int(col, row));

                //记录棋子和棋盘数据
                cells[col, row] = cell; 
                board[col, row] = CellState.Empty;
            }
        }
        Debug.Log("棋盘生成完毕");
    }


    //落子完成后的处理
    public void OnDrawMarkFinished(Cell cell,bool isPlayerDrawed)
    {
        if(isPlayerDrawed)  //这个子是玩家下的
        {
            board[cell.logicalPosition.x, cell.logicalPosition.y] = playerRole == Role.X ? CellState.X : CellState.O;   //更新棋盘数据
            if (IsGameOver(out CellState winner, out List<Vector2Int> winLine))  //游戏结束
            {
                //禁止玩家输入
                isPlayerTurn = false;
                ShowGameResult(winner, winLine);
                return;
            }
            else
            {
                //胜负没分就进入下一回合
                GoAiTurn();
            }
        }
        else
        {     
            //这个子是电脑下的
            //检查游戏是否结束
            if (IsGameOver(out CellState winner, out List<Vector2Int> winLine))  //游戏结束
            {
                //禁止玩家输入
                isPlayerTurn = false;
                ShowGameResult(winner, winLine);
                return;
            }
            else
            {
                //胜负没分就进入下一回合
                GoPlayerTurn();     //进入玩家回合
            }
        }

    }

    //进入玩家回合
    private void GoPlayerTurn()
    {
        //允许点击
        isPlayerTurn = true;
        //刷新鼠标当前所指格子的显示状态
        RefreshHover();
    }

    //进入电脑回合
    private void GoAiTurn()
    {
        //禁用玩家点击
        isPlayerTurn = false;
        RefreshHover(); 

        UIManager.Instance.ShowThinkingText(); // 显示电脑正在思考
        StartCoroutine(AIDelayedMove());    //假装思考后下子
    }

    //AI思考后下棋
    private IEnumerator AIDelayedMove()
    {
        //假装思考一下
        yield return new WaitForSeconds(Random.Range(0.9f, 1.5f));

        Vector2Int aiMove = FindBestMove(); //选择下棋位置
        cells[aiMove.x, aiMove.y].SetMark(aiRole);  //落子
        board[aiMove.x, aiMove.y] = aiRole == Role.X ? CellState.X : CellState.O;   //更新棋盘数据

        UIManager.Instance.HideThinkingText(); // 隐藏电脑正在思考的文本
    }

    // AI寻找最佳落子位置
    public Vector2Int FindBestMove()
    {
        int bestScore = int.MinValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);

        //映射到棋子状态
        CellState checkState = aiRole == Role.X ? CellState.X : CellState.O;
        CellState playerState = aiRole == Role.X ? CellState.O : CellState.X;

        // 最优走法list
        List<Vector2Int> bestMoves = new List<Vector2Int>();

        // 遍历所有可能的走法
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (cells[i, j].currentState == CellState.Empty)
                {
                    // 尝试当前走法
                    CellState[,] newBoard = CloneBoard(board);
                    newBoard[i, j] = checkState;

                    // 计算分数（起始current回合是玩家回合）              
                    int score = Minimax(newBoard, playerState, checkState, 1);

                    // 更新最优走法list
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMoves.Clear(); // 清空之前记录的次优解
                        bestMoves.Add(new Vector2Int(i, j)); // 加入新最优走法
                    }
                    else if (score == bestScore)
                    {
                        // 加入最优走法
                        bestMoves.Add(new Vector2Int(i, j));
                    }
                }
            }
        }

        // 从所有最优走法中随机选择
        if (bestMoves.Count > 0)
        {
            int index = Random.Range(0, bestMoves.Count);
            bestMove = bestMoves[index];
        }
        return bestMove;
    }

    // 极大极小算法
    private int Minimax(CellState[,] board, CellState currentCellState, CellState aiCellState, int depth)
    {
        // 检查游戏是否结束
        CellState winner = CheckWinner(board);

        // 游戏结束
        if (winner != CellState.Empty)
        {
            if (winner == aiCellState)
                return 10 - depth;  // 赢得越早，得分越高
            else
                return depth - 10;  // 输得越晚，损失越小
        }

        if (IsBoardFull(board))
            return 0;

        // 获取所有可能的走法
        List<Vector2Int> moves = GetPossibleMoves(board);
        int bestValue;

        if (currentCellState == aiCellState)  // 最大化层 电脑下
        {
            bestValue = int.MinValue;
            foreach (var move in moves)
            {
                CellState[,] newBoard = CloneBoard(board);
                newBoard[move.x, move.y] = currentCellState;
                CellState nextCellState = currentCellState == CellState.X ? CellState.O : CellState.X;
                bestValue = Mathf.Max(bestValue, Minimax(newBoard, nextCellState, aiCellState, depth + 1));
            }
        }
        else  // 最小化层 玩家下
        {
            bestValue = int.MaxValue;
            foreach (var move in moves)
            {
                CellState[,] newBoard = CloneBoard(board);
                newBoard[move.x, move.y] = currentCellState;
                CellState nextCellState = currentCellState == CellState.X ? CellState.O : CellState.X;
                bestValue = Mathf.Min(bestValue, Minimax(newBoard, nextCellState, aiCellState, depth + 1));
            }
        }
        return bestValue;
    }



    //对局已结束，展示游戏结果
    private void ShowGameResult(CellState winner, List<Vector2Int> winLine)
    {
        if (winner == CellState.Empty)
        {
            UIManager.Instance.ShowGameOver("平局");
        }
        else
        {           
            Vector3 start = cells[winLine[0].x, winLine[0].y].transform.position;
            Vector3 end = cells[winLine[2].x, winLine[2].y].transform.position;

            winLineRenderer.textureMode = LineTextureMode.Stretch; 
            winLineRenderer.startWidth = 1f;
            winLineRenderer.endWidth = 1f;

            start.z = -1;
            end.z = -1;

            winLineRenderer.positionCount = 2;
            winLineRenderer.SetPosition(0, start);
            winLineRenderer.SetPosition(1, start); 

            winLineRenderer.enabled = true;

            DOTween.To(
                 () => start,                                     
                 val => winLineRenderer.SetPosition(1, val),      
                 end,                                            
                 1f  // 用时1秒
             )
             .SetEase(Ease.Linear)
             .OnComplete(() =>
             {
                 CellState playerState = playerRole == Role.X ? CellState.X : CellState.O;
                 string resultText = winner == playerState ? "你赢了（你怎么做到的？）" : "你输了";        
                 UIManager.Instance.ShowGameOver(resultText);       //显示游戏结果
             });                    
        }
    }


    //重新开始游戏 重新开始的按钮的点击事件
    public void RestartGame()
    {
        winLineRenderer.enabled = false;    //隐藏胜利的3子连线
        boardGO.SetActive(false);       //关掉棋盘
        UIManager.Instance.RestartGame();
    }

    //退出游戏 退出游戏的按钮点击事件
    public void ExitGame()
    {
        Application.Quit();
    }


    #region 辅助方法

    // 复制棋盘数据
    private CellState[,] CloneBoard(CellState[,] board)
    {
        CellState[,] clone = new CellState[3, 3];
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                clone[i, j] = board[i, j];
        return clone;
    }

    // 检查胜负(算法内使用)
    private CellState CheckWinner(CellState[,] board)
    {
        // 检查行
        for (int i = 0; i < 3; i++)
            if (board[i, 0] != CellState.Empty && board[i, 0] == board[i, 1] && board[i, 0] == board[i, 2])
                return board[i, 0];

        // 检查列
        for (int j = 0; j < 3; j++)
            if (board[0, j] != CellState.Empty && board[0, j] == board[1, j] && board[0, j] == board[2, j])
                return board[0, j];

        // 检查对角线
        if (board[0, 0] != CellState.Empty && board[0, 0] == board[1, 1] && board[0, 0] == board[2, 2])
            return board[0, 0];

        if (board[0, 2] != CellState.Empty && board[0, 2] == board[1, 1] && board[0, 2] == board[2, 0])
            return board[0, 2];

        return CellState.Empty;
    }

    //判断游戏是否结束 记录3子连线   每次落子行为后检查
    private bool IsGameOver(out CellState winner, out List<Vector2Int> winLine)
    {
        winLine = new List<Vector2Int>();
        winner = CellState.Empty;

        // 横向
        for (int i = 0; i < 3; i++)
        {
            if (board[i, 0] != CellState.Empty &&
                board[i, 0] == board[i, 1] &&
                board[i, 1] == board[i, 2])
            {
                winner = board[i, 0];
                winLine.Add(new Vector2Int(i, 0));
                winLine.Add(new Vector2Int(i, 1));
                winLine.Add(new Vector2Int(i, 2));
                return true;
            }
        }

        // 纵向
        for (int j = 0; j < 3; j++)
        {
            if (board[0, j] != CellState.Empty &&
                board[0, j] == board[1, j] &&
                board[1, j] == board[2, j])
            {
                winner = board[0, j];
                winLine.Add(new Vector2Int(0, j));
                winLine.Add(new Vector2Int(1, j));
                winLine.Add(new Vector2Int(2, j));
                return true;
            }
        }

        // 主对角线
        if (board[0, 0] != CellState.Empty &&
            board[0, 0] == board[1, 1] &&
            board[1, 1] == board[2, 2])
        {
            winner = board[0, 0];
            winLine.Add(new Vector2Int(0, 0));
            winLine.Add(new Vector2Int(1, 1));
            winLine.Add(new Vector2Int(2, 2));
            return true;
        }

        // 副对角线
        if (board[0, 2] != CellState.Empty &&
            board[0, 2] == board[1, 1] &&
            board[1, 1] == board[2, 0])
        {
            winner = board[0, 2];
            winLine.Add(new Vector2Int(0, 2));
            winLine.Add(new Vector2Int(1, 1));
            winLine.Add(new Vector2Int(2, 0));
            return true;
        }

        // 是否平局
        bool hasEmpty = false;
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (board[i, j] == CellState.Empty)
                    hasEmpty = true;

        if (!hasEmpty)
        {
            winner = CellState.Empty;
            winLine = null; // 平局没有连线
            return true;
        }

        // 游戏未结束
        return false;
    }


    // 检查棋盘是否已经下满
    private bool IsBoardFull(CellState[,] board)
    {
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (board[i, j] == CellState.Empty)
                    return false;
        return true;
    }

    // 获取所有可落子位置
    private List<Vector2Int> GetPossibleMoves(CellState[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (board[i, j] == CellState.Empty)
                    moves.Add(new Vector2Int(i, j));
        return moves;
    }

    //刷新鼠标指针
    private void RefreshHover()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
        if (hit.collider != null)
        {
            Cell cell = hit.collider.GetComponent<Cell>();
            if (cell != null)
            {
                cell.RefreshHoverVisual(); 
            }
        }
    }
    #endregion


}


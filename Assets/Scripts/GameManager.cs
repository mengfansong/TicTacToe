using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//����X������O
public enum Role
{
    X,
    O
}

//���״̬
public enum CellState
{
    Empty,
    X,
    O
}

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    public GameObject cellPrefab;      // ����Prefab
    public Transform cellsParent;      // ��������

    public GameObject boardGO;  //����GO
     
    #region ����ͼƬ
    private const float CELL_PIXEL_SIZE = 144f;
    private const float LINE_THICKNESS = 12f;
    private const float PPU = 60f;

    private float CellSize => CELL_PIXEL_SIZE / PPU;
    private float CellSpacing => (CELL_PIXEL_SIZE + LINE_THICKNESS) / PPU;
    #endregion

    private Role playerRole;       // ���ѡ����������� X��O
    private Role aiRole;    //AI����������

    //private Role currentRole;       // ��ǰ�غϵ��������� X��O

    //�����б�
    private Cell[,] cells = new Cell[3, 3];
    //��������
    private CellState[,] board = new CellState[3, 3];

    //��ҿ��Բ���
    public bool IsPlayerTurn;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        
    }

    //��Ϸ��ʼ�˵�
    private void ShowTitle()
    {

    }

    public void StartNewGame(string role)
    {
        //�����������
        IsPlayerTurn = false;

        //������Һ͵��Ե���������
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
                Debug.LogError("��ťѡ�����ô���");
                break;                
        }          

        //���ؿ�ʼ�˵�UI
        UIManager.Instance.HideMenu();

        //������������
        DrawBoard();

        GoFirstTurn(playerRole == Role.X);

    }

    private void GoFirstTurn(bool player)
    {        
        if (player)
        {
            //����µ�һ��
            IsPlayerTurn = true;
        }
        else
        {
            //�����µ�һ��
            IsPlayerTurn = false;
            AIThinkPlay();
        }
    }   

    //��������
    public void DrawBoard()
    {
        boardGO.SetActive(true);
        if (cellPrefab == null || cellsParent == null)
        {
            Debug.LogError("û�ϸ���");
            return;
        }

        // �������
        foreach (Transform child in cellsParent)
        {
            Destroy(child.gameObject);
        }

        // ����3��3�и���
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                // ����λ�ã����Ͻ�Ϊ��0��0�У�
                float x = (col - 1) * CellSpacing;
                float y = (1 - row) * CellSpacing;
                Vector3 position = new Vector3(x, y, 0f);

                // ʵ��������
                GameObject cellGO = Instantiate(cellPrefab, position, Quaternion.identity, cellsParent);
                cellGO.name = $"Cell_{row}_{col}"; // ����Ϊ Cell_0_0 ~ Cell_2_2

                // ��ʼ������
                Cell cell = cellGO.GetComponent<Cell>();
                cell.Init(new Vector2Int(col, row));

                cells[col, row] = cell;
                board[col, row] = CellState.Empty;
            }
        }
        Debug.Log("���̸����������");
    }

    public void OnCellClicked(Cell cell)
    {        
        cell.SetMark(playerRole);
        board[cell.logicalPosition.x, cell.logicalPosition.y] = playerRole == Role.X ? CellState.X : CellState.O;
        if(IsGameOver(out CellState winner, out List<Vector2Int> winLine))  //��Ϸ����
        {
            //��ֹ�������
            IsPlayerTurn = false;
            ShowGameResult(winner, winLine);
            return;
        }
        else
        {
            AIThinkPlay();
        }        
    }

    //������һ�غ�
    private void GoPlayerTurn()
    {
        IsPlayerTurn = true;
    }

    private void ShowGameResult(CellState winner, List<Vector2Int> winLine)
    {
        if (winner == CellState.Empty)
        {
            Debug.Log("��Ϸ������ƽ�֣�");
        }
        else
        {
            Debug.Log($"��Ϸ������{winner} ��ʤ��");
            Debug.Log("���߸������꣺");
            foreach (var pos in winLine)
            {
                Debug.Log($"({pos.x}, {pos.y})");
            }
        }
    }



    private void AIThinkPlay()
    {
        //ai˼������λ��
        Vector2Int aiMove = FindBestMove();
        //AI����
        cells[aiMove.x, aiMove.y].SetMark(aiRole);
        // ������������
        board[aiMove.x, aiMove.y] = aiRole == Role.X ? CellState.X : CellState.O;

        //�����Ϸ�Ƿ����
        if (IsGameOver(out CellState winner, out List<Vector2Int> winLine))  //��Ϸ����
        {
            //��ֹ�������
            IsPlayerTurn = false;
            ShowGameResult(winner, winLine);
            return;
        }
        else
        {
            GoPlayerTurn();     //������һغ�
        }
        
    }

    // AIѰ���������λ��
    public Vector2Int FindBestMove()
    {
        int bestScore = int.MinValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);

        //ӳ�䵽����״̬
        CellState checkState = aiRole == Role.X ? CellState.X : CellState.O;
        CellState playerState = aiRole == Role.X ? CellState.O : CellState.X;

        // �����߷�list
        List<Vector2Int> bestMoves = new List<Vector2Int>();

        // �������п��ܵ��߷�
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (cells[i, j].currentState == CellState.Empty)
                {
                    // ���Ե�ǰ�߷�
                    CellState[,] newBoard = CloneBoard(board);
                    newBoard[i, j] = checkState;

                    // �����������ʼ��ǰ�غ�����һغϣ�              
                    int score = Minimax(newBoard, playerState, checkState, 1);

                    // ���������߷�list
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMoves.Clear(); // ���֮ǰ��¼�Ĵ��Ž�
                        bestMoves.Add(new Vector2Int(i, j)); // �����������߷�
                    }
                    else if (score == bestScore)
                    {
                        // ���������߷�
                        bestMoves.Add(new Vector2Int(i, j));
                    }
                }
            }
        }

        // �����������߷������ѡ��
        if (bestMoves.Count > 0)
        {
            int index = Random.Range(0, bestMoves.Count);
            bestMove = bestMoves[index];
        }

        return bestMove;
    }



    // ����С�㷨
    private int Minimax(CellState[,] board, CellState currentCellState, CellState aiCellState, int depth)
    {
        // �����Ϸ�Ƿ����
        CellState winner = CheckWinner(board);

        // ��Ϸ����
        if (winner != CellState.Empty)
        {
            if (winner == aiCellState)
                return 10 - depth;  // Ӯ��Խ�磬�÷�Խ��
            else
                return depth - 10;  // ���Խ����ʧԽС
        }

        if (IsBoardFull(board))
            return 0;

        // ��ȡ���п��ܵ��߷�
        List<Vector2Int> moves = GetPossibleMoves(board);
        int bestValue;

        if (currentCellState == aiCellState)  // ��󻯲� ������
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
        else  // ��С���� ģ�������
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

    #region ��������

    // ������������
    private CellState[,] CloneBoard(CellState[,] board)
    {
        CellState[,] clone = new CellState[3, 3];
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                clone[i, j] = board[i, j];
        return clone;
    }

    // ���ʤ��(�㷨��ʹ��)
    private CellState CheckWinner(CellState[,] board)
    {
        // �����
        for (int i = 0; i < 3; i++)
            if (board[i, 0] != CellState.Empty && board[i, 0] == board[i, 1] && board[i, 0] == board[i, 2])
                return board[i, 0];

        // �����
        for (int j = 0; j < 3; j++)
            if (board[0, j] != CellState.Empty && board[0, j] == board[1, j] && board[0, j] == board[2, j])
                return board[0, j];

        // ���Խ���
        if (board[0, 0] != CellState.Empty && board[0, 0] == board[1, 1] && board[0, 0] == board[2, 2])
            return board[0, 0];

        if (board[0, 2] != CellState.Empty && board[0, 2] == board[1, 1] && board[0, 2] == board[2, 0])
            return board[0, 2];

        return CellState.Empty;
    }

    //�ж���Ϸ�Ƿ���� ��¼����
    private bool IsGameOver(out CellState winner, out List<Vector2Int> winLine)
    {
        winLine = new List<Vector2Int>();
        winner = CellState.Empty;

        // ����
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

        // ����
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

        // ���Խ���
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

        // ���Խ���
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

        // �Ƿ�ƽ��
        bool hasEmpty = false;
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (board[i, j] == CellState.Empty)
                    hasEmpty = true;

        if (!hasEmpty)
        {
            winner = CellState.Empty;
            winLine = null; // ƽ��û������
            return true;
        }

        // ��Ϸδ����
        return false;
    }


    // ��������Ƿ��Ѿ�����
    private bool IsBoardFull(CellState[,] board)
    {
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (board[i, j] == CellState.Empty)
                    return false;
        return true;
    }

    // ��ȡ���п�����λ��
    private List<Vector2Int> GetPossibleMoves(CellState[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (board[i, j] == CellState.Empty)
                    moves.Add(new Vector2Int(i, j));
        return moves;
    }
    #endregion




    //void StartGame(bool playerGoesFirst);
    //void RestartGame();
    //void PlayerMove(int x, int y);
    //void AIMove();
    //void EndGame(string result); // "Player Wins", "AI Wins", "Draw"

    //void UndoMove(); // ���幦��

    //bool IsPlayerTurn();
    //bool IsGameOver();
}


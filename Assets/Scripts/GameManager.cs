using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//����X������O   ��ҵ�ѡ��
public enum Role
{
    X,
    O
}

//���״̬  ÿ������
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
    public Transform cellsParent;      // ���ӵ�����
    public GameObject boardGO;  //����GO


    #region ����ͼƬ
    private const float CELL_PIXEL_SIZE = 144f;
    private const float LINE_THICKNESS = 12f;
    private const float PPU = 60f;

    private float CellSize => CELL_PIXEL_SIZE / PPU;
    private float CellSpacing => (CELL_PIXEL_SIZE + LINE_THICKNESS) / PPU;
    #endregion

    public Role playerRole;       // ���ѡ����������� X��O
    public Role aiRole;    //AI����������

    //���3�ӵ�����
    public LineRenderer winLineRenderer;

    //���ӵ��б�
    private Cell[,] cells = new Cell[3, 3];
    //���̵�����
    private CellState[,] board = new CellState[3, 3];

    //����Ƿ���Բ���
    public bool isPlayerTurn;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UIManager.Instance.ShowMenu();  //����˵����
    }


    //��ʼ����Ϸ  ѡ�߰�ť�ĵ���¼� ����X ����O
    public void StartNewGame(string role)
    {
        winLineRenderer.enabled = false;    //���ص�3�ӵ�����
        //�����������
        isPlayerTurn = false;

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
            GoPlayerTurn();
        }
        else
        {
            //�����µ�һ��            
            GoAiTurn();
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

                // ʵ�������Ӹ���
                GameObject cellGO = Instantiate(cellPrefab, position, Quaternion.identity, cellsParent);
                cellGO.name = $"Cell_{row}_{col}"; // Cell_0_0 - Cell_2_2

                // �����ӽ��г�ʼ��
                Cell cell = cellGO.GetComponent<Cell>();
                cell.Init(new Vector2Int(col, row));

                //��¼���Ӻ���������
                cells[col, row] = cell; 
                board[col, row] = CellState.Empty;
            }
        }
        Debug.Log("�����������");
    }


    //������ɺ�Ĵ���
    public void OnDrawMarkFinished(Cell cell,bool isPlayerDrawed)
    {
        if(isPlayerDrawed)  //�����������µ�
        {
            board[cell.logicalPosition.x, cell.logicalPosition.y] = playerRole == Role.X ? CellState.X : CellState.O;   //������������
            if (IsGameOver(out CellState winner, out List<Vector2Int> winLine))  //��Ϸ����
            {
                //��ֹ�������
                isPlayerTurn = false;
                ShowGameResult(winner, winLine);
                return;
            }
            else
            {
                //ʤ��û�־ͽ�����һ�غ�
                GoAiTurn();
            }
        }
        else
        {     
            //������ǵ����µ�
            //�����Ϸ�Ƿ����
            if (IsGameOver(out CellState winner, out List<Vector2Int> winLine))  //��Ϸ����
            {
                //��ֹ�������
                isPlayerTurn = false;
                ShowGameResult(winner, winLine);
                return;
            }
            else
            {
                //ʤ��û�־ͽ�����һ�غ�
                GoPlayerTurn();     //������һغ�
            }
        }

    }

    //������һغ�
    private void GoPlayerTurn()
    {
        //������
        isPlayerTurn = true;
        //ˢ����굱ǰ��ָ���ӵ���ʾ״̬
        RefreshHover();
    }

    //������Իغ�
    private void GoAiTurn()
    {
        //������ҵ��
        isPlayerTurn = false;
        RefreshHover(); 

        UIManager.Instance.ShowThinkingText(); // ��ʾ��������˼��
        StartCoroutine(AIDelayedMove());    //��װ˼��������
    }

    //AI˼��������
    private IEnumerator AIDelayedMove()
    {
        //��װ˼��һ��
        yield return new WaitForSeconds(Random.Range(0.9f, 1.5f));

        Vector2Int aiMove = FindBestMove(); //ѡ������λ��
        cells[aiMove.x, aiMove.y].SetMark(aiRole);  //����
        board[aiMove.x, aiMove.y] = aiRole == Role.X ? CellState.X : CellState.O;   //������������

        UIManager.Instance.HideThinkingText(); // ���ص�������˼�����ı�
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

                    // �����������ʼcurrent�غ�����һغϣ�              
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
        else  // ��С���� �����
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



    //�Ծ��ѽ�����չʾ��Ϸ���
    private void ShowGameResult(CellState winner, List<Vector2Int> winLine)
    {
        if (winner == CellState.Empty)
        {
            UIManager.Instance.ShowGameOver("ƽ��");
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
                 1f  // ��ʱ1��
             )
             .SetEase(Ease.Linear)
             .OnComplete(() =>
             {
                 CellState playerState = playerRole == Role.X ? CellState.X : CellState.O;
                 string resultText = winner == playerState ? "��Ӯ�ˣ�����ô�����ģ���" : "������";        
                 UIManager.Instance.ShowGameOver(resultText);       //��ʾ��Ϸ���
             });                    
        }
    }


    //���¿�ʼ��Ϸ ���¿�ʼ�İ�ť�ĵ���¼�
    public void RestartGame()
    {
        winLineRenderer.enabled = false;    //����ʤ����3������
        boardGO.SetActive(false);       //�ص�����
        UIManager.Instance.RestartGame();
    }

    //�˳���Ϸ �˳���Ϸ�İ�ť����¼�
    public void ExitGame()
    {
        Application.Quit();
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

    //�ж���Ϸ�Ƿ���� ��¼3������   ÿ��������Ϊ����
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

    //ˢ�����ָ��
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


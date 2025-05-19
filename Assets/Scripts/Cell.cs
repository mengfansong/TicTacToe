using UnityEngine;
using DG.Tweening;

public class Cell : MonoBehaviour
{     
    //棋格状态
    public CellState currentState;

    [SerializeField] private SpriteRenderer cellRenderer;   // 格子的SpriteRenderer
    [SerializeField] private SpriteRenderer oSprite;  //O
    [SerializeField] private SpriteRenderer xleftToRightLine; // X \  斜杠
    [SerializeField] private SpriteRenderer xrightToLeftLine; // X /  斜杠

    public Sprite oPre;
    public Sprite xPre;
    public Sprite cellSprite;

    private Material oMaterial;
    private Material xltrMaterial;
    private Material xrtlMaterial;


    private bool isPlayerDrawed = false;    //是不是玩家画的  玩家画的为真

    //逻辑坐标
    public Vector2Int logicalPosition; // 格子在棋盘中的行列    

    //初始化格子
    public void Init(Vector2Int pos)
    {
        currentState= CellState.Empty;
        logicalPosition = pos;
        isPlayerDrawed = false;
        cellRenderer.sprite = cellSprite; // 初始图

        // 获取 Sprite 的材质
        oMaterial = oSprite.material;
        xltrMaterial = xleftToRightLine.material;
        xrtlMaterial = xrightToLeftLine.material;

        // 初始化材质
        oMaterial.SetFloat("_FillAmount", 0f);
        xltrMaterial.SetFloat("_FillAmount", 0);
        xrtlMaterial.SetFloat("_FillAmount", 0);
    }


    private void OnMouseDown()
    {
        if (IsClickable())
        {
            isPlayerDrawed= true;   //玩家点的
            GameManager.Instance.isPlayerTurn = false;  //禁用输入
            SetMark(GameManager.Instance.playerRole);   //绘制玩家所选符号
        }
    }

    private void OnMouseEnter()
    {
        if (IsClickable())
        {
            Sprite sprite = GameManager.Instance.playerRole == Role.X ? xPre : oPre;
            cellRenderer.sprite = sprite;   //显示玩家所选符号
        }
    }

    private void OnMouseExit()
    {
        // 恢复为原来的空图
        cellRenderer.sprite = cellSprite;
    }

    //刷新鼠标所指的格子
    public void RefreshHoverVisual()
    {
        if (IsClickable())
        {
            Sprite sprite = GameManager.Instance.playerRole == Role.X ? xPre : oPre;
            cellRenderer.sprite = sprite;   //显示玩家所选符号
        }
        else
        {
            // 恢复为原来的空图
            cellRenderer.sprite = cellSprite;
        }
    }

    //绘制棋子符号
    public void SetMark(Role role)
    {             
        switch (role)
        {
            case Role.X:
                currentState = CellState.X;
                DrawXMark();    //播放X的绘制动画
                break;
            case Role.O:
                currentState = CellState.O;
                DrawOMark();    //播放O的绘制动画
                break;
        }
    }

    private void DrawXMark()
    {
        // 顺序播放动画
        DOTween.Sequence()
            // 先播放从左到右的斜杠 (/)
            .Append(DOTween.To(
                () => xltrMaterial.GetFloat("_FillAmount"),
                x => xltrMaterial.SetFloat("_FillAmount", x),
                1f, 0.5f).SetEase(Ease.Linear))
            // 然后播放从右到左的斜杠 (\)
            .Append(DOTween.To(
                () => xrtlMaterial.GetFloat("_FillAmount"),
                x => xrtlMaterial.SetFloat("_FillAmount", x),
                1f, 0.5f).SetEase(Ease.Linear))
            .OnComplete(() =>
            {
                GameManager.Instance.OnDrawMarkFinished(this,isPlayerDrawed);   //绘制完成，通知GameManager继续处理
            });
       
    {

    }
    }
    private void DrawOMark()
    {
        // 顺时针动画
        DOTween.To(
            () => oMaterial.GetFloat("_FillAmount"), // 获取当前值
            (value) => oMaterial.SetFloat("_FillAmount", value), 
            1f, // 完全填充
            0.7f  // 持续时间
        )
        .SetEase(Ease.Linear) 
        .OnComplete(() =>
        {
            GameManager.Instance.OnDrawMarkFinished(this,isPlayerDrawed);   //绘制完成
        });
    }

    //查询棋格是否可点
    public bool IsClickable()
    {
        return currentState == CellState.Empty && GameManager.Instance.isPlayerTurn;
    }

}

using UnityEngine;
using DG.Tweening;

public class Cell : MonoBehaviour
{     
    public CellState currentState;

    [SerializeField] private SpriteRenderer oSprite;  //O
    [SerializeField] private SpriteRenderer xleftToRightLine; // X \  斜杠
    [SerializeField] private SpriteRenderer xrightToLeftLine; // X /  斜杠

    private Material oMaterial;
    private Material xltrMaterial;
    private Material xrtlMaterial;

    //逻辑坐标
    public Vector2Int logicalPosition; // 该格子在棋盘中的行列（如 0,0）    

    //初始化格子
    public void Init(Vector2Int pos)
    {
        currentState= CellState.Empty;
        logicalPosition = pos;

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

        if (currentState != CellState.Empty)
            return;

        GameManager.Instance.OnCellClicked(this);
        Debug.Log("点了");
    }

    public void SetMark(Role role)
    {             
        switch (role)
        {
            case Role.X:
                Debug.Log("X");
                currentState = CellState.X;
                DrawXMark();
                break;
            case Role.O:
                Debug.Log("O");
                currentState = CellState.O;
                DrawOMark();
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
            .OnComplete(() => Debug.Log("X绘制完成"));
    }
    private void DrawOMark()
    {
        // 使用 DOTween 动画从 0 填充到 1（顺时针动画）
        DOTween.To(
            () => oMaterial.GetFloat("_FillAmount"), // 获取当前值
            (value) => oMaterial.SetFloat("_FillAmount", value), // 设置新值
            1f, // 目标值（1 = 完全填充）
            0.7f  // 动画持续时间
        )
        .SetEase(Ease.Linear) // 线性动画（类似 CD 旋转）
        .OnComplete(() =>
        {
            Debug.Log("填充动画完成！");
            // 动画结束后的回调（可选）
        });
    }

    public Vector2Int GetGridPosition()
    {
        return logicalPosition;
    }
}

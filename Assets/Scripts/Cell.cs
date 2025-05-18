using UnityEngine;
using DG.Tweening;

public class Cell : MonoBehaviour
{     
    public CellState currentState;

    [SerializeField] private SpriteRenderer oSprite;  //O
    [SerializeField] private SpriteRenderer xleftToRightLine; // X \  б��
    [SerializeField] private SpriteRenderer xrightToLeftLine; // X /  б��

    private Material oMaterial;
    private Material xltrMaterial;
    private Material xrtlMaterial;

    //�߼�����
    public Vector2Int logicalPosition; // �ø����������е����У��� 0,0��    

    //��ʼ������
    public void Init(Vector2Int pos)
    {
        currentState= CellState.Empty;
        logicalPosition = pos;

        // ��ȡ Sprite �Ĳ���
        oMaterial = oSprite.material;
        xltrMaterial = xleftToRightLine.material;
        xrtlMaterial = xrightToLeftLine.material;

        // ��ʼ������
        oMaterial.SetFloat("_FillAmount", 0f);
        xltrMaterial.SetFloat("_FillAmount", 0);
        xrtlMaterial.SetFloat("_FillAmount", 0);
    }

    private void OnMouseDown()
    {

        if (currentState != CellState.Empty)
            return;

        GameManager.Instance.OnCellClicked(this);
        Debug.Log("����");
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
        // ˳�򲥷Ŷ���
        DOTween.Sequence()
            // �Ȳ��Ŵ����ҵ�б�� (/)
            .Append(DOTween.To(
                () => xltrMaterial.GetFloat("_FillAmount"),
                x => xltrMaterial.SetFloat("_FillAmount", x),
                1f, 0.5f).SetEase(Ease.Linear))
            // Ȼ�󲥷Ŵ��ҵ����б�� (\)
            .Append(DOTween.To(
                () => xrtlMaterial.GetFloat("_FillAmount"),
                x => xrtlMaterial.SetFloat("_FillAmount", x),
                1f, 0.5f).SetEase(Ease.Linear))
            .OnComplete(() => Debug.Log("X�������"));
    }
    private void DrawOMark()
    {
        // ʹ�� DOTween ������ 0 ��䵽 1��˳ʱ�붯����
        DOTween.To(
            () => oMaterial.GetFloat("_FillAmount"), // ��ȡ��ǰֵ
            (value) => oMaterial.SetFloat("_FillAmount", value), // ������ֵ
            1f, // Ŀ��ֵ��1 = ��ȫ��䣩
            0.7f  // ��������ʱ��
        )
        .SetEase(Ease.Linear) // ���Զ��������� CD ��ת��
        .OnComplete(() =>
        {
            Debug.Log("��䶯����ɣ�");
            // ����������Ļص�����ѡ��
        });
    }

    public Vector2Int GetGridPosition()
    {
        return logicalPosition;
    }
}

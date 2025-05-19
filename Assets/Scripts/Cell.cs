using UnityEngine;
using DG.Tweening;

public class Cell : MonoBehaviour
{     
    //���״̬
    public CellState currentState;

    [SerializeField] private SpriteRenderer cellRenderer;   // ���ӵ�SpriteRenderer
    [SerializeField] private SpriteRenderer oSprite;  //O
    [SerializeField] private SpriteRenderer xleftToRightLine; // X \  б��
    [SerializeField] private SpriteRenderer xrightToLeftLine; // X /  б��

    public Sprite oPre;
    public Sprite xPre;
    public Sprite cellSprite;

    private Material oMaterial;
    private Material xltrMaterial;
    private Material xrtlMaterial;


    private bool isPlayerDrawed = false;    //�ǲ�����һ���  ��һ���Ϊ��

    //�߼�����
    public Vector2Int logicalPosition; // �����������е�����    

    //��ʼ������
    public void Init(Vector2Int pos)
    {
        currentState= CellState.Empty;
        logicalPosition = pos;
        isPlayerDrawed = false;
        cellRenderer.sprite = cellSprite; // ��ʼͼ

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
        if (IsClickable())
        {
            isPlayerDrawed= true;   //��ҵ��
            GameManager.Instance.isPlayerTurn = false;  //��������
            SetMark(GameManager.Instance.playerRole);   //���������ѡ����
        }
    }

    private void OnMouseEnter()
    {
        if (IsClickable())
        {
            Sprite sprite = GameManager.Instance.playerRole == Role.X ? xPre : oPre;
            cellRenderer.sprite = sprite;   //��ʾ�����ѡ����
        }
    }

    private void OnMouseExit()
    {
        // �ָ�Ϊԭ���Ŀ�ͼ
        cellRenderer.sprite = cellSprite;
    }

    //ˢ�������ָ�ĸ���
    public void RefreshHoverVisual()
    {
        if (IsClickable())
        {
            Sprite sprite = GameManager.Instance.playerRole == Role.X ? xPre : oPre;
            cellRenderer.sprite = sprite;   //��ʾ�����ѡ����
        }
        else
        {
            // �ָ�Ϊԭ���Ŀ�ͼ
            cellRenderer.sprite = cellSprite;
        }
    }

    //�������ӷ���
    public void SetMark(Role role)
    {             
        switch (role)
        {
            case Role.X:
                currentState = CellState.X;
                DrawXMark();    //����X�Ļ��ƶ���
                break;
            case Role.O:
                currentState = CellState.O;
                DrawOMark();    //����O�Ļ��ƶ���
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
            .OnComplete(() =>
            {
                GameManager.Instance.OnDrawMarkFinished(this,isPlayerDrawed);   //������ɣ�֪ͨGameManager��������
            });
       
    {

    }
    }
    private void DrawOMark()
    {
        // ˳ʱ�붯��
        DOTween.To(
            () => oMaterial.GetFloat("_FillAmount"), // ��ȡ��ǰֵ
            (value) => oMaterial.SetFloat("_FillAmount", value), 
            1f, // ��ȫ���
            0.7f  // ����ʱ��
        )
        .SetEase(Ease.Linear) 
        .OnComplete(() =>
        {
            GameManager.Instance.OnDrawMarkFinished(this,isPlayerDrawed);   //�������
        });
    }

    //��ѯ����Ƿ�ɵ�
    public bool IsClickable()
    {
        return currentState == CellState.Empty && GameManager.Instance.isPlayerTurn;
    }

}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject menuGO;   //���˵����������⡢��ʼ��Ϸ���˳���Ϸ����Ϸѡ�ߣ�
    public GameObject btnsGO;   //�˵�ѡ���飺 ��ʼ���˳�
    public GameObject roleGO;   //����ѡ����壺 ����X �� ����O
    public GameObject tutorialGO;   //�淨˵��

    //AI˼����ʾ����  AI˼���С�����
    public TextMeshProUGUI thinkingText;    
    private Coroutine thinkingCoroutine;

    //��Ϸ���������
    public GameObject gameOverPanel;
    public TextMeshProUGUI resultText;  //���������  ƽ�֣���Ӯ�ˣ�������


    private void Awake()
    {
        Instance = this;
    }

    //��ʾ�˵����
    public void ShowMenu()
    {
        menuGO.SetActive(true);
    }

    //����ѡ�����
    public void ChooseRole()
    {
        btnsGO.SetActive(false);
        roleGO.SetActive(true);
    }

    //���ز˵����
    public void HideMenu()
    {
        menuGO.SetActive(false);
    }

    //��ʾAI˼����
    public void ShowThinkingText()
    {
        if (thinkingCoroutine != null)
            StopCoroutine(thinkingCoroutine);

        thinkingText.gameObject.SetActive(true);
        thinkingCoroutine = StartCoroutine(AnimateThinkingText());
    }

    //����AI˼����
    public void HideThinkingText()
    {
        if (thinkingCoroutine != null)
        {
            StopCoroutine(thinkingCoroutine);
            thinkingCoroutine = null;
        }
        thinkingText.gameObject.SetActive(false);
    }

    //��ʾ˼��״̬���ֵĶ���
    private IEnumerator AnimateThinkingText()
    {
        string baseText = "AI˼����";
        int dotCount = 0;

        while (true)
        {
            dotCount = (dotCount % 3) + 1;
            thinkingText.text = baseText + new string('.', dotCount);
            yield return new WaitForSeconds(0.3f);  //ÿ0.3��Ӹ���
        }
    }

    //��ʾ��Ϸ�Ծֽ��
    public void ShowGameOver(string message)
    {
        resultText.text = message;
        gameOverPanel.SetActive(true);
    }

    //���¿��֣��ص���Ϸ������壬ֱ�ӽ���ѡ�����
    public void RestartGame()
    {
        menuGO.SetActive(true);
        gameOverPanel.SetActive(false);
        ChooseRole();
    }

    public void ShowTutorial()
    {
        tutorialGO.SetActive(true);
    }

    public void HideTutorial()
    {
        tutorialGO.SetActive(false);
    }

}

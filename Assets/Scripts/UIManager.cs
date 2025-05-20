using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject menuGO;   //主菜单（包括标题、开始游戏、退出游戏、游戏选边）
    public GameObject btnsGO;   //菜单选项组： 开始、退出
    public GameObject roleGO;   //棋子选边面板： 先手X 和 后手O
    public GameObject tutorialGO;   //玩法说明

    //AI思考提示文字  AI思考中。。。
    public TextMeshProUGUI thinkingText;    
    private Coroutine thinkingCoroutine;

    //游戏结束的面板
    public GameObject gameOverPanel;
    public TextMeshProUGUI resultText;  //结果的文字  平局，你赢了，你输了


    private void Awake()
    {
        Instance = this;
    }

    //显示菜单面板
    public void ShowMenu()
    {
        menuGO.SetActive(true);
    }

    //进入选边面板
    public void ChooseRole()
    {
        btnsGO.SetActive(false);
        roleGO.SetActive(true);
    }

    //隐藏菜单面板
    public void HideMenu()
    {
        menuGO.SetActive(false);
    }

    //显示AI思考中
    public void ShowThinkingText()
    {
        if (thinkingCoroutine != null)
            StopCoroutine(thinkingCoroutine);

        thinkingText.gameObject.SetActive(true);
        thinkingCoroutine = StartCoroutine(AnimateThinkingText());
    }

    //隐藏AI思考中
    public void HideThinkingText()
    {
        if (thinkingCoroutine != null)
        {
            StopCoroutine(thinkingCoroutine);
            thinkingCoroutine = null;
        }
        thinkingText.gameObject.SetActive(false);
    }

    //显示思考状态文字的动画
    private IEnumerator AnimateThinkingText()
    {
        string baseText = "AI思考中";
        int dotCount = 0;

        while (true)
        {
            dotCount = (dotCount % 3) + 1;
            thinkingText.text = baseText + new string('.', dotCount);
            yield return new WaitForSeconds(0.3f);  //每0.3秒加个点
        }
    }

    //显示游戏对局结果
    public void ShowGameOver(string message)
    {
        resultText.text = message;
        gameOverPanel.SetActive(true);
    }

    //重新开局，关掉游戏结束面板，直接进入选边面板
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

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginController : MonoBehaviour
{
    [Header("UI 引用")]
    public TMP_InputField accountInput;   // 账号输入框
    public TMP_InputField passwordInput;  // 密码输入框
    public Button loginButton;        // 登录按钮

    [Header("场景设置")]
    public string nextSceneName = "SampleScene"; // 登录成功后切换的场景名

    void Start()
    {
        // 为登录按钮添加点击监听
        loginButton.onClick.AddListener(OnLoginClicked);
    }

    void OnLoginClicked()
    {
        string account = accountInput.text;
        string password = passwordInput.text;

        if (!string.IsNullOrEmpty(account) && !string.IsNullOrEmpty(password))
        {
            // 账号密码均非空，切换场景
            SceneManager.LoadScene(nextSceneName);   
        }
        else
        {
            // 提示（可替换为更友好的 UI 提示）
            Debug.LogWarning("账号或密码不能为空！");
        }
    }
}
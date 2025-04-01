using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Inspector에서 설정할 수 있는 씬 이름
    public string sceneName;

    // 버튼에 이 함수를 연결하세요.
    public void ChangeScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            if (sceneName == "GameScene" && GameManager.Instance != null)
            {
                GameManager.Instance.curScore = 0;
            }
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("씬 이름이 비어있습니다!");
        }
    }
}

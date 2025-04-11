using UnityEngine;

public class SettingManager : MonoBehaviour
{
    public GameObject panel; // Kéo thả Panel từ Inspector vào đây

    // Hàm ẩn/hiện Panel
    public void TogglePanel()
    {
        if (panel != null)
        {
            panel.SetActive(!panel.activeSelf);
        }
    }

    // Hàm thoát game
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Dùng cho chế độ Editor
#endif
    }
}

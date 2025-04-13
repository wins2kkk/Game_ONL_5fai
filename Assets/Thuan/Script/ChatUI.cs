using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI chatContent;
    public GameObject chatPanel;

    private bool isChatting = false;

    public static bool IsChatting { get; private set; } = false;

    private void Start()
    {
        chatPanel.SetActive(false);
    }

    private void Update()
    {
        // Đóng chat bằng ESC hoặc `
        if (isChatting && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.BackQuote)))
        {
            isChatting = false;
            IsChatting = false;
            chatPanel.SetActive(false);
            inputField.DeactivateInputField();
            return;
        }

        // Mở chat hoặc gửi tin nhắn bằng Enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!isChatting)
            {
                isChatting = true;
                IsChatting = true;
                chatPanel.SetActive(true);
                inputField.text = "";
                inputField.Select();
                inputField.ActivateInputField();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(inputField.text))
                {
                    SendMessage();
                }

                inputField.text = "";
                inputField.Select();
                inputField.ActivateInputField();
            }
        }
    }

    private void SendMessage()
    {
        string message = inputField.text;
        if (!string.IsNullOrEmpty(message))
        {
            ChatManager.Instance.SendChatMessage(message);
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }
}

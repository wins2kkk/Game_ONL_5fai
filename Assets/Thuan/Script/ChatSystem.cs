using Fusion;
using Starter;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChatSystem : NetworkBehaviour
{
    [Header("Objects")]
    public GameObject chatEntryCanvas;
    public TMP_InputField chatEntryInput;
    public TextMeshProUGUI chatBody;

    [Header("Action References")]
   public InputActionReference startChat;
   public InputActionReference sendChat;

    [Header("Networked")]
    private GameMode placeholder;
    [HideInInspector] [Networked] private NetworkString<_256> LastPublicChat { get; set; }
    [HideInInspector] [Networked] private NetworkString<_256> LastPrivateChat { get; set; }
    private NetworkString<_256> lastChatCache;
    private NetworkString<_256> lastPrivateChatCache;

    public string PlayersName; // Placeholder for player name

    private void Start()
    {
        if (HasInputAuthority)
        {
            // Lấy NetworkObject của chính player này
            UIGameMenu nickname = GetComponentInChildren<UIGameMenu>();
            if (nickname != null)
            {
                PlayersName = nickname.NicknameText.text;
            }

            startChat.action.performed += StartChat;
            sendChat.action.performed += SendChat;
        }
    }



    public override void FixedUpdateNetwork()
    {
        // Kiểm tra nếu tin nhắn công khai đã thay đổi
        if (LastPublicChat != lastChatCache)
        {
            lastChatCache = LastPublicChat;
            OnPublicChatChanged();
        }

        // Kiểm tra nếu tin nhắn riêng tư đã thay đổi
        if (LastPrivateChat != lastPrivateChatCache)
        {
            lastPrivateChatCache = LastPrivateChat;
            OnPrivateChatChanged();
        }
    }

    private void OnPublicChatChanged()
    {
        chatBody.text += "\n" + PlayersName + ": " + LastPublicChat.ToString();
    }

    private void OnPrivateChatChanged()
    {
        chatBody.text += "\n[Private] " + LastPrivateChat;
    }

    private void SendChat(InputAction.CallbackContext obj)
    {
        //if (HasStateAuthority && !string.IsNullOrEmpty(chatEntryInput.text))
        //{
        //    LastPublicChat = chatEntryInput.text;
        //    chatEntryInput.text = "";
        //    chatEntryCanvas.SetActive(false);
        //}
        LastPublicChat = chatEntryInput.text;
        chatEntryCanvas.SetActive(false);
    }

    private void StartChat(InputAction.CallbackContext obj)
    {
        chatEntryCanvas.SetActive(true);
      //  chatEntryInput.ActivateInputField();
        chatEntryInput.Select();
    }
}

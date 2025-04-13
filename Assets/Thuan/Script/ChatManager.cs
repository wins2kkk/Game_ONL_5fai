using Photon.Chat;
using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    public ChatUI chatUI;
    private ChatClient chatClient;
    public static ChatManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize(string nickname)
    {
        chatClient = new ChatClient(this);
        PhotonNetwork.LocalPlayer.NickName = nickname;

        chatClient.Connect(
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat,
            PhotonNetwork.AppVersion,
            new AuthenticationValues($"{nickname}")
        );
    }

    void Start()
    {
        // Có thể để trống hoặc kiểm tra nếu chưa được khởi tạo
        if (chatClient == null)
        {
            Debug.LogWarning("ChatClient chưa được khởi tạo. Hãy gọi ChatManager.Instance.Initialize(nickname)");
        }
        

    }


    public void SendChatMessage(string message)
    {
        chatClient.PublishMessage("General", message);
    }

    void Update()
    {
        if (chatClient != null)
        {
            chatClient.Service();
        }
    }


    // IChatClientListener implementations

    public void OnConnected()
    {
        chatClient.Subscribe(new string[] { "General" });
    }

    public void OnDisconnected()
    {
        Debug.Log("Chat disconnected");
    }

    public void OnChatStateChange(ChatState state)
    {
        Debug.Log("Chat state changed: " + state);
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < messages.Length; i++)
        {
            string sender = senders[i];
            string message = messages[i].ToString();

            // Gán màu khác nhau tùy người gửi
            string senderColor = sender == PhotonNetwork.NickName ? "#00FFFF" : "#FF69B4";     // Ví dụ: bạn là xanh, người khác là hồng
            string messageColor = sender == PhotonNetwork.NickName ? "#FFFFFF" : "#DDDDDD";    // Màu tin nhắn cũng khác

            string formattedMsg = $"<color={senderColor}>{sender}</color>: <color={messageColor}>{message}</color>";

            chatUI.chatContent.text += formattedMsg + "\n";
        }
    }


    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        Debug.Log($"Private message from {sender}: {message}");
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log("Subscribed to channel(s): " + string.Join(", ", channels));
    }

    public void OnUnsubscribed(string[] channels)
    {
        Debug.Log("Unsubscribed from channel(s): " + string.Join(", ", channels));
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        Debug.Log($"Status update from {user}: {status} - {message}");
    }

    public void OnUserSubscribed(string channel, string user)
    {
        Debug.Log($"{user} has joined channel {channel}");
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        Debug.Log($"{user} has left channel {channel}");
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log($"DebugReturn [{level}]: {message}");
    }
}

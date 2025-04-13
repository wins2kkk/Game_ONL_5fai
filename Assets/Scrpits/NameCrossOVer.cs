using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameCrossOVer : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] Text PlayernameTextHolder;
    [SerializeField] Text AnotherPlayernameTextHolder; // New text field
    bool Updatedname = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Updatename();
    }

    private void Updatename()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        if (gameManager != null)
        {
            string playerName = gameManager.playerName;
            PlayernameTextHolder.text = playerName;
            AnotherPlayernameTextHolder.text = gameManager.playerName; // Update the new text field
        }
    }
}

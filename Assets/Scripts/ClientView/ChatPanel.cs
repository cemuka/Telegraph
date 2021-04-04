using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatPanel : MonoBehaviour
{
    public InputField inputField;
    public Text chatLogText;

    public void AddLog(string log)
    {
        chatLogText.text += log + "\n";
    }
}

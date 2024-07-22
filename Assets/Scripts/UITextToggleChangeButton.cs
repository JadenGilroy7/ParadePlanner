using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    [DisallowMultipleComponent]
    public class UITextToggleChangeButton : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI m_TextMeshPro;
        public void OnButtonPressed()
        {
            Debug.Log("On Button Clicked!");
            m_TextMeshPro.text = m_TextMeshPro.text == "On" ? "Off" : "On";
        }
    }
}



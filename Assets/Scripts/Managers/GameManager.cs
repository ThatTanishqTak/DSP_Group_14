using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;


public class GameManager : MonoBehaviour
{
    [SerializeField] public bool isPaused;
    
    [SerializeField] public int coin;

    [SerializeField] public float time;

    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text timeText;

    private void Awake()
    {
        isPaused = false;

        coin = 0;

        time = 0.0f;
    }

    private void Update()
    {
        if (!isPaused)
        {
            time += Time.deltaTime;
            timeText.text = time.ToString("F1");

            OnCoinUpdate();
        }

        //--------------------------------------------------------//

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            isPaused = !isPaused;
        }
    }

    private void OnCoinUpdate()
    {

    }
}
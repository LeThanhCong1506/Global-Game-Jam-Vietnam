using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenController : MonoBehaviour
{
    [Header("Scene")]
    public string nextSceneName = "Room1";

    [Header("Audio")]
    public AudioSource clickAudio;

    // Hàm gọi khi bấm nút Start
    public void StartGame()
    {
        // Phát sound click nếu có
        if (clickAudio != null)
        {
            clickAudio.Play();
        }

        // Load scene ngay (đơn giản, không delay)
        SceneManager.LoadScene(nextSceneName);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    //[SerializeField] private ConnectionsPool _shapesPool;
    private static readonly int FadeOut = Animator.StringToHash("FadeIn");

    private void OnEnable()
    {
        //_shapesPool.GameOver += OnGameOver;
    }

    private void OnGameOver()
    {
        LoadGame();
    }

    private void LoadGame()
    {
        _animator.SetTrigger(FadeOut);
    }

    public void OnFadeComplete()
    {
        SceneManager.LoadScene(0);
    }

}

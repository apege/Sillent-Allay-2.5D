using UnityEngine;
using UnityEngine.Playables;

public class TimelineCutsceneManager : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private PlayableDirector playableDirector;

    [Header("Scene setelah cutscene selesai")]
    [SerializeField] private string nextSceneName;

    private void Start()
    {
        if (playableDirector == null)
        {
            Debug.LogError("[CutsceneManager] PlayableDirector tidak ditemukan!");
            return;
        }

        // Subscribe ke event selesai
        playableDirector.stopped += OnCutsceneFinished;

        // Play cutscene
        playableDirector.Play();
    }

    private void OnDestroy()
    {
        if (playableDirector != null)
            playableDirector.stopped -= OnCutsceneFinished;
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(nextSceneName);
        else
            Debug.LogError("[CutsceneManager] SceneTransitionManager tidak ditemukan!");
    }
}
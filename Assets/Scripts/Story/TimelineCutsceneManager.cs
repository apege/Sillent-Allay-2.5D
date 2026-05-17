using UnityEngine;
using UnityEngine.Playables;

public class TimelineCutsceneManager : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private PlayableDirector playableDirector;

    [Header("Scene setelah cutscene selesai")]
    [SerializeField] private string nextSceneName;

    [Header("Spawn Point")]
    [SerializeField] private string targetSpawnPointID; // isi ID spawn point tujuan

    private void Start()
    {
        if (playableDirector == null)
        {
            Debug.LogError("[TimelineCutsceneManager] PlayableDirector tidak ditemukan!");
            return;
        }

        playableDirector.stopped += OnCutsceneFinished;
        playableDirector.Play();
    }

    private void OnDestroy()
    {
        if (playableDirector != null)
            playableDirector.stopped -= OnCutsceneFinished;
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        // Set spawn point sebelum pindah scene
        if (!string.IsNullOrEmpty(targetSpawnPointID))
            SpawnPointManager.nextSpawnPointID = targetSpawnPointID;

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(nextSceneName);
        else
            Debug.LogError("[TimelineCutsceneManager] SceneTransitionManager tidak ditemukan!");
    }
}
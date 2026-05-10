using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class orb_script : MonoBehaviour
{
    [SerializeField] AudioClip collectSound;
    [SerializeField] int cutsceneIndex;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (collectSound != null)
                AudioSource.PlayClipAtPoint(collectSound, transform.position);

            // create runner before deactivating
            GameObject runner = new GameObject("CoroutineRunner");
            DontDestroyOnLoad(runner);
            runner.AddComponent<OrbRunner>().Init(cutsceneIndex);

            gameObject.SetActive(false);
        }
    }
}

public class OrbRunner : MonoBehaviour
{
    int sceneIndex;

    public void Init(int index)
    {
        sceneIndex = index;
        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        yield return null; // wait one frame
        SceneManager.LoadScene(sceneIndex, LoadSceneMode.Additive);
        Destroy(gameObject);
    }
}
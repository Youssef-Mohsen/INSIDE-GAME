using UnityEngine;
using UnityEngine.SceneManagement;

public class NewMonoBehaviourScript : MonoBehaviour
{    
    bool dead = false;
    void Update()
    {
        if (transform.position.y < -1f && !dead) 
        {
            Die();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy Body"))
            Die();
    }

    void Die() 
    {
        //GetComponent<MeshRenderer>().enabled = false; // make the player disapear (not working meshrenderer is null)
        GetComponent<Rigidbody>().isKinematic = true; // stop the player when colision
     
        GetComponent<boyMovement>().enabled = false; // disable the movement by the user
        //Invoke(nameof(ReloadLv1), 1.3f); //same as calling ReloadLvl(); but with delay 1.3f (not working)
        dead = true;
    }

    void ReloadLv1() 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WolvesDropper : MonoBehaviour
{
    public GameObject WolfPrefab;
    public float XaxisVariance = .25f;

    public void OnDropWolves(int wolfcount)
    {
        StartCoroutine("DropWolves", wolfcount);
    }

    public IEnumerator DropWolves(int wolfcount)
    {
        for (int i = 0; i < wolfcount; i++)
        {
            var distanceFromX = Random.Range(-XaxisVariance, XaxisVariance);
            transform.position = new Vector3(distanceFromX, transform.position.y, transform.position.z);

            var wolf = GameObject.Instantiate(WolfPrefab, transform.position, Quaternion.identity);
            // wolf.transform.position = new Vector3(wolf.transform.position.x,
            // 3, wolf.transform.position.z);
            if (i == wolfcount - 1)
            {
                yield return new WaitForSeconds(4f);
                if (Stars.stars != null && Stars.stars.won)
                {
                    SceneManager.LoadScene("title");
                }
                break;
            }
            
            yield return new WaitForSeconds(.5f);
        }        

        
    }


}

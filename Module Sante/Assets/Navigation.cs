using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation : MonoBehaviour
{
    public List<GameObject> waypoints;
    private int n=1;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(n<5 && Vector3.Distance(transform.position, waypoints[n-1].transform.position)<1){
            Debug.Log("Point n�" + n + " proche !");
            n++;
        }
        
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.red;
        Gizmos.DrawLine(waypoints[n-1].transform.position, transform.position);
        Gizmos.color = Color.yellow;
        for(int i=0; i<waypoints.Count-1; i++){
            Gizmos.DrawLine(waypoints[i].transform.position, waypoints[i+1].transform.position);
        }
    }
}

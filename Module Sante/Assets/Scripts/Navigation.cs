using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Navigation : MonoBehaviour
{
    public List<GameObject> waypoints;
    private int _n=1;
    public VibrationManager_minimal vb;
    public int counter=0;
    private long _timeDeb;
    public static long timeTotal;
    public static bool succes = false;


    // Start is called before the first frame update
    void Start()
    {
        if (menu.mode)
        {
            this.transform.position = waypoints[8].transform.position;
        }

        //light.transform.position = new Vector3(-210, 3, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if(_n<10 && Vector3.Distance(transform.position, waypoints[_n-1].transform.position)<3){
            Debug.Log("Point n°" + _n + " proche !");
            deplacement(_n);
            _n++;
        }
        
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.red;
        Gizmos.DrawLine(waypoints[_n-1].transform.position, transform.position);
        Gizmos.color = Color.yellow;
        for(int i=0; i<waypoints.Count-1; i++){
            Gizmos.DrawLine(waypoints[i].transform.position, waypoints[i+1].transform.position);
        }
    }

    void deplacement(int n)
    {
        switch (n)
        {
            case 1:
                vb.avance();
                _timeDeb = System.DateTime.Now.Ticks;
                Debug.Log("Avance");
                break;
            case 2:
                vb.tourne_droite();
                Debug.Log("Tourne a droite");
                break;
            case 3:
                vb.tourne_gauche();
                Debug.Log("Tourne a gauche");
                break;
            case 4:
                vb.tourne_droite();
                Debug.Log("Tourne a droite");
                break;
            case 5:
                vb.tourne_droite();
                Debug.Log("Tourne a droite");
                break;
            case 6:
                vb.tourne_gauche();
                Debug.Log("Tourne a gauche");
                break;
            case 7:
                vb.tourne_gauche();
                Debug.Log("Tourne a gauche");
                break;
            case 8:
                vb.tourne_droite();
                Debug.Log("Tourne a droite");
                break;
            case 9:
                vb.succes();
                timeTotal = (System.DateTime.Now.Ticks - _timeDeb)/10000000;
                Debug.Log("Succes");
                Debug.Log("Temps total : " + timeTotal);
                succes = true;
                break;
        }
        this.counter++;
            
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube1 : MonoBehaviour
{
    public VibrationManager_minimal moteurs;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider collision){
        Vector3 pos = collision.ClosestPointOnBounds(this.transform.position);
        Vector3 pointLocal = transform.InverseTransformPoint(pos);
        if(pointLocal.x < 0){
            Debug.Log("Obs gauche");
            this.moteurs.obstacleGauche();
        }else{
            Debug.Log("Obs droite");
            this.moteurs.obstacleDroite();
        }
    }

    public void OnTriggerStay(Collider collision){
        Vector3 pos = collision.ClosestPointOnBounds(this.transform.position);
        Vector3 pointLocal = transform.InverseTransformPoint(pos);
        if(pointLocal.x < 0){
            //Debug.Log("Obs gauche");
            this.moteurs.obstacleGauche();
        }else{
            //Debug.Log("Obs droite");
            this.moteurs.obstacleDroite();
        }
    }
}
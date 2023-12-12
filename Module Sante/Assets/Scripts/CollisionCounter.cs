using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCounter : MonoBehaviour
{
    public int collisionCounter = 0;

    public void OnTriggerEnter(Collider collision){
        this.collisionCounter = this.collisionCounter + 1;
        //Debug.Log("Nb collisions : " + collisionCounter);

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCounter : MonoBehaviour
{
    public static int collisionCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void OnTriggerEnter(Collider collision){
        collisionCounter = collisionCounter + 1;
        Debug.Log("Nb collisions : " + collisionCounter);

    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

public class TestLog:MonoBehaviour
{
    private CsvWriter writer = new CsvWriter();
    private float lastLog = 0;
    public CollisionCounter collisionCounter;
    public Navigation navigation;
    
    private void Start()
    {
        writer.filename = System.DateTime.Now.ToString("MM-dd-HH-mm-ss") + "-log";
        writer.addColnames(new List<string>(){"time","nb_etapes_parcours","nb_collisions"});
    }

    private void Update()
    {
        // log every 1sec
        if (Time.time - lastLog > 1)
        {
            writer.record(new List<string>()
            {
                Time.time.ToString(),
                this.navigation.counter.ToString(),
                this.collisionCounter.collisionCounter.ToString()
            });

            lastLog = Time.time;
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.IO;


public class TestLog:MonoBehaviour
{
    private CsvWriter writer = new CsvWriter();
    private float lastLog = 0;
    
    private void Start()
    {
        writer.filename = System.DateTime.Now.ToString("MM-dd-HH-mm-ss") + "-log";
        writer.addColnames(new List<string>(){"time","pos_x","pos_y","pos_z"});
    }

    private void Update()
    {
        // log every 1sec
        if (Time.time - lastLog > 1)
        {
            writer.record(new List<string>()
            {
                Time.time.ToString(),
                transform.position.x.ToString(),
                transform.position.y.ToString(),
                transform.position.z.ToString()
            });

            lastLog = Time.time;
        }

    }
    
    private void OnApplicationQuit()
    {
        writer.record(new List<string>()
        {
            Time.time.ToString()
        });
    }
}

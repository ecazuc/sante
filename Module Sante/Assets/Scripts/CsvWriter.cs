using System;
using System.Collections.Generic;
using System.Drawing.Text;
using UnityEngine;
using System.IO;
using System.Text;

public class CsvWriter : MonoBehaviour
{
     
     public string path = "/Resources/";
     public string filename = "expe";
     public int idUsager = 1;
     
     StringBuilder sb = new System.Text.StringBuilder();

     public void addColnames(List<String> colnames)
     {
          sb.AppendLine(String.Join(";", colnames));
     }
     
     public void record(List<String> values)
     {
          sb.AppendLine(String.Join(";", values));
          SaveToFile(sb.ToString());
     }
     
     public void SaveToFile(string content)
     {
          var folder = Application.dataPath + path;
          if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);


          var filePath = Path.Combine(folder, filename + ".csv");

          using (var writer = new StreamWriter(filePath, true))
          {
               writer.Write(content);
          }
     }
     
     //record the information provided during the session
     private void OnDestroy()
     {
          if (Navigation.succes)
          {
               record(new List<string>()
               {
                    idUsager.ToString(),
                    //menu.idUsager,
                    "Succes",
                    Navigation.timeTotal.ToString(),
                    CollisionCounter.collisionCounter.ToString()
               });
          }
          else
          {
               record(new List<string>()
               {
                    idUsager.ToString(),
                    //menu.idUsager,
                    "Echec",
                    "-1",
                    CollisionCounter.collisionCounter.ToString()
               });
          }
     }
}

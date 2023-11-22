using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class CsvWriter
{
     public string path = "/Resources/";
     public string filename = "log";
     
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

          using (var writer = new StreamWriter(filePath, false))
          {
               writer.Write(content);
          }
     }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Dornell.Geography {
    public class IgnMnt {
        private readonly string dataDirectory;
        private readonly string dataFileTemplate;
        private readonly Dictionary<(int, int), float[]> mntData;
        private const int tileSize = 1000;  // tiles are squared
        private const float nodata = -99999f;
        private readonly Lambert93 l93;

        public IgnMnt(in string dataDirectory, in string dataFileTemplate) {
            this.dataDirectory = dataDirectory;
            this.dataFileTemplate = dataFileTemplate;
            this.mntData = new Dictionary<(int, int), float[]>();
            this.l93 = new Lambert93();
        }

        public bool TryGetAltitudeFromGeographic(float lat, float lon, out float altitude) {
                (double x, double y) = l93.GeographicToCartesian(lat, lon);
                return TryGetAltitudeFromCartesian(Convert.ToInt32(x), Convert.ToInt32(y), out altitude);
        }

        public bool TryGetAltitudeFromCartesian(int x, int y, out float altitude) {
            altitude = 0;
            (int x, int y) index = (x / tileSize, (y - 1) / tileSize + 1);
            int column = x - index.x * tileSize;
            int row = index.y * tileSize - y;
            if (!mntData.ContainsKey(index)) {
                if (!LoadDataFile(index))
                    return false;
            }
            altitude = mntData[index][row * tileSize + column];
            return altitude != nodata;
        }

        private bool LoadDataFile((int x, int y) index) {
            string filename = Path.Combine(dataDirectory, String.Format(dataFileTemplate, index.x, index.y));
            try {
                using (StreamReader sr = new StreamReader(filename)) {
                    float[] tileData = new float[tileSize * tileSize];
                    string[] lineData;
                    string nodatavalue = "";
                    // read header (6 lines)
                    for (int i = 0; i < 6; ++i) {
#if NETSTANDARD2_1
                        lineData = sr.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
#else
                        lineData = sr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
#endif
                        switch (lineData[0].ToLower()) {
                            case "ncols":
                            case "nrows":
                                if (Convert.ToInt32(lineData[1]) != tileSize) {
                                    Console.WriteLine($"Unexpected tile size in {filename}");
                                    return false;
                                }
                                break;
                            case "xllcorner":
                                float value = Convert.ToSingle(lineData[1], CultureInfo.InvariantCulture);
                                value += 0.5f;
                                int iv = Convert.ToInt32(value);
                                if (Convert.ToInt32(Convert.ToSingle(lineData[1], CultureInfo.InvariantCulture) + 0.5f) != index.x * tileSize) {
                                    Console.WriteLine($"Unexpected xll corner value in {filename}");
                                    return false;
                                }
                                break;
                            case "yllcorner":
                                if (Convert.ToInt32(Convert.ToSingle(lineData[1], CultureInfo.InvariantCulture) - 0.5f) != (index.y - 1) * tileSize) {
                                    Console.WriteLine($"Unexpected xll corner value in {filename}");
                                    return false;
                                }
                                break;
                            case "cellsize":
                                if (Convert.ToSingle(lineData[1], CultureInfo.InvariantCulture) != 1.0f) {
                                    Console.WriteLine($"Unexpected cellsize value in {filename}");
                                    return false;
                                }
                                break;
                            case "nodata_value":
                                nodatavalue = lineData[1];
                                break;
                        }
                    }

                    // read data
                    int dataIndex = 0;
                    for (int y = 0; y < tileSize; ++y) {
#if NETSTANDARD2_1
                        lineData = sr.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
#else
                        lineData = sr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
#endif
                        for (int x = 0; x < tileSize; ++x) {
                            tileData[dataIndex++] = lineData[x].Equals(nodatavalue) ? nodata : Convert.ToSingle(lineData[x], CultureInfo.InvariantCulture);
                        }
                    }
                    mntData.Add(index, tileData);
                }
                return true;
            } catch (Exception ex) {
                Console.WriteLine($"Unable to load data from {filename} : {ex.Message}");
                return false;
            }
        }
    }

}

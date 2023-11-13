using System;
using System.IO;

namespace Dornell.Geography {
    public class HeightMap {

        public float[] altitudes;
        public int x0, x1, y0, y1;

        public int width;
        public int height;

        // x and y are Cartesian coordinates (for example from Lambert93 projection)
        public bool TryGetAltitude(in int x, in int y, out float altitude) {
            if (x0 <= x && x <= y1 && y0 <= y && y <= y1) {
                altitude = altitudes[(y - y0) * width + (x - x0)];
                return true;
            }
            altitude = 0;
            return false;
        }

        public void SetBounds(int x0, int y0, int x1, int y1) {
            this.x0 = x0;
            this.y0 = y0;
            this.x1 = x1;
            this.y1 = y1;
            UpdateSize();
        }

        public bool LoadFromFile(string filename) {
            try {
                using (FileStream fs = new FileStream(filename, FileMode.Open)) {
                    using (BinaryReader br = new BinaryReader(fs)) {
                        Load(br);
                    }
                }
                return true;
            } catch (Exception ex) {
                Console.WriteLine($"Error loading file {filename} : {ex.Message}");
                return false;
            }
        }

        public bool SaveToFile(string filename) {
            try {
                using (FileStream fs = new FileStream(filename, FileMode.Create)) {
                    using (BinaryWriter bw = new BinaryWriter(fs)) {
                        Save(bw);
                        return true;
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error loading file {filename} : {ex.Message}");
                return false;
            }
        }

        public void Load(BinaryReader br) {
            x0 = br.ReadInt32();
            y0 = br.ReadInt32();
            x1 = br.ReadInt32();
            y1 = br.ReadInt32();

            UpdateSize();

            for (int i = 0; i < width * height; ++i) {
                altitudes[i] = br.ReadSingle();
            }
        }

        public void Save(BinaryWriter bw) {
            bw.Write(x0);
            bw.Write(y0);
            bw.Write(x1);
            bw.Write(y1);
            for (int i = 0; i < width * height; ++i) {
                bw.Write(altitudes[i]);
            }
        }


        private void UpdateSize() {
            width = x1 - x0 + 1;
            height = y1 - y0 + 1;
            altitudes = new float[width * height];
        }

    }
}

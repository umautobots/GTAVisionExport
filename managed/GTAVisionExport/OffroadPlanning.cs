using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using GTAVisionUtils;

namespace GTAVisionExport {
    public class OffroadPlanning : Script {
//        constant for tried and sufficient offroad model
        public static VehicleHash OffroadModel = VehicleHash.Contender;
        private bool showOffroadAreas = false;

        public List<List<Rect>> areas;
        
        public OffroadPlanning() {
            UI.Notify("Loaded OffroadPlanning.cs");
            
            // attach time methods 
            Tick += OnTick;
            KeyUp += OnKeyUp;
            areas = new List<List<Rect>>();
            createOffroadAreas();
        }

        private void createOffroadAreas() {
            var area1 = new List<Rect>();
            area1.Add(new Rect(1400, -2650, 800, 1750));
            var area2 = new List<Rect>();
            area2.Add(new Rect(1624, -418, 770, 1410));
            area2.Add(new Rect(1750, 992, 300, 410));
            var area3 = new List<Rect>();
            area3.Add(new Rect(2180, 1300, 300, 1410));
            area3.Add(new Rect(1980, 1700, 200, 770));
            area3.Add(new Rect(2060, 1500, 120, 200));
            area3.Add(new Rect(2060, 2470, 120, 100));
            var area4 = new List<Rect>();
            area4.Add(new Rect(2620, 1800, 350, 780));
            area4.Add(new Rect(2970, 2180, 250, 400));
            area4.Add(new Rect(2600, 2580, 750, 450));
            area4.Add(new Rect(3350, 2850, 220, 300));
            var area5 = new List<Rect>();
            area5.Add(new Rect(150, 1400, 1500, 980));
            area5.Add(new Rect(150, 2380, 1300, 220));
            var area6 = new List<Rect>();
            area6.Add(new Rect(300, 2720, 1200, 700));
            area6.Add(new Rect(1500, 2950, 230, 470));
            var area7 = new List<Rect>();
//            todo: dodělat
            area7.Add(new Rect(2300, 3100, 300, 500));
            areas.Add(area1);
            areas.Add(area2);
            areas.Add(area3);
            areas.Add(area4);
            areas.Add(area5);
            areas.Add(area6);
            areas.Add(area7);
        }
        // Test vehicle controls 
        private void OnKeyUp(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Subtract:
                    UI.Notify("Pressed numpad -");
                    showOffroadAreas = !showOffroadAreas;
                    if (showOffroadAreas) {
                        UI.Notify("enabled offroad areas showing");
                    }
                    else {
                        UI.Notify("disabled offroad areas showing");
                    }

                    break;
            }
        }

        public void OnTick(object sender, EventArgs e) {
            if (showOffroadAreas) {
                DrawOffroadAreas();
            }
//            drawAxesBoxesAround(new Vector3(-1078f, -216f, 200f));
        }

        public void DrawOffroadAreas() {
            foreach (var area in areas) {
                foreach (var rect in area) {
                    HashFunctions.Draw3DBox(
                        new Vector3((float) (rect.X + rect.Width/2), (float) (rect.Y + rect.Height/2), 0),
                        new Vector3((float) rect.Width, (float) rect.Height, 400), 
                        255, 255, 255, 50);
                }
            }
        }
        
    }
}

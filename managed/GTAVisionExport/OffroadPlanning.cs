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
            areas.Add(area1);
            areas.Add(area2);
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

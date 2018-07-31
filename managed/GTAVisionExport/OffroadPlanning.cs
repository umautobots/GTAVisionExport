using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using GTAVisionUtils;
using VAutodrive;

namespace GTAVisionExport {
    public class OffroadPlanning : Script {
//        constant for tried and sufficient offroad model
        public static VehicleHash OffroadModel = VehicleHash.Contender;
        private bool showOffroadAreas;

        public List<List<Rect>> areas;
        private Random rnd;

        private bool offroadDrivingStarted;
        private bool currentlyDrivingToTarget;
        private Vector2 currentTarget;
        private int targetsFromSameStart = 0;
        private List<Rect> currentArea = null;
        
        public OffroadPlanning() {
            UI.Notify("Loaded OffroadPlanning.cs");
            
            // attach time methods 
            Tick += OnTick;
            KeyUp += OnKeyUp;
            areas = new List<List<Rect>>();
            rnd = new Random();
            CreateOffroadAreas();
        }

        private void CreateOffroadAreas() {
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
            area5.Add(new Rect(-540, 2120, 690, 500));
            var area6 = new List<Rect>();
            area6.Add(new Rect(300, 2720, 1200, 700));
            area6.Add(new Rect(1500, 2950, 230, 470));
            var area7 = new List<Rect>();
            area7.Add(new Rect(2300, 3100, 300, 700));
            var area8 = new List<Rect>();
            area8.Add(new Rect(-570, 4700, 410, 950));
            area8.Add(new Rect(-820, 4700, 250, 740));
            area8.Add(new Rect(-690, 5440, 120, 80));
            area8.Add(new Rect(-1100, 4700, 280, 600));
            area8.Add(new Rect(-1270, 4700, 170, 520));
            area8.Add(new Rect(-1470, 4700, 200, 300));
            area8.Add(new Rect(-1600, 4700, 130, 170));
            area8.Add(new Rect(-1700, 4700, 100, 90));
            area8.Add(new Rect(-1700, 4550, 300, 150));
            areas.Add(area1);
            areas.Add(area2);
            areas.Add(area3);
            areas.Add(area4);
            areas.Add(area5);
            areas.Add(area6);
            areas.Add(area7);
            areas.Add(area8);
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
                case Keys.Pause:
                    UI.Notify("Pressed Pause/Break");
                    offroadDrivingStarted = !offroadDrivingStarted;
                    if (offroadDrivingStarted) {
                        UI.Notify("offroad driving enabled");
                    }
                    else {
                        UI.Notify("offroad driving disabled");
                    }

                    break;
            }
        }

        public void checkDrivingToTarget() {
            if (Game.Player.Character.CurrentVehicle.Position.DistanceTo2D(new Vector3(currentTarget.X, currentTarget.Y, 0)) < 2) {
                currentlyDrivingToTarget = false;
            }
        }

        public void setNextTarget() {
            if (currentlyDrivingToTarget) {
                return;
            }
            
//            setting the new start in new area
            var targetsPerArea = 10;
            if (targetsPerArea < targetsFromSameStart || currentArea == null) {
                currentArea = GetRandomArea();
                var startRect = GetRandomRect(currentArea);
                var start = GetRandomPoint(startRect);
                var startZ = World.GetGroundHeight(new Vector2(start.X, start.Y));
                Game.Player.Character.CurrentVehicle.Position = new Vector3(start.X, start.Y, startZ);
                targetsFromSameStart = 0;
            }
            
//                firstly, select some area and for a while, perform random walk in that area, then randomly selct other area
//                at first, I'll randomly sample rectangle from area, then randomly sample point from that rectangle
//                sampling rectangles is in ratio corresponsing to their sizes, so smaller rectangle is not sampled more often
                
            var targetRect = GetRandomRect(currentArea);
            var target = GetRandomPoint(targetRect);
            DriveToPoint(target);

            currentlyDrivingToTarget = true;
            currentTarget = target;
            targetsFromSameStart += 1;
        }

        private void SetTargetAsWaypoint(Vector2 target) {
            HashFunctions.SetNewWaypoint(target);
        }

        private void DriveToPoint(Vector2 target) {
            SetTargetAsWaypoint(target);
            var kh = new KeyHandling();
            var inf = kh.GetType().GetMethod("AtToggleAutopilot", BindingFlags.NonPublic | BindingFlags.Instance);
            inf.Invoke(kh, new object[] {new KeyEventArgs(Keys.J)});
        }
        
        private List<Rect> GetRandomArea() {
            return areas[rnd.Next(areas.Count)];
        }

        private Rect GetRandomRect(List<Rect> area) {
            var volumes = (List<int>) (from rect in area select rect.Width * rect.Height);    //calculating volumes
            var sum = 0;
            var rectIdx = MathUtils.digitize(rnd.Next(volumes.Sum()), MathUtils.cumsum(volumes));
            return area[rectIdx];
        }

        private Vector2 GetRandomPoint(Rect rect) {
            return new Vector2(rnd.Next((int) (rect.Width)), rnd.Next((int) rect.Height));
        }
        
        public void OnTick(object sender, EventArgs e) {
            if (showOffroadAreas) {
                DrawOffroadAreas();
            }

            if (offroadDrivingStarted) {
                checkDrivingToTarget();
                setNextTarget();
            }
        }

        public void DrawOffroadAreas() {
            foreach (var area in areas) {
                foreach (var rect in area) {
                    HashFunctions.Draw3DBox(
                        new Vector3((float) (rect.X + rect.Width/2), (float) (rect.Y + rect.Height/2), 0),
                        new Vector3((float) rect.Width, (float) rect.Height, 500), 
                        255, 255, 255, 50);
                }
            }
        }
        
    }
}

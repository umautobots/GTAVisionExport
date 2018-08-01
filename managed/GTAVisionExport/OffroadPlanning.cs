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
    // all driving and path planning related things are static and called from VisionExport script which behaves as controlling point
    // it makes coordination and switching between onroad and offroad easier
    public class OffroadPlanning : Script {
//        constant for tried and sufficient offroad model
        public static VehicleHash OffroadModel = VehicleHash.Contender;
        private bool showOffroadAreas;

        public static List<List<Rect>> areas;
        private static Random rnd;

        public static bool offroadDrivingStarted;
        private static bool currentlyDrivingToTarget;
        private static Vector2 currentTarget;
        private static int targetsFromSameStart = 0;
        private static List<Rect> currentArea = null;
        
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
                        VisionExport.drivingOffroad = true;
                    }
                    else {
                        UI.Notify("offroad driving disabled");
                        VisionExport.drivingOffroad = false;
                    }

                    break;
            }
        }

        public static void checkDrivingToTarget() {
            if (Game.Player.Character.CurrentVehicle.Position.DistanceTo2D(new Vector3(currentTarget.X, currentTarget.Y, 0)) < 2) {
                currentlyDrivingToTarget = false;
            }
        }

        /// <summary>
        /// behold, this function wraps most of fuckups that GetGroundHeight causes,
        /// teleports player multiple times and perform waiting, call only when you know what your're doing
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static float GetGroundHeightMagic(Vector2 position) {
//            for distant position from player, which are not bufferent, function returns 0, so we need to teleport
//            above that position and let player fall, in the meantime texture loads properly and getting ground height starts 
//            working. Raycasting returns exactly same results, so it seems that it's used as an implementation of this function
//            when the player is falling on the exact same x,y location, this returns player's position as ground
//            that is heavy evidence for thinking raycasting from above is used in that function. That's why here is +5, +5
//            for player position. For each position, height in which it starts working, differs, so that's why I tryit in cycle
//            and check when it returns nonzero, and thus correct result
            for (int i = 900; i > 100; i-= 50) {
//                    when I use the same position, the GetGroundHeight call takes coordinates of player as ground height
                Game.Player.Character.Position = new Vector3(position.X + 5, position.Y + 5, i);
                //just some more waiting for textures to load
                Script.Wait(500);
                var startZ = World.GetGroundHeight(new Vector2(position.X, position.Y));
                if (startZ != 0) {
                    return startZ;
                }
            }

            throw new Exception("height measurement is fucked up somehow, aborting");
        }
        
        public static void setNextStart() {
            Logger.WriteLine($"setting the next start");
            currentArea = GetRandomArea();
            var startRect = GetRandomRect(currentArea);
            var start = GetRandomPoint(startRect);
            var startZ = GetGroundHeightMagic(start);
            Game.Player.Character.IsInvincible = true;
            Logger.WriteLine($"{startZ} is ground height of {start}");
            var newPosition = new Vector3(start.X, start.Y, startZ + 2);    // so we have some reserve, when setting to ground z coord, it falls through
            if (Game.Player.Character.IsInVehicle()) {
                Game.Player.Character.CurrentVehicle.Position = newPosition;
            }
            else {
                Game.Player.Character.Position = newPosition;
            }
            targetsFromSameStart = 0;            
            Logger.WriteLine($"setting next start in {newPosition}");
        }
        
        public static void setNextTarget() {
            if (currentlyDrivingToTarget) {
                return;
            }
            
//            setting the new start in new area after some number of targets from same start
            var targetsPerArea = 10;
            if (targetsPerArea < targetsFromSameStart || currentArea == null) {
                setNextStart();
            }
            
//                firstly, select some area and for a while, perform random walk in that area, then randomly selct other area
//                at first, I'll randomly sample rectangle from area, then randomly sample point from that rectangle
//                sampling rectangles is in ratio corresponsing to their sizes, so smaller rectangle is not sampled more often
                
            var targetRect = GetRandomRect(currentArea);
            var target = GetRandomPoint(targetRect);
            Logger.WriteLine($"setting next target in {target}");
            DriveToPoint(target);

            currentlyDrivingToTarget = true;
            currentTarget = target;
            targetsFromSameStart += 1;
        }

        private static void SetTargetAsWaypoint(Vector2 target) {
            HashFunctions.SetNewWaypoint(target);
        }

        private static void DriveToPoint(Vector2 target) {
            SetTargetAsWaypoint(target);
            var kh = new KeyHandling();
            var inf = kh.GetType().GetMethod("AtToggleAutopilot", BindingFlags.NonPublic | BindingFlags.Instance);
            inf.Invoke(kh, new object[] {new KeyEventArgs(Keys.J)});
        }
        
        public static List<Rect> GetRandomArea() {
            var areaIdx = rnd.Next(areas.Count);
            Logger.WriteLine($"randomly selected area index {areaIdx} from {areas.Count} areas");
            var area = areas[areaIdx];
            Logger.WriteLine($"selected area: {string.Join(", ", area)}, with size {area.Count}");
            return area;
        }

        public static Rect GetRandomRect(List<Rect> area) {
            Logger.WriteLine($"selecting random rect for area with size {area.Count}");
            Logger.ForceFlush();
            var volumes = new List<int>(area.Count);
            for (var i = 0; i < area.Count; i++) {
                volumes.Add((int) (area[i].Width * area[i].Height));
            }
            var sum = 0;
            var rectIdx = MathUtils.digitize(rnd.Next(volumes.Sum()), MathUtils.cumsum(volumes));
            return area[rectIdx];
        }

        public static Vector2 GetRandomPoint(Rect rect) {
            return new Vector2((float) (rect.X + rnd.Next((int) (rect.Width))), (float) (rect.Y + rnd.Next((int) rect.Height)));
        }
        
        public void OnTick(object sender, EventArgs e) {
            if (showOffroadAreas) {
                DrawOffroadAreas();
            }

            // driving and planning related things are in VisionExport
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

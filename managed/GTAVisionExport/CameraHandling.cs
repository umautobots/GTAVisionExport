using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Native;
using GTA.Math;
using GTA.NaturalMotion;
using GTAVisionUtils;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace GTAVisionExport {
// Controls: 
// P - mounts rendering camera on vehicle 
// O - restores the rendering camera to original control

    public class CameraHandling : Script {
        // camera used on the vehicle
        private Camera activeCamera;
        private bool enabled = false;
        private int activeCameraIndex = -1;
        private bool showCameras = false;

        public CameraHandling() {
            UI.Notify("Loaded CameraHandling.cs");

            // create a new camera 
//            World.DestroyAllCameras();
            
            // attach time methods 
            Tick += OnTick;
            KeyUp += onKeyUp;
        }

        // Function used to take control of the world rendering camera.
        public void mountCameraOnVehicle() {
            UI.Notify("Mounting camera to the vehicle.");
            if (Game.Player.Character.IsInVehicle()) {
                if (activeCameraIndex == -1) {
                    UI.Notify("Mounting main camera");
                    CamerasList.ActivateMainCamera();
                }
                else {
                    UI.Notify("Mounting camera from list");
                    UI.Notify("My current rotation: " + Game.Player.Character.CurrentVehicle.Rotation);
                    Logger.WriteLine("My current rotation: " + Game.Player.Character.CurrentVehicle.Rotation);
                    activeCamera = CamerasList.ActivateCamera(activeCameraIndex);
                }
            }
            else {
                UI.Notify("Please enter a vehicle.");
            }
        }

        // Function used to allows the user original control of the camera.
        public void restoreCamera() {
            UI.Notify("Relinquishing control");
            CamerasList.Deactivate();
        }

        // Function used to keep camera on vehicle and facing forward on each tick step.
        public void keepCameraOnVehicle() {
            if (Game.Player.Character.IsInVehicle() && enabled) {
                // keep the camera in the same position relative to the car
                CamerasList.mainCamera.AttachTo(Game.Player.Character.CurrentVehicle, CamerasList.mainCameraPosition);

                // rotate the camera to face the same direction as the car 
                CamerasList.mainCamera.Rotation = Game.Player.Character.CurrentVehicle.Rotation;
            }
        }

        public void doRayCasting() {
            var result = GTA.World.Raycast(Game.Player.Character.Position,
                Game.Player.Character.Position + (Vector3.RelativeLeft * 100), IntersectOptions.Everything);
            UI.Notify("raycast result:");
            Logger.WriteLine("raycast result:");
            UI.Notify(result.DitHitAnything.ToString());
            Logger.WriteLine(result.DitHitAnything.ToString());
            if (result.DitHitAnything) {
                UI.Notify(result.HitCoords.ToString());        
                Logger.WriteLine(result.HitCoords.ToString());
                GTA.World.DrawMarker(MarkerType.CheckeredFlagCircle, result.HitCoords, Vector3.RelativeRight, Vector3.WorldUp, new Vector3(10, 10, 10), Color.Chartreuse);
            }
        }
        
        // Test vehicle controls 
        private void onKeyUp(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.B:
                    doRayCasting();
                    break;
                case Keys.P:
                    activeCameraIndex = -1;
                    mountCameraOnVehicle();
                    enabled = true;
                    break;
                case Keys.O:
                    restoreCamera();
                    enabled = false;
                    break;

//            UI.Notify("keycode is:" + e.KeyCode);
                case Keys.Add:
                    UI.Notify("Pressed numpad +");
                    showCameras = !showCameras;
                    UI.Notify("there are " + CamerasList.camerasPositions.Count + " cameras");
                    if (showCameras) {
                        UI.Notify("enabled cameras showing");
                    }
                    else {
                        UI.Notify("disabled cameras showing");
                    }

                    break;
                case Keys.NumPad0:
                    UI.Notify("Pressed numpad 0");
                    activeCameraIndex = 0;
                    mountCameraOnVehicle();
                    break;
                case Keys.NumPad1:
                    UI.Notify("Pressed numpad 1");
                    activeCameraIndex = 1;
                    mountCameraOnVehicle();
                    break;
                case Keys.NumPad2:
                    UI.Notify("Pressed numpad 2");
                    activeCameraIndex = 2;
                    mountCameraOnVehicle();
                    break;
                case Keys.NumPad3:
                    UI.Notify("Pressed numpad 3");
                    activeCameraIndex = 3;
                    mountCameraOnVehicle();
                    break;
                case Keys.Decimal:
                    UI.Notify("Pressed numpad ,");
                    activeCameraIndex = -1;
                    mountCameraOnVehicle();
                    break;
            }
        }

        public void OnTick(object sender, EventArgs e) {
            keepCameraOnVehicle();
            if (showCameras) {
                drawCamerasBoxes();
            }
//            drawAxesBoxesAround(new Vector3(-1078f, -216f, 200f));
        }

        public void drawCamerasBoxes() {
//            this shows white boxes where cameras are
            foreach (var camPos in CamerasList.camerasPositions) {
                var curVehicle = Game.Player.Character.CurrentVehicle;
//                Logger.writeLine("rotation");
//                Logger.writeLine(curVehicle.Rotation);
//                Logger.writeLine("forward vector");
//                Logger.writeLine(curVehicle.ForwardVector);
//                Logger.writeLine("right vector");
//                Logger.writeLine(curVehicle.RightVector);
//                Logger.writeLine("up vector");
//                Logger.writeLine(curVehicle.UpVector);
//                var camPosToCar = Vector3.Modulate(curVehicle.ForwardVector, cameraPosition);
//                camPosToCar += Vector3.Modulate(curVehicle.RightVector, cameraPosition);
//                camPosToCar += Vector3.Modulate(curVehicle.UpVector, cameraPosition);
                var rot = curVehicle.Rotation;
                var rotX = Matrix3D.RotationAroundXAxis(Angle.FromDegrees(rot.X));
                var rotY = Matrix3D.RotationAroundYAxis(Angle.FromDegrees(rot.Y));
                var rotZ = Matrix3D.RotationAroundZAxis(Angle.FromDegrees(rot.Z));
                var rotMat = rotX * rotY * rotZ;
                var camPosToCar = rotMat * new Vector3D(camPos.X, camPos.Y, camPos.Z);
                var absolutePosition = curVehicle.Position + new Vector3((float) camPosToCar[0], (float) camPosToCar[1], (float) camPosToCar[2]);
                HashFunctions.Draw3DBox(absolutePosition, new Vector3(0.3f, 0.3f, 0.3f));
            }
        }
        
        void drawAxesBoxesAround(Vector3 position) {
            var dist = 10;
            var vectors = new[] {
                new Vector3(dist, 0, 0), new Vector3(-dist, 0, 0), // x, pos and neg
                new Vector3(0, dist, 0), new Vector3(0, -dist, 0), // y, pos and neg 
                new Vector3(0, 0, dist), new Vector3(0, 0, -dist), // z, pos and neg
            };
            var colors = new[] {
                new Vector3(255, 0, 0), new Vector3(255, 180, 180), // x, pos and neg
                new Vector3(0, 255, 0), new Vector3(180, 255, 180), // y, pos and neg 
                new Vector3(0, 0, 255), new Vector3(180, 180, 255), // z, pos and neg
            };
            for (int i = 0; i < vectors.Length; i++) {
                var relativePos = vectors[i];
                var color = colors[i];
                var absolutePosition = position + new Vector3((float) relativePos[0], (float) relativePos[1], (float) relativePos[2]);
                HashFunctions.Draw3DBox(absolutePosition, new Vector3(0.3f, 0.3f, 0.3f), 
                    (byte) colors[i][0], (byte) colors[i][1], (byte) colors[i][2]);
            }
        }
    }
}
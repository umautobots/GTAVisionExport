using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Native;
using GTA.Math;
using GTAVisionUtils;

namespace GTAVisionExport {
// Controls: 
// P - mounts rendering camera on vehicle 
// O - restores the rendering camera to original control

    public class CameraHandling : Script {
        // camera used on the vehicle
        private Camera activeCamera;
        private bool enabled = false;
        private int activeCameraIndex = -1;

        public CameraHandling() {
            UI.Notify("Loaded TestVehicle.cs");

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
                    Logger.writeLine("My current rotation: " + Game.Player.Character.CurrentVehicle.Rotation);
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

        // Test vehicle controls 
        private void onKeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.P) {
                activeCameraIndex = -1;
                mountCameraOnVehicle();
                enabled = true;
            }

            if (e.KeyCode == Keys.O) {
                restoreCamera();
                enabled = false;
            }

            if (e.KeyCode == Keys.NumPad0) {
                UI.Notify("Pressed numpad 0");
                activeCameraIndex = 0;
                mountCameraOnVehicle();
            }

            if (e.KeyCode == Keys.NumPad1) {
                UI.Notify("Pressed numpad 1");
                activeCameraIndex = 1;
                mountCameraOnVehicle();
            }

            if (e.KeyCode == Keys.NumPad2) {
                UI.Notify("Pressed numpad 2");
                activeCameraIndex = 2;
                mountCameraOnVehicle();
            }

            if (e.KeyCode == Keys.NumPad3) {
                UI.Notify("Pressed numpad 3");
                activeCameraIndex = 3;
                mountCameraOnVehicle();
            }

            if (e.KeyCode == Keys.Decimal) {
                UI.Notify("Pressed numpad ,");
                activeCameraIndex = -1;
                mountCameraOnVehicle();
            }
        }

        void OnTick(object sender, EventArgs e) {
            keepCameraOnVehicle();
        }
    }
}
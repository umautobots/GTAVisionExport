using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Native;
using GTA.Math;

namespace GTAVisionExport
{
    
// Controls: 
// P - mounts rendering camera on vehicle 
// O - restores the rendering camera to original control

    public class CameraHandling : Script
    {
        // camera used on the vehicle
        Camera mainCamera = null;
        Camera[] cameras = null;
        private bool enabled = false;
        private int activeCameraIndex = -1;

        public CameraHandling()
        {
            UI.Notify("Loaded TestVehicle.cs");

            // create a new camera 
//            World.DestroyAllCameras();
            mainCamera = World.CreateCamera(new Vector3(), new Vector3(), 50);
            mainCamera.IsActive = false;

            // attach time methods 
            Tick += OnTick;
            KeyUp += onKeyUp;
        }

        // Function used to take control of the world rendering camera.
        public void mountCameraOnVehicle()
        {
            if (Game.Player.Character.IsInVehicle())
            {
//                void RENDER_SCRIPT_CAMS(BOOL render, BOOL ease, int easeTime, BOOL p3,
//                    BOOL p4) // 0x07E5B515DB0636FC 0x74337969
                if (activeCameraIndex == -1)
                {
                    World.RenderingCamera = mainCamera;                    
                }
                else
                {
                    World.RenderingCamera = cameras[activeCameraIndex];
                }
            }
            else
            {
                UI.Notify("Please enter a vehicle.");
            }
        }

        // Function used to allows the user original control of the camera.
        public void restoreCamera()
        {
            UI.Notify("Relinquishing control");
            mainCamera.IsActive = false;
            World.RenderingCamera = mainCamera;
        }

        // Function used to keep camera on vehicle and facing forward on each tick step.
        public void keepCameraOnVehicle()
        {
            if (Game.Player.Character.IsInVehicle() && enabled)
            {
                // keep the camera in the same position relative to the car
                mainCamera.AttachTo(Game.Player.Character.CurrentVehicle, CamerasList.mainCamera.Value);

                // rotate the camera to face the same direction as the car 
                mainCamera.Rotation = Game.Player.Character.CurrentVehicle.Rotation;
            }

            if (Game.Player.Character.IsInVehicle() && enabled)
            {
                for (int i = 0; i < cameras.Length; i++)
                {
                    CamerasList.ActivateCamera(i);
                }
            }
        }

        // Test vehicle controls 
        private void onKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.P)
            {
                activeCameraIndex = -1;
                mountCameraOnVehicle();
                enabled = true;
            }

            if (e.KeyCode == Keys.O)
            {
                restoreCamera();
                enabled = false;
            }

            if (e.KeyCode == Keys.NumPad0)
            {
                activeCameraIndex = 0;
                mountCameraOnVehicle();
            }

            if (e.KeyCode == Keys.NumPad1)
            {
                activeCameraIndex = 1;
                mountCameraOnVehicle();
            }

            if (e.KeyCode == Keys.NumPad2)
            {
                activeCameraIndex = 2;
                mountCameraOnVehicle();
            }

            if (e.KeyCode == Keys.NumPad3)
            {
                activeCameraIndex = 3;
                mountCameraOnVehicle();
            }

            if (e.KeyCode == Keys.Decimal)
            {
                activeCameraIndex = -1;
                mountCameraOnVehicle();
            }

        }

        void OnTick(object sender, EventArgs e)
        {
            keepCameraOnVehicle();
        }
    }
}
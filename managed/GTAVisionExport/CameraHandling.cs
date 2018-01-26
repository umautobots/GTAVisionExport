using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
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
        Camera camera = null;

        public CameraHandling()
        {
            UI.Notify("Loaded TestVehicle.cs");

            // create a new camera 
            World.DestroyAllCameras();
            camera = World.CreateCamera(new Vector3(), new Vector3(), 50);
            camera.IsActive = true;
            GTA.Native.Function.Call(Hash.RENDER_SCRIPT_CAMS, false, true, camera.Handle, true, true);

            // attach time methods 
            Tick += OnTick;
            KeyUp += onKeyUp;
        }

        // Function used to take control of the world rendering camera.
        public void mountCameraOnVehicle()
        {
            if (Game.Player.Character.IsInVehicle())
            {
                GTA.Native.Function.Call(Hash.RENDER_SCRIPT_CAMS, true, true, camera.Handle, true, true);
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
            GTA.Native.Function.Call(Hash.RENDER_SCRIPT_CAMS, false, false, camera.Handle, true, true);
        }

        // Function used to keep camera on vehicle and facing forward on each tick step.
        public void keepCameraOnVehicle()
        {
            if (Game.Player.Character.IsInVehicle())
            {
                // keep the camera in the same position relative to the car
                camera.AttachTo(Game.Player.Character.CurrentVehicle, new Vector3(0f, 2f, 0.4f));

                // rotate the camera to face the same direction as the car 
                camera.Rotation = Game.Player.Character.CurrentVehicle.Rotation;
            }
        }

        // Test vehicle controls 
        private void onKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.P)
            {
                mountCameraOnVehicle();
            }

            if (e.KeyCode == Keys.O)
            {
                restoreCamera();
            }
        }

        void OnTick(object sender, EventArgs e)
        {
            keepCameraOnVehicle();
        }
    }
}
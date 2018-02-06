using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;

namespace GTAVisionExport {
    public static class CamerasList
    {
        public static Vector3? mainCamera { get; set; } = null;
        public static List<Camera> cameras { get; set; } = new List<Camera>();
        public static List<Vector3> camerasPositions { get; set; } = new List<Vector3>();
        public static List<Vector3> camerasRotations { get; set; } = new List<Vector3>();
        private static Camera gameCam;
        
        static CamerasList()
        {
            gameCam = World.RenderingCamera;
        }

        public static void addCamera(Vector3 position, Vector3 rotation)
        {
            var newCamera = World.CreateCamera(new Vector3(), new Vector3(), GameplayCamera.FieldOfView);
            cameras.Add(newCamera);
            camerasPositions.Add(position);
            camerasRotations.Add(rotation);
        }
        
        public static void ActivateCamera(int i)
        {
            cameras[i].IsActive = true;
            World.RenderingCamera = cameras[i];
            cameras[i].AttachTo(Game.Player.Character.CurrentVehicle, camerasPositions[i]);
            cameras[i].Rotation = Game.Player.Character.CurrentVehicle.Rotation + camerasRotations[i];
        }

        public static void Deactivate()
        {
            foreach (var camera in cameras)
            {
                camera.IsActive = false;
            }
            World.RenderingCamera = gameCam;
        }
    }
}

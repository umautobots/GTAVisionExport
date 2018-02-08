using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;
using GTAVisionUtils;

namespace GTAVisionExport {
    public static class CamerasList
    {
        public static Camera mainCamera { get; private set; }
        public static Vector3 mainCameraPosition { get; private set; }
        
        public static List<Camera> cameras { get; } = new List<Camera>();
        public static List<Vector3> camerasPositions { get; } = new List<Vector3>();
        public static List<Vector3> camerasRotations { get; } = new List<Vector3>();
//        public static Camera gameCam;
        private static bool initialized = false;

        public static void initialize() {
            if (initialized) {
                return;
            }
            
            World.DestroyAllCameras();
            Logger.writeLine("destroying all cameras at the beginning, to be clear");
//            gameCam = World.RenderingCamera;

//            mainCamera.IsActive = false;

            initialized = true;
        }
        
        public static void setMainCamera(Vector3 position, float? fov = null)
        {
            if (!initialized) {
                throw new Exception("not initialized, please, call CamerasList.initialize() method before this one");
            }

            Logger.writeLine("setting main camera");
            if (!fov.HasValue)
            {
                fov = GameplayCamera.FieldOfView;
            }

            mainCamera = World.CreateCamera(new Vector3(), new Vector3(), fov.Value);
//            mainCamera.IsActive = true;
            mainCameraPosition = position;

            mainCamera.IsActive = false;
            World.RenderingCamera = null;
        }
        
        public static void addCamera(Vector3 position, Vector3 rotation, float? fov = null)
        {
            if (!initialized) {
                throw new Exception("not initialized, please, call CamerasList.initialize() method before this one");
            }

            Logger.writeLine("adding new camera");
            if (!fov.HasValue)
            {
                fov = GameplayCamera.FieldOfView;
            }

            var newCamera = World.CreateCamera(new Vector3(), new Vector3(), fov.Value);
            cameras.Add(newCamera);
            camerasPositions.Add(position);
            camerasRotations.Add(rotation);
        }

        public static void ActivateMainCamera()
        {
            if (!initialized) {
                throw new Exception("not initialized, please, call CamerasList.initialize() method before this one");
            }

            if (mainCamera == null) {
                throw new Exception("please, set main camera");
            }

            mainCamera.IsActive = true;
            World.RenderingCamera = mainCamera;
        }

        public static Camera ActivateCamera(int i)
        {
            if (!initialized) {
                throw new Exception("not initialized, please, call CamerasList.initialize() method before this one");
            }

            if (i >= cameras.Count) {
                throw new Exception("there is no camera with index " + i);
            }

            cameras[i].IsActive = true;
            World.RenderingCamera = cameras[i];
            cameras[i].AttachTo(Game.Player.Character.CurrentVehicle, camerasPositions[i]);
            var rotation = Game.Player.Character.CurrentVehicle.Rotation + camerasRotations[i];
            cameras[i].Rotation = rotation;
//            UI.Notify("new camera rotation is: " + rotation.ToString());
            Script.Wait(10);
            Logger.writeLine("new camera position is: " + World.RenderingCamera.Position.ToString());
            Logger.writeLine("new camera rotation is: " + World.RenderingCamera.Rotation.ToString());
            Logger.writeLine("new camera position offset is: " + camerasPositions[i].ToString());
            Logger.writeLine("new camera rotation offset is: " + camerasRotations[i].ToString());
            return cameras[i];
        }

        public static void Deactivate()
        {
            if (!initialized) {
                throw new Exception("not initialized, please, call CamerasList.initialize() method before this one");
            }

            mainCamera.IsActive = false;
            foreach (var camera in cameras)
            {
                camera.IsActive = false;
            }
            World.RenderingCamera = null;
        }
    }
}

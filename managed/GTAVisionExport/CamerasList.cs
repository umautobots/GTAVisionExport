﻿using System;
using System.Collections.Generic;
using System.IO;
using GTA;
using GTA.Math;
using GTA.Native;
using GTAVisionUtils;
using IniParser;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace GTAVisionExport {
    public static class CamerasList {
        public static Camera mainCamera { get; private set; }
        public static Vector3 mainCameraPosition { get; private set; }
        public static Vector3 mainCameraRotation { get; private set; }

        public static List<Camera> cameras { get; } = new List<Camera>();
        public static List<Vector3> camerasPositions { get; } = new List<Vector3>();
        public static List<Vector3> camerasRotations { get; } = new List<Vector3>();

        public static Vector3? activeCameraRotation { get; private set; } = null;
        public static Vector3? activeCameraPosition { get; private set; } = null;

        private static int? gameplayInterval = null;

//        public static Camera gameCam;
        private static bool initialized = false;

        public static void initialize() {
            if (initialized) {
                return;
            }

            World.DestroyAllCameras();
            Logger.WriteLine("destroying all cameras at the beginning, to be clear");
            var parser = new FileIniDataParser();
            var data = parser.ReadFile(Path.Combine(VisionExport.location, "GTAVision.ini"));
            gameplayInterval = Convert.ToInt32(data["MultiCamera"]["GameplayTimeAfterSwitch"]);
//            gameCam = World.RenderingCamera;

//            mainCamera.IsActive = false;

            initialized = true;
        }

        public static void setMainCamera(Vector3 position = new Vector3(), Vector3 rotation = new Vector3(), float? fov = null, float? nearClip = null) {
            if (!initialized) {
                throw new Exception("not initialized, please, call CamerasList.initialize() method before this one");
            }

            Logger.WriteLine("setting main camera");
            if (!fov.HasValue) {
                fov = GameplayCamera.FieldOfView;
            }
            if (!nearClip.HasValue) {
                nearClip = World.RenderingCamera.NearClip;
            }

            mainCamera = World.CreateCamera(position, rotation, fov.Value);
//            mainCamera = World.CreateCamera(new Vector3(), new Vector3(), fov.Value);
            mainCamera.NearClip = nearClip.Value;
//            mainCamera.IsActive = true;
            mainCameraPosition = position;
            mainCameraRotation = rotation;

            mainCamera.IsActive = false;
            World.RenderingCamera = null;
        }

        public static void addCamera(Vector3 position, Vector3 rotation, float? fov = null, float? nearClip = null) {
            if (!initialized) {
                throw new Exception("not initialized, please, call CamerasList.initialize() method before this one");
            }

            Logger.WriteLine("adding new camera");
            if (!fov.HasValue) {
                fov = GameplayCamera.FieldOfView;
            }
            if (!nearClip.HasValue) {
                nearClip = World.RenderingCamera.NearClip;
            }

            var newCamera = World.CreateCamera(new Vector3(), new Vector3(), fov.Value);
            newCamera.NearClip = nearClip.Value;
            cameras.Add(newCamera);
            camerasPositions.Add(position);
            camerasRotations.Add(rotation);
        }

        public static void ActivateMainCamera() {
            if (!initialized) {
                throw new Exception("not initialized, please, call CamerasList.initialize() method before this one");
            }

            if (mainCamera == null) {
                throw new Exception("please, set main camera");
            }

            mainCamera.IsActive = true;
            World.RenderingCamera = mainCamera;
            activeCameraRotation = mainCameraRotation;
            activeCameraPosition = mainCameraPosition;
        }

        public static void ActivateGameplayCamera() {
            if (!initialized) {
                throw new Exception("not initialized, please, call CamerasList.initialize() method before this one");
            }

            if (mainCamera == null) {
                throw new Exception("please, set main camera");
            }

            mainCamera.IsActive = false;
            World.RenderingCamera = null;
            activeCameraRotation = GameplayCamera.Rotation;
            activeCameraPosition = GameplayCamera.Position;
        }

        public static Vector3 rotationMatrixToDegrees(Matrix<double> r) {
            var sy = Math.Sqrt(r[0, 0] * r[0, 0] + r[1, 0] * r[1, 0]);

            var singular = sy < 1e-6;

            var x = 0d;
            var y = 0d;
            var z = 0d;
            if (!singular) {                
                x = Math.Atan2(r[2, 1], r[2, 2]);
                y = Math.Atan2(-r[2, 0], sy);
                z = Math.Atan2(r[1, 0], r[0, 0]);
            } else {                
                x = Math.Atan2(-r[1, 2], r[1, 1]);
                y = Math.Atan2(-r[2, 0], sy);
                z = 0;
            }

            return new Vector3((float) Angle.FromRadians(x).Degrees, (float) Angle.FromRadians(y).Degrees, (float) Angle.FromRadians(z).Degrees);
        }

        public static Camera ActivateCamera(int i) {
            if (!initialized) {
                throw new Exception("not initialized, please, call CamerasList.initialize() method before this one");
            }

            if (i >= cameras.Count) {
                throw new Exception("there is no camera with index " + i);
            }

            Game.Pause(false);
            cameras[i].IsActive = true;
            World.RenderingCamera = cameras[i];
            cameras[i].AttachTo(Game.Player.Character.CurrentVehicle, camerasPositions[i]);
//            if we want to rotate camera relatively to the car, we must do through rotation matrix multiplication, not addition
//            cameras[i].Rotation = Game.Player.Character.CurrentVehicle.Rotation + camerasRotations[i];    // this row is wrong
            var rot = Game.Player.Character.CurrentVehicle.Rotation;
            var rotX = Matrix3D.RotationAroundXAxis(Angle.FromDegrees(rot.X));
            var rotY = Matrix3D.RotationAroundYAxis(Angle.FromDegrees(rot.Y));
            var rotZ = Matrix3D.RotationAroundZAxis(Angle.FromDegrees(rot.Z));
            var rotMat = rotZ * rotY * rotX;
            var relRotX = Matrix3D.RotationAroundXAxis(Angle.FromDegrees(camerasRotations[i].X));
            var relRotY = Matrix3D.RotationAroundYAxis(Angle.FromDegrees(camerasRotations[i].Y));
            var relRotZ = Matrix3D.RotationAroundZAxis(Angle.FromDegrees(camerasRotations[i].Z));
            var relRotMat = relRotZ * relRotY * relRotX;

//            cameras[i].Rotation = rotationMatrixToDegrees(relRotMat * rotMat);
            cameras[i].Rotation = rotationMatrixToDegrees(rotMat * relRotMat);
//            WARNING: CAMERAS SETTING DO NOT WORK WHEN GAME IS PAUSED, SO WE NEED TO UNPAUSE THE GAME, SET THINGS UP, AND THEN PAUSE GAME AGAIN
//            Script.Wait(1);
// //with time 1, sometimes depth does not correspond, and bounding boxes dont correspond by 1 frames
// //with time 2, depth does correspond, but bounding boxes dont correspond by 2 frames
// //with time 3, depth does correspond, but bounding boxes dont correspond by 3 frames
// //with time 4, depth does NOT correspond sometimes, but bounding boxes dont correspond by 3 frames
// //with time 5, depth is OK
//            todo: pair detections with scene id, not with snapshot id, and gather them once per scene, not per snapshot
//            Script.Wait(4);//tried 4 milliseconds instead of one, so screenshots correspond to their data
            Script.Wait(gameplayInterval.Value);
            Game.Pause(true);
//            UI.Notify("new camera rotation is: " + rotation.ToString());
            Script.Wait(20);
            Logger.WriteLine("new camera position is: " + World.RenderingCamera.Position);
            Logger.WriteLine("new camera rotation is: " + World.RenderingCamera.Rotation);
            Logger.WriteLine("new camera position offset is: " + camerasPositions[i]);
            Logger.WriteLine("new camera rotation offset is: " + camerasRotations[i]);
            activeCameraRotation = camerasRotations[i];
            activeCameraPosition = camerasPositions[i];
            return cameras[i];
        }

        public static void Deactivate() {
            if (!initialized) {
                throw new Exception("not initialized, please, call CamerasList.initialize() method before this one");
            }

            mainCamera.IsActive = false;
            foreach (var camera in cameras) {
                camera.IsActive = false;
            }

            World.RenderingCamera = null;
        }
    }
}
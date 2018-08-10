using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using GTA;
using GTA.Math;
using System.Globalization;
using GTA.Native;
using Npgsql;
using SharpDX;
using SharpDX.Mathematics;
using NativeUI;
using System.Drawing;
using Amazon.KeyManagementService.Model.Internal.MarshallTransformations;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;
using Color = System.Drawing.Color;
using Vector2 = GTA.Math.Vector2;
using Vector3 = GTA.Math.Vector3;
using Point = System.Drawing.Point;

namespace GTAVisionUtils
{
    public class GTARun
    {
        public Guid guid { get; set; }
        public string archiveKey { get; set; }
    }
    public class GTABoundingBox2
    {
        public GTAVector2 Min { get; set; }
        public GTAVector2 Max { get; set; }
        public float Area {
            get {
                return (Max.X - Min.X) * (Max.Y - Min.Y);
            }
        }
    }
    
    public class GTAVehicle
    {
        public GTAVector Pos { get; set; }
        public GTABoundingBox2 BBox { get; set; }

        public GTAVehicle(Vehicle v)
        {
            Pos = new GTAVector(v.Position);
            BBox = GTAData.ComputeBoundingBox(v, v.Position);
        }
    }
    public class GTAPed {
        public GTAVector Pos { get; set; }
        public GTABoundingBox2 BBox { get; set; }
        public GTAPed(Ped p)
        {
            Pos = new GTAVector(p.Position);
            BBox = GTAData.ComputeBoundingBox(p, p.Position);
        }
    }

    public enum DetectionType
    {
        background,
        person,
        car,
        bicycle
    }
    public enum DetectionClass {
        Unknown = -1,
        Compacts = 0,
        Sedans = 1,
        SUVs = 2,
        Coupes = 3,
        Muscle = 4,
        SportsClassics = 5,
        Sports = 6,
        Super = 7,
        Motorcycles = 8,
        OffRoad = 9,
        Industrial = 10,
        Utility = 11,
        Vans = 12,
        Cycles = 13,
        Boats = 14,
        Helicopters = 15,
        Planes = 16,
        Service = 17,
        Emergency = 18,
        Military = 19,
        Commercial = 20,
        Trains = 21
    }
    public class GTADetection
    {
        public DetectionType Type { get; set; }
        public DetectionClass cls { get; set; }
        public GTAVector Pos { get; set; }
        public GTAVector Rot { get; set; }
        public float Distance { get; set; }
        public GTABoundingBox2 BBox { get; set; }
        public BoundingBox BBox3D { get; set; }
        public int Handle { get; set; }
        public GTAVector velocity { get; set; }
        public GTADetection(Entity e, DetectionType type)
        {
            Type = type;
            Pos = new GTAVector(e.Position);
            Distance = Game.Player.Character.Position.DistanceTo(e.Position);
            BBox = GTAData.ComputeBoundingBox(e, e.Position);
            Handle = e.Handle;
            
            Rot = new GTAVector(e.Rotation);
            velocity = new GTAVector(e.Velocity);
            cls = DetectionClass.Unknown;
            Vector3 gmin;
            Vector3 gmax;
            e.Model.GetDimensions(out gmin, out gmax);
            BBox3D = new BoundingBox((SharpDX.Vector3) new GTAVector(gmin), (SharpDX.Vector3)new GTAVector(gmax));
        }

        public GTADetection(Ped p) : this(p, DetectionType.person)
        {
        }

        public GTADetection(Vehicle v) : this(v, DetectionType.car)
        {
            cls = (DetectionClass)Enum.Parse(typeof(DetectionClass),  v.ClassType.ToString());
        }
    }
    public class GTAVector
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public GTAVector(Vector3 v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public static explicit operator SharpDX.Vector3(GTAVector i)
        {
            return new SharpDX.Vector3(i.X, i.Y, i.Z);
        }
    }

    public class GTAVector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public static GTAVector2 fromVector2(Vector2 vector2) {
            return new GTAVector2(vector2.X, vector2.Y);
        }
        
        public GTAVector2(float x, float y)
        {
            X = x;
            Y = y;
        }
        public GTAVector2()
        {
            X = 0f;
            Y = 0f;
        }
        public GTAVector2(Vector2 v)
        {
            X = v.X;
            Y = v.Y;
        }
    }

    public class GTAData
    {
        public int Version { get; set; }
        public string ImageName { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public DateTime Timestamp { get; set; }
        public TimeSpan LocalTime { get; set; }
        public Weather CurrentWeather { get; set; }
        public List<Weather> CapturedWeathers;
        public GTAVector CamPos { get; set; }
        public GTAVector CamRot { get; set; }
        public BoundingBox CarModelBox { get; set; }
        public GTAVector CamDirection { get; set; }
        //mathnet's matrices are in heap storage, which is super annoying, 
        //but we want to use double matrices to avoid numerical issues as we
        //decompose the MVP matrix into seperate M,V and P matrices
        public DenseMatrix WorldMatrix { get; set; }
        public DenseMatrix ViewMatrix { get; set; }
        public DenseMatrix ProjectionMatrix { get; set; }
        public double CamFOV { get; set; }

        public double CamNearClip { get; set; }
        public double CamFarClip { get; set; }
        public GTAVector playerPos { get; set; }
        public GTAVector velocity { get; set; }
        public int UIHeight { get; set; }
        public int UIWidth { get; set; }
        public Guid sceneGuid { get; set; }
        public GTAVector CamRelativeRot { get; set; }
        public GTAVector CamRelativePos { get; set; }
        public GTAVector2 CurrentTarget { get; set; }

        public List<GTADetection> Detections { get; set; }
        public static SharpDX.Vector3 CvtVec(GTA.Math.Vector3 inp) {
            return (SharpDX.Vector3)new GTAVector(inp);

        }
        public static GTABoundingBox2 ComputeBoundingBox(Entity e, Vector3 offset, float scale = 0.5f)
        {
            
            var m = e.Model;
            var rv = new GTABoundingBox2
            {
                Min = new GTAVector2(float.PositiveInfinity, float.PositiveInfinity),
                Max = new GTAVector2(float.NegativeInfinity, float.NegativeInfinity)
            };
            Vector3 gmin;
            Vector3 gmax;
            m.GetDimensions(out gmin, out gmax);
            var bbox = new SharpDX.BoundingBox((SharpDX.Vector3)new GTAVector(gmin), (SharpDX.Vector3)new GTAVector(gmax));
            //Console.WriteLine(bbox.GetCorners()[0]);
            /*
            for (int i = 0; i < bbox.GetCorners().Length; ++i) {
                for (int j = 0; j < bbox.GetCorners().Length; ++j) {
                    if (j == i) continue;
                    var c1 = bbox.GetCorners()[i];
                    var c2 = bbox.GetCorners()[j];
                    HashFunctions.Draw3DLine(e.GetOffsetInWorldCoords(new Vector3(c1.X, c1.Y, c1.Z)), e.GetOffsetInWorldCoords(new Vector3(c2.X, c2.Y, c2.Z)), 0,0);
                }
            }
            */
            /*
            for (int i = 0; i < bbox.GetCorners().Length; ++i)
            {
                var corner = bbox.GetCorners()[i];
                var cornerinworld = e.GetOffsetInWorldCoords(new Vector3(corner.X, corner.Y, corner.Z));


            }*/
            //UI.Notify(e.HeightAboveGround.ToString());
            var sp = HashFunctions.Convert3dTo2d(e.GetOffsetInWorldCoords(e.Position));
            foreach (var corner in bbox.GetCorners()) {
                var c = new Vector3(corner.X, corner.Y, corner.Z);

                c = e.GetOffsetInWorldCoords(c);
                var s = HashFunctions.Convert3dTo2d(c);
                if (s.X == -1f || s.Y == -1f)
                {
                    rv.Min.X = float.PositiveInfinity;
                    rv.Max.X = float.NegativeInfinity;
                    rv.Min.Y = float.PositiveInfinity;
                    rv.Max.Y = float.NegativeInfinity;
                    return rv;
                }
                /*
                if(s.X == -1) {
                    if (sp.X < 0.5) s.X = 0f;
                    if (sp.X >= 0.5) s.X = 1f;
                }
                if(s.Y == -1) {
                    if (sp.Y < 0.5) s.Y = 0f;
                    if (sp.Y >= 0.5) s.Y = 1f;
                }
                */
                rv.Min.X = Math.Min(rv.Min.X, s.X);
                rv.Min.Y = Math.Min(rv.Min.Y, s.Y);
                rv.Max.X = Math.Max(rv.Max.X, s.X);
                rv.Max.Y = Math.Max(rv.Max.Y, s.Y);
            }

            float w = rv.Max.X - rv.Min.X;
            float h = rv.Max.Y - rv.Min.Y;
//            just for debug purposes, show visible and not visible entities in other color
//            if (CheckVisible(e))
//            {
//                HashFunctions.DrawRect(rv.Min.X + w / 2, rv.Min.Y + h / 2, rv.Max.X - rv.Min.X, rv.Max.Y - rv.Min.Y,
//                    Color.White, 100);
//            }
//            else
//            {
//                HashFunctions.DrawRect(rv.Min.X + w / 2, rv.Min.Y + h / 2, rv.Max.X - rv.Min.X, rv.Max.Y - rv.Min.Y,
//                    Color.Red, 100);                
//            }
//            HashFunctions.DrawRect(rv.Min.X + w/2, rv.Min.Y + h/2, rv.Max.X - rv.Min.X, rv.Max.Y - rv.Min.Y, 255, 255, 255, 100);
            return rv;
        }
        public static bool CheckVisible(Entity e) {
//            return true;

            var ppos = World.RenderingCamera.Position;
            var isLOS = Function.Call<bool>((GTA.Native.Hash) 0x0267D00AF114F17A, Game.Player.Character, e);
            if (isLOS) return true;
//            return isLOS;
//            var ppos = GameplayCamera.Position;

            var res = World.Raycast(ppos, e.Position, IntersectOptions.Everything, Game.Player.Character.CurrentVehicle);
//            HashFunctions.Draw3DLine(ppos, e.Position);
//            UI.Notify("Camera: " + ppos.X + " Ent: " + e.Position.X);
//            World.DrawMarker(MarkerType.HorizontalCircleSkinny_Arrow, ppos, (e.Position - ppos).Normalized, Vector3.Zero, new Vector3(1, 1, 1), System.Drawing.Color.Green);
            
//            debugging visualization for visible or invisible vehicles
//            var p = Game.Player.LastVehicle;
//            HashFunctions.Draw3DLine(p.Position, e.Position, System.Drawing.Color.Green);
//            var s = HashFunctions.Convert3dTo2d(e.Position);
//            HashFunctions.Draw2DText("Just Monika", s.X, s.Y, 255, 255, 255, 255);
            
//            World.DrawMarker(MarkerType.HorizontalCircleSkinny_Arrow, p.Position, (e.Position - p.Position).Normalized, Vector3.Zero, new Vector3(1, 1, 1), System.Drawing.Color.Green);
//            return res.HitEntity == e;
            if (res.HitEntity == e) return true;
            if (res.HitCoords == null) return false;
            return e.IsInRangeOf(res.HitCoords, 10);
            //return res.HitEntity == e;
        }

        public static GTAData DumpData(string imageName, Weather capturedWeather)
        {
            return DumpData(imageName, new List<Weather>() {capturedWeather});
        }

        public static GTAData DumpData(string imageName, List<Weather> capturedWeathers)
        {
            var ret = new GTAData();
            ret.Version = 3;
            ret.ImageName = imageName;
            ret.CurrentWeather = World.Weather;
            ret.CapturedWeathers = capturedWeathers;
            
            ret.Timestamp = DateTime.UtcNow;
            ret.LocalTime = World.CurrentDayTime;
            ret.CamPos = new GTAVector(World.RenderingCamera.Position);
            ret.CamRot = new GTAVector(World.RenderingCamera.Rotation);
            //getting information about currently driving vehicle model size
            Vector3 gmin;
            Vector3 gmax;
            Game.Player.Character.CurrentVehicle.Model.GetDimensions(out gmin, out gmax);
            ret.CarModelBox = new BoundingBox((SharpDX.Vector3) new GTAVector(gmin), (SharpDX.Vector3) new GTAVector(gmax));
            ret.CamDirection = new GTAVector(World.RenderingCamera.Direction);
            ret.CamFOV = World.RenderingCamera.FieldOfView;
            ret.ImageWidth = Game.ScreenResolution.Width;
            ret.ImageHeight = Game.ScreenResolution.Height;
            ret.UIWidth = UI.WIDTH;
            ret.UIHeight = UI.HEIGHT;
            ret.playerPos = new GTAVector(Game.Player.Character.Position);
            ret.velocity = new GTAVector(Game.Player.Character.Velocity);
            ret.CamNearClip = World.RenderingCamera.NearClip;
            ret.CamFarClip = World.RenderingCamera.FarClip;
            
            var peds = World.GetNearbyPeds(Game.Player.Character, 500.0f);
            var cars = World.GetNearbyVehicles(Game.Player.Character, 500.0f);
            //var props = World.GetNearbyProps(Game.Player.Character.Position, 300.0f);
            
            var constants = VisionNative.GetConstants();
            if (!constants.HasValue) return null;
            var W = MathNet.Numerics.LinearAlgebra.Single.DenseMatrix.OfColumnMajor(4, 4, constants.Value.world.ToArray()).ToDouble();
            var WV =
                MathNet.Numerics.LinearAlgebra.Single.DenseMatrix.OfColumnMajor(4, 4,
                    constants.Value.worldView.ToArray()).ToDouble();
            var WVP =
                MathNet.Numerics.LinearAlgebra.Single.DenseMatrix.OfColumnMajor(4, 4,
                    constants.Value.worldViewProjection.ToArray()).ToDouble();

            var V = WV*W.Inverse();
            var P = WVP*WV.Inverse();
            ret.ProjectionMatrix = P as DenseMatrix;
            ret.ViewMatrix = V as DenseMatrix;
            ret.WorldMatrix = W as DenseMatrix;
            
            var pedList = from ped in peds
                where ped.IsHuman && ped.IsOnFoot
//                where ped.IsHuman && ped.IsOnFoot && CheckVisible(ped)
                select new GTADetection(ped);
            var cycles = from ped in peds
                where ped.IsOnBike
//                where ped.IsOnBike && CheckVisible(ped)
                select new GTADetection(ped, DetectionType.bicycle);
            
            var vehicleList = from car in cars
//                where CheckVisible(car)
                select new GTADetection(car);
            ret.Detections = new List<GTADetection>();
            ret.Detections.AddRange(pedList);
            ret.Detections.AddRange(vehicleList);
            //ret.Detections.AddRange(cycles);
            
            return ret;
        }
        
    }
}
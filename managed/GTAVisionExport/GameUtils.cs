using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace GTAVisionExport {
    
    public class TimeChecker {
        public int startTime { get; set; }
        public bool initialized { get; set; }
        public TimeSpan interval { get; set; }
        
        public TimeChecker(TimeSpan interval) {
            /* Game.Gametime is in ms, so 1000000 ms = 16.6 min*/
            startTime = 0;
            initialized = false;
            this.interval = interval;
        }
        
        public void clear() {
            this.initialized = false;
        }
        
        public bool isPassed(int time) {
            //UI.Notify("last time" + this.gameTime);
            //UI.Notify("time now" + time);
            if (!initialized) {
                startTime = time;
                initialized = true;
                return false;
            }
            
            return time >= startTime + interval.TotalMilliseconds;
        }
    }
    
    public abstract class TimeDistanceChecker {
        public int startTime { get; set; }
        public bool initialized { get; set; }
        public TimeSpan interval { get; set; }
        public Vector3 center;
        public int distance;
        
        public TimeDistanceChecker(TimeSpan interval, int distance, Vector3 center) {
            /* Game.Gametime is in ms, so 1000000 ms = 16.6 min*/
            startTime = 0;
            initialized = false;
            this.interval = interval;
            this.center = center;
            this.distance = distance;
        }
        
        public void clear() {
            initialized = false;
        }

        public abstract bool isDistanceSatisfied(Vector3 position);
        
        public bool isPassed(int time, Vector3 position) {
            //UI.Notify("last time" + this.gameTime);
            //UI.Notify("time now" + time);
            if (!initialized) {
                startTime = time;
                initialized = true;
                center = position;
                return false;
            }

            if (time >= startTime + interval.TotalMilliseconds) {
                return isDistanceSatisfied(position);
            }
            return false;
        }
    }

    /// <summary>
    /// Use to check if vehicle is stuck in some area for some time (e.g. has not moved 1 meter or more from position in last minute)
    /// 
    /// </summary>
    public class TimeNearPointChecker : TimeDistanceChecker {

        public TimeNearPointChecker(TimeSpan interval, int distance, Vector3 center) : base(interval, distance, center) {
        }

        public override bool isDistanceSatisfied(Vector3 position) {
            return position.DistanceTo(center) < distance;
        }
    }

    /// <summary>
    /// Use to check if vehicle is stuck in some area for some time (e.g. has not come nearer to a location (not moving to a target))
    /// </summary>
    public class TimeDistantFromPointChecker : TimeDistanceChecker {
        
        public TimeDistantFromPointChecker(TimeSpan interval, int distance, Vector3 center) : base(interval, distance, center) {
        }

        public override bool isDistanceSatisfied(Vector3 position) {
            return position.DistanceTo(center) > distance;
        }
        
    }

    /// <summary>
    /// Use to check if vehicle is stuck in some area for some time (e.g. has not come nearer to a location (not moving to a target))
    /// Updates distance, checks if min distance is changing or not after some time.
    /// </summary>
    public class TimeNotMovingTowardsPointChecker {
        public int startTime { get; set; }
        public bool initialized { get; set; }
        public TimeSpan interval { get; set; }
        public Vector2 center { get; set; }
        public float distance;
        public float minDistance;
        
        public TimeNotMovingTowardsPointChecker(TimeSpan interval, Vector2 center) {
            /* Game.Gametime is in ms, so 1000000 ms = 16.6 min*/
            startTime = 0;
            initialized = false;
            this.interval = interval;
            this.center = center;
            minDistance = float.MaxValue;
        }
        
        public void clear() {
            this.initialized = false;
        }

        public bool isPassed(int time, Vector3 position) {
            //UI.Notify("last time" + this.gameTime);
            //UI.Notify("time now" + time);
            if (!initialized) {
                startTime = time;
                initialized = true;
                minDistance = float.MaxValue;
                return false;
            }

            distance = center.DistanceTo(new Vector2(position.X, position.Y));
            if (distance < minDistance) {
                minDistance = distance;
                startTime = time;
            }

            if (time >= startTime + interval.TotalMilliseconds) {
                return distance > minDistance;
            }
            return false;
        }

        
    }

    public enum GameStatus {
        NeedReload,
        NeedStart,
        NoActionNeeded
    }

    public class GTAConst {
        public static Vector3 OriginalStartPos = new Vector3(311.7819f, -1372.574f, 31.84874f);
        public static Vector3 HighwayStartPos = new Vector3(1209.5412f,-1936.0394f,38.3709f);

        public static VehicleHash OnroadVehicleHash = VehicleHash.Asea;
        public static VehicleHash OffroadVehicleHash = OffroadPlanning.OffroadModel;
    }
}

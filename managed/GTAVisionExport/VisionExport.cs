using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using YamlDotNet;
using YamlDotNet.Serialization;
using BitMiracle.LibTiff.Classic;
using System.Drawing;
using System.Drawing.Imaging;
using YamlDotNet.RepresentationModel;
using System.IO.Pipes;
using System.Net;
using VAutodrive;
using System.Net.Sockets;
using System.Windows.Media.Imaging;
using GTAVisionUtils;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using GTA.Native;
using Color = System.Windows.Media.Color;
using System.Configuration;
using System.Threading;
using IniParser;
using Newtonsoft.Json;

namespace GTAVisionExport {
    class VisionExport : Script {
#if DEBUG
        const string session_name = "NEW_DATA_CAPTURE_NATURAL_V4_3";
#else
        const string session_name = "NEW_DATA_CAPTURE_NATURAL_V4_3";
#endif
        //private readonly string dataPath =
        //    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Data");
        private readonly string dataPath;
        public static string logFilePath;

        private readonly Weather[] wantedWeathers = new Weather[]
            {Weather.Clear, Weather.Clouds, Weather.Overcast, Weather.Raining, Weather.Christmas};

        private readonly Weather wantedWeather = Weather.Clear;
        private readonly bool multipleWeathers = false; // decides whether to use multiple weathers or just one
        private readonly bool currentWeather = true;
        private readonly bool clearEverything = false;
//        private readonly bool useMultipleCameras = false;    // when false, cameras handling script is not used at all
        private readonly bool useMultipleCameras = true;    // when false, cameras handling script is not used at all
//        private readonly bool staticCamera = true;        // this turns off whole car spawning, teleportation and autodriving procedure
        private readonly bool staticCamera = false;        // this turns off whole car spawning, teleportation and autodriving procedure
        private Player player;
        private GTARun run;
        private bool enabled = false;
        private Socket server;
        private Socket connection;
        private UTF8Encoding encoding = new UTF8Encoding(false);

//        this is the vaustodrive keyhandling
        private KeyHandling kh = new KeyHandling();

        private Task postgresTask;

        private Task runTask;
        private int curSessionId = -1;
        private TimeChecker lowSpeedTime = new TimeChecker(TimeSpan.FromSeconds(200));
        private TimeChecker notMovingTime = new TimeChecker(TimeSpan.FromSeconds(30));
        private TimeDistanceChecker distanceFromStart = new TimeDistanceChecker(TimeSpan.FromSeconds(30), 2, new Vector3());
        private bool isGamePaused = false; // this is for external pause, not for internal pause inside the script
        private static bool notificationsAllowed = true;
        private StereoCamera cams;
        private bool timeIntervalEnabled = false;
        private TimeSpan timeFrom;
        private TimeSpan timeTo;
        public static string location;
        private static Vector2 somePos;

        //this variable, when true, should be disabling car spawning and autodrive starting here, because offroad has different settings
        public static bool drivingOffroad;
        public static bool gatheringData = true;

        public VisionExport() {
            // loading ini file
            var parser = new FileIniDataParser();
            location = AppDomain.CurrentDomain.BaseDirectory;
            var data = parser.ReadFile(Path.Combine(location, "GTAVision.ini"));

            //UINotify(ConfigurationManager.AppSettings["database_connection"]);
            dataPath = data["Snapshots"]["OutputDir"];
            logFilePath = data["Snapshots"]["LogFile"];
            Logger.logFilePath = logFilePath;

            Logger.WriteLine("VisionExport constructor called.");
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            PostgresExport.InitSQLTypes();
            player = Game.Player;
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Loopback, 5555));
            server.Listen(5);
            //server = new UdpClient(5555);
            //outputPath = @"D:\Datasets\GTA\";
            //outputPath = Path.Combine(outputPath, "testData.yaml");
            //outStream = File.CreateText(outputPath);
            this.Tick += new EventHandler(this.OnTick);
            this.KeyDown += OnKeyDown;

            Interval = 50;
            if (enabled) {
                postgresTask?.Wait();
                postgresTask = StartSession();
                runTask?.Wait();
                runTask = StartRun();
            }

            Logger.WriteLine("Logger prepared");
            UINotify("Logger initialized. Going to initialize cameras.");
            CamerasList.initialize();
            initialize4cameras();
            
//            var newCamera = World.CreateCamera(new Vector3(), new Vector3(), 50);
//            newCamera.NearClip = 0.15f;
//            newCamera.IsActive = true;
//            newCamera.Position = new Vector3(-1078f, -216f, 37f);
////            newCamera.Rotation = new Vector3(270f, 0f, 0f);  // x and y rotation seem to be switched. Can be fixed by setting the last parameter to 2
//            newCamera.Rotation = new Vector3(0f, 270f, 0f);  // x and y rotation seem to be switched. Can be fixed by setting the last parameter to 2
//            World.RenderingCamera = newCamera;

//            {-1078,-216,37}
//            CamerasList.setMainCamera(new Vector3(358f, -1308f, 52f), new Vector3(0f, 90f, 0f), 150, 0.15f);

            UINotify("VisionExport plugin initialized.");
        }

        private void initialize4cameras() {
//            cameras initialization:
            
//            for cameras mapping area before the car
//            float r = 8f; //radius of circle with 4 cameras
//            CamerasList.setMainCamera();
//            CamerasList.addCamera(new Vector3(0f, 2f, 0.4f), new Vector3(0f, 0f, 0f), 50, 1.5f);
//            CamerasList.addCamera(new Vector3(r, r + 2f, 0.4f), new Vector3(0f, 0f, 90f), 50, 1.5f);
//            CamerasList.addCamera(new Vector3(0f, 2*r + 2f, 0.4f), new Vector3(0f, 0f, 180f), 50, 1.5f);
//            CamerasList.addCamera(new Vector3(-r, r + 2f, 0.4f), new Vector3(0f, 0f, 270f), 50, 1.5f);

////            for 4 cameras of different sides of the car, for šochman
//            CamerasList.setMainCamera();
//            CamerasList.addCamera(new Vector3(0f, 2f, 0.3f), new Vector3(0f, 0f, 0f), 50, 0.15f);
//            CamerasList.addCamera(new Vector3(-0.8f, 0.8f, 0.4f), new Vector3(0f, 0f, 90f), 50, 0.15f);
//            CamerasList.addCamera(new Vector3(0f, -2.3f, 0.3f), new Vector3(0f, 0f, 180f), 50, 0.15f);
//            CamerasList.addCamera(new Vector3(0.8f, 0.8f, 0.4f), new Vector3(0f, 0f, 270f), 50, 0.15f);

//            for 4 cameras on top of car, heading 4 directions
//            CamerasList.setMainCamera();
//            CamerasList.addCamera(new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 0f), 58, 0.15f);
//            CamerasList.addCamera(new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 90f), 58, 0.15f);
//            CamerasList.addCamera(new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 180f), 58, 0.15f);
//            CamerasList.addCamera(new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 270f), 58, 0.15f);

//            set only main camera for static traffic camera
//            CamerasList.setMainCamera(new Vector3(-1078f, -216f, 57f), new Vector3(270f, 0f, 0f), 50, 0.15f);
            
////            two "cameras", as in KITTI dataset, so we have 4-camera setup in stereo
////            for cameras mapping area before the car
//            CamerasList.setMainCamera();
//            const float r = 8f; //radius of circle with 4 cameras
//            // this height is for 1.65 m above ground, as in KITTI. The car has height of model ASEA is 1.5626, its center is in 0.5735 above ground
//            var carCenter = 0.5735f;
//            var camOne = new Vector3(-0.06f, 0.27f, 1.65f - carCenter);
//            var camTwo = new Vector3(-0.06f+0.54f, 0.27f, 1.65f - carCenter);
//            CamerasList.addCamera(camOne + new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 50, 0.15f);
//            CamerasList.addCamera(camOne + new Vector3(r, r, 0f), new Vector3(0f, 0f, 90f), 50, 0.15f);
//            CamerasList.addCamera(camOne + new Vector3(0, 2*r, 0f), new Vector3(0f, 0f, 180f), 50, 0.15f);
//            CamerasList.addCamera(camOne + new Vector3(-r, r, 0f), new Vector3(0f, 0f, 270f), 50, 0.15f);
////            4 camera layout from 1 camera should be ernough to reconstruct 3D map for both cameras
//            CamerasList.addCamera(camTwo + new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 50, 0.15f);
////            CamerasList.addCamera(camTwo + new Vector3(r, r, 0f), new Vector3(0f, 0f, 90f), 50, 0.15f);
////            CamerasList.addCamera(camTwo + new Vector3(0, 2*r, 0f), new Vector3(0f, 0f, 180f), 50, 0.15f);
////            CamerasList.addCamera(camTwo + new Vector3(-r, r, 0f), new Vector3(0f, 0f, 270f), 50, 0.15f);
////            and now, one camera from birds-eye view, with this configuration, it sees all other cameras
//            CamerasList.addCamera(camOne + new Vector3(0, r, r + 4), new Vector3(270f, 0f, 0f), 70, 0.15f);
            
////            two "cameras", as in KITTI dataset, so we have 4-camera setup in stereo, but for offroad car, specifically, for Mesa
////            for cameras mapping area before the car
////            KITTI images have ratio of 3.32, they are very large and thus have large horizontal fov. This ratio can not be obtained here
////            so I set higher vertical fov and image may be then cropped into KITTI-like one
//            CamerasList.setMainCamera();
//            const float r = 8f; //radius of circle with 4 cameras
//            // this height is for 1.65 m above ground, as in KITTI. The car has height of model ASEA is 1.5626, its center is in 0.5735 above ground
//            var carCenter = 0.5735f;
//            var camOne = new Vector3(-0.06f, 1.5f, 1.65f - carCenter);
//            var camTwo = new Vector3(-0.06f+0.54f, 1.5f, 1.65f - carCenter);
//            CamerasList.addCamera(camOne + new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 90, 0.15f);
//            CamerasList.addCamera(camOne + new Vector3(r, r, 0f), new Vector3(0f, 0f, 90f), 90, 0.15f);
//            CamerasList.addCamera(camOne + new Vector3(0, 2*r, 0f), new Vector3(0f, 0f, 180f), 90, 0.15f);
//            CamerasList.addCamera(camOne + new Vector3(-r, r, 0f), new Vector3(0f, 0f, 270f), 90, 0.15f);
////            4 camera layout from 1 camera should be ernough to reconstruct 3D map for both cameras
//            CamerasList.addCamera(camTwo + new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 90, 0.15f);
////            CamerasList.addCamera(camTwo + new Vector3(r, r, 0f), new Vector3(0f, 0f, 90f), 50, 0.15f);
////            CamerasList.addCamera(camTwo + new Vector3(0, 2*r, 0f), new Vector3(0f, 0f, 180f), 50, 0.15f);
////            CamerasList.addCamera(camTwo + new Vector3(-r, r, 0f), new Vector3(0f, 0f, 270f), 50, 0.15f);
////            and now, one camera from birds-eye view, with this configuration, it sees all other cameras
//            CamerasList.addCamera(camOne + new Vector3(0, r, r + 4), new Vector3(270f, 0f, 0f), 70, 0.15f);

//            na 32 metrů průměr, výš a natočit dolů            
//            two "cameras", as in KITTI dataset, so we have 4-camera setup in stereo, but for offroad car, specifically, for Mesa
//            for cameras mapping area before the car
//            KITTI images have ratio of 3.32, they are very large and thus have large horizontal fov. This ratio can not be obtained here
//            so I set higher vertical fov and image may be then cropped into KITTI-like one
            CamerasList.setMainCamera();
            const float r = 16f; //radius of circle with 4 cameras
            // this height is for 1.65 m above ground, as in KITTI. The car has height of model ASEA is 1.5626, its center is in 0.5735 above ground
            var carCenter = 0.5735f;
            var camOne = new Vector3(-0.06f, 1.5f, 1.65f - carCenter);
            var camTwo = new Vector3(-0.06f+0.54f, 1.5f, 1.65f - carCenter);
            CamerasList.addCamera(camOne + new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 90, 0.15f);
            
            CamerasList.addCamera(camOne + new Vector3(r, r, 5f), new Vector3(-30f, 0f, 90f), 90, 0.15f);
            CamerasList.addCamera(camOne + new Vector3(0, 2*r, 5f), new Vector3(-30f, 0f, 180f), 90, 0.15f);
            CamerasList.addCamera(camOne + new Vector3(-r, r, 5f), new Vector3(-30f, 0f, 270f), 90, 0.15f);
//            4 camera layout from 1 camera should be ernough to reconstruct 3D map for both cameras
            CamerasList.addCamera(camTwo + new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 90, 0.15f);
//            and now, one camera from birds-eye view, with this configuration, it sees all other cameras
            CamerasList.addCamera(camOne + new Vector3(0, r, r + 8), new Vector3(270f, 0f, 90f), 70, 0.15f);    //to have bigger view of area in front of and behind car
        }
        
        private void HandlePipeInput() {
//            Logger.writeLine("VisionExport handlePipeInput called.");
//            UINotify("handlePipeInput called");
            UINotify("server connected:" + server.Connected);
            UINotify(connection == null ? "connection is null" : "connection:" + connection);
            if (connection == null) return;

            var inBuffer = new byte[1024];
            var str = "";
            var num = 0;
            try {
                num = connection.Receive(inBuffer);
                str = encoding.GetString(inBuffer, 0, num);
            }
            catch (SocketException e) {
                if (e.SocketErrorCode == SocketError.WouldBlock) {
                    return;
                }

                throw;
            }

            if (num == 0) {
                connection.Shutdown(SocketShutdown.Both);
                connection.Close();
                connection = null;
                return;
            }

            UINotify("str: " + str);
            Logger.WriteLine("obtained json: " + str);
            dynamic parameters = JsonConvert.DeserializeObject(str);
            string commandName = parameters.name;
            switch (commandName) {
                case "START_SESSION":
                    postgresTask?.Wait();
                    postgresTask = StartSession();
                    runTask?.Wait();
                    runTask = StartRun();
                    break;
                case "STOP_SESSION":
                    StopRun();
                    StopSession();
                    break;
                case "TOGGLE_AUTODRIVE":
                    ToggleNavigation();
                    break;
                case "ENTER_VEHICLE":
                    UINotify("Trying to enter vehicle");
                    EnterVehicle();
                    break;
                case "AUTOSTART":
                    Autostart();
                    break;
                case "RELOADGAME":
                    ReloadGame();
                    break;
                case "RELOAD":
                    var f = GetType()
                        .GetField("_scriptdomain", BindingFlags.NonPublic | BindingFlags.Instance);
                    var domain = f.GetValue(this);
                    var m = domain.GetType()
                        .GetMethod("DoKeyboardMessage", BindingFlags.Instance | BindingFlags.Public);
                    m.Invoke(domain, new object[] {Keys.Insert, true, false, false, false});
                    break;
                case "SET_TIME":
                    string time = parameters.time;
                    UINotify("starting set time, obtained: " + time);
                    var hoursAndMinutes = time.Split(':');
                    var hours = int.Parse(hoursAndMinutes[0]);
                    var minutes = int.Parse(hoursAndMinutes[1]);
                    GTA.World.CurrentDayTime = new TimeSpan(hours, minutes, 0);
                    UINotify("Time Set");
                    break;
                case "SET_WEATHER":
                    try {
                        string weather = parameters.weather;
                        UINotify("Weather Set to " + weather.ToString());
                        var weatherEnum = (Weather) Enum.Parse(typeof(Weather), weather);
                        GTA.World.Weather = weatherEnum;
                    }
                    catch (Exception e) {
                        Logger.WriteLine(e);
                    }

                    break;
                case "SET_TIME_INTERVAL":
                    string timeFrom = parameters.timeFrom;
                    string timeTo = parameters.timeTo;
                    UINotify("starting set time, obtained from: " + timeFrom + ", to: " + timeTo);
                    var hoursAndMinutesFrom = timeFrom.Split(':');
                    var hoursAndMinutesTo = timeTo.Split(':');
                    var hoursFrom = int.Parse(hoursAndMinutesFrom[0]);
                    var minutesFrom = int.Parse(hoursAndMinutesFrom[1]);
                    var hoursTo = int.Parse(hoursAndMinutesTo[0]);
                    var minutesTo = int.Parse(hoursAndMinutesTo[1]);
                    this.timeIntervalEnabled = true;
                    this.timeFrom = new TimeSpan(hoursFrom, minutesFrom, 0);
                    this.timeTo = new TimeSpan(hoursTo, minutesTo, 0);
                    UINotify("Time Interval Set");
                    break;
                case "PAUSE":
                    UINotify("game paused");
                    isGamePaused = true;
                    Game.Pause(true);
                    break;
                case "UNPAUSE":
                    UINotify("game unpaused");
                    isGamePaused = false;
                    Game.Pause(false);
                    break;
//                    uncomment when resolving, how the hell should I get image by socket correctly
//                case "GET_SCREEN":
//                    var last = ImageUtils.getLastCapturedFrame();
//                    Int64 size = last.Length;
//                    UINotify("last size: " + size.ToString());
//                    size = IPAddress.HostToNetworkOrder(size);
//                    connection.Send(BitConverter.GetBytes(size));
//                    connection.Send(last);
//                    break;
            }
        }

        public void startRunAndSessionManual() {
//            this method does not enable mod (used for manual data gathering)
            postgresTask?.Wait();
            postgresTask = StartSession();
            runTask?.Wait();
            runTask = StartRun(false);
        }

        public void OnTick(object o, EventArgs e) {
            if (server.Poll(10, SelectMode.SelectRead) && connection == null) {
                connection = server.Accept();
                UINotify("CONNECTED");
                connection.Blocking = false;
            }

            HandlePipeInput();
            if (!enabled) return;

            //Array values = Enum.GetValues(typeof(Weather));


            switch (checkStatus()) {
                case GameStatus.NeedReload:
                    //TODO: need to get a new session and run?
                    Logger.WriteLine("Status is NeedReload");
                    StopRun();
                    runTask?.Wait();
                    runTask = StartRun();
                    //StopSession();
                    //Autostart();
                    UINotify("need reload game");
                    Wait(100);
                    ReloadGame();
                    break;
                case GameStatus.NeedStart:
                    //TODO do the autostart manually or automatically?
                    Logger.WriteLine("Status is NeedStart");
                    //Autostart();
                    // use reloading temporarily
                    StopRun();

                    ReloadGame();
                    Wait(100);
                    runTask?.Wait();
                    runTask = StartRun();
                    //Autostart();
                    break;
                case GameStatus.NoActionNeeded:
                    break;
            }

//            UINotify("runTask.IsCompleted: " + runTask.IsCompleted.ToString());
//            UINotify("postgresTask.IsCompleted: " + postgresTask.IsCompleted.ToString());
            if (!runTask.IsCompleted) return;
            if (!postgresTask.IsCompleted) return;
            
            if (drivingOffroad && OffroadPlanning.offroadDrivingStarted) {
                OffroadPlanning.checkDrivingToTarget();
                OffroadPlanning.setNextTarget();
            }

//            UINotify("going to save images and save to postgres");

            if (gatheringData) {
                try {
                    GamePause(true);
                    gatherData();
                    GamePause(false);
                }
                catch (Exception exception) {
                    GamePause(false);
                    Logger.WriteLine("exception occured, logging and continuing");
                    Logger.WriteLine(exception);
                }                
            }

//            if time interval is enabled, checkes game time and sets it to timeFrom, it current time is after timeTo
            if (timeIntervalEnabled) {
                var currentTime = World.CurrentDayTime;
                if (currentTime > timeTo) {
                    World.CurrentDayTime = timeFrom;
                }
            }
        }

        private void gatherData(int delay = 5) {
            if (clearEverything) {
                ClearSurroundingEverything(Game.Player.Character.Position, 1000f);
            }

            Wait(100);

            var dateTimeFormat = @"yyyy-MM-dd--HH-mm-ss--fff";
            var guid = Guid.NewGuid();
            Logger.WriteLine("generated scene guid: " + guid.ToString());
            
            if (useMultipleCameras) {
                for (var i = 0; i < CamerasList.cameras.Count; i++) {
                    Logger.WriteLine("activating camera " + i.ToString());
                    CamerasList.ActivateCamera(i);
                    gatherDatForOneCamera(dateTimeFormat, guid);
                    Wait(delay);
                }
                CamerasList.Deactivate();
            }
            else {
//                when multiple cameras are not used, only main camera is being used. 
//                now it checks if it is active or not, and sets it
                if (!CamerasList.mainCamera.IsActive) {
                    CamerasList.ActivateMainCamera();
                }
                gatherDatForOneCamera(dateTimeFormat, guid);
            }
        }

        private void gatherDatForOneCamera(string dateTimeFormat, Guid guid) {
            GTAData dat;
            bool success;
            if (multipleWeathers) {
                dat = GTAData.DumpData(DateTime.UtcNow.ToString(dateTimeFormat), wantedWeathers.ToList());
            }
            else {
                Weather weather = currentWeather ? GTA.World.Weather : wantedWeather;
                dat = GTAData.DumpData(DateTime.UtcNow.ToString(dateTimeFormat), weather);
            }

            if (CamerasList.activeCameraRotation.HasValue) {
                dat.CamRelativeRot = new GTAVector(CamerasList.activeCameraRotation.Value);                
            }
            else {
                dat.CamRelativeRot = null;
            }

            if (CamerasList.activeCameraPosition.HasValue) {
                dat.CamRelativePos = new GTAVector(CamerasList.activeCameraPosition.Value);                
            }
            else {
                dat.CamRelativePos = null;
            }
            
            dat.sceneGuid = guid;

            if (dat == null) {
                return;
            }

            if (multipleWeathers) {
                success = saveSnapshotToFile(dat.ImageName, wantedWeathers, false);
            }
            else {
                Weather weather = currentWeather ? GTA.World.Weather : wantedWeather;
                success = saveSnapshotToFile(dat.ImageName, weather, false);
            }

            if (!success) {
//                    when getting data and saving to file failed, saving to db is skipped
                return;
            }

            PostgresExport.SaveSnapshot(dat, run.guid);            
        }
        
        /* -1 = need restart, 0 = normal, 1 = need to enter vehicle */
        public GameStatus checkStatus() {
            var player = Game.Player.Character;
            if (player.IsDead) return GameStatus.NeedReload;
            if (player.IsInVehicle()) {
                var vehicle = player.CurrentVehicle;
//                here checking the time in low or no speed 
                if (vehicle.Speed < 1.0f) {    //speed is in mph
                    if (lowSpeedTime.isPassed(Game.GameTime)) {
                        return GameStatus.NeedReload;
                    }
                } else {
                    lowSpeedTime.clear();
                }

                if (vehicle.Speed < 0.001f) {
                    if (notMovingTime.isPassed(Game.GameTime)) {
                        return GameStatus.NeedReload;
                    }
                } else {
                    notMovingTime.clear();
                }

//                here checking the movement from previous position on some time
                if (distanceFromStart.isPassed(Game.GameTime, vehicle.Position)) {
                    return GameStatus.NeedReload;
                }

                return GameStatus.NoActionNeeded;
            }
            else {
                return GameStatus.NeedReload;
            }
        }

        public Bitmap CaptureScreen() {
            UINotify("CaptureScreen called");
            var cap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            var gfx = Graphics.FromImage(cap);
            //var dat = GTAData.DumpData(Game.GameTime + ".jpg");
            gfx.CopyFromScreen(0, 0, 0, 0, cap.Size);
            /*
            foreach (var ped in dat.ClosestPeds) {
                var w = ped.ScreenBBMax.X - ped.ScreenBBMin.X;
                var h = ped.ScreenBBMax.Y - ped.ScreenBBMin.Y;
                var x = ped.ScreenBBMin.X;
                var y = ped.ScreenBBMin.Y;
                w *= cap.Size.Width;
                h *= cap.Size.Height;
                x *= cap.Size.Width;
                y *= cap.Size.Height;
                gfx.DrawRectangle(new Pen(Color.Lime), x, y, w, h);
            } */
            return cap;
            //cap.Save(GetFileName(".png"), ImageFormat.Png);
        }

        public void Autostart() {
            if (! staticCamera) {
                EnterVehicle();
                Wait(200);
                ToggleNavigation();
                Wait(200);                
            }
            postgresTask?.Wait();
            postgresTask = StartSession();
        }

        public async Task StartSession(string name = session_name) {
            if (name == null) name = Guid.NewGuid().ToString();
            if (curSessionId != -1) StopSession();
            int id = await PostgresExport.StartSession(name);
            curSessionId = id;
        }

        public void StopSession() {
            if (curSessionId == -1) return;
            PostgresExport.StopSession(curSessionId);
            curSessionId = -1;
        }

        public async Task StartRun(bool enable = true) {
            await postgresTask;
            if (run != null) PostgresExport.StopRun(run);
            var runid = await PostgresExport.StartRun(curSessionId);
            run = runid;
            if (enable) {
                enabled = true;
            }
        }

        public void StopRun() {
            runTask?.Wait();
            ImageUtils.WaitForProcessing();
            enabled = false;
            PostgresExport.StopRun(run);
//            UploadFile();
            run = null;

            Game.Player.LastVehicle.Alpha = int.MaxValue;
        }

        public static void UINotify(string message) {
            //just wrapper for UI.Notify, but lets us disable showing notifications ar all
            if (notificationsAllowed) {
                UI.Notify(message);
            }
        }

        public void GamePause(bool value) {
            //wraper for pausing and unpausing game, because if its paused, I don't want to pause it again and unpause it. 
            if (!isGamePaused) {
                Game.Pause(value);
            }
        }

        public static void EnterVehicle() {
            /*
            var vehicle = World.GetClosestVehicle(player.Character.Position, 30f);
            player.Character.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            */
            Model mod = null;
            if (drivingOffroad) {
                mod = new Model(GTAConst.OffroadVehicleHash);
            } else {
                mod = new Model(GTAConst.OnroadVehicleHash);  
            }

            var player = Game.Player;
            if (mod == null) {
                UINotify("mod is null");
            }

            if (player == null) {
                UINotify("player is null");
            }

            if (player.Character == null) {
                UINotify("player.Character is null");
            }

            UINotify("player position: " + player.Character.Position);
            var vehicle = World.CreateVehicle(mod, player.Character.Position);
            if (vehicle == null) {
                UINotify("vehicle is null. Something is fucked up");
            }
            else {
                player.Character.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            }

//            vehicle.Alpha = 0; //transparent
//            player.Character.Alpha = 0;
            vehicle.Alpha = int.MaxValue;    //back to visible, not sure what the exact value means in terms of transparency
            player.Character.Alpha = int.MaxValue;
        }

        public void ToggleNavigation() {
            if (drivingOffroad) {
                //offroad driving script should handle that separately
                OffroadPlanning.setNextTarget();
            }
            
            MethodInfo inf = kh.GetType().GetMethod("AtToggleAutopilot", BindingFlags.NonPublic | BindingFlags.Instance);
            inf.Invoke(kh, new object[] {new KeyEventArgs(Keys.J)});
        }

        private void ClearSurroundingVehicles(Vector3 pos, float radius) {
            ClearSurroundingVehicles(pos.X, pos.Y, pos.Z, radius);
        }

        private void ClearSurroundingVehicles(float x, float y, float z, float radius) {
            Function.Call(Hash.CLEAR_AREA_OF_VEHICLES, x, y, z, radius, false, false, false, false);
        }

        private void ClearSurroundingEverything(Vector3 pos, float radius) {
            ClearSurroundingEverything(pos.X, pos.Y, pos.Z, radius);
        }

        private void ClearSurroundingEverything(float x, float y, float z, float radius) {
            Function.Call(Hash.CLEAR_AREA, x, y, z, radius, false, false, false, false);
        }

        public void ReloadGame() {
            if (staticCamera) {
                return;
            }

            lowSpeedTime.clear();
            notMovingTime.clear();
            distanceFromStart.clear();
            /*
            Process p = Process.GetProcessesByName("Grand Theft Auto V").FirstOrDefault();
            if (p != null)
            {
                IntPtr h = p.MainWindowHandle;
                SetForegroundWindow(h);
                SendKeys.SendWait("{ESC}");
                //Script.Wait(200);
            }
            */
            // or use CLEAR_AREA_OF_VEHICLES
            Ped player = Game.Player.Character;
            //UINotify("x = " + player.Position.X + "y = " + player.Position.Y + "z = " + player.Position.Z);
            // no need to release the autodrive here
            // delete all surrounding vehicles & the driver's car
//            ClearSurroundingVehicles(player.Position, 1000f);
            player.LastVehicle.Delete();
            // teleport to the spawning position, defined in GameUtils.cs, subject to changes
//            player.Position = GTAConst.OriginalStartPos;
            if (drivingOffroad) {
                OffroadPlanning.setNextStart();
                OffroadPlanning.setNextTarget();
            }
            else {
                player.Position = GTAConst.HighwayStartPos;
            }
//            ClearSurroundingVehicles(player.Position, 100f);
//            ClearSurroundingVehicles(player.Position, 50f);
            ClearSurroundingVehicles(player.Position, 20f);
            // start a new run
            EnterVehicle();
            //Script.Wait(2000);
            ToggleNavigation();

            lowSpeedTime.clear();
        }

        public void TraverseWeather() {
            for (int i = 1; i < 14; i++) {
                //World.Weather = (Weather)i;
                World.TransitionToWeather((Weather) i, 0.0f);
                //Script.Wait(1000);
            }
        }

        public void OnKeyDown(object o, KeyEventArgs k) {
            Logger.WriteLine("VisionExport OnKeyDown called.");
            switch (k.KeyCode) {
                case Keys.PageUp:
                    postgresTask?.Wait();
                    postgresTask = StartSession();
                    runTask?.Wait();
                    runTask = StartRun();
                    UINotify("GTA Vision Enabled");
//                there is set weather, just for testing
                    World.Weather = wantedWeather;
                    break;
                case Keys.PageDown:
                    if (staticCamera) {
                        CamerasList.Deactivate();
                    }
                    StopRun();
                    StopSession();
                    UINotify("GTA Vision Disabled");
                    break;
                // temp modification
                case Keys.H:
                    EnterVehicle();
                    UINotify("Trying to enter vehicle");
                    ToggleNavigation();
                    break;
                // temp modification
                case Keys.Y:
                    ReloadGame();
                    break;
                // temp modification
                case Keys.X:
                    notificationsAllowed = !notificationsAllowed;
                    if (notificationsAllowed) {
                        UI.Notify("Notifications Enabled");
                    }
                    else {
                        UI.Notify("Notifications Disabled");
                    }

                    break;
                // temp modification
                case Keys.U:
                    var settings = ScriptSettings.Load("GTAVisionExport.xml");
                    var loc = AppDomain.CurrentDomain.BaseDirectory;

                    //UINotify(ConfigurationManager.AppSettings["database_connection"]);
                    var str = settings.GetValue("", "ConnectionString");
                    UINotify("BaseDirectory: " + loc);
                    UINotify("ConnectionString: " + str);
                    break;
                // temp modification
                case Keys.G:
                    /*
                IsGamePaused = true;
                Game.Pause(true);
                Script.Wait(500);
                TraverseWeather();
                Script.Wait(500);
                IsGamePaused = false;
                Game.Pause(false);
                */
                    GTAData data;
                    if (multipleWeathers) {
                        data = GTAData.DumpData(Game.GameTime + ".tiff", wantedWeathers.ToList());
                    }
                    else {
                        Weather weather = currentWeather ? GTA.World.Weather : wantedWeather;
                        data = GTAData.DumpData(Game.GameTime + ".tiff", weather);
                    }

                    string path = @"D:\GTAV_extraction_output\trymatrix.txt";
                    // This text is added only once to the file.
                    if (!File.Exists(path)) {
                        // Create a file to write to.
                        using (StreamWriter file = File.CreateText(path)) {
                            file.WriteLine("cam direction file");
                            file.WriteLine("direction:");
                            file.WriteLine(
                                $"{World.RenderingCamera.Direction.X} {World.RenderingCamera.Direction.Y} {World.RenderingCamera.Direction.Z}");
                            file.WriteLine("Dot Product:");
                            file.WriteLine(Vector3.Dot(World.RenderingCamera.Direction, World.RenderingCamera.Rotation));
                            file.WriteLine("position:");
                            file.WriteLine(
                                $"{World.RenderingCamera.Position.X} {World.RenderingCamera.Position.Y} {World.RenderingCamera.Position.Z}");
                            file.WriteLine("rotation:");
                            file.WriteLine(
                                $"{World.RenderingCamera.Rotation.X} {World.RenderingCamera.Rotation.Y} {World.RenderingCamera.Rotation.Z}");
                            file.WriteLine("relative heading:");
                            file.WriteLine(GameplayCamera.RelativeHeading.ToString());
                            file.WriteLine("relative pitch:");
                            file.WriteLine(GameplayCamera.RelativePitch.ToString());
                            file.WriteLine("fov:");
                            file.WriteLine(GameplayCamera.FieldOfView.ToString());
                        }
                    }

                    break;
                // temp modification
                case Keys.T:
                    World.Weather = Weather.Raining;
                    /* set it between 0 = stop, 1 = heavy rain. set it too high will lead to sloppy ground */
                    Function.Call(Hash._SET_RAIN_FX_INTENSITY, 0.5f);
                    var test = Function.Call<float>(Hash.GET_RAIN_LEVEL);
                    UINotify("" + test);
                    World.CurrentDayTime = new TimeSpan(12, 0, 0);
                    //Script.Wait(5000);
                    break;
                case Keys.N:
                    UINotify("N pressed, going to take screenshots");

                    startRunAndSessionManual();
                    postgresTask?.Wait();
                    runTask?.Wait();
                    UINotify("starting screenshots");
                    for (int i = 0; i < 2; i++) {
                        GamePause(true);
                        gatherData(100);
                        GamePause(false);
                        Script.Wait(200); // hoping game will go on during this wait
                    }

                    if (staticCamera) {
                        CamerasList.Deactivate();
                    }

                    StopRun();
                    StopSession();
                    break;
                case Keys.OemMinus: //to tlačítko vlevo od pravého shiftu, -
                    UINotify("- pressed, going to rotate cameras");
                
                    Game.Pause(true);
                    for (int i = 0; i < CamerasList.cameras.Count; i++) {
                        Logger.WriteLine($"activating camera {i}");
                        CamerasList.ActivateCamera(i);
                        Script.Wait(1000);
                    }
                    CamerasList.Deactivate();
                    Game.Pause(false);
                    break;
                case Keys.I:
                    var info = new GTAVisionUtils.InstanceData();
                    UINotify(info.type);
                    UINotify(info.publichostname);
                    break;
                case Keys.Divide:
                    Logger.WriteLine($"{World.GetGroundHeight(Game.Player.Character.Position)} is the current player ({Game.Player.Character.Position}) ground position.");
                    var startRect = OffroadPlanning.GetRandomRect(OffroadPlanning.GetRandomArea());
                    var start = OffroadPlanning.GetRandomPoint(startRect);
                    var startZ = World.GetGroundHeight(new Vector2(start.X, start.Y));
                    Logger.WriteLine($"{startZ} is the ground position of {start}.");
//                    OffroadPlanning.setNextStart();
                    startRect = OffroadPlanning.GetRandomRect(OffroadPlanning.GetRandomArea());
                    start = OffroadPlanning.GetRandomPoint(startRect);
                    somePos = start;
                    startZ = World.GetGroundHeight(new Vector2(start.X, start.Y));
                    Logger.WriteLine($"{startZ} is the ground position of {start}.");
//                    when I use the same position, the GetGroundHeight call takes coordinates of player as ground height
                    Game.Player.Character.Position = new Vector3(start.X + 5, start.Y + 5, 800);                    
                    Logger.WriteLine($"teleporting player above teh position.");
                    Script.Wait(50);
                    startZ = World.GetGroundHeight(new Vector2(start.X, start.Y));
                    Logger.WriteLine($"{startZ} is the ground position of {start}.");
                    Logger.WriteLine($"{World.GetGroundHeight(Game.Player.Character.Position)} is the current player ({Game.Player.Character.Position}) ground position.");
                    Logger.ForceFlush();
                    break;
                case Keys.F12:
                    Logger.WriteLine($"{World.GetGroundHeight(Game.Player.Character.Position)} is the current player ({Game.Player.Character.Position}) ground position.");
                    Logger.WriteLine($"{World.GetGroundHeight(somePos)} is the {somePos} ground position.");
                    break;
                case Keys.F11:
                    Model mod = new Model(GTAConst.OffroadVehicleHash);
                    var player = Game.Player;
                    var vehicle = World.CreateVehicle(mod, player.Character.Position);
                    player.Character.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                    break;
                case Keys.F10:
                    startRect = OffroadPlanning.GetRandomRect(OffroadPlanning.GetRandomArea());
                    start = OffroadPlanning.GetRandomPoint(startRect);
                    somePos = start;
                    startZ = World.GetGroundHeight(new Vector2(start.X, start.Y));
                    Logger.WriteLine($"{startZ} is the ground position of {start}.");
                    for (int i = 900; i > 100; i-= 50) {
//                    when I use the same position, the GetGroundHeight call takes coordinates of player as ground height
                        Game.Player.Character.Position = new Vector3(start.X + 5, start.Y + 5, i);
                        Logger.WriteLine($"teleporting player above teh position to height {i}.");
                        Script.Wait(500);
                        startZ = World.GetGroundHeight(new Vector2(start.X, start.Y));
                        Logger.WriteLine($"{startZ} is the ground position of {start}.");                        
                    }
                    break;
                case Keys.F9:
                    //turn on and off for datagathering during driving, mostly for testing offroad
                    gatheringData = !gatheringData;
                    if (gatheringData) {
                        UI.Notify("will be gathering data");
                    }
                    else {
                        UI.Notify("won't be gathering data");
                    }

                    break;

            }
        }

        private bool saveSnapshotToFile(String name, Weather[] weathers, bool manageGamePauses = true) {
//            returns true on success, and false on failure
            List<byte[]> colors = new List<byte[]>();

            if (manageGamePauses) {
                GamePause(true);                
            }
            
            var depth = VisionNative.GetDepthBuffer();
            var stencil = VisionNative.GetStencilBuffer();
            if (depth == null || stencil == null) {
                return false;
            }

            foreach (var wea in weathers) {
                World.TransitionToWeather(wea, 0.0f);
                Script.Wait(1);
                var color = VisionNative.GetColorBuffer();
                if (color == null) {
                    return false;
                }

                colors.Add(color);
            }

            if (manageGamePauses) {
                GamePause(false);
            }

            var res = Game.ScreenResolution;
            var fileName = Path.Combine(dataPath, name);
            ImageUtils.WriteToTiff(fileName, res.Width, res.Height, colors, depth, stencil, false);
//            UINotify("file saved to: " + fileName);
            return true;
        }

        private bool saveSnapshotToFile(String name, Weather weather, bool manageGamePauses = true) {
//            returns true on success, and false on failure
            if (manageGamePauses) {
                GamePause(true);                
            }

            World.TransitionToWeather(weather,
                0.0f);
            Script.Wait(10);
            var depth = VisionNative.GetDepthBuffer();
            var stencil = VisionNative.GetStencilBuffer();
            var color = VisionNative.GetColorBuffer();
            if (depth == null || stencil == null || color == null) {
                return false;
            }


            if (manageGamePauses) {
                GamePause(false);
            }

            var res = Game.ScreenResolution;
            var fileName = Path.Combine(dataPath, name);
            ImageUtils.WriteToTiff(fileName, res.Width, res.Height, new List<byte[]>() {color}, depth, stencil, false);
//            UINotify("file saved to: " + fileName);
            return true;
        }

        private void dumpTest() {
            List<byte[]> colors = new List<byte[]>();
            Game.Pause(true);
            Script.Wait(1);
            var depth = VisionNative.GetDepthBuffer();
            var stencil = VisionNative.GetStencilBuffer();
            foreach (var wea in wantedWeathers) {
                World.TransitionToWeather(wea, 0.0f);
                Script.Wait(1);
                colors.Add(VisionNative.GetColorBuffer());
            }

            Game.Pause(false);
            if (depth != null) {
                var res = Game.ScreenResolution;
                ImageUtils.WriteToTiff(Path.Combine(dataPath, "test"), res.Width, res.Height, colors, depth, stencil);
                UINotify(World.RenderingCamera.FieldOfView.ToString());
            }
            else {
                UINotify("No Depth Data quite yet");
            }

            UINotify((connection != null && connection.Connected).ToString());
        }
    }
}
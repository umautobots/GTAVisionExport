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
using Amazon;
using Amazon.Runtime;
using YamlDotNet.RepresentationModel;
using Amazon.S3;
using Amazon.S3.IO;
using Amazon.S3.Model;
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
        private readonly bool useMultipleCameras = true;
        private Player player;
        private string outputPath;
        private GTARun run;
        private bool enabled = false;
        private Socket server;
        private Socket connection;
        private UTF8Encoding encoding = new UTF8Encoding(false);

        private KeyHandling kh = new KeyHandling();

        private Task postgresTask;

        private Task runTask;
        private int curSessionId = -1;
        private speedAndTime lowSpeedTime = new speedAndTime();
        private bool isGamePaused = false; // this is for external pause, not for internal pause inside the script
        private bool notificationsAllowed = true;
        private StereoCamera cams;
        private bool timeIntervalEnabled = false;
        private TimeSpan timeFrom;
        private TimeSpan timeTo;

        public VisionExport() {
            // loading ini file
            var parser = new FileIniDataParser();
            var location = AppDomain.CurrentDomain.BaseDirectory;
            var data = parser.ReadFile(Path.Combine(location, "GTAVision.ini"));

            //UINotify(ConfigurationManager.AppSettings["database_connection"]);
            dataPath = data["Snapshots"]["OutputDir"];
            logFilePath = data["Snapshots"]["LogFile"];
            Logger.setLogFilePath(logFilePath);

            System.IO.File.WriteAllText(logFilePath, "VisionExport constructor called.\n");
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

//            cameras initialization:
            float r = 8f; //radius of circle with 4 cameras
            CamerasList.mainCamera = new Vector3(0f, 2f, 0.4f);
            CamerasList.addCamera(new Vector3(0f, 2f, 0.4f), new Vector3(0f, 0f, 0f), 100);
            CamerasList.addCamera(new Vector3(r, r + 2f, 0.4f), new Vector3(0f, 0f, 90f), 100);
            CamerasList.addCamera(new Vector3(2 * r, 2f, 0.4f), new Vector3(0f, 0f, 180f), 100);
            CamerasList.addCamera(new Vector3(-r, r + 2f, 0.4f), new Vector3(0f, 0f, 270f), 100);
        }

        private void handlePipeInput() {
//            Logger.writeLine("VisionExport handlePipeInput called.");
            UINotify("handlePipeInput called");
            UINotify("server connected:" + server.Connected.ToString());
            UINotify(connection == null ? "connection is null" : "connection:" + connection.ToString());
            if (connection == null) return;

            byte[] inBuffer = new byte[1024];
            string str = "";
            int num = 0;
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

            UINotify("str: " + str.ToString());
            Logger.writeLine("obtained json: " + str.ToString());
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
                    FieldInfo f = this.GetType()
                        .GetField("_scriptdomain", BindingFlags.NonPublic | BindingFlags.Instance);
                    object domain = f.GetValue(this);
                    MethodInfo m = domain.GetType()
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
                        Weather weatherEnum = (Weather) Enum.Parse(typeof(Weather), weather);
                        GTA.World.Weather = weatherEnum;
                    }
                    catch (Exception e) {
                        Logger.writeLine(e);
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

            handlePipeInput();
            if (!enabled) return;

            //Array values = Enum.GetValues(typeof(Weather));


            switch (checkStatus()) {
                case GameStatus.NeedReload:
                    //TODO: need to get a new session and run?
                    StopRun();
                    runTask?.Wait();
                    runTask = StartRun();
                    //StopSession();
                    //Autostart();
                    UINotify("need reload game");
                    Script.Wait(100);
                    ReloadGame();
                    break;
                case GameStatus.NeedStart:
                    //TODO do the autostart manually or automatically?
                    //Autostart();
                    // use reloading temporarily
                    StopRun();

                    ReloadGame();
                    Script.Wait(100);
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

//            UINotify("going to save images and save to postgres");

            try {
                GamePause(true);
                gatherData();
                GamePause(false);
            }
            catch (Exception exception) {
                GamePause(false);
                Logger.writeLine("exception occured, logging and continuing");
                Logger.writeLine(exception);
            }

//            if time interval is enabled, checkes game time and sets it to timeFrom, it current time is after timeTo
            if (timeIntervalEnabled) {
                var currentTime = GTA.World.CurrentDayTime;
                if (currentTime > timeTo) {
                    GTA.World.CurrentDayTime = timeFrom;
                }
            }
        }

        private void gatherData() {
            if (clearEverything) {
                ClearSurroundingEverything(Game.Player.Character.Position, 1000f);
            }

            Script.Wait(100);

            var dateTimeFormat = @"yyyy-MM-dd--HH-mm-ss--fff";
            var guid = Guid.NewGuid();
            
            if (useMultipleCameras) {
                for (int i = 0; i < CamerasList.cameras.Count; i++) {
                    CamerasList.ActivateCamera(i);
                    gatherDatForOneCamera(dateTimeFormat, guid);
                    Script.Wait(5);
                }
            }
            else {
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

            if (dat == null) {
                return;
            }

            if (multipleWeathers) {
                success = saveSnapshotToFile(dat.ImageName, wantedWeathers);
            }
            else {
                Weather weather = currentWeather ? GTA.World.Weather : wantedWeather;
                success = saveSnapshotToFile(dat.ImageName, weather);
            }

            if (!success) {
//                    when getting data and saving to file failed, saving to db is skipped
                return;
            }

            PostgresExport.SaveSnapshot(dat, run.guid);            
        }
        
        /* -1 = need restart, 0 = normal, 1 = need to enter vehicle */
        public GameStatus checkStatus() {
            Ped player = Game.Player.Character;
            if (player.IsDead) return GameStatus.NeedReload;
            if (player.IsInVehicle()) {
                Vehicle vehicle = player.CurrentVehicle;
                //UINotify("T:" + Game.GameTime.ToString() + " S: " + vehicle.Speed.ToString());
                if (vehicle.Speed < 1.0f) //speed is in mph
                {
                    if (lowSpeedTime.checkTrafficJam(Game.GameTime, vehicle.Speed)) {
                        return GameStatus.NeedReload;
                    }
                }
                else {
                    lowSpeedTime.clearTime();
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
            EnterVehicle();
            Script.Wait(200);
            ToggleNavigation();
            Script.Wait(200);
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

        public void UINotify(string message) {
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

        public void EnterVehicle() {
            /*
            var vehicle = World.GetClosestVehicle(player.Character.Position, 30f);
            player.Character.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            */
            Model mod = new Model(GTA.Native.VehicleHash.Asea);
            if (mod == null) {
                UINotify("mod is null");
            }

            if (player == null) {
                UINotify("player is null");
            }

            if (player.Character == null) {
                UINotify("player.Character is null");
            }

            UINotify("player position: " + player.Character.Position.ToString());
            var vehicle = GTA.World.CreateVehicle(mod, player.Character.Position);
            if (vehicle == null) {
                UINotify("vehicle is null. Something is fucked up");
            }
            else {
                player.Character.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            }

            //vehicle.Alpha = 0; //transparent
            //player.Character.Alpha = 0;
        }

        public void ToggleNavigation() {
            //todo: probably here try to set camera, maybe by SET_FOLLOW_VEHICLE_CAM_VIEW_MODE(int viewMode), or by SET_FOLLOW_VEHICLE_CAM_ZOOM_LEVEL(int zoomLevel)
            // or just something with the GTA.GameplayCamera
            //YOLO
            MethodInfo inf =
                kh.GetType().GetMethod("AtToggleAutopilot", BindingFlags.NonPublic | BindingFlags.Instance);
            inf.Invoke(kh, new object[] {new KeyEventArgs(Keys.J)});
        }

        private void ClearSurroundingVehicles(Vector3 pos, float radius) {
            ClearSurroundingVehicles(pos.X, pos.Y, pos.Z, radius);
        }

        private void ClearSurroundingVehicles(float x, float y, float z, float radius) {
            Function.Call(GTA.Native.Hash.CLEAR_AREA_OF_VEHICLES, x, y, z, radius, false, false, false, false);
        }

        private void ClearSurroundingEverything(Vector3 pos, float radius) {
            ClearSurroundingEverything(pos.X, pos.Y, pos.Z, radius);
        }

        private void ClearSurroundingEverything(float x, float y, float z, float radius) {
            Function.Call(GTA.Native.Hash.CLEAR_AREA, x, y, z, radius, false, false, false, false);
        }

        public void ReloadGame() {
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
            ClearSurroundingVehicles(player.Position, 1000f);
            Function.Call(GTA.Native.Hash.CLEAR_AREA_OF_VEHICLES, player.Position.X, player.Position.Y,
                player.Position.Z, 1000f, false, false, false, false);
            player.LastVehicle.Delete();
            // teleport to the spawning position, defined in GameUtils.cs, subject to changes
            player.Position = GTAConst.StartPos;
            ClearSurroundingVehicles(player.Position, 100f);
            // start a new run
            EnterVehicle();
            //Script.Wait(2000);
            ToggleNavigation();

            lowSpeedTime.clearTime();
        }

        public void TraverseWeather() {
            for (int i = 1; i < 14; i++) {
                //World.Weather = (Weather)i;
                World.TransitionToWeather((Weather) i, 0.0f);
                //Script.Wait(1000);
            }
        }

        public void OnKeyDown(object o, KeyEventArgs k) {
            Logger.writeLine("VisionExport OnKeyDown called.");
            if (k.KeyCode == Keys.PageUp) {
                postgresTask?.Wait();
                postgresTask = StartSession();
                runTask?.Wait();
                runTask = StartRun();
                UINotify("GTA Vision Enabled");
//                there is set weather, just for testing
                World.Weather = wantedWeather;
            }

            if (k.KeyCode == Keys.PageDown) {
                StopRun();
                StopSession();
                UINotify("GTA Vision Disabled");
            }

            if (k.KeyCode == Keys.H) // temp modification
            {
                EnterVehicle();
                UINotify("Trying to enter vehicle");
                ToggleNavigation();
            }

            if (k.KeyCode == Keys.Y) // temp modification
            {
                ReloadGame();
            }

            if (k.KeyCode == Keys.X) // temp modification
            {
                notificationsAllowed = !notificationsAllowed;
                if (notificationsAllowed) {
                    UI.Notify("Notifications Enabled");
                }
                else {
                    UI.Notify("Notifications Disabled");
                }
            }

            if (k.KeyCode == Keys.U) // temp modification
            {
                var settings = ScriptSettings.Load("GTAVisionExport.xml");
                var loc = AppDomain.CurrentDomain.BaseDirectory;

                //UINotify(ConfigurationManager.AppSettings["database_connection"]);
                var str = settings.GetValue("", "ConnectionString");
                UINotify("BaseDirectory: " + loc);
                UINotify("ConnectionString: " + str);
            }

            if (k.KeyCode == Keys.G) // temp modification
            {
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
                        file.WriteLine(World.RenderingCamera.Direction.X.ToString() + ' ' +
                                       World.RenderingCamera.Direction.Y.ToString() + ' ' +
                                       World.RenderingCamera.Direction.Z.ToString());
                        file.WriteLine("Dot Product:");
                        file.WriteLine(Vector3.Dot(World.RenderingCamera.Direction, World.RenderingCamera.Rotation));
                        file.WriteLine("position:");
                        file.WriteLine(World.RenderingCamera.Position.X.ToString() + ' ' +
                                       World.RenderingCamera.Position.Y.ToString() + ' ' +
                                       World.RenderingCamera.Position.Z.ToString());
                        file.WriteLine("rotation:");
                        file.WriteLine(World.RenderingCamera.Rotation.X.ToString() + ' ' +
                                       World.RenderingCamera.Rotation.Y.ToString() + ' ' +
                                       World.RenderingCamera.Rotation.Z.ToString());
                        file.WriteLine("relative heading:");
                        file.WriteLine(GameplayCamera.RelativeHeading.ToString());
                        file.WriteLine("relative pitch:");
                        file.WriteLine(GameplayCamera.RelativePitch.ToString());
                        file.WriteLine("fov:");
                        file.WriteLine(GameplayCamera.FieldOfView.ToString());
                    }
                }
            }

            if (k.KeyCode == Keys.T) // temp modification
            {
                World.Weather = Weather.Raining;
                /* set it between 0 = stop, 1 = heavy rain. set it too high will lead to sloppy ground */
                Function.Call(GTA.Native.Hash._SET_RAIN_FX_INTENSITY, 0.5f);
                var test = Function.Call<float>(GTA.Native.Hash.GET_RAIN_LEVEL);
                UINotify("" + test);
                World.CurrentDayTime = new TimeSpan(12, 0, 0);
                //Script.Wait(5000);
            }

            if (k.KeyCode == Keys.N) {
                UINotify("N pressed, going to take screenshots");

                startRunAndSessionManual();
                postgresTask?.Wait();
                runTask?.Wait();
                var dateTimeFormat = @"yyyy-MM-dd--HH-mm-ss--fff";
                UINotify("starting screenshots");
                for (int i = 0; i < 5; i++) {
                    GamePause(true);
                    Script.Wait(200);

                    GTAData dat;
                    if (multipleWeathers) {
                        dat = GTAData.DumpData(DateTime.UtcNow.ToString(dateTimeFormat), wantedWeathers.ToList());
                        saveSnapshotToFile(dat.ImageName, wantedWeathers);
                    }
                    else {
                        Weather weather = currentWeather ? GTA.World.Weather : wantedWeather;
                        dat = GTAData.DumpData(DateTime.UtcNow.ToString(dateTimeFormat), weather);
                        saveSnapshotToFile(dat.ImageName, weather);
                    }

                    PostgresExport.SaveSnapshot(dat, run.guid);
                    GamePause(false);
                    Script.Wait(200); // hoping game will go on during this wait
                }

                StopRun();
                StopSession();
            }

            if (k.KeyCode == Keys.I) {
                var info = new GTAVisionUtils.InstanceData();
                UINotify(info.type);
                UINotify(info.publichostname);
            }
        }

        private bool saveSnapshotToFile(String name, Weather[] weathers) {
//            returns true on success, and false on failure
            List<byte[]> colors = new List<byte[]>();
            GamePause(true);
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

            GamePause(false);
            var res = Game.ScreenResolution;
            var fileName = Path.Combine(dataPath, name);
            ImageUtils.WriteToTiff(fileName, res.Width, res.Height, colors, depth, stencil, false);
//            UINotify("file saved to: " + fileName);
            return true;
        }

        private bool saveSnapshotToFile(String name, Weather weather) {
//            returns true on success, and false on failure
            GamePause(true);
            World.TransitionToWeather(weather,
                0.0f); //trying to set weather only in the beginning, because of depth =/= RGB
            Script.Wait(10);
            var depth = VisionNative.GetDepthBuffer();
            var stencil = VisionNative.GetStencilBuffer();
            var color = VisionNative.GetColorBuffer();
            if (depth == null || stencil == null || color == null) {
                return false;
            }

            GamePause(false);
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
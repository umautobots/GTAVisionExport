using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using GTAVisionUtils;
using IniParser;
using Newtonsoft.Json;
using Npgsql;
using Npgsql.NameTranslation;
using NpgsqlTypes;

namespace Sandbox
{
    public class WMIGraphicsInformation
    {
        public string deviceId;
        public string AdapterCompat;
        public string AdapterDACType;
        public string AdapterRAM;
        public int Availability;
        public string Caption;
        public string Description;
        public DateTime DriverDate;
        public string DriverVersion;
        public string PnPDeviceId;
        public string name;
        public string VideoArch;
        public string MemType;
        public string VideoProcessor;
        public string bpp;
        public string hrez;
        public string vrez;
        public string num_colors;
        public string cols;
        public string rows;
        public string refresh;
        public string scanMode;
        public string videoModeDesc;

        public WMIGraphicsInformation(ManagementBaseObject from)
        {
            deviceId = from.GetPropertyValue("DeviceID") as string;
            AdapterCompat = from.GetPropertyValue("AdapterCompatibility") as string;
            AdapterDACType = from.GetPropertyValue("AdapterDACType") as string;
            AdapterRAM = from.GetPropertyValue("AdapterRAM") as string;
            Caption = from.GetPropertyValue("Caption") as string;
            DriverVersion = from.GetPropertyValue("DriverVersion") as string;
            DriverDate = ManagementDateTimeConverter.ToDateTime( from.GetPropertyValue("DriverDate") as string);
            VideoProcessor = from.GetPropertyValue("VideoProcessor") as string;
            name = from.GetPropertyValue("Name") as string;

        }
    }
//    public class WMIInformation
//    {
//        public Guid system_uuid;
//        public string vendor;
//        public string dnshostname;
//        public string username;
//        public string systemtype;
//        public UInt64 totalmem;
//        public List<WMIGraphicsInformation> gfxCards;
//        /// <summary>
//        /// gets wmi info for the current computer
//        /// </summary>
//        public WMIInformation()
//        {
//            var scope = new ManagementScope("ROOT\\CIMV2");
//            var genQuery = new ObjectQuery("SELECT * FROM Win32_ComputerSystem");
//            var result = new ManagementObjectSearcher(scope, genQuery).Get().Cast<ManagementBaseObject>();
//            dnshostname = result.First().GetPropertyValue("DNSHostName") as string;
//            username = result.First().GetPropertyValue("UserName") as string;
//            systemtype = result.First().GetPropertyValue("SystemType") as string;
//            totalmem = (UInt64) result.First().GetPropertyValue("TotalPhysicalMemory");
//            var prodQuery = new ObjectQuery("SELECT * FROM Win32_ComputerSystemProduct");
//            result = new ManagementObjectSearcher(scope, prodQuery).Get().Cast<ManagementBaseObject>();
//            system_uuid = Guid.Parse( result.First().GetPropertyValue("UUID") as string);
//            vendor = result.First().GetPropertyValue("Vendor") as string;
//
//            var videoQuery = new ObjectQuery("SELECT * FROM Win32_VideoController");
//            result = new ManagementObjectSearcher(scope, videoQuery).Get().Cast<ManagementBaseObject>();
//            gfxCards = new List<WMIGraphicsInformation>();
//            foreach (var obj in result)
//            {
//                gfxCards.Add(new WMIGraphicsInformation(obj));
//            }
//
//        }
//    }
    
    public enum DetectionType
    {
        background,
        person,
        car,
        bicycle
    }

    public enum DetectionClass
    {
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

    public enum Weather
    {
        Unknown = -1,
        ExtraSunny = 0,
        Clear = 1,
        Clouds = 2,
        Smog = 3,
        Foggy = 4,
        Overcast = 5,
        Raining = 6,
        ThunderStorm = 7,
        Clearing = 8,
        Neutral = 9,
        Snowing = 10,
        Blizzard = 11,
        Snowlight = 12,
        Christmas = 13,
        Halloween = 14,
    }

    public class MainClass
    {
        public static void InitSQLTypes()
        {
            NpgsqlConnection.MapEnumGlobally<Weather>("weather", new NpgsqlNullNameTranslator());
            NpgsqlConnection.MapEnumGlobally<DetectionType>();
            NpgsqlConnection.MapEnumGlobally<DetectionClass>(pgName: "detection_class",
                nameTranslator: new NpgsqlNullNameTranslator());
        }

        public static NpgsqlConnection OpenConnection()
        {
            var parser = new FileIniDataParser();
            var location = @"D:\Program Files\Rockstar Games\Grand Theft Auto V\scripts";
            var data = parser.ReadFile(Path.Combine(location, "GTAVision.ini"));

            //UI.Notify(ConfigurationManager.AppSettings["database_connection"]);
            var str = data["Database"]["ConnectionString"];
            
            var conn = new NpgsqlConnection(str);
            conn.Open();
            return conn;
        }

        public static void InsertEnum()
        {
            var conn = OpenConnection();
            var trans = conn.BeginTransaction();
            using (NpgsqlCommand cmd = new NpgsqlCommand())
            {

                cmd.Connection = conn;
                cmd.Transaction = trans;
                //UI.Notify(data.CurrentWeather.ToString());
                cmd.Parameters.Clear();
                cmd.CommandText =
                    "INSERT INTO test_enums (detection_type, weather_type, detection_class) VALUES (@type, @weather, @class);";
//                cmd.CommandText =
//                    "INSERT INTO test_enums (detection_type) VALUES (@type);";
                cmd.Parameters.AddWithValue("@weather", NpgsqlDbType.Enum, Weather.Foggy);
//                cmd.Parameters.AddWithValue("@type", NpgsqlDbType.Enum, DetectionType.background);
                cmd.Parameters.AddWithValue("@type", NpgsqlDbType.Enum, DetectionType.bicycle);
                cmd.Parameters.AddWithValue("@class", NpgsqlDbType.Enum, DetectionClass.Commercial);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                trans.Commit();
                conn.Close();
            }
        }
                
        public static int InsertInstanceData(NpgsqlConnection conn)
        {
            
//            var instanceinfo = new InstanceData();
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                
//                cmd.Parameters.AddWithValue("@host", System.Environment.MachineName);
//                cmd.Parameters.AddWithValue("@iid", DBNull.Value);
//                cmd.Parameters.AddWithValue("@typ", instanceinfo.type);
//                cmd.Parameters.AddWithValue("@pubhost", DBNull.Value);
//                cmd.Parameters.AddWithValue("@amiid", DBNull.Value);
//                
//                if (instanceinfo.type != "LOCALHOST")
//                {
//                    cmd.Parameters.AddWithValue("@host", instanceinfo.hostname);
//                    cmd.Parameters.AddWithValue("@iid", instanceinfo.instanceid);
//                    cmd.Parameters.AddWithValue("@typ", instanceinfo.type);
//                    cmd.Parameters.AddWithValue("@pubhost", instanceinfo.publichostname);
//                    cmd.Parameters.AddWithValue("@amiid", instanceinfo.amiid);
//                }
                cmd.CommandText =
                    "SELECT instance_id FROM instances WHERE hostname=@host AND instancetype=@typ AND instanceid=@iid AND amiid=@amiid AND publichostname=@pubhost";
                var id = cmd.ExecuteScalar();
                if (id == null)
                {
                    cmd.CommandText =
                        "INSERT INTO instances (hostname, instanceid, instancetype, publichostname, amiid) VALUES (@host, @iid, @typ, @pubhost, @amiid) " +
                        "RETURNING instance_id";
                    return (int) cmd.ExecuteScalar();
                }

                return (int) id;

            }
        }

        public static Guid InsertSystemData(NpgsqlConnection conn)
        {
            var systemInfo = new WMIInformation();
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@system_uuid", systemInfo.system_uuid);
                cmd.Parameters.AddWithValue("@vendor", systemInfo.vendor);
                cmd.Parameters.AddWithValue("@dnshostname", systemInfo.dnshostname);
                cmd.Parameters.AddWithValue("@username", systemInfo.username);
                cmd.Parameters.AddWithValue("@systemtype", systemInfo.systemtype);
                cmd.Parameters.AddWithValue("@totalmem", NpgsqlDbType.Bigint, systemInfo.totalmem);
//                cmd.CommandText =
//                    "INSERT INTO systems (system_uuid, vendor, dnshostname, username, systemtype, totalmem) VALUES " +
//                    "(@system_uuid, @vendor, @dnshostname, @username, @systemtype, @totalmem) ON CONFLICT DO NOTHING RETURNING system_uuid";
                cmd.CommandText =
                    "INSERT INTO systems (system_uuid, vendor, dnshostname, username, systemtype, totalmem) VALUES " +
                    "(@system_uuid, @vendor, @dnshostname, @username, @systemtype, @totalmem) ON CONFLICT(system_uuid) DO UPDATE SET system_uuid = EXCLUDED.system_uuid RETURNING system_uuid";
                return Guid.Parse(cmd.ExecuteScalar().ToString());
            }
        }

        public static async void Printing(int i)
        {
            await Task.Run(() => PrintingImpl(i));
        }
        
        public static void PrintingImpl(int i)
        {
            System.Threading.Thread.Sleep(1000); // wait 1 second
            Console.WriteLine(i);
        }

        public static async void Printing(string i)
        {
            await Task.Run(() => PrintingImpl(i));
        }
        
        public static void PrintingImpl(string i)
        {
            System.Threading.Thread.Sleep(1000); // wait 1 second
            Console.WriteLine(i);
        }

        public static async void Printing(byte[] i)
        {
            await Task.Run(() => PrintingImpl(i));
        }
        
        public static void PrintingImpl(byte[] i)
        {
            System.Threading.Thread.Sleep(1000); // wait 1 second
            Console.WriteLine(BitConverter.ToString(i));
        }

        public static void tryThreads()
        {
//            var i = 1;
//            var i = "h";
            var i = new byte[] { 0x20, 0x20, 0x20, 0x20};
            Printing(i);
            System.Threading.Thread.Sleep(500);
//            i = 2;
//            i = "he";
            i = new byte[] { 0x30, 0x30, 0x30, 0x30};
            Printing(i);
            System.Threading.Thread.Sleep(500);
//            i = 3;
//            i = "hello";
            i = new byte[] { 0x40, 0x40, 0x40, 0x40};
            Printing(i);
            System.Threading.Thread.Sleep(3000);
        }
        
        public static void Main(string[] args)
        {
//            tryThreads();
            
            string weather = "Clear";
//            string weather = "Overcast";
            var weatherEnum = Enum.Parse(typeof(Weather), weather);
            Console.WriteLine(weatherEnum);
//            var dateTimeFormat = @"dd-MM-yyyy--HH-mm-ss";
//            var dateTimeFormat = @"dd-MM-yyyy--HH-mm-ss-fff";
//            for (int i = 0; i < 1000; i++)
//            {
//                var fileName = DateTime.UtcNow.ToString(dateTimeFormat) + ".tiff";
//                Console.WriteLine(fileName);
//            }
//            var systemInfo = new WMIInformation();
//            Console.WriteLine(systemInfo.dnshostname);
//            Console.WriteLine(systemInfo.username);
//            var conn = OpenConnection();
//            conn = null;
//            int instanceid = InsertInstanceData(conn);
//            InsertSystemData(conn);

//            InitSQLTypes();
//            InsertEnum();
//            var str = "{\"name\": \"SET_TIME\", \"time\": \"05:20\"}";
//            dynamic parameters = JsonConvert.DeserializeObject(str);
//            var time = parameters.time.Value<string>();
//            var hoursAndMinutes = time.Split(':');
//            var hours = int.Parse(hoursAndMinutes[0]);
//            var minutes = int.Parse(hoursAndMinutes[1]);
//            var timeSpan = new TimeSpan(hours, minutes, 0);

            Console.WriteLine("the end");

        }
    }
}
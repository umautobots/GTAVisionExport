using System;
using System.IO;
using GTAVisionUtils;
using IniParser;
using Npgsql;
using Npgsql.NameTranslation;
using NpgsqlTypes;

namespace Sandbox
{
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
            var location = AppDomain.CurrentDomain.BaseDirectory;
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

        public static void Main(string[] args)
        {
            var systemInfo = new GTAVisionUtils.WMIInformation();
            InitSQLTypes();
//            InsertEnum();
        }
    }
}
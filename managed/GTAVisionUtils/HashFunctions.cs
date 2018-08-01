using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Point = System.Windows.Markup;

namespace GTAVisionUtils
{
    public class HashFunctions
    {
        public static Vector2 Convert3dTo2d(Vector3 pos)
        {
            OutputArgument tmpResX = new OutputArgument();
            OutputArgument tmpResY = new OutputArgument();
            if (Function.Call<bool>(Hash._WORLD3D_TO_SCREEN2D, (InputArgument) pos.X,
                (InputArgument) pos.Y, (InputArgument) pos.Z,
                (InputArgument) tmpResX, (InputArgument) tmpResY))
            {
                Vector2 v2;
                v2.X = tmpResX.GetResult<float>();
                v2.Y = tmpResY.GetResult<float>();
                return v2;
            }
            return new Vector2(-1f, -1f);
        }
        public static void Draw3DLine(Vector3 iniPos, Vector3 finPos, byte col_r = 255, byte col_g = 255, byte col_b = 255, byte col_a = 255) {
            Function.Call(Hash.DRAW_LINE, new InputArgument[]
            {
                iniPos.X,
                iniPos.Y,
                iniPos.Z,
                finPos.X,
                finPos.Y,
                finPos.Z,
                (int)col_r,
                (int)col_g,
                (int)col_b,
                (int)col_a
            });
        }

        public static void Draw3DBox(Vector3 pos, Vector3 size, byte col_r = 255, byte col_g = 255, byte col_b = 255, byte col_a = 255) {
            Function.Call(Hash.DRAW_BOX, new InputArgument[] {
                pos.X - size.X / 2,
                pos.Y - size.Y / 2,
                pos.Z - size.Z / 2,
                pos.X + size.X / 2,
                pos.Y + size.Y / 2,
                pos.Z + size.Z / 2,
                (int)col_r,
                (int)col_g,
                (int)col_b,
                (int)col_a
            });
        }
        
        public static void Draw3DLine(Vector3 iniPos, Vector3 finPos, Color color) {
            Draw3DLine(iniPos, finPos, color.R, color.G, color.B, color.A);
        }
        
        public static void Draw3DLine(Vector3 iniPos, Vector3 finPos, Color color, byte a) {
            Draw3DLine(iniPos, finPos, color.R, color.G, color.B, a);
        }
        
        public static void DrawRect(float x, float y, float w, float h, byte r = 255, byte g = 255, byte b = 255, byte a = 255) {
            Function.Call(Hash.DRAW_RECT, new InputArgument[] {
                x, y,
                w, h,
                (int)r, (int)g, (int)b, (int)a
            });
        }

        public static void DrawRect(float x, float y, float w, float h, Color color) {
            DrawRect(x, y, w, h, color.R, color.G, color.B, color.A);
        }

        public static void DrawRect(float x, float y, float w, float h, Color color, byte a) {
            DrawRect(x, y, w, h, color.R, color.G, color.B, a);
        }
        
        public static void Draw2DText(string text, Vector2 pos, Color color) {
            Draw2DText(text, pos.X, pos.Y, color.R, color.G, color.B, color.A);
        }
        
        public static void Draw2DText(string text, Vector3 pos, Color color) {
            Draw2DText(text, Convert3dTo2d(pos), color);
        }
        
        public static void Draw2DText(string text, float x, float y, Color color) {
            Draw2DText(text, x, y, color.R, color.G, color.B, color.A);
        }

        public static void Draw2DText(string text, float x, float y, byte r = 255, byte g = 255, byte b = 255, byte a = 255) {
            Function.Call(Hash.SET_TEXT_FONT, 0);
            Function.Call(Hash.SET_TEXT_SCALE, 0.3f, 0.3f);
            Function.Call(Hash.SET_TEXT_COLOUR, (int)r, (int)g, (int)b, (int)a);
            Function.Call(Hash.SET_TEXT_CENTRE, 1);
            Function.Call(Hash._SET_TEXT_ENTRY, "STRING");
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, text);
            Function.Call(Hash._DRAW_TEXT, x, y);
        }

        public static void Draw2DText(string text, float x, float y, Color color, byte a) {
            Draw2DText(text, x, y, color.R, color.G, color.B, a);
        }

//        public static void SetCameraRotation(Camera camera, Vector3 value) {
//            InputArgument[] inputArgumentArray = new InputArgument[5];
//            InputArgument inputArgument1 = new InputArgument(camera.Handle);
//            inputArgumentArray[0] = inputArgument1;
//            InputArgument inputArgument2 = new InputArgument(value.X);
//            inputArgumentArray[1] = inputArgument2;
//            InputArgument inputArgument3 = new InputArgument(value.Y);
//            inputArgumentArray[2] = inputArgument3;
//            InputArgument inputArgument4 = new InputArgument(value.Z);
//            inputArgumentArray[3] = inputArgument4;
//            InputArgument inputArgument5 = new InputArgument(3);
//            inputArgumentArray[4] = inputArgument5;
//            Function.Call(Hash.SET_CAM_ROT, inputArgumentArray);
//        }

        public static void SetNewWaypoint(Vector2 point) {
            Function.Call(Hash.SET_NEW_WAYPOINT, point.X, point.Y);
        }

        public static void PlaceObjectOnGroundProperly(Entity entity) {
            Function.Call(Hash.PLACE_OBJECT_ON_GROUND_PROPERLY, entity);
        }
    }
}

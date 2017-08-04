**GTAVisionExport (native) build steps**
-------------------------------------
Note: there may be some unessesary steps that could be removed from this procedure at some point, but this is what I did. Also I have a tendency write Linux style paths. All paths (except URLs) should be Windows style.

My set-up:
----------
GTX770, Windows 10 64bit, Visual Studio 2017 (community edition), cmake 3.9.0, GTAV version 1.0.231.0

Needed tools and libraries
--------------------------
AsiLoader and ScriptHookV	: http://www.dev-c.com/files/ScriptHookV_1.0.1103.2.zip
cmake					: https://cmake.org/download/
Eigen3					: http://bitbucket.org/eigen/eigen/get/3.3.4.zip

Build steps
-----------
1. git clone https://github.com/umautobots/GTAVisionExport (**latest!** as of today)
2. Extract Eigen3 somwhere convenient (Your GTAVisionExport folder is as good as any).
3. Extract ScriptHookV archive and drop the files in 'bin' into your GTAV exe folder.
4. Run cmake (cmake-gui) from your Windows start menu.
5. Hit 'Browse Source' and select your GTAVisionExport/native folder.
6. Hit 'configure' (first time around it will fail but dont worry).
7. Choose project generator 'Visual Studio 15 2017 Win64' and keep the option 'use default native compilers'
8. After the fail dialog, modify the EIGEN3_INCLUDE_DIR to point to your Eigen3 folder.
9. Run 'configure' followed by 'generate'.
10. cmake should now have generated the Visual Studio solution into GTAVisionExport/build.
11. Open 'GTANativePlugin.sln' in Visual Studio.
12. Select 'release' from the 'Solution Configurations' drop down.
13. Edit GTAVisionNative project properties/configuration properties/c/c++/additional include dirs in VS to add the GTAVisionExport/src folder (this allows VS to find MinHook.h)
14. Edit GTAVisionNative project properties/configuration properties/linker/additional dependencies to add :
`"..\..\deps\libMinHook.x64.lib"` 
15. Press F6 to build the solution. it should now succeed and the products should be in 'GTAVisionExport\native\build\src\Release'
16. Copy GTAVisionNative.asi & GTAVisionNative.lib to your GTAV exe folder.
17. Run GTAV.
18. Get to a place where you want to grab frames and press 'l' (lowercase 'L') to grab a frame. GTAVisionExport should now create color.raw, stencil.raw and depth.raw files in your GTAV exe folder.

HTH

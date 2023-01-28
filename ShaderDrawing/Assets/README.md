# ShaderDrawing Unity Project
In this unity project, there are three folders under Scenes folder, and each of them contains a scene, several shaders and materials, and a C# script attached to the main camera in the scene.
- Scene0_OnGUI
- Scene0_PrevNotSave
- Scene1_PrevSaved

### Scene0_PrevNotSave
In this scene, ClickShow.cs script is attached to the main camera and responsible for coloring pixels when mouse clicked on the game window. It mainly uses Graphics.Blit function in the OnRenderImage function to render image on the camera.

### Scene0_OnGUI
Drawing.cs is attached to the main camera, in which the rendering process is written in OnGUI function. 

### Scene1_PrevSaved
In this scene, the previous drawn points are stored and shown when mouse drags. DrawSaved.cs is the script attached to the main camera. The rendering process is done in OnGUI function. It renders the updated render texture to the screen.

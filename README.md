# Face-3D

This is an AR mobile application made with Unity Engine. Face-3D app uses Unity ARFoundation along with ARCORE to create a 3D model user face. The key features in the app/code are:
1.	The face is first detected by AugmentedFace feature of ARCORE.
2.	The detected face mesh data is sorted as vertices, UV Coordinates, Triangle Indices in respective .txt files in default app path.
3.	The face image is rasterised from taking screenshot of the application and then mapping the world space vertices to screenshot positions.
4.	The rasterised texture is also created.

Notes:

The project files do not contain either TextMeshPro or ARFoundation packages. To make project work, use the default package install system by unity.

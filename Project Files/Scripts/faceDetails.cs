using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class faceDetails : MonoBehaviour
{
    // Start is called before the first frame update
    string s;
    Camera cam;
    Vector2 screenPoint = new Vector2();
    Vector2[] screenPointInfo;
    bool isDoFile = false;
    Texture2D tex;
    Texture2D screenshotTex;
    Mesh faceMesh;
    GameObject button;
    public int texSize = 1024;
    void Start()
    {
        cam = GameObject.Find("AR Camera").GetComponent<Camera>();
        tex = new Texture2D(texSize,texSize);
        screenshotTex = new Texture2D(cam.pixelWidth, cam.pixelHeight);
        button = GameObject.Find("Button");
        button.GetComponent<Button>().onClick.RemoveAllListeners();
        button.GetComponent<Button>().onClick.AddListener(drawFaceOnTex);
    }
    void checkingOfFiles()
    {
        if (System.IO.File.Exists(Application.persistentDataPath + "/screenpointinfo.png"))
        {
            System.IO.File.Delete(Application.persistentDataPath + "/screenpointinfo.png");
        }
        if (System.IO.File.Exists(Application.persistentDataPath + "/screenshot.png"))
        {
            System.IO.File.Delete(Application.persistentDataPath + "/screenshot.png");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (this.GetComponent<ARFaceMeshVisualizer>().mesh.vertexCount > 0 && !isDoFile)
        {
            GameObject.Find("ButtonCanvas").GetComponent<Canvas>().enabled = true;
            s = "Face Found";
            GameObject.Find("detailsLabel").GetComponent<TextMeshProUGUI>().text = s;
        }
    }
    public void writeMeshData()
    {
        //for vertices

        StreamWriter sr = File.CreateText(Application.persistentDataPath + "/verticesInfo.txt");
        sr.WriteLine("" + faceMesh.vertexCount);
        for(int i=0; i<faceMesh.vertexCount;i++)
        {
            sr.WriteLine("" + faceMesh.vertices[i].x + "," + faceMesh.vertices[i].y + "," + faceMesh.vertices[i].z);
        }
        sr.Close();

        //for uv

        sr = File.CreateText(Application.persistentDataPath + "/uvInfo.txt");
        sr.WriteLine("" + faceMesh.vertexCount);
        for (int i = 0; i < faceMesh.vertexCount; i++)
        {
            sr.WriteLine("" + faceMesh.uv[i].x + "," + faceMesh.uv[i].y);
        }
        sr.Close();

        //for triangleIndices

        sr = File.CreateText(Application.persistentDataPath + "/triangleInfo.txt");
        sr.WriteLine("" + faceMesh.triangles.Length);
        for (int i = 0; i < faceMesh.triangles.Length; i++)
        {
            sr.WriteLine("" + faceMesh.triangles[i]);
        }
        sr.Close();
    }
    public void drawFaceOnTex()
    {
        if (this.GetComponent<ARFaceMeshVisualizer>().mesh.vertexCount > 0 && !isDoFile)
        {
            s = "Face Found";
            GameObject.Find("detailsLabel").GetComponent<TextMeshProUGUI>().text = s;
            checkingOfFiles();
            faceMesh = Instantiate(this.GetComponent<ARFaceMeshVisualizer>().mesh);
            screenPointInfo = new Vector2[faceMesh.vertexCount];
            ScreenCapture.CaptureScreenshot("screenshot.png");
            for (int i = 0; i < faceMesh.vertexCount; i++)
            {
                screenPoint = cam.WorldToScreenPoint(transform.TransformPoint((Vector3)faceMesh.vertices.GetValue(i)));
                screenPointInfo[i].x = Mathf.RoundToInt(screenPoint.x);
                screenPointInfo[i].y = Mathf.RoundToInt(screenPoint.y);
            }
            s = "Face processing is Done";
            GameObject.Find("detailsLabel").GetComponent<TextMeshProUGUI>().text = s;
            isDoFile = !isDoFile;
        }
        if (!isDoFile)
        {
            s = "Face processing Not Yet Done";
            GameObject.Find("detailsLabel").GetComponent<TextMeshProUGUI>().text = s;
            return;
        }
        if (!System.IO.File.Exists(Application.persistentDataPath + "/screenshot.png"))
        {
            s = "Face data is not yet saved.\nPlease press again.";
            GameObject.Find("detailsLabel").GetComponent<TextMeshProUGUI>().text = s;
            return;
        }
        if (System.IO.File.Exists(Application.persistentDataPath + "/screenshot.png"))
        {
            byte[] screeshotImage =  File.ReadAllBytes(Application.persistentDataPath + "/screenshot.png");
            ImageConversion.LoadImage(screenshotTex, screeshotImage);
        }      
        
        s = "Face data saving Started.";
        GameObject.Find("detailsLabel").GetComponent<TextMeshProUGUI>().text = s;
        Color color;
        Vector2 scale = new Vector2(texSize,texSize);

        for (int i = 0; i < faceMesh.triangles.Length; i = i + 3)
        {            
            //for facetexture
            Vector2 p1 = faceMesh.uv[faceMesh.triangles[i]];
            Vector2 p2 = faceMesh.uv[faceMesh.triangles[i + 1]];
            Vector2 p3 = faceMesh.uv[faceMesh.triangles[i + 2]];
            
            //for screenshot
            Vector2 ps1 = screenPointInfo[faceMesh.triangles[i]];
            Vector2 ps2 = screenPointInfo[faceMesh.triangles[i + 1]];
            Vector2 ps3 = screenPointInfo[faceMesh.triangles[i + 2]];
            
            float Hscale;
            float Vscale;
            
            p1.Scale(scale);
            p2.Scale(scale);
            p3.Scale(scale);
            
            Line l1 = new Line(p1, p2);
            Line l2 = new Line(p2, p3);
            Line l3 = new Line(p3, p1);
            
            Line ls1 = new Line(ps1, ps2);
            Line ls2 = new Line(ps2, ps3);
            Line ls3 = new Line(ps3, ps1);

            Line l4 = l1.getPerpendicularLine(p3);
            Line ls4 = ls1.getPerpendicularLine(ps3);

            IntersectionPoints ip1 = new IntersectionPoints(l1, l4);
            IntersectionPoints ips1 = new IntersectionPoints(ls1, ls4);

            Vscale = getDistance(ps3, ips1.getInteresectionPoint()) / getDistance(p3, ip1.getInteresectionPoint());

            List<Vector2> altLinePoints = l4.getBoundingPoints(p3, ip1.getInteresectionPoint());
            foreach(Vector2 point in altLinePoints)
            {
                Line l5 = l1.getParallelLine(point);
                IntersectionPoints ip2 = new IntersectionPoints(l5, l2);
                IntersectionPoints ip3 = new IntersectionPoints(l5, l3);

                Vector2 point1 = ls4.getPointOnLineByDistance(ps3, ips1.getInteresectionPoint(), getDistance(p3, point) * Vscale);
                Line ls5 = ls1.getParallelLine(point1);
                IntersectionPoints ips2 = new IntersectionPoints(ls5, ls2);
                IntersectionPoints ips3 = new IntersectionPoints(ls5, ls3);

                Hscale = getDistance(ips2.getInteresectionPoint(), ips3.getInteresectionPoint()) / getDistance(ip2.getInteresectionPoint(), ip3.getInteresectionPoint());

                List<Vector2> parallelLinePoints = l5.getBoundingPoints(ip2.getInteresectionPoint(), ip3.getInteresectionPoint());
                foreach(Vector2 subPoint in parallelLinePoints)
                {
                    Vector2 subPoint1 = ls5.getPointOnLineByDistance(ips2.getInteresectionPoint(), ips3.getInteresectionPoint(), getDistance(ip2.getInteresectionPoint(), subPoint) * Hscale);
                    color = screenshotTex.GetPixel(Mathf.RoundToInt(subPoint1.x), Mathf.RoundToInt(subPoint1.y));
                    tex.SetPixel(Mathf.RoundToInt(subPoint.x), Mathf.RoundToInt(subPoint.y), color);
                }
            }
            
            
        }
        tex.Apply();
        System.IO.File.WriteAllBytes(Application.persistentDataPath + "/screenpointinfo.png", tex.EncodeToPNG());
        s = "Face data is saved.";
        GameObject.Find("detailsLabel").GetComponent<TextMeshProUGUI>().text = s;
        writeMeshData();
        SceneManager.LoadScene("FaceDisplay", LoadSceneMode.Single);
    }
    public float getDistance(Vector2 p1, Vector2 p2)
    {
        return Mathf.Sqrt((p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y));
    }
    public class IntersectionPoints //For determination of intersection points for given two lines. It also ensures that for given two lines, intersection is possible or not.
    {
        public bool isIntersectionPointAvaialble;
        public float x;
        public float y;
        public IntersectionPoints(Line l1, Line l2)
        {
            if (l1.isLinePerpendicular && l2.isLinePerpendicular)
            {
                isIntersectionPointAvaialble = false;
            }
            else if (l1.isLinePerpendicular && !l2.isLinePerpendicular)
            {
                isIntersectionPointAvaialble = true;
                x = l1.xintercept;
                y = l2.slope * x + l2.intercept;
            }
            else if (!l1.isLinePerpendicular && l2.isLinePerpendicular)
            {
                isIntersectionPointAvaialble = true;
                x = l2.xintercept;
                y = l1.slope * x + l1.intercept;
            }
            else if (l1.slope == l2.slope)
            {
                isIntersectionPointAvaialble = false;
            }
            else
            {
                isIntersectionPointAvaialble = true;
                x = (l2.intercept - l1.intercept) / (l1.slope - l2.slope);
                y = l1.slope * x + l1.intercept;
            }
        }
        public Vector2 getInteresectionPoint()
        {
            return new Vector2(x, y);
        }
    }
    public class Line
    {
        public float slope;
        public bool isLinePerpendicular;
        public float intercept;
        public float xintercept;
        public Vector2 p1 = new Vector2();
        public Vector2 p2 = new Vector2();
        public Line()
        {
            //empty constructor;
        }
        public Line(Vector2 po1, Vector2 po2) //p1 and p2 are points
        {
            this.p1 = po1;
            this.p2 = po2;
            findLineAttributes();
        }
        public float getDistance(Vector2 p1, Vector2 p2)
        {
            return Mathf.Sqrt((p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y));
        }
        public float returnYValue(float x) //returns y value of the line for a given x value. But care has to be ensure that line is not perpendicular
        {
            return this.slope * x + this.intercept;   
        }
        public float returnXValue(float y) //same as returnYvalue() but here subject is x.
        {
            return (y-this.intercept)/ this.slope;
        }
        public void findLineAttributes() //to find slope and intercept and find whether line is perpendicular or not to x-axis.
        {
            //for slope
            float deltaX = p2.x - p1.x;
            float deltaY = p2.y - p1.y;
            if (deltaX==0f)
            {
                isLinePerpendicular = true;
            }
            else
            {
                isLinePerpendicular = false;
                slope = deltaY / deltaX;
            }
            //for intercept
            float numarator = (p1.y * p2.x) - (p2.y * p1.x);
            if(deltaX!=0f)
            {
                intercept = numarator / deltaX;
            }
            else
            {
                xintercept = p1.x;
            }
        }
        public Vector2 getPointOnLineByDistance(Vector2 ps1, Vector2 ps2 ,float distance)
        {
            Vector2 returnVect = new Vector2();

            if(this.isLinePerpendicular)
            {
                returnVect.x = ps1.x;
                returnVect.y = ps1.y + distance / (ps2.y - ps1.y);
            }
            else
            {
                returnVect.x = ps1.x + distance / getDistance(ps1, ps2) * (ps2.x - ps1.x);
                returnVect.y = ps1.y + distance / getDistance(ps1, ps2) * (ps2.y - ps1.y);
            }

            return returnVect;
        }
        public float getLineLength()
        {
            return Mathf.Sqrt((p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y)* (p2.y - p1.y));
        }
        public Line getParallelLine(Vector2 p3) // to get a parallel line to the current line which passes through given point
        {
            Line line = new Line();
            if(isLinePerpendicular)
            {
                line.isLinePerpendicular = true;
                line.xintercept = p3.x;
            }
            else
            {
                line.slope = this.slope;
                line.intercept = p3.y - line.slope * p3.x;
            }
            return line;
        }
        public Line getPerpendicularLine(Vector2 p3) // to get a perpendicular line to the current line which passes through given point
        {
            Line line = new Line();
            if(isLinePerpendicular)
            {
                line.slope = 0f;
                line.intercept = p3.y;
            }
            else if (slope==0f)
            {
                line.isLinePerpendicular = true;
                line.xintercept = p3.x;
            }
            else
            {
                line.slope = - 1f / slope;
                line.intercept = p3.y - line.slope * p3.x;
            }
            return line;
        }
        
        public IntersectionPoints getIntersectionpoints(Line l2)
        {
            IntersectionPoints ip1 = new IntersectionPoints(this, l2);
            return ip1;
        }
        public List<Vector2> getBoundingPoints(Vector2 po1, Vector2 po2) 
            //getting points between two given points in this line. However no check for point to be on line is done here.
        {
            List<Vector2> vectList = new List<Vector2>();
            float stepSize = 0.5f;
            float x1, x2, y1, y2, deltaX, deltaY;
            x1 = (po1.x);
            x2 = (po2.x);
            y1 = (po1.y);
            y2 = (po2.y);
            deltaX = x2 - x1;
            deltaY = y2 - y1;
            if (!this.isLinePerpendicular) // the perpendicular nature of line is taken care of.
            {
               if(Mathf.Abs(deltaX)>Mathf.Abs(deltaY)) //here the logic ensure that we get maximum no. of points in return.
                {
                    if(deltaX>0f)
                    {
                        for (float i = x1; i <= x2; i=i+stepSize)
                        {
                            vectList.Add(new Vector2(i, this.returnYValue(i)));
                        }
                    }
                    else
                    {
                        for (float i = x2; i <= x1; i = i + stepSize)
                        {
                            vectList.Add(new Vector2(i, this.returnYValue(i)));
                        }
                    }
                }
               else
                {
                    if (deltaY > 0f)
                    {
                        for (float i = y1; i <= y2; i = i + stepSize)
                        {
                            vectList.Add(new Vector2(this.returnXValue(i),i));
                        }
                    }
                    else
                    {
                        for (float i = y2; i <= y1; i = i + stepSize)
                        {
                            vectList.Add(new Vector2(this.returnXValue(i), i));
                        }
                    }
                }
            }
            else
            {
                if (deltaY > 0f)
                {
                    for (float i = y1; i <= y2; i = i + stepSize)
                    {
                        vectList.Add(new Vector2(x1, i));
                    }
                }
                else
                {
                    for (float i = y2; i <= y1; i = i + stepSize)
                    {
                        vectList.Add(new Vector2(x1, i));
                    }
                }
            }
            return vectList;
        }
    }
}

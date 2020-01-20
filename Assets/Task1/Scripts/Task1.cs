using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class Task1 : MonoBehaviour
{
    // Config file with Images Paths
    public string configPath = "/Task1/InputFiles/config.xml";  

    public RenderTexture rTexA, rTexB, rTexC;
    LoadConfigXML loadConfigXML;
    public MeshRenderer renderPlane;

    void Start()
    {
        loadConfigXML = LoadConfigXML.Load(Application.dataPath + configPath);

        LoadTexturesFromDisk();
    }

    public void LoadTexturesFromDisk()
    {
        
        Texture2D texA = new Texture2D(256,256);
        Texture2D texB = new Texture2D(256, 256);
        byte[] fileBytes;

        // Loading Texture A
        if (File.Exists(Application.dataPath + loadConfigXML.pathA))
        {
            fileBytes = File.ReadAllBytes(Application.dataPath + loadConfigXML.pathA);
            texA.LoadImage(fileBytes);

        }
        else
            Debug.LogError("Invalid Path");

        // Loading Texture B
        if (File.Exists(Application.dataPath +loadConfigXML.pathB))
        {
            fileBytes = File.ReadAllBytes(Application.dataPath +loadConfigXML.pathB);
            texB.LoadImage(fileBytes);
        }
        else
            Debug.LogError("Invalid Path");
        
        //Applying To RenderTextures
        Graphics.Blit(texA, rTexA);
        Graphics.Blit(texB, rTexB);
    }

    // Function Against UI Button for subtracting textures
    public void SubtractTextures()
    {
        renderPlane.enabled = true;
        StartCoroutine(SaveTextureToDisk());
    }

    //Co-routine to save render-texture to disk
    public IEnumerator SaveTextureToDisk()
    {
        yield return new WaitForEndOfFrame();
        RenderTexture.active = rTexC;
        Texture2D texC = new Texture2D(rTexC.width, rTexC.height, TextureFormat.RGB24, false);
        texC.ReadPixels(new Rect(0, 0, rTexC.width, rTexC.height), 0, 0);
        texC.Apply();

        byte[] bytes = texC.EncodeToPNG();
        Object.Destroy(texC);
        string path = Application.dataPath + "/Task1/OutputFiles/TextureC.png";
        File.WriteAllBytes(path, bytes);
        Debug.Log("File Saved at:" + path);
    }






}

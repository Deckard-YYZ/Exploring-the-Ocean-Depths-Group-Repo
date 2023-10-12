using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUnderWater : MonoBehaviour
{
    // Start is called before the first frame update
    public Shader shader;
    [Header("Depth Effect")]
    public Color depthColor = new Color(0, 0.42f, 0.87f);
    public float depthStart = -12, depthEnd = 98;
    public LayerMask depthLayers = ~0;

    private Camera m_Cam, m_depthCam;
    private RenderTexture m_DepthTexture, m_ColorTexture;
    private Material m_Material;
    void Start()
    {
        m_Cam = GetComponent<Camera>();
        
        //Make our camera send depth information (i.e. how far a pixel if from the screen) to the shader
        //m_Cam.depthTextureMode = DepthTextureMode.Depth;
        
        //create a new material for the shader if it has none
        if (shader) m_Material = new Material(shader);

        //a improve from 6 lines above, create render textures for the camera to save color and depth information
        //prevent the camera from rendering onto the game scene
        m_DepthTexture = RenderTexture.GetTemporary(m_Cam.pixelWidth, m_Cam.pixelHeight, 16, RenderTextureFormat.Depth);
        m_ColorTexture = RenderTexture.GetTemporary(m_Cam.pixelWidth, m_Cam.pixelHeight, 0, RenderTextureFormat.Default);
        
        //create depthCam and parent it to main cam
        GameObject go = new GameObject("Depth Cam");
        m_depthCam = go.AddComponent<Camera>();
        go.transform.SetParent(transform);
        go.transform.position = transform.position;
        //copy over main camera settings, but with a different culling mask and depthtexturemode.depth
        m_depthCam.CopyFrom(m_Cam);
        m_depthCam.cullingMask = depthLayers;
        m_depthCam.depthTextureMode = DepthTextureMode.Depth;
        //make depthCam use colorTexture and depthTexture and also disable depthCam so we can turn it on manually.
        m_depthCam.SetTargetBuffers(m_ColorTexture.colorBuffer, m_DepthTexture.depthBuffer);
        m_depthCam.enabled = false;
        
        //send the depth texture to the shader
        m_Material.SetTexture("_DepthMap", m_DepthTexture);

    }

    private void OnApplicationQuit()
    {
        RenderTexture.ReleaseTemporary(m_DepthTexture);
        RenderTexture.ReleaseTemporary(m_ColorTexture);
    }

    private void Reset()
    {
        Shader[] shaders = Resources.FindObjectsOfTypeAll<Shader>();
        foreach (Shader s in shaders)
        {
            //so we are naming the two files the same
            if (s.name.Contains(this.GetType().Name))
            {
                shader = s;
                return;
            }
            
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (m_Material)
        {
            //Update the depth render texture
            m_depthCam.Render();
            
            //pas information to our material
            m_Material.SetColor("_DepthColor", depthColor);
            m_Material.SetFloat("DepthStart", depthStart);
            m_Material.SetFloat("DepthEnd", depthEnd);
            
            Graphics.Blit(source,destination,m_Material);
        }
        else
        {
            Graphics.Blit(source,destination);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

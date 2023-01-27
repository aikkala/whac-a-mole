using UnityEngine;

// Adapted from https://samarth-robo.github.io/blog/2021/12/28/unity_rgbd_rendering.html

namespace UserInTheBox
{
    public class RenderShader : MonoBehaviour
    {
        private Material _shaderMaterial;
        
        void Start()
        {
            _shaderMaterial = GameObject.Find("UserInTheBox").GetComponent<MeshRenderer>().material;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, _shaderMaterial);
        }
    }
}
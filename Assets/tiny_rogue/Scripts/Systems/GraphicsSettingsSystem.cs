using System;
using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Mathematics;
using UnityEngine;
using Unity.Tiny.Input;
using KeyCode = Unity.Tiny.Input.KeyCode;
#if !UNITY_WEBGL
using InputSystem = Unity.Tiny.GLFW.GLFWInputSystem;
#else
    using InputSystem =  Unity.Tiny.HTML.HTMLInputSystem;
#endif

namespace game
{
    public static class GlobalGraphicsSettings
    {
        public static bool ascii;
    }
    
    public struct GraphicsSettings : IComponentData
    {
        public bool ascii;
    }
    
    public class GraphicsSettingsSystem : ComponentSystem
    {

        protected override void OnCreate()
        {
            Entities.ForEach( (ref GraphicsSettings g) => { GlobalGraphicsSettings.ascii = g.ascii; });
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
        }
    }
}
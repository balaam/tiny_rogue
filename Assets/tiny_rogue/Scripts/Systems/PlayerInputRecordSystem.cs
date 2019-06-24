using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Tiny.Core2D;
using UnityEngine;
using KeyCode = Unity.Tiny.Input.KeyCode;
#if !UNITY_WEBGL
using InputSystem = Unity.Tiny.GLFW.GLFWInputSystem;
#else
    using InputSystem =  Unity.Tiny.HTML.HTMLInputSystem;
#endif

namespace game
{
    public class PlayerInputRecordSystem : ComponentSystem
    {
        private List<Action> ActionStream = new List<Action>();

        public void AddAction(Action a)
        {
            ActionStream.Add(a);
        }
        
        protected override void OnUpdate() { }

        public void OnUpdateManual()
        {
            
        }
    }
}
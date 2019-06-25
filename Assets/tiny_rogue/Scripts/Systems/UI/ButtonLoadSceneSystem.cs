﻿using Unity.Entities;
using Unity.Tiny.Scenes;
using Unity.Tiny.UIControls;

/// <summary>
/// When the user press the UI button with a ButtonPlayAscii or 
/// ButtonPlayGraphics component, load the relevant scene.
/// </summary>
public class ButtonLoadSceneSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		Entities.WithAll<ButtonLoadScene>().ForEach((Entity entity, ref PointerInteraction pointerInteraction) =>
            {
                if (pointerInteraction.clicked)
				{
                    // Go to the referenced scene
                    var buttonData = EntityManager.GetComponentData<ButtonLoadScene>(entity);
                    SceneService.LoadSceneAsync(buttonData.sceneReference);
                }
            });
	}
}
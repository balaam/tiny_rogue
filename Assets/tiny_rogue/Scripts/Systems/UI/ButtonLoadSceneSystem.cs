using Unity.Entities;
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
		SceneReferenceData scene = new SceneReferenceData();
		bool sceneSelected = false;
		Entity e = Entity.Null;
		Entities.WithAll<SceneReferenceData>().ForEach((Entity entity, ref PointerInteraction pointerInteraction) =>
            {
                if (pointerInteraction.clicked)
				{
                    // Go to the referenced scene
                    scene = EntityManager.GetComponentData<SceneReferenceData>(entity);
                    sceneSelected = true;
                    e = entity;
				}
            });

		if (sceneSelected)
		{
			SceneService.UnloadSceneInstance(e);
			SceneService.LoadSceneAsync(scene.sceneReference);
		}
	}
}

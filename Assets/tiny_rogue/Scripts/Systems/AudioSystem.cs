using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Tiny.Audio;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Input;

#if !UNITY_WEBGL
using Unity.Tiny.GLFW;
using RuntimeWindowSystem = Unity.Tiny.GLFW.GLFWWindowSystem;
using InputSystem = Unity.Tiny.GLFW.GLFWInputSystem;
#else
using Unity.Tiny.HTML;
using RuntimeWindowSystem = Unity.Tiny.HTML.HTMLWindowSystem;
using InputSystem =  Unity.Tiny.HTML.HTMLInputSystem;
#endif

public class AudioSystem
{
    static private World world;
    static private Entity wavSoundClip;
    static private Entity mp3SoundClip;
    static private Entity[] theSoundSource = new Entity[2];

    static bool MainLoop()
    {
        world.Update();
        return !world.QuitUpdate;
    }

    static void Main()
    {
        NativeLeakDetection.Mode = NativeLeakDetectionMode.Enabled;
        world = DefaultTinyWorldInitialization.InitializeWorld("main");

        // Run configuration between creation and initialization.
        TinyEnvironment env = world.TinyEnvironment();
        DisplayInfo di = env.GetConfigData<DisplayInfo>();
        di.width = 1024;
        di.height = 768;
        di.renderMode = RenderMode.Auto;
        di.autoSizeToFrame = true;
        env.SetConfigData(di);

        DefaultTinyWorldInitialization.InitializeSystems(world);

        var entityManager = world.EntityManager;

        {
            wavSoundClip = entityManager.CreateEntity();
            AudioClip audioClip = new AudioClip();
            entityManager.AddComponentData(wavSoundClip, audioClip);

            entityManager.AddBufferFromString<AudioClipLoadFromFileAudioFile>(wavSoundClip, "Audio/Effects/Death Screams/sfx_deathscream_alien1.wav");
            entityManager.AddComponent(wavSoundClip, typeof(AudioClipLoadFromFile));
        }

        for (int i = 0; i < theSoundSource.Length; ++i)
        {
            var e = entityManager.CreateEntity();
            var audioSource = new AudioSource()
            {
                clip = wavSoundClip,
                volume = 1.0f,
                loop = false
            };
            entityManager.AddComponentData(e, audioSource);
            theSoundSource[i] = e;
        }

        Console.WriteLine("About to tick");

        // main loop
        world.GetExistingSystem<RuntimeWindowSystem>().InfiniteMainLoop(MainLoop);
        world.Dispose();

        Console.WriteLine("Done");
    }


    class PlaySoundSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var input = World.GetExistingSystem<InputSystem>();
            var entityManager = World.EntityManager;


            if (input.GetKeyDown(KeyCode.Space))
            {

                Console.WriteLine("IsPlaying query:");
                for (int i = 0; i < theSoundSource.Length; ++i)
                {
                    AudioSource source = entityManager.GetComponentData<AudioSource>(theSoundSource[i]);
                    Console.WriteLine(source.isPlaying ? "..playing" : "..not playing");
                }


                for (int i = 0; i < theSoundSource.Length; ++i)
                {
                    AudioSource source = entityManager.GetComponentData<AudioSource>(theSoundSource[i]);
                    if (!source.isPlaying)
                    {
                        source.clip = wavSoundClip;
                        source.volume = 1.0f;
                        source.loop = false;

                        entityManager.SetComponentData(theSoundSource[i], source);
                        entityManager.AddComponentData(theSoundSource[i], new AudioSourceStart());
                        break;
                    }
                }
            }
        }
    }
}
using System;
using Unity.Collections;
using Unity.Mathematics;

namespace game
{
    public struct SavedPath : IDisposable
    {
        public NativeArray<int2> pathSteps;
        public int currentIdx;

        public static SavedPath GetEmpty()
        {
            return new SavedPath
            {
                pathSteps = new NativeArray<int2>(0, Allocator.Temp), // Will become deallocated, you should replace this
                currentIdx = -1
            };
        }

        public void Dispose()
        {
            pathSteps.Dispose();
        }
    }
}
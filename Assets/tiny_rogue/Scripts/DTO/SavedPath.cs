using System;
using Unity.Collections;
using Unity.Mathematics;

namespace game
{
    public struct SavedPath : IDisposable
    {
        public NativeArray<int2> pathSteps;
        public int currentIdx;

        public void Dispose()
        {
            pathSteps.Dispose();
        }
    }
}
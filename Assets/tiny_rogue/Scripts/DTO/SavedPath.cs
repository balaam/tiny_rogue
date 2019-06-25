using Unity.Collections;
using Unity.Mathematics;

namespace game
{
    public struct SavedPath
    {
        public NativeArray<int2> pathSteps;
        public int currentIdx;

    }
}
using Unity.Collections;
using Unity.Mathematics;

namespace game
{
    public struct SavedPath
    {
        public int2[] pathSteps;
        //public NativeArray<int2> pathSteps;
        public int currentIdx;

    }
}
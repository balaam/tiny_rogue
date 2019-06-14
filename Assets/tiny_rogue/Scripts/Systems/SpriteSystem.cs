using Unity.Entities;

namespace game
{
    public class SpriteSystem : ComponentSystem
    {
        // Probably need wrapping up into some better names classes
        public static Entity[] AsciiToSprite = new Entity[256];
        static bool _loaded = false;

        public static bool Loaded
        {
            get { return _loaded; }
        }

        protected override void OnUpdate()
        {
            if (SpriteSystem.Loaded)
                return;

            Entities.ForEach((ref SpriteLookUp lookUp) =>
            {
                // This bit is generated, not sure of a better way to handle this at the moment.  
                // See Utils/gen_char_map.rb

                AsciiToSprite[32] = lookUp.Space;
                AsciiToSprite[35] = lookUp.Hash;
                AsciiToSprite[46] = lookUp.Dot;

                AsciiToSprite[65] = lookUp.A;
                AsciiToSprite[66] = lookUp.B;
                AsciiToSprite[67] = lookUp.C;
                AsciiToSprite[68] = lookUp.D;
                AsciiToSprite[69] = lookUp.E;
                AsciiToSprite[70] = lookUp.F;
                AsciiToSprite[71] = lookUp.G;
                AsciiToSprite[72] = lookUp.H;
                AsciiToSprite[73] = lookUp.I;
                AsciiToSprite[74] = lookUp.J;
                AsciiToSprite[75] = lookUp.K;
                AsciiToSprite[76] = lookUp.L;
                AsciiToSprite[77] = lookUp.M;
                AsciiToSprite[78] = lookUp.N;
                AsciiToSprite[79] = lookUp.O;
                AsciiToSprite[80] = lookUp.P;
                AsciiToSprite[81] = lookUp.Q;
                AsciiToSprite[82] = lookUp.R;
                AsciiToSprite[83] = lookUp.S;
                AsciiToSprite[84] = lookUp.T;
                AsciiToSprite[85] = lookUp.U;
                AsciiToSprite[86] = lookUp.V;
                AsciiToSprite[87] = lookUp.W;
                AsciiToSprite[88] = lookUp.X;
                AsciiToSprite[89] = lookUp.Y;
                AsciiToSprite[90] = lookUp.Z;
                
                AsciiToSprite[97] = lookUp.ALower;
//                AsciiToSprite[98] = lookUp.BLower;
//                AsciiToSprite[99] = lookUp.CLower;
//                AsciiToSprite[100] = lookUp.DLower;
//                AsciiToSprite[101] = lookUp.ELower;
//                AsciiToSprite[102] = lookUp.FLower;
//                AsciiToSprite[103] = lookUp.GLower;
//                AsciiToSprite[104] = lookUp.HLower;
//                AsciiToSprite[105] = lookUp.ILower;
//                AsciiToSprite[106] = lookUp.JLower;
//                AsciiToSprite[107] = lookUp.KLower;
//                AsciiToSprite[108] = lookUp.LLower;
//                AsciiToSprite[109] = lookUp.MLower;
//                AsciiToSprite[110] = lookUp.NLower;
//                AsciiToSprite[111] = lookUp.OLower;
//                AsciiToSprite[112] = lookUp.PLower;
//                AsciiToSprite[113] = lookUp.QLower;
//                AsciiToSprite[114] = lookUp.RLower;
//                AsciiToSprite[115] = lookUp.SLower;
//                AsciiToSprite[116] = lookUp.TLower;
//                AsciiToSprite[117] = lookUp.ULower;
//                AsciiToSprite[118] = lookUp.VLower;
//                AsciiToSprite[119] = lookUp.WLower;
//                AsciiToSprite[120] = lookUp.XLower;
//                AsciiToSprite[121] = lookUp.YLower;
//                AsciiToSprite[122] = lookUp.ZLower;
                
                SpriteSystem._loaded = true;
            });

        }
    }
}

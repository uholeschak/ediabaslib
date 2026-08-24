using PsdzClient.Core;
using System.ComponentModel;
using System.Media;
using BMW.Rheingold.CoreFramework.Media;

namespace BMW.Authoring.Helper
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class Sound
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void SetVolumeLevel(int targetLevel)
        {
            VolumeChanger.SetVolume(targetLevel);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void Play(Sounds sound)
        {
            switch (sound)
            {
                case Sounds.NegativeFeedback:
                    //[-] SoundPlayer.PlaySound(Resource.neg);
                    break;
                case Sounds.PositiveFeedback:
                    //[-] SoundPlayer.PlaySound(Resource.pos);
                    break;
            }
        }
    }
}

using System.ComponentModel;

namespace BMW.Authoring.API.Math
{
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IMathFunctions
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        double[] MirrorAngleRawAmplitudeFit(double[] mirrorAngles, double[] rawAmplitudes);
    }
}

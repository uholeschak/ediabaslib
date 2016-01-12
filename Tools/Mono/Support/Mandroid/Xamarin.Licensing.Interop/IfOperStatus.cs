using System;
namespace Xamarin.Licensing.Interop
{
	internal enum IfOperStatus
	{
		Up = 1,
		Down,
		Testing,
		Unknown,
		Dormant,
		NotPresent,
		LowerLayerDown
	}
}

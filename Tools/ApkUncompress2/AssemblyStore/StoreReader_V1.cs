using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using System.IO;

namespace Xamarin.Android.AssemblyStore;

class StoreReader_V1 : AssemblyStoreReader
{
	public override string Description => "Assembly store v1";
	public override bool NeedsExtensionInName => false;

	public static IList<string> ApkPaths      { get; }
	public static IList<string> AabPaths      { get; }
	public static IList<string> AabBasePaths  { get; }

	static StoreReader_V1 ()
	{
		ApkPaths = new List<string> ().AsReadOnly ();
		AabPaths = new List<string> ().AsReadOnly ();
		AabBasePaths = new List<string> ().AsReadOnly ();
	}

	public StoreReader_V1 (Stream? store, ZipFile? zf, ZipEntry? zipEntry, string path)
		: base (store, zf, zipEntry, path)
	{}

	protected override bool IsSupported ()
	{
		return false;
	}

	protected override void Prepare ()
	{
	}

	protected override ulong GetStoreStartDataOffset () => 0;
}

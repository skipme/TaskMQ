using System;

namespace SourceControl.Build
{
	public interface IAssemblyBuilder
	{
		bool BuildProject ();

		string ProjectLocation { get; set; }

		string Log { get; }

		string BuildResultDll { get; }

		string BuildResultSymbols { get; }

		string[] BuildResultAssets { get; }
	}
}

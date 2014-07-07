using System;
namespace SourceControl.Build
{
    public interface IAssemblyBuilder
    {
        bool BuildProject();
        string ProjectLocation { get; set; }
    }
}

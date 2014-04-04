using System;
namespace TaskUniversum.Assembly
{
    public interface IAssemblyStatus
    {
        bool Loaded { get; set; }
        string LoadedRemarks { get; set; }
        string LoadedRevision { get; set; }
        DateTime packagedDate { get; set; }
        string State { get; set; }

        IRevision BuildServerRev { get; }
        IRevision PackageRev { get; }
    }
}

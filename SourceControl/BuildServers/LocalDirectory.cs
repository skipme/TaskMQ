using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace SourceControl.BuildServers
{
    public class LocalDirectory : IBuildServer
    {
        LocalDirParams parameters = new LocalDirParams();
        string AssemblyFileName
        {
            get
            {
                return Path.Combine(Directory.GetCurrentDirectory(), parameters.AssemblyFileName);
            }
        }

        public string Name
        {
            get { return "LocalDirectory"; }
        }

        public string Description
        {
            get { return "For self referenced assemblys"; }
        }

        public TaskQueue.Providers.TItemModel GetParametersModel()
        {
            return parameters;
        }

        public void SetParameters(Dictionary<string, object> Mparameters)
        {
            parameters.SetHolder(Mparameters);
        }

        public BuildArtifacts GetArtifacts()
        {
            if (!File.Exists(this.AssemblyFileName))
                return null;

            System.Reflection.AssemblyName ver = Assemblys.AssemblyHelper.GetAssemblyVersion(this.AssemblyFileName);

            return BuildArtifacts.FromDirectory(this.AssemblyFileName, ver.ToString(), true);
        }

        public bool CheckParameters(out string explanation)
        {
            explanation = null;
            if (!File.Exists(this.AssemblyFileName))
            {
                explanation = "Assembly File not found.";
                return false;
            }
            return true;
        }

        public Ref.SCMRevision GetVersion()
        {
            if (!File.Exists(this.AssemblyFileName))
                return null;

            System.Reflection.AssemblyName ver = Assemblys.AssemblyHelper.GetAssemblyVersion(this.AssemblyFileName);
            return new Ref.SCMRevision() { Commiter = "LocalDirectory TASKMQ BS", CommitTime = DateTime.UtcNow, CreateAt = File.GetLastWriteTimeUtc(this.AssemblyFileName), Revision = ver.ToString() };
        }

        public BuildServerState GetState()
        {
            return BuildServerState.build_ok;
        }

        public bool DobBSJob(BuildServerJobs jobDef)
        {
            return true;
        }

        public List<BuildServerJobs> GetAllowedJobs()
        {
            return null;
        }
    }
    public class LocalDirParams : TItemModel
    {
        public LocalDirParams() { }
        public LocalDirParams(TItemModel tm) : base(tm.GetHolder()) { }

        [TaskQueue.FieldDescription("assembly file name", Required: true)]
        public string AssemblyFileName { get; set; }

        [TaskQueue.FieldDescription(Ignore = true, Inherited = true)]
        public override string ItemTypeName
        {
            get
            {
                return "LocalDirParams";
            }
            set { }
        }
    }
}

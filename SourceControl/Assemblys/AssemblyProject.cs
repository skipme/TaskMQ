using SourceControl.Build;
using SourceControl.Containers;
using SourceControl.Ref;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SourceControl.Assemblys
{
    public class AssemblyProject
    {
        public enum ProjectState
        {

            source_absent,
            fetch,
            build,
            fetch_required,
            build_required,
            fetch_error,
            build_error,
            ok,
            major_error,

            build_deferred,
        }
        public ProjectState State { get; private set; }

        public void SetBuildDeferredFlag()
        {
            if (State == ProjectState.build_required)
                State = ProjectState.build_deferred;
        }

        private AssemblySource Source;
        private AssemblyBinVersions Versions;

        public string projectRelativePath { get; private set; }
        public string scmUrl { get; private set; }
        public string moduleName { get; private set; }

        public List<VersionRevision> GetStoredVersions()
        {
            return Versions.GetVersions();
        }

        public AssemblyProject(string workingDirectory, string projectRelativePath, string scmUrl, string moduleName)
        {
            this.projectRelativePath = projectRelativePath;
            this.scmUrl = scmUrl;
            this.moduleName = moduleName;
            if (scmUrl != null)
            {
                this.Source = new SourceControl.Assemblys.AssemblySource(workingDirectory, projectRelativePath, scmUrl);
                switch (this.Source.Status)
                {
                    case SCM.Status.fetchRequied:
                    case SCM.Status.cloneRequired:
                        State = ProjectState.fetch_required;
                        break;
                    case SCM.Status.allUpToDate:
                        State = ProjectState.ok;
                        break;
                    case SCM.Status.none:
                        State = ProjectState.fetch_required;
                        break;
                    default:
                        State = ProjectState.major_error;
                        break;
                }
            }
            else
            {
                State = ProjectState.source_absent;
            }
            Versions = new AssemblyBinVersions(workingDirectory, moduleName);
        }
        public bool IsSourceUpToDate
        {
            get
            {
                if (Source == null)
                    return false;

                return Source.Status == SCM.Status.allUpToDate;
            }
        }
        public bool SetUpSourceToDate()
        {
            if (Source == null || Source.Status == SCM.Status.allUpToDate)
                return false;
            State = ProjectState.fetch;

            bool result = Source.SetUpToDate();
            Console.WriteLine("source '{0}' update: {1}", Source.Name, result ? "ok" : "fail");
            if (result)
            {
                VersionRevision rev = sourceVersionRevision;
                if (rev == null)
                {
                    State = ProjectState.major_error;
                }
                else
                {
                    if (rev.Revision != edgeStoredVersionRevision.Tag)
                    {
                        State = ProjectState.build_required;
                    }
                    else
                    {
                        State = ProjectState.ok;
                    }
                }
            }
            else State = ProjectState.fetch_error;

            return result;
        }
        //public bool GetSpecificVersion(string revision, out AssemblyBinaryBuildResult binary)
        //{
        //    if (GetStoredVersions().Where(v => v.Revision == revision).Count() == 1)
        //    {
        //        return Versions.GetSpecificVersion(revision, out binary);
        //    }
        //    else
        //    {
        //        binary = null;
        //        return false;
        //    }
        //}
        public bool StoreNewIfRequired(out string buildLog)
        {
            buildLog = string.Empty;
            if (Source == null)
            {
                buildLog = "sourceScm not initiated.";
                return false;
            }
            VersionRevision rev = sourceVersionRevision;
            if (rev == null)
            {
                State = ProjectState.major_error;
                return false;
            }

            bool result = false;
            if (rev.Revision != edgeStoredVersionRevision.Tag)
            {
                State = ProjectState.fetch;
                if (Source.SetUpToDate())
                {
                    AssemblyBinaryBuildResult bin;
                    State = ProjectState.build;
                    if (result = Source.BuildProject(out bin))
                    {
                        Versions.AddVersion(rev, bin);
                        result = true;
                    }
                    State = result ? ProjectState.ok : ProjectState.build_error;
                }
                else
                {
                    State = ProjectState.fetch_error;
                    //
                }
            }
            else
            {
                State = ProjectState.ok;
            }
            buildLog = Source.lastBuildLog;
            return result;
        }
        public VersionRevision sourceVersionRevision
        {
            get
            {
                if (Source == null)
                    return null;

                return Source.Version;
            }
        }
        public Ref.PackageVersion edgeStoredVersionRevision
        {
            get
            {
                return Versions.LatestRevision;
            }
        }
    }
}

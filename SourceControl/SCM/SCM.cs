using SourceControl.Ref;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SourceControl
{
    public abstract class SCM
    {
        public enum Status
        {
            none = 0x11, // update status required
            cloneFailure,
            cloneRequired, // clone operation required
            fetchFailure, // last fetch unsuccess, but container not empty and clone succeeded
            fetchRequied,
            allUpToDate
        }

        public string LocalContainerDirectory { get; private set; }
        public string cloneUri { get; private set; }

        public SCM(string localRepositoryPath, string cloneUri)
        {
            if (!localRepositoryPath.StartsWith("/"))
            {
                localRepositoryPath = Path.Combine(
                     Directory.GetCurrentDirectory(),
                     localRepositoryPath);
            }
            if (!Directory.Exists(localRepositoryPath))
            {
                Directory.CreateDirectory(localRepositoryPath);
            }
            LocalContainerDirectory = localRepositoryPath;
            this.cloneUri = cloneUri;
        }
        public abstract void UpdateStatus();
        public abstract bool Fetch();
        public abstract bool Clone();
        public abstract SCMRevision LocalVersion { get; }

        public abstract Status CurrentStatus { get; }
    }
}

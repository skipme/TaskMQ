using System;
using System.Collections.Generic;
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
        //public string StatusExpalanation { get; set; }



        public string LocalContainerDirectory { get; private set; }
        public string cloneUri { get; private set; }

        public SCM(string localRepositoryPath, string cloneUri)
        {
            LocalContainerDirectory = localRepositoryPath;
            this.cloneUri = cloneUri;
        }
        public abstract void UpdateStatus();
        public abstract bool Fetch();
        public abstract bool Clone();
        public abstract string LocalVersion { get; }
        public abstract Status CurrentStatus { get; }
    }
}

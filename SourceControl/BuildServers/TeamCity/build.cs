using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.BuildServers.TeamCity
{
    [ServiceStack.Route("{url}")]
    public class BuildRequest
    {
        int _id;
        string _url;
        public string url
        {
            get { return _url; }
            set
            {
                _url = value;
            }
        }
        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                _url = string.Format("httpAuth/app/rest/builds/id:{0}", _id);// because colons not supported!
            }
        }
    }
    public class BuildType
    {
        public string id { get; set; }
        public string name { get; set; }
        public string href { get; set; }
        public string projectName { get; set; }
        public string projectId { get; set; }
        public string webUrl { get; set; }
    }

    public class Agent
    {
        public string name { get; set; }
        public int id { get; set; }
        public string href { get; set; }
    }

    public class Tags
    {
        public List<object> tag { get; set; }
    }

    public class Property
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class Properties
    {
        public List<Property> property { get; set; }
    }

    public class SnapshotDependencies
    {
        public int count { get; set; }
        public List<object> build { get; set; }
    }

    public class ArtifactDependencies
    {
        public int count { get; set; }
        public List<object> build { get; set; }
    }

    public class VcsRootInstance
    {
        public string id { get; set; }
        public string vcs_root_id { get; set; }
        public string name { get; set; }
        public string href { get; set; }
    }

    public class Revision
    {
        public string version { get; set; }
        public VcsRootInstance vcs_root_instance { get; set; }
    }

    public class Revisions
    {
        public List<Revision> revision { get; set; }
    }

    public class User
    {
        public string username { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        public string href { get; set; }
    }

    public class Triggered
    {
        public string date { get; set; }
        public User user { get; set; }
    }

    public class Changes
    {
        public int count { get; set; }
        public string href { get; set; }
    }

    public class RelatedIssues
    {
        public string href { get; set; }
    }

    public class Artifacts
    {
        public string href { get; set; }
    }

    public class BuildRootObject
    {
        public int id { get; set; }
        public string number { get; set; }
        public string status { get; set; }
        public string href { get; set; }
        public string webUrl { get; set; }
        public bool personal { get; set; }
        public bool history { get; set; }
        public bool pinned { get; set; }
        public string statusText { get; set; }
        public BuildType buildType { get; set; }
        public string startDate { get; set; }
        public string finishDate { get; set; }
        public Agent agent { get; set; }
        public Tags tags { get; set; }
        public Properties properties { get; set; }
        public SnapshotDependencies snapshot_dependencies { get; set; }
        public ArtifactDependencies artifact_dependencies { get; set; }
        public Revisions revisions { get; set; }
        public Triggered triggered { get; set; }
        public Changes changes { get; set; }
        public RelatedIssues relatedIssues { get; set; }
        public Artifacts artifacts { get; set; }
    }
}

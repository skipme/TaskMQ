using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.BuildServers.TeamCity
{
    [ServiceStack.Route("{url}")]
    public class ChangesRequest
    {
        string _url;
        public string url
        {
            get { return _url; }
            set
            {
                _url = value.StartsWith("/") ? value.Remove(0, 1) : value;
            }
        }
    }
    public class File
    {
        public string before_revision { get; set; }
        public string after_revision { get; set; }
        public string file { get; set; }
        public string relative_file { get; set; }
    }

    public class Files
    {
        public List<File> file { get; set; }
    }

    //public class VcsRootInstance
    //{
    //    public string id { get; set; }
    //    public string vcs_root_id { get; set; }
    //    public string name { get; set; }
    //    public string href { get; set; }
    //}

    public class ChangesRootObject
    {
        public string username { get; set; }
        public string date { get; set; }
        public string comment { get; set; }
        public Files files { get; set; }
        public VcsRootInstance vcsRootInstance { get; set; }
        public int id { get; set; }
        public string version { get; set; }
        public string webLink { get; set; }
        public string href { get; set; }
    }
}

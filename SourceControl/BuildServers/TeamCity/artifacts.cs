using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.BuildServers.TeamCity
{
    [ServiceStack.Route("{url}")]
    public class ArtifactsRequest
    {
        int _id;
        string _url;
        public string url
        {
            get { return _url; }
            set
            {
                _url = value.StartsWith("/") ? value.Remove(0, 1) : value;
            }
        }
        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                _url = string.Format("/httpAuth/app/rest/builds/id:{0}/artifacts/children", _id);// because colons not supported!
            }
        }
    }
    public class Content
    {
        public string href { get; set; }
    }

    public class Children
    {
        public string href { get; set; }
    }

    public class ArtFile
    {
        public int size { get; set; }
        public string modificationTime { get; set; }
        public Content content { get; set; }
        public Children children { get; set; }
        public string name { get; set; }
        public string href { get; set; }
    }

    public class ArtifcatsRootObject
    {
        public List<ArtFile> files { get; set; }
    }
}

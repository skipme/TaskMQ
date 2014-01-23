using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.BuildServers.TeamCity
{
    [ServiceStack.Route("{url}")]
    public class ChangeRequest //: IReturn<ChangeRootObject>
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
    public class Change
    {
        public int id { get; set; }
        public string version { get; set; }
        public string href { get; set; }
        public string webLink { get; set; }
    }

    public class ChangeRootObject
    {
        public int count { get; set; }
        public List<Change> change { get; set; }
    }
}

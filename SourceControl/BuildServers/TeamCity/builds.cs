using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.BuildServers.TeamCity
{
    [ServiceStack.Route("httpAuth/app/rest/builds")]
    public class BuildsRequest
    {
        public string buildType { get; set; }
        public string status { get; set; }
    }
    public class Build
    {
        public int id { get; set; }
        public string number { get; set; }
        public string status { get; set; }
        public string buildTypeId { get; set; }
        public string startDate { get; set; }
        public string href { get; set; }
        public string webUrl { get; set; }
    }

    public class BuildsRootObject
    {
        public int count { get; set; }
        public List<Build> build { get; set; }
    }

}

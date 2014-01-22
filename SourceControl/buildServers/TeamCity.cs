using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.buildServers
{
    public class TeamCity : BuildServer
    {
        // required parameters: build type, artifact name
        // required connection parameters: host, user, password

        // http://localhost:8380/httpAuth/app/rest/builds?buildType=TaskMQ_DefaltBC&status=SUCCESS
        // http://localhost:8380/httpAuth/app/rest/builds/id:7
        // http://localhost:8380/httpAuth/app/rest/builds/id:7/artifacts/children
        // http://localhost:8380/httpAuth/app/rest/builds/id:7/artifacts/content/debug.zip
    }
}

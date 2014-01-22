using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace SourceControl.BuildServers.TeamCity
{
    public class TeamCityBS : BuildServer
    {
        // required parameters: build type, artifact name
        // required connection parameters: host, user, password

        // http://localhost:8380/httpAuth/app/rest/builds?buildType=TaskMQ_DefaltBC&status=SUCCESS
        // http://localhost:8380/httpAuth/app/rest/builds/id:7
        // http://localhost:8380/httpAuth/app/rest/builds/id:7/artifacts/children
        // http://localhost:8380/httpAuth/app/rest/builds/id:7/artifacts/content/debug.zip

        TeamCityBSParams parameters = new TeamCityBSParams();
        public TItemModel GetParametersModel()
        {
            return parameters;
        }

        public void SetParameters(TItemModel parameters)
        {
            parameters = new TeamCityBSParams(parameters);
        }


        private BuildsRootObject GetBuilds()
        {
            ServiceStack.JsonServiceClient client = new ServiceStack.JsonServiceClient(parameters.Host);
            client.SetCredentials(parameters.User, parameters.Password);
            client.AlwaysSendBasicAuthHeader = true;
            BuildsRootObject b = client.Get<BuildsRootObject>(new BuildsRequest
            {
                buildType = parameters.BuildType,
                status = "SUCCESS"
            });
            return b;
        }
        private BuildRootObject GetBuild(TeamCity.Build id)
        {
            ServiceStack.JsonServiceClient client = new ServiceStack.JsonServiceClient(parameters.Host);
            client.SetCredentials(parameters.User, parameters.Password);
            client.AlwaysSendBasicAuthHeader = true;
            BuildRootObject b = client.Get<BuildRootObject>(new BuildRequest
            {
                url = id.href
            });
            return b;
        }
        private ArtifcatsRootObject GetArtifact(TeamCity.BuildRootObject id)
        {
            ServiceStack.JsonServiceClient client = new ServiceStack.JsonServiceClient(parameters.Host);
            client.SetCredentials(parameters.User, parameters.Password);
            client.AlwaysSendBasicAuthHeader = true;
            ArtifcatsRootObject b = client.Get<ArtifcatsRootObject>(new ArtifactsRequest
            {
                url = id.artifacts.href
            });
            return b;
        }
        public string Name
        {
            get { return "TeamCity Artifacts"; }
        }

        public string Description
        {
            get { return "TeamCity artifacts access interface"; }
        }


        public Ref.AssemblyArtifacts GetArtifacts()
        {
            throw new NotImplementedException();
        }
    }
    public class TeamCityBSParams : TItemModel
    {
        public TeamCityBSParams() { }
        public TeamCityBSParams(TItemModel tm) : base(tm.Holder) { }

        [TaskQueue.FieldDescription("host address (hostname:port)", Required: true)]
        public string Host { get; set; }

        [TaskQueue.FieldDescription("username", Required: true)]
        public string User { get; set; }
        [TaskQueue.FieldDescription("password", Required: true)]
        public string Password { get; set; }

        [TaskQueue.FieldDescription("artifact name, make sure artifact path configuration is ok, for example(QueueService/bin/Debug/* => debug.zip) there debug.zip is artifact name", Required: true)]
        public string ArtifactName { get; set; }
        [TaskQueue.FieldDescription("Build Type name (Build configuration ID)", Required: true)]
        public string BuildType { get; set; }

        public override string ItemTypeName
        {
            get
            {
                return "TeamCityParams";
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}

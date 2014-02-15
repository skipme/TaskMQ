using SourceControl.Ref;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;
using SourceControl.BuildServers.TeamCity;
using TaskQueue;

namespace SourceControl.BuildServers
{
    public class TeamCityBS : IBuildServer
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
            return this.parameters;
        }

        public void SetParameters(Dictionary<string, object> m_parameters)
        {
            this.parameters.SetHolder(m_parameters);
        }


        private BuildsRootObject GetBuilds()
        {
            try
            {
                using (ServiceStack.JsonServiceClient client = new ServiceStack.JsonServiceClient(parameters.Host))
                {
                    client.SetCredentials(parameters.User, parameters.Password);
                    client.AlwaysSendBasicAuthHeader = true;
                    BuildsRootObject b = client.Get<BuildsRootObject>(new BuildsRequest
                    {
                        buildType = parameters.BuildType,
                        status = "SUCCESS"
                    });
                    return b;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        private BuildRootObject GetBuild(TeamCity.Build id)
        {
            try
            {
                using (ServiceStack.JsonServiceClient client = new ServiceStack.JsonServiceClient(parameters.Host))
                {
                    client.SetCredentials(parameters.User, parameters.Password);
                    client.AlwaysSendBasicAuthHeader = true;

                    BuildRootObject b = client.Get<BuildRootObject>(id.href);
                    return b;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        private ArtifcatsRootObject GetArtifact(TeamCity.BuildRootObject id)
        {
            try
            {
                using (ServiceStack.JsonServiceClient client = new ServiceStack.JsonServiceClient(parameters.Host))
                {
                    client.SetCredentials(parameters.User, parameters.Password);
                    client.AlwaysSendBasicAuthHeader = true;
                    ArtifcatsRootObject b = client.Get<ArtifcatsRootObject>(id.artifacts.href);
                    return b;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        private ChangeRootObject GetChange(TeamCity.BuildRootObject id)
        {
            try
            {
                using (ServiceStack.JsonServiceClient client = new ServiceStack.JsonServiceClient(parameters.Host))
                {
                    client.SetCredentials(parameters.User, parameters.Password);
                    client.AlwaysSendBasicAuthHeader = true;
                    ChangeRootObject b = client.Get<ChangeRootObject>(id.changes.href);
                    return b;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        private ChangesRootObject GetChanges(TeamCity.ChangeRootObject id)
        {
            try
            {
                using (ServiceStack.JsonServiceClient client = new ServiceStack.JsonServiceClient(parameters.Host))
                {
                    client.SetCredentials(parameters.User, parameters.Password);
                    client.AlwaysSendBasicAuthHeader = true;
                    ChangesRootObject b = client.Get<ChangesRootObject>(id.change[0].href);
                    return b;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        public string Name
        {
            get { return "TeamCityArtifacts"; }
        }

        public string Description
        {
            get { return "TeamCity artifacts access interface"; }
        }


        public BuildArtifacts GetArtifacts()
        {
            BuildsRootObject builds = this.GetBuilds();
            if (builds == null || builds.count == 0)
                return null;

            BuildRootObject build = this.GetBuild(builds.build[0]);
            ArtifcatsRootObject arts = this.GetArtifact(build);
            for (int i = 0; i < arts.files.Count; i++)
            {
                if (arts.files[i].name.Equals(parameters.ArtifactName, StringComparison.InvariantCultureIgnoreCase))
                {
                    using (System.Net.WebClient cl = new System.Net.WebClient())
                    {
                        Uri uri = new Uri(new Uri(parameters.Host), arts.files[i].content.href);
                        cl.Headers.Add("Authorization", "Basic " +
                            Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", parameters.User, parameters.Password))));
                        byte[] zip = cl.DownloadData(uri);
                        BuildArtifacts result = BuildArtifacts.FromZipArchive(parameters.Assembly, build.revisions.revision[0].version, zip);

                        return result;
                    }
                }
            }
            return null;
        }
        public bool CheckParameters(out string explanation)
        {
            if (!parameters.ArtifactName.EndsWith(".zip"))
            {
                explanation = "zip archives supported only";
                return false;
            }
            using (ServiceStack.JsonServiceClient client = new ServiceStack.JsonServiceClient(parameters.Host))
            {
                client.SetCredentials(parameters.User, parameters.Password);
                client.AlwaysSendBasicAuthHeader = true;
                try
                {
                    client.Get("httpAuth/app/rest/projects");
                    explanation = "";
                    return true;
                }
                catch (Exception e)
                {
                    explanation = e.Message;
                }
                return false;
            }
        }
        public SCMRevision GetVersion()
        {
            BuildsRootObject builds = this.GetBuilds();
            if (builds == null || builds.count == 0)
                return null;

            BuildRootObject build = GetBuild(builds.build[0]);
            ChangeRootObject change = GetChange(build);
            if (change.count == 0)// no changes --> trail for build with changes
            {
                bool changesFound = false;
                for (int i = 1; i < builds.build.Count; i++)
                {
                    build = GetBuild(builds.build[i]);
                    change = GetChange(build);
                    if (change.count > 0)
                    {
                        changesFound = true;
                        break;
                    }
                }
                if (!changesFound)
                    return new SCMRevision
                    {
                        Revision = build.revisions.revision[0].version
                    };
            }
            ChangesRootObject changes = GetChanges(change);

            DateTime at = DateTime.ParseExact(changes.date, "yyyyMMddTHHmmsszzzz", System.Globalization.CultureInfo.InvariantCulture);
            SCMRevision ver = new SCMRevision
            {
                Commiter = changes.username,
                CommitMessage = changes.comment,
                CommitTime = at,
                CreateAt = at,
                Revision = changes.version
            };

            return ver;
        }


        public BuildServerState GetState()
        {
            return BuildServerState.build_ok;
        }

        public void FetchSource()
        {

        }

        public void BuildSource()
        {

        }
    }
    public class TeamCityBSParams : TItemModel
    {
        public TeamCityBSParams() { }
        public TeamCityBSParams(TItemModel tm) : base(tm.GetHolder()) { }

        [TaskQueue.FieldDescription("assembly path in artifacts zip archive", Required: true)]
        public string Assembly { get; set; }

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

        [FieldDescription(Ignore = true, Inherited = true, Required = false)]
        public override string ItemTypeName
        {
            get
            {
                return "TeamCityParams";
            }
            set
            {

            }
        }
    }
}

using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.Git
{
    public class LocalBranch
    {
        // focus only on remote branch
        // fetch if required
        // compare with build result to see changes
        // of course result must contain repo meta information and commit id

        // assume we work with default branch
        public string FocusRepoBranchName { get; set; }
        public string uriRemoteOrigin { get; set; }

        public string BranchVersion { get; set; }

        public string RemoteBranchVersion { get; set; }
        public string Directory { get; set; }
        public bool TakeRequired { get; set; }

        public LocalBranch(string dir, string uriRemoteOrigin)
        {
            Directory = dir;
            this.uriRemoteOrigin = uriRemoteOrigin;
            FocusRepoBranchName = "origin/master";
            CheckRemoteBranch();
            CheckCopy();
        }
        public void ReCreate()
        {
            PurgeDir();
            Clone(uriRemoteOrigin);
            //string dc = Repository.Init(Directory, false);// sources required...
            //using (var repo = new Repository(dc))
            //{
            //    CloneRequired = true;
            //}
        }
        public void Clone(string uri)
        {
            string dc = Repository.Clone(uri, Directory);
            using (var repo = new Repository(dc))
            {
                FocusRepoBranchName = repo.Head.TrackedBranch.Name;
                BranchVersion = repo.Head.TrackedBranch.Commits.First().Sha;
            }
        }
        private void PurgeDir()
        {

        }
        public void TakeChanges()
        {
            if (TakeRequired)
            {
                using (var repo = new Repository(Directory))
                {
                    var tb = repo.Branches.Where(b => b.IsTracking).FirstOrDefault();
                    //var branch = repo.Checkout(tb);
                    // this required merge... but we neeed only remote branch
                }
                CheckCopy();
            }
        }
        public void CheckRemoteBranch()
        {
            //    using (var repo = new Repository(uriRemoteOrigin))
            //    {
            //        RemoteBranchVersion = repo.Head.TrackedBranch.Commits.First().Sha;
            //    }
        }
        public void CheckCopy()
        {
            if (!System.IO.Directory.Exists(Directory))
            {
                System.IO.Directory.CreateDirectory(Directory);
            }
            try
            {
                using (var repo = new Repository(Directory))
                {
                    var remb = repo.Branches.Where(b => { return b.IsRemote == true && b.Name == FocusRepoBranchName; }).FirstOrDefault();
                    BranchVersion = remb.Commits.First().Sha;

                    var remote = repo.Network.Remotes["origin"];
                    repo.Network.Fetch(remote);
                    remb = repo.Branches.Where(b => { return b.IsRemote == true && b.Name == FocusRepoBranchName; }).FirstOrDefault();
                    RemoteBranchVersion = remb.Commits.First().Sha;
                    //var tb = repo.Branches.Where(b => b.IsTracking).FirstOrDefault();
                    //FocusRepoBranchName = tb.Name;
                    //BranchVersion = tb.Commits.First().Sha;

                    //var remote = repo.Network.Remotes["origin"];
                    //repo.Network.Fetch(remote);

                    //var remb = repo.Branches.Where(b => { return b.IsRemote == true && b.Name == "origin/master"; }).FirstOrDefault();
                    //if (remb != null)
                    //    RemoteBranchVersion = remb.Commits.First().Sha;

                    //TakeRequired = RemoteBranchVersion != BranchVersion;
                    //bool updated = repo.Network.FetchHeads.Any();
                    //string pathRemote = repo.Network.Remotes["origin"].Url;

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("repo error(recreating): {0}", Directory);
                ReCreate();
            }
        }
    }
}

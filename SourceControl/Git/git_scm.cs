using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.Git
{
    public class git_scm : SCM
    {
        public git_scm(string localRepositoryPath, string cloneUri) :
            base(localRepositoryPath, cloneUri)
        { }
        private Branch focusBranch;
        private SCM.Status status = Status.none;

        public override bool Fetch()
        {
            focusBranch = null;
            if (CheckLocalCopy())
            {
                using (var repo = new Repository(base.LocalContainerDirectory))
                {
                    try
                    {
                        repo.Network.Fetch(repo.Network.Remotes[GetRemoteBranchName(repo)]);
                        CheckoutRemoteBranch(repo);
                        status = Status.allUpToDate;
                        return true;
                    }
                    catch (Exception e)
                    {
                        status = Status.fetchFailure;
                        Console.WriteLine("exception while trying to fetch: {0}", e.Message);
                    }
                }
            }
            else
            {
                status = Status.cloneRequired;
            }
            return false;
        }

        public override string LocalVersion
        {
            get
            {
                if (focusBranch == null)
                    return null;
                return focusBranch.Commits.First().Sha;
            }
        }

        public override SCM.Status CurrentStatus
        {
            get { return status; }
        }

        public override void UpdateStatus()
        {
            focusBranch = null;
            if (CheckLocalCopy())
            {
                status = Status.fetchRequied;
            }
            else
            {
                status = Status.cloneRequired;
            }
        }
        private bool CheckLocalCopy()
        {
            try
            {
                using (var repo = new Repository(base.LocalContainerDirectory))
                {
                    CheckoutRemoteBranch(repo);
                }
            }
            catch (Exception e)
            { return false; }
            return true;
        }
        private void CheckoutRemoteBranch(Repository rep)
        {
            // set to remote branch
            focusBranch = rep.Checkout(GetRemoteBranch(rep));
        }
        public override bool Clone()
        {
            string dc = null;
            try
            {
                dc = Repository.Clone(base.cloneUri, base.LocalContainerDirectory);
            }
            catch
            {
                status = Status.cloneFailure;
                return false;
            }

            UpdateStatus();
            return true;
        }
        private Branch GetRemoteBranch(Repository rep)
        {
            string remb = GetRemoteBranchName(rep);
            Branch bc = rep.Branches.Where(b => { return b.IsRemote == true && b.Remote.Name == remb; }).FirstOrDefault();
            return bc;
        }
        private string GetRemoteBranchName(Repository rep)
        {
            string name = null;
            foreach (var b in rep.Network.Remotes)
            {
                name = b.Name;
            }
            if (name == null)
                throw new Exception("repository does not contain remote branch");
            return name;
        }
    }
}

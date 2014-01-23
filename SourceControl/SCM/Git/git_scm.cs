using LibGit2Sharp;
using SourceControl.Ref;
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

        private SCM.Status status = Status.none;// this means to determine status by UpdateStatus

        public override bool Fetch()
        {
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



        public override SCM.Status CurrentStatus
        {
            get { return status; }
        }

        public override void UpdateStatus()
        {
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
            {
                Console.WriteLine("git-scm '{0}' local copy error: {1}", this.cloneUri, e.Message);
                return false; 
            }
            return true;
        }

        private Branch CheckoutRemoteBranch(Repository rep)
        {
            // set to remote branch
            return rep.Checkout(GetRemoteBranch(rep));// TODO: if lock error raised: version take methods(assemblys part) not thread-save
        }
        public override bool Clone()
        {
            string dc = null;
            try
            {
                dc = Repository.Clone(base.cloneUri, base.LocalContainerDirectory);
                if (CheckLocalCopy())
                    status = Status.allUpToDate;
                else
                {
                    status = Status.cloneFailure;
                    return false;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("git scm: Clone failure at '{0}, msg: {1}", cloneUri, e.Message);
                status = Status.cloneFailure;
                return false;
            }

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

        public override SCMRevision LocalVersion
        {
            get
            {
                Branch focusBranch = null;
                using (var repo = new Repository(base.LocalContainerDirectory))
                {
                    focusBranch = CheckoutRemoteBranch(repo);
                    if (focusBranch == null)
                        return null;
                    Commit commit = focusBranch.Commits.First();
                    return new SCMRevision
                    {
                        CommitMessage = commit.Message,
                        CommitTime = commit.Committer.When.LocalDateTime,
                        Revision = commit.Sha,
                        Commiter = commit.Committer.Email
                    };
                }
            }
        }
    }
}

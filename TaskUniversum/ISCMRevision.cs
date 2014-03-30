using System;
namespace TaskUniversum
{
    public interface IRevision
    {
        string Commiter { get; set; }
        string CommitMessage { get; set; }
        DateTime CommitTime { get; set; }
        DateTime CreateAt { get; set; }
        string Revision { get; set; }
    }
}

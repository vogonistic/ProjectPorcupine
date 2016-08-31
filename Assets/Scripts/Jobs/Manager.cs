#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Collections.Generic;

namespace ProjectPorcupine.Jobs
{
    // Seriously dirty hack together impl, this is all we need, if we don't want to change anything.
    public class Manager
    {
        // TODO: find a better, room based, implementation of this!
        private JobQueue jobQueue;
        private JobQueue jobWaitingQueue;

        public Manager()
        {
            jobQueue = new JobQueue();
            jobWaitingQueue = new JobQueue();
        }

        // This NEEDS to be removed and changed AS SOON as the new impl is made!
        public event Action<Job> OnJobCreated;

        public void Enqueue(Job j)
        {
            jobQueue.Enqueue(j);

            if (OnJobCreated != null)
            {
                OnJobCreated(j);
            }
        }

        public void EnqueueWaiting(Job j)
        {
            jobWaitingQueue.Enqueue(j);

            if (OnJobCreated != null)
            {
                OnJobCreated(j);
            }
        }

        public void Remove(Job j)
        {
            jobQueue.Remove(j);
        }

        public void RemoveWaiting(Job j)
        {
            jobWaitingQueue.Remove(j);
        }

        public Job Dequeue()
        {
            return jobQueue.Dequeue();
        }

        public Job DequeueWaiting()
        {
            return jobWaitingQueue.Dequeue();
        }

        public IEnumerable<Job> PeekJobs()
        {
            return jobQueue.PeekJobs();
        }

        public IEnumerable<Job> PeekJobsWaiting()
        {
            return jobWaitingQueue.PeekJobs();
        }
    }
}
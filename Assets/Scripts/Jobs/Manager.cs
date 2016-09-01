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

        public Manager()
        {
            jobQueue = new JobQueue();
        }

        // This NEEDS to be removed and changed AS SOON as the new impl is made!
        public event Action<Job> OnJobCreated;

        public void Enqueue(Job job)
        {
            jobQueue.Enqueue(job);

            if (OnJobCreated != null)
            {
                OnJobCreated(job);
            }
        }

        public void Remove(Job job)
        {
            jobQueue.Remove(job);
        }

        public Job GetJob(Character character)
        {
            // Prioritize jobs with the same type as last job
            // Prioritize jobs from the same room as the character is in

            foreach (Job job in PeekJobs())
            {
                // Test that character can do job
                // TODO

                // Test that job can be fulfilled
                // TODO: We will probably need something faster than this.
                // Should InventoryManager keep a running count of stack sizes? Is it notified when they change?
                if (job.FulfillableInventoryRequirements() == null)
                {
                    continue;
                }

                // Ask character for affinity (What do we do with this?)
                // TODO

                // We can do this job. Remove is safe because we return immediately
                jobQueue.Remove(job);
                return job;
            }

            // No jobs available
            return null;
        }

        public IEnumerable<Job> PeekJobs()
        {
            return jobQueue.PeekJobs();
        }
    }
}
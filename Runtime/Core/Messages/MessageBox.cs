using System;
using System.Collections.Generic;

namespace ErmineGames.Features
{
    public class MessageBox<T>
    {
        private List<T> fresh = new();
        private List<T> ready = new();
        
        public void AddMessage(T message)
        {
            fresh.Add(message);
        }

        public List<T> GetReadyContent()
        {
            return ready;
        }

        public void Deliver(Predicate<T> clearOldCondition = null)
        {
            if (clearOldCondition == null)
            {
                ready.Clear();
            }
            else
            {
                ready.RemoveAll(clearOldCondition);
            }
            
            (fresh, ready) = (ready, fresh);

            if (fresh.Count > 0)
            {
                foreach (var message in fresh)
                {
                    ready.Add(message);
                }
                
                fresh.Clear();
            }
        }
    }
}
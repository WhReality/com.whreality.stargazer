using System;
using System.Collections.Generic;
using UnityEditor;

namespace WhReality.Stargazer.Infrastructure
{
    public class UnityMainThreadDispatcher
    {
        private static UnityMainThreadDispatcher instance;

        private Queue<Action> ActionQueue { get; set; } = new Queue<Action>();

        public static UnityMainThreadDispatcher Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UnityMainThreadDispatcher();
                }

                return instance;
            }
        }

        private UnityMainThreadDispatcher()
        {
            EditorApplication.update += Update;
        }

        private void Update()
        {
            if (ActionQueue.Count <= 0) return;
            var action = ActionQueue.Dequeue();
            action?.Invoke();
        }

        public void Enqueue(Action action)
        {
            ActionQueue.Enqueue(action);
        }
        
    }
}
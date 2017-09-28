using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchPointTennis_Crawler
{
    /// <summary>
    /// Available cross ViewModel messages
    /// </summary>
    public enum ViewModelMessages
    {
        ItemProcessed,
        ItemFailed,
        ItemsCollected,
        Finished,
        RequestSent,
        RequestReceived,
    };


    public sealed class Mediator
    {
        static readonly Mediator instance = new Mediator();

        private volatile object locker = new object();

        MultiDictionary<ViewModelMessages, Action<Object>> internalList = new MultiDictionary<ViewModelMessages, Action<Object>>();
        
        static Mediator()
        { }

        private Mediator()
        { }

        /// <summary>
        /// The singleton instance
        /// </summary>
        public static Mediator Instance => instance;
        
        /// <summary>
        /// Registers a Colleague to a specific message
        /// </summary>
        /// <param name="callback">The callback to use when the message it seen</param>
        /// <param name="message">The message to register to</param>
        public void Register(Action<Object> callback, ViewModelMessages message)
        {
            internalList.AddValue(message, callback);
        }


        /// <summary>
        /// Notify all colleagues that are registed to the specific message
        /// </summary>
        /// <param name="message">The message for the notify by</param>
        /// <param name="args">The arguments for the message</param>
        public void Notify(ViewModelMessages message, object args)
        {
            if (internalList.ContainsKey(message))
            {
                // Forward the message to all listeners.
                foreach (Action<object> callback in internalList[message])
                {
                    callback(args);
                }
            }
        }

    }
}

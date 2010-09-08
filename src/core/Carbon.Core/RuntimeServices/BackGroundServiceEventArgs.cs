using System;

namespace Carbon.Core.RuntimeServices
{
    public class BackGroundServiceEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public BackGroundServiceEventArgs()
        {

        }
        public BackGroundServiceEventArgs(string message)
        {
            Message = message;
        }
    }
}
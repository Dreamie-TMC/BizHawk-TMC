using System;

namespace MinishCapTools.Exceptions
{
    public class AutosplitterConnectionException : Exception
    {
        public AutosplitterConnectionException(string message) : base(message)
        {
        }
    }
}
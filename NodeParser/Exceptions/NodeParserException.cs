using System;
using System.Runtime.Serialization;

namespace NodeParser.Exceptions
{
    /// <summary>
    ///     Base class for exceptions in the node parser.
    /// </summary>
    [Serializable]
    public abstract class NodeParserException : Exception
    {
        public NodeParserException() {}
        public NodeParserException(string message) : base(message) {}
        public NodeParserException(string message, Exception inner) : base(message, inner) {}

        protected NodeParserException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {}
    }
}
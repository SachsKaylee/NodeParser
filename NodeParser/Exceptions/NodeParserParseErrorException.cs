using System;
using System.Runtime.Serialization;

namespace NodeParser.Exceptions
{
    /// <summary>
    ///     Used to indicate an error while parsing a node parser grammar.
    /// </summary>
    [Serializable]
    public class NodeParserParseErrorException : Exception
    {
        public NodeParserParseErrorException(NodeLocation location)
        {
            Location = location;
        }

        public NodeParserParseErrorException(NodeLocation location, string message) : base(message)
        {
            Location = location;
        }

        public NodeParserParseErrorException(NodeLocation location, string message, Exception inner) : base(message, inner)
        {
            Location = location;
        }

        protected NodeParserParseErrorException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {}

        public NodeLocation Location { get; }
    }
}
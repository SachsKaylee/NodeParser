using System;
using Irony.Parsing;

namespace NodeParser
{
    /// <summary>
    ///     Represents the location of a node.
    /// </summary>
    public struct NodeLocation
    {
        /// <summary>
        ///     Which line index is this location at?
        /// </summary>
        public readonly int LineIndex;

        /// <summary>
        ///     Which character index is its line is this location?
        /// </summary>
        public readonly int ColumnIndex;

        /// <summary>
        ///     The characer index in the file.
        /// </summary>
        public readonly int FileIndex;

        /// <summary>
        ///     The file name this locations is in.
        /// </summary>
        public readonly string File;

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="file" /> is <see langword="null" /></exception>
        public NodeLocation(int lineIndex, int columnIndex, int fileIndex, string file)
        {
            if (file == null) {
                throw new ArgumentNullException(nameof(file));
            }
            LineIndex = lineIndex;
            ColumnIndex = columnIndex;
            FileIndex = fileIndex;
            File = file;
        }

        /// <summary>
        ///     Creates a new node location from an irony location.
        /// </summary>
        /// <param name="file">The file name.</param>
        /// <param name="iLoc">The source location.</param>
        /// <returns>The node location.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file" /> is <see langword="null" /></exception>
        public static NodeLocation FromIrony(string file, SourceLocation iLoc)
        {
            if (file == null) {
                throw new ArgumentNullException(nameof(file));
            }
            return new NodeLocation(iLoc.Line, iLoc.Column, iLoc.Position, file);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return File + ":" + (LineIndex + 1) + ":" + (ColumnIndex + 1);
        }
    }
}
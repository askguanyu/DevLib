//-----------------------------------------------------------------------
// <copyright file="AddInAssemblyCompilerException.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.CodeDom.Compiler;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    /// <summary>
    /// Class AddInAssemblyCompilerException.
    /// </summary>
    [Serializable]
    public class AddInAssemblyCompilerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddInAssemblyCompilerException" /> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
        public AddInAssemblyCompilerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddInAssemblyCompilerException" /> class.
        /// </summary>
        /// <param name="message">Message string.</param>
        /// <param name="errors">Instance of CompilerErrorCollection.</param>
        public AddInAssemblyCompilerException(string message, CompilerErrorCollection errors)
            : base(message)
        {
            this.Errors = errors;
        }

        /// <summary>
        /// Gets instance of CompilerErrorCollection.
        /// </summary>
        public CompilerErrorCollection Errors
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns>Errors string.</returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(this.Message);

            foreach (CompilerError error in this.Errors)
            {
                stringBuilder.AppendFormat("{0}\r\n", error);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}

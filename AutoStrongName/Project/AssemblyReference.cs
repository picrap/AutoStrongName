﻿#region AutoStrongName
// AutoStrongName
// An automatic tool to presign unsigned dependencies
// https://github.com/picrap/AutoStrongName
#endregion

namespace AutoStrongName.Project
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Reference to assembly
    /// </summary>
    [DebuggerDisplay("{Literal} / GAC={Gac}")]
    public class AssemblyReference : IReferences
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public AssemblyName Name { get; private set; }
        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; private set; }

        private Assembly _assembly;

        /// <summary>
        /// Gets the assembly.
        /// </summary>
        /// <value>
        /// The assembly.
        /// </value>
        public Assembly Assembly
        {
            get
            {
                if (_assembly == null)
                    _assembly = Resolve();
                return _assembly;
            }
        }

        private AssemblyName _assemblyName;

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        /// <value>
        /// The name of the assembly.
        /// </value>
        public AssemblyName AssemblyName
        {
            get
            {
                if (Assembly == null)
                    return null;
                if (_assemblyName == null)
                {
                    _assemblyName = Assembly.GetName();
                    if (Path == null)
                        Path = Assembly.Location;
                }
                return _assemblyName;
            }
        }

        private IEnumerable<AssemblyReference> _references;

        /// <summary>
        /// Gets the references.
        /// </summary>
        /// <value>
        /// The references.
        /// </value>
        public IEnumerable<AssemblyReference> References
        {
            get
            {
                if (Assembly == null)
                    return null;
                if (_references == null)
                    _references = Assembly.GetReferencedAssemblies().Select(a => new AssemblyReference(a));
                return _references;
            }
        }

        /// <summary>
        /// Indicates wheter the assembly is signed.
        /// </summary>
        /// <value>
        /// The is signed.
        /// </value>
        public bool? IsSigned
        {
            get
            {
                if (Assembly == null)
                    return null;
                return Assembly.GetName().GetPublicKey().Length > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="AssemblyReference"/> is in GAC.
        /// Currently, we suppose this when no path and no version are specifed
        /// </summary>
        /// <value>
        ///   <c>true</c> if gac; otherwise, <c>false</c>.
        /// </value>
        public bool Gac
        {
            get
            {
                if (Path != null)
                    return false;
                if (Name.Version != null)
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Gets the literal.
        /// </summary>
        /// <value>
        /// The literal.
        /// </value>
        private string Literal { get { return Path ?? Name.ToString(); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyReference"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public AssemblyReference(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyReference"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public AssemblyReference(AssemblyName name)
        {
            Name = name;
        }

        /// <summary>
        /// Resolves the assembly (and loads it).
        /// </summary>
        /// <returns></returns>
        private Assembly Resolve()
        {
            return ResolveName() ?? ResolvePath();
        }

        /// <summary>
        /// Resolves by path.
        /// </summary>
        /// <returns></returns>
        private Assembly ResolvePath()
        {
            if (Path == null)
                return null;
            try
            {
                var absolutePath = System.IO.Path.GetFullPath(Path);
                Path = absolutePath;
                return Assembly.ReflectionOnlyLoad(File.ReadAllBytes(absolutePath));
            }
            catch (FileLoadException)
            { }
            catch (FileNotFoundException)
            { }
            catch (BadImageFormatException)
            { }
            return null;
        }

        /// <summary>
        /// Resolves by name.
        /// </summary>
        /// <returns></returns>
        private Assembly ResolveName()
        {
            if (Name == null)
                return null;
            try
            {
                return Assembly.ReflectionOnlyLoad(Name.FullName);
            }
            catch (FileLoadException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            catch (BadImageFormatException)
            {
            }
            try
            {
#pragma warning disable 618
                return Assembly.LoadWithPartialName(Name.ToString());
#pragma warning restore 618
            }
            catch (FileLoadException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            catch (BadImageFormatException)
            {
            }
            return null;
        }
    }
}

﻿// -----------------------------------------------------------------------
// Copyright © 2012 Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Lightweight.ProgrammingModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel.Composition.Lightweight.Hosting.Core
{
    /// <summary>
    /// Represents an export descriptor that an available part can provide.
    /// </summary>
    /// <remarks>This type is central to the cycle-checking, adaptation and 
    /// compilation features of the container.</remarks>
    public class ExportDescriptorPromise
    {
        readonly string _origin;
        readonly bool _isShared;
        readonly Lazy<Dependency[]> _dependencies;
        readonly Lazy<ExportDescriptor> _descriptor;
        readonly Contract _contract;

        bool _creating;

        /// <summary>
        /// Create a promise for an export descriptor.
        /// </summary>
        /// <param name="origin">A description of where the export is being provided from (e.g. the part type).
        /// Used to provide friendly errors.</param>
        /// <param name="isShared">True if the export is shared within some context, otherwise false. Used in cycle
        /// checking.</param>
        /// <param name="dependencies">A function providing dependencies required in order to fulfill the promise.</param>
        /// <param name="fulfillment">A function providing the promise.</param>
        /// <param name="contract">The contract fulfilled by this promise.</param>
        /// <seealso cref="ExportDescriptorProvider"/>.
        public ExportDescriptorPromise(
            Contract contract,
            string origin,
            bool isShared,
            Func<IEnumerable<Dependency>> dependencies,
            Func<Dependency[], ExportDescriptor> fulfillment)
        {
            _contract = contract;
            _origin = origin;
            _isShared = isShared;
            _dependencies = new Lazy<Dependency[]>(() => dependencies().ToArray());
            _descriptor = new Lazy<ExportDescriptor>(() => fulfillment(_dependencies.Value));
        }

        /// <summary>
        /// A description of where the export is being provided from (e.g. the part type).
        /// Used to provide friendly errors.
        /// </summary>
        public string Origin { get { return _origin; } }

        /// <summary>
        /// True if the export is shared within some context, otherwise false. Used in cycle
        /// checking.
        /// </summary>
        public bool IsShared { get { return _isShared; } }

        /// <summary>
        /// The dependencies required in order to fulfill the promise.
        /// </summary>
        public Dependency[] Dependencies { get { return _dependencies.Value; } }

        /// <summary>
        /// The contract fulfilled by this promise.
        /// </summary>
        public Contract Contract { get { return _contract; } }

        /// <summary>
        /// Retrieve the promised export descriptor.
        /// </summary>
        /// <returns>The export descriptor.</returns>
        public ExportDescriptor GetDescriptor()
        {
            if (_creating && !_descriptor.IsValueCreated)
                return new CycleBreakingExportDescriptor(_descriptor);

            _creating = true;
            if (_descriptor.Value == null) throw new InvalidOperationException("Export descriptor fulfillment function returned null.");
            _creating = false;

            return _descriptor.Value;
        }

        /// <summary>
        /// Describes the promise.
        /// </summary>
        /// <returns>A description of the promise.</returns>
        public override string ToString()
        {
            return string.Format("{0} supplied by {1}", Contract, Origin);
        }
    }
}

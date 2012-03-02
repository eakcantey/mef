﻿// -----------------------------------------------------------------------
// Copyright © 2012 Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.Composition.Lightweight.Hosting;
using System.ComponentModel.Composition.Registration;
using System.ComponentModel.Composition.Lightweight.Registration;

namespace System.ComponentModel.Composition.Web.Mvc
{
    /// <summary>
    /// A <see cref="ContainerConfiguration"/> that loads parts in the currently-executing ASP.NET MVC web application.
    /// Parts are detected by namespace - classes in a ".Parts" namespace will be considered to be parts. These classes,
    /// and any interfaces they implement, will be exported and shared on a per-HTTP-request basis. When resolving
    /// dependencies, the longest public constructor on a part type will be used. The <see cref="ImportAttribute"/>,
    /// <see cref="ExportAttribute"/> and associated MEF attributes can be applied to modify composition structure.
    /// </summary>
    /// <remarks>
    /// This implementation emulates CompositionContainer and the composition-container based MVC
    /// integration for all types under the Parts namespace, for controllers, and for model binders. This will
    /// aid migration from one composition engine to the other, but this decision should be revisited if it
    /// causes confusion.
    /// </remarks>
    public class MvcContainerConfiguration : ContainerConfiguration
    {
        MvcContainerConfiguration(IEnumerable<Assembly> assemblies, ReflectionContext reflectionContext)
        {
            if (assemblies == null) throw new ArgumentNullException("assemblies");
            if (reflectionContext == null) throw new ArgumentNullException("reflectionContext");

            if (reflectionContext != null)
                this.WithDefaultConventions(reflectionContext);

            this.WithAssemblies(assemblies);
        }

        /// <summary>
        /// Construct an <see cref="MvcContainerConfiguration"/> using parts in the specified assemblies.
        /// </summary>
        /// <param name="assemblies">Assemblies in which to search for parts.</param>
        public MvcContainerConfiguration(IEnumerable<Assembly> assemblies)
            : this(assemblies, DefineConventions())
        {
        }

        /// <summary>
        /// Construct an <see cref="MvcContainerConfiguration"/> using parts in the main application assembly.
        /// In some applications this may not be the expected assembly - in those cases specify the
        /// assemblies explicitly using the other constructor.
        /// </summary>
        public MvcContainerConfiguration()
            : this(new[] { GuessGlobalApplicationAssembly() })
        {
        }

        internal static Assembly GuessGlobalApplicationAssembly()
        {
            return HttpContext.Current.ApplicationInstance.GetType().BaseType.Assembly;
        }

        private static ReflectionContext DefineConventions()
        {
            var rb = new RegistrationBuilder();

            rb.ForTypesDerivedFrom<IController>()
                .Export();

            rb.ForTypesMatching(IsAPart)
                .Export()
                .ExportInterfaces();

            return rb;
        }

        private static bool IsAPart(Type t)
        {
            return !t.IsAssignableFrom(typeof(Attribute)) &&
                                    t.Namespace != null &&
                                    t.IsInNamespace("Parts");
        }
    }
}

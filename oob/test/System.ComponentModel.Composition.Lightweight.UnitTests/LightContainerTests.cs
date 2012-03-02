﻿using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Registration;
using System.ComponentModel.Composition.Lightweight.Hosting;
using System.ComponentModel.Composition.Lightweight.Registration;
using System.Reflection;
using TestLibrary;

namespace System.ComponentModel.Composition.Lightweight.UnitTests
{
    [TestClass]
    public class LightContainerTests : ContainerTests
    {
        public interface IA { }

        [Export(typeof(IA))]
        public class A : IA, IDisposable
        {
            public bool IsDisposed;

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        [Export(typeof(IA))]
        public class A2 : IA { }

        [Export]
        public class B
        {
            public IA A;

            [ImportingConstructor]
            public B(IA ia)
            {
                A = ia;
            }
        }

        public class BarePart { }

        public class HasPropertyA
        {
            public IA A { get; set; }
        }

        [TestMethod]
        public void CreatesInstanceWithNoDependencies()
        {
            var cc = CreateContainer(typeof(A));
            var x = cc.GetExport<IA>();
            Assert.IsInstanceOfType(x, typeof(A));
        }

        [TestMethod]
        public void DefaultLifetimeIsNonShared()
        {
            var cc = CreateContainer(typeof(A));
            var x = cc.GetExport<IA>();
            var y = cc.GetExport<IA>();
            Assert.AreNotSame(x, y);
        }

        [TestMethod]
        public void Composes()
        {
            var cc = CreateContainer(typeof(A), typeof(B));
            var x = cc.GetExport<B>();
            Assert.IsInstanceOfType(x.A, typeof(A));
        }

        [TestMethod]
        public void CanSpecifyExportsWithRegistrationBuilder()
        {
            var rb = new RegistrationBuilder();
            rb.ForType<BarePart>().Export();
            var cc = CreateContainer(rb, typeof(BarePart));
            var x = cc.GetExport<BarePart>();
            Assert.IsNotNull(x);
        }

        [TestMethod]
        public void CanSpecifyLifetimeWithRegistrationBuilder()
        {
            var rb = new RegistrationBuilder();
            rb.ForType<BarePart>().Export().Shared();
            var cc = CreateContainer(rb, typeof(BarePart));
            var x = cc.GetExport<BarePart>();
            var y = cc.GetExport<BarePart>();
            Assert.AreSame(x, y);
        }

        [TestMethod]
        public void InjectsPropertyImports()
        {
            var rb = new RegistrationBuilder();
            rb.ForType<HasPropertyA>().ImportProperty(a => a.A).Export();
            var cc = CreateContainer(rb, typeof(HasPropertyA), typeof(A));
            var x = cc.GetExport<HasPropertyA>();
            Assert.IsInstanceOfType(x.A, typeof(A));
        }

        [TestMethod]
        public void VerifyAssemblyNameCanBeUsedWithContainer()
        {
            AssemblyName name = new AssemblyName("TestLibrary");
            Assembly asmbly = Assembly.Load(name);
            var test = new ContainerConfiguration().WithAssembly(asmbly).CreateContainer();
            var b = test.Value.GetExport<ClassWithDependecy>();
            Assert.IsNotNull(b);
            Assert.IsNotNull(b._dep);
        }

        [TestMethod]
        public void VerifyAssemblyWithTwoBaseTypeWithOnlyOneExportedWorks()
        {
            AssemblyName name = new AssemblyName("TestLibrary");
            Assembly asmbly = Assembly.Load(name);
            var test = new ContainerConfiguration().WithAssembly(asmbly).CreateContainer();
            var b = test.Value.GetExport<ClassWithDependecyAndSameBaseType>();
            Assert.IsNotNull(b);
            Assert.IsNotNull(b._dep);
            Assert.AreEqual<Type>(b._dep.GetType(), typeof(Dependency));
        }
    }
}

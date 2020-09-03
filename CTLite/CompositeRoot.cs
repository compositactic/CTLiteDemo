// CTLite - Made in the USA - Indianapolis, IN  - Copyright (c) 2020 Matt J. Crouch

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN 
// NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace CTLite
{
    [DataContract]
    public abstract class CompositeRoot : Composite, IDisposable
    {
        protected CompositeRoot()
        {
            var assemblies = GetServiceAssemblyNames().Select(ca => Assembly.Load(ca.FullName));
            InitializeServices(assemblies);
        }

        protected CompositeRoot(params IService[] services)
        {
            _services = new Collection<IService>(services.ToList());
            SetCompositeRoots();
        }

        public abstract void InitializeCompositeModel(object model); 

        [DataMember]
        public abstract long Id { get; } 

        private static IEnumerable<AssemblyName> GetServiceAssemblyNames()
        {
            var serviceAssemblyNames = new Collection<AssemblyName>();
            var dirInfo = new DirectoryInfo(Environment.CurrentDirectory);
            foreach (var file in dirInfo.EnumerateFiles("", SearchOption.AllDirectories).Where(f => f.Extension.ToUpperInvariant() == ".DLL"))
            {
                try
                {
                    serviceAssemblyNames.Add(AssemblyName.GetAssemblyName(file.FullName));
                }
                catch (BadImageFormatException)
                {
                    continue;
                }
            }

            return serviceAssemblyNames;
        }

        private Collection<IService> _services;
        protected void InitializeServices(IEnumerable<Assembly> assemblies)
        {
            _services = new Collection<IService>(
                    assemblies
                        .SelectMany(assembly => assembly.GetTypes()
                        .Where(t => t.GetInterface(nameof(IService)) != null && t.IsClass && !t.IsAbstract))
                            .Select(serviceType => (IService)serviceType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null).Invoke(null))
                            .ToList());

            SetCompositeRoots();
        }

        public TService GetService<TService>() where TService : IService
        {
            return (TService)_services.Single(service => service is TService);
        }

        private void SetCompositeRoots()
        {
            foreach (var service in _services)
                service.CompositeRoot = this;
        }

        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing) { }

            disposed = true;
        }

        ~CompositeRoot()
        {
            Dispose(false);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using Zenject;

namespace Unity.CompanionAppCommon
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// Inspired by <see cref="SubContainerCreatorUtil.ApplyBindSettings"/>.
        /// Call this method on a subcontainer to enable Initialize, Dispose, Tick, etc on its contents.
        /// Topic is described in https://github.com/modesttree/Zenject/blob/master/Documentation/SubContainers.md#using-byinstaller--bymethod-with-kernel.
        /// </summary>
        public static DiContainer WithKernel(this DiContainer container)
        {
            var parentContainer = container.ParentContainers.OnlyOrDefault();
            Assert.IsNotNull(parentContainer, "Could not find unique container when using WithKernel");

            parentContainer.BindInterfacesTo<Kernel>().FromSubContainerResolve().ByInstance(container).AsCached();
            container.Bind<Kernel>().AsCached();

            return container;
        }

        // Return the first item when the list is of length one and otherwise returns default
        static TSource OnlyOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            Assert.IsNotNull(source);

            if (source.Count() > 1)
            {
                return default(TSource);
            }

            return source.FirstOrDefault();
        }
    }
}

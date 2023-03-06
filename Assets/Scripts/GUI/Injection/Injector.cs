using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace DDEngine.GUI.Injection
{
    public class Injector
    {
        private Dictionary<GUIControlID, List<Tuple<MethodInfo, InjectIntoControlAttribute>>> injectorFunctions;
        private static List<Injector> globalInjectors = new();
        private static Dictionary<string, List<Injector>> specificInjector = new(StringComparer.OrdinalIgnoreCase);

        public static void AddInjector<T>() where T: Injector, new()
        {
            var injectorType = typeof(T);
            var injectorAttribute = injectorType.GetCustomAttribute<InjectIntoSetAttribute>();
            var injectorList = globalInjectors;

            if (injectorAttribute != null)
            {
                if (!specificInjector.TryGetValue(injectorAttribute.ControlSetPath, out injectorList))
                {
                    injectorList = new();
                    specificInjector.Add(injectorAttribute.ControlSetPath, injectorList);
                }
            }

            if (injectorList.FirstOrDefault(item => item.GetType() == injectorType) == null)
            {
                injectorList.Add(new T());
            } else
            {
                throw new System.Exception("The injector already exists!");
            }
        }

        public static List<Injector> GetInjectorsFor(string controlSetPath)
        {
            var listClone = globalInjectors.ToList();
            if (specificInjector.TryGetValue(controlSetPath, out var specificList))
            {
                listClone.AddRange(specificList);
            }
            return listClone;
        }

        public Injector()
        {
            var myType = GetType();
            var methods = myType.GetMethods();
            var attributes = methods
                .Select(method => new Tuple<MethodInfo, InjectIntoControlAttribute>(method, method.GetCustomAttribute<InjectIntoControlAttribute>()))
                .Where(attributePair => attributePair.Item2 != null);

            injectorFunctions = attributes
                .GroupBy(attributePair => attributePair.Item2.ControlToInject)
                .ToDictionary(attributeGroup => attributeGroup.Key, attributeGroup => attributeGroup.ToList());
        }

        public void CallInjectorFunction<T>(GUIControlID control, T controlToInject)
        {
            if (injectorFunctions.TryGetValue(control, out var injectorList))
            {
                foreach (var injector in injectorList) {
                    injector.Item1.Invoke(this, new object[] { controlToInject });
                }
            }
        }
    }
}

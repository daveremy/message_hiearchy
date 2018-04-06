using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

/// <summary>
/// The hierarchy of types inheriting from the Message Type.  
/// </summary>
public static class MessageHierarchy
{
    public static EventHandler MessageTypesAdded;
    static MessageHierarchy()
    {
        // Get the all of the types in the Assembly that are derived from Message and then build a 
        // backing TypeTree.
        // TODO: Put in timing logging
        TypeTree.BuildTree(GetTypesDerivedFrom(typeof(Message)));
        AppDomain.CurrentDomain.AssemblyLoad += AssemblyLoadEventHandler;
    }

    public static IEnumerable<Type> AncestorsAndSelf(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        return TypeTree.AncestorsAndSelf(type);
    }

    public static IEnumerable<Type> DescendantsAndSelf(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        return TypeTree.DescendantsAndSelf(type);
    }

    private static void AssemblyLoadEventHandler(object sender, AssemblyLoadEventArgs args)
    {
        // TODO: Comment on this if condition.
        if (!args.LoadedAssembly.IsDynamic && args.LoadedAssembly.Location.Contains(AppDomain.CurrentDomain.BaseDirectory))
        {
            TypeTree.ResetTypeTree(GetTypesDerivedFrom(typeof(Message)));
            MessageTypesAdded(null, null);
        }
    }

    private static List<Type> GetTypesDerivedFrom(Type rootType)
    {
        // Get all the derived types from loaded assemblies (with some systems assemblies
        // filtered out).  
        var derivedTypes = new List<Type>();
        foreach(var assembly in FilteredAssemblies())
        {
            // TODO: Filter already processed assemblies (known assemblies?)
            foreach (var subType in assembly.GetLoadableTypes().Where(rootType.IsAssignableFrom))
            {
                derivedTypes.Add(subType);
            }
        }
        return derivedTypes;
    }

    private static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        // See https://stackoverflow.com/questions/7889228/how-to-prevent-reflectiontypeloadexception-when-calling-assembly-gettypes
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null);
        }
    }

    private static string[] ExcludedAssemblies =
    {
        "mscorlib",
        "System.Core",
        "System",
        "System.Xml",
        "Microsoft.VisualStudio.Debugger.Runtime",
        "Telerik"
    };
    private static IEnumerable<Assembly> FilteredAssemblies()
    {
        // Filter own assemblies that we know won't have usertypes
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !ExcludedAssemblies.Any(excluded => a.FullName.StartsWith(excluded + ",")));
    }
}

internal static class TypeTree
{
    private static TypeTreeNode _root;
    private static Dictionary<Type, TypeTreeNode> _typeToNode = new Dictionary<Type, TypeTreeNode>();
    private static readonly object _loadingLock = new object();
    internal static void BuildTree(List<Type> types)
    {
        BuildTypeTree(types);
    }

    internal static void ResetTypeTree(List<Type> types)
    {
        // Rebuild the type tree. Lock in case multiple overlapping resets are called.
        lock (_loadingLock)
        {
            _typeToNode = new Dictionary<Type, TypeTreeNode>();
            BuildTypeTree(types);
        }
    }

    private static void BuildTypeTree(List<Type> types)
    {
        // One time pass through types to create initial node list.
        foreach (var type in types)
        {
            _typeToNode[type] = new TypeTreeNode(type);
        }
        // One more time build tree (adding child to parent).
        foreach (var type in _typeToNode.Keys)
        {
            if (_typeToNode.ContainsKey(type.BaseType))
            {
                // Found the parent so add child to parent.
                _typeToNode[type.BaseType].AddChild(_typeToNode[type]);
            }
            else
            {
                // If the base type is not found then it must be root.
                _root = _typeToNode[type];
            }
        }
    }
    internal static IEnumerable<Type> AncestorsAndSelf(Type type)
    {
        TypeTreeNode typeNode = _typeToNode[type];
        while (typeNode != null)
        {
            yield return typeNode.Type;
            typeNode = typeNode.Parent;
        }
    }

    internal static IEnumerable<Type> DescendantsAndSelf(Type type)
    {
        // non recursive depth first search
        // Initialize a stack of typeNodes to visit with the type passed in.
        Stack<TypeTreeNode> _nodesToVisit = new Stack<TypeTreeNode>(new[] { _typeToNode[type] });
        while (_nodesToVisit.Count != 0)
        {
            var typeNode = _nodesToVisit.Pop();
            yield return typeNode.Type;
            foreach (var node in typeNode.Children)
            {
                _nodesToVisit.Push(node);
            }
        }
    }
}

internal class TypeTreeNode
{
    public Type Type { get; private set; }
    public TypeTreeNode Parent { get; private set; }
    public List<TypeTreeNode> Children { get; private set; }

    public TypeTreeNode(Type type)
    {
        Type = type;
        Children = new List<TypeTreeNode>();
    }
    internal void AddChild(TypeTreeNode typeTreeNode)
    {
        typeTreeNode.Parent = this;
        Children.Add(typeTreeNode);
    }
}

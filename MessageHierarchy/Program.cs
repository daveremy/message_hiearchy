using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public static class Program
{
    public static void Main()
    {
        var type = typeof(Message);
        Console.WriteLine("MessageChild has " + MessageHierarchy.AncestorsAndSelf(type).Count() + " Ancestors");
        foreach (var parent in MessageHierarchy.AncestorsAndSelf(type))
        {
            Console.WriteLine(parent);
        }

        Console.WriteLine($"{type} has {MessageHierarchy.DescendantsAndSelf(type).Count()} Descendants");
        foreach (var descendant in MessageHierarchy.DescendantsAndSelf(type))
        {
            Console.WriteLine(descendant);
        }
    }
}

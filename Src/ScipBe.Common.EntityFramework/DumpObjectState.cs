// ==============================================================================================
// Namespace   : ScipBe.Common.EntityFramework
// Class(es)   : Extension methods for ObjectStateManager
// Version     : 1.0
// Author      : Stefan Cruysberghs
// Website     : http://www.scip.be
// Date        : June 2008 - December 2008
// Status      : Open source - MIT License
// ==============================================================================================

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Text;

namespace ScipBe.Common.EntityFramework
{
  public static partial class EntityFrameworkExtensionMethods
  {
    /// <summary>
    /// Convert an ObjectStateEntry object to a string representation
    /// </summary>
    /// <param name="entry">The given ObjectStateEntry</param>
    /// <returns>The string representation</returns>
    private static string ObjectStateEntryToString(ObjectStateEntry entry)
    {
      if (entry == null)
      {
        throw new ArgumentNullException("entry", "ObjectStateEntry is required");
      }

      StringBuilder builder = new StringBuilder();

      builder.AppendFormat("\n- <b>{0} ", entry.State.ToString());

      if (entry.EntityKey == null)
      {
        if (entry.EntitySet == null)
          builder.Append("Entity : null </b>[null]");
        else
          builder.AppendFormat("EntitySet : {0}</b>", entry.EntitySet.Name);
      }
      else
      {
        builder.AppendFormat("Entity : {0} </b>", entry.EntityKey.EntitySetName);

        if (entry.EntityKey.IsTemporary)
        {
          builder.Append("[Temporary]");
        }
        else
        {
          foreach (var key in entry.EntityKey.EntityKeyValues)
          {
            builder.AppendFormat("[{0} = {1}]", key.Key, ObjectToString(key.Value));
          }
        }
      }
      return (builder.ToString());
    }

    /// <summary>
    /// Convert an object to a string representation
    /// </summary>
    /// <param name="obj">The given object</param>
    /// <returns>The string representation</returns>
    private static string ObjectToString(Object obj)
    {
      if (obj.GetType().Name == "String")
        return String.Format("\"{0}\"", obj.ToString());
      if (obj.ToString() == "")
        return "null";
      return obj.ToString();
    }

    /// <summary>
    /// Private extension method for ObjectStateManager class
    /// Dump all tracking info to a string 
    /// </summary>
    /// <param name="manager">ObjectStateManager</param>
    /// <param name="objectStateEntries">Collection of ObjectStateEntries. If null, then all entities will be displayed</param>
    /// <param name="entityKey">EntityKey of given entity. If null, then all entities will be displayed</param>
    /// <param name="asHtml">Output string as HTML</param>
    /// <returns>String with tracking info about entries</returns>
    private static string Dump(
      this ObjectStateManager manager,
      IEnumerable<ObjectStateEntry> objectStateEntries,
      EntityKey entityKey,
      bool asHtml)
    {
      StringBuilder dump = new StringBuilder();

      if (entityKey != null)
      {
        objectStateEntries = new List<ObjectStateEntry>();
        (objectStateEntries as List<ObjectStateEntry>).Add(manager.GetObjectStateEntry(entityKey));
      }
      else if (objectStateEntries == null)
      {
        objectStateEntries =
          manager.GetObjectStateEntries(EntityState.Added)
          .Union(manager.GetObjectStateEntries(EntityState.Modified)
          .Union(manager.GetObjectStateEntries(EntityState.Deleted)));
      }

      dump.AppendFormat("ObjectStateManager entries : Change count = {0}\n", objectStateEntries.Count());

      foreach (var entry in objectStateEntries)
      {
        dump.Append(ObjectStateEntryToString(entry));

        if (entry.State == EntityState.Added)
        {
          for (int i = 0; i < entry.CurrentValues.FieldCount; i++)
          {
            dump.AppendFormat("\n\t- {0} = {1}",
              entry.CurrentValues.GetName(i),
              ObjectToString(entry.CurrentValues[i]));
          }
        }
        else if (entry.State == EntityState.Modified)
        {
          foreach (string prop in entry.GetModifiedProperties())
          {
            dump.AppendFormat("\n\t- {0} : {1} -> {2}",
                prop,
                ObjectToString(entry.OriginalValues[prop]),
                ObjectToString(entry.CurrentValues[prop]));
          }
        }
      }

      if (asHtml)
      {
        dump.Replace("\n", "<br />");
        dump.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
      }
      else
      {
        dump.Replace("<b>", "");
        dump.Replace("</b>", "");
      }

      return dump.ToString();
    }

    /// <summary>
    /// Extension method for ObjectStateManager class
    /// Dump all tracking info about the entries in the ObjectStateManager to a string 
    /// </summary>
    /// <param name="manager">ObjectStateManager</param>
    /// <returns>String with tracking info about entries</returns>
    /// <example><code>
    /// NorthwindEntities context = new NorthwindEntities();
    ///  
    /// // Modify Employee
    /// Employee firstEmployee = context.Employees.First(e =&amp;gt; e.EmployeeID == 1);
    /// if (firstEmployee != null)
    /// {
    ///   firstEmployee.City = "San Francisco";
    ///   firstEmployee.Notes = "New notes for employee 1";
    /// }
    /// // Delete Employee
    /// context.DeleteObject(context.Employees.First(e =&amp;gt; e.EmployeeID == 2));
    /// // Add Employee
    /// context.AddToEmployees(new Employee() { EmployeeID = 1000, FirstName = "Jan", LastName = "Jansen" });
    /// // Add Employee
    /// context.AddToEmployees(new Employee() { EmployeeID = 1001, FirstName = "Piet", LastName = "Pieters" });
    /// // Add Product
    /// context.AddToProducts(new Product() { ProductID = 1000, ProductName = "Visual Studio 2008" });
    ///  
    /// // Dump all tracking info
    /// Console.WriteLine(context.ObjectStateManager.Dump());
    ///  
    /// // Dump tracking info of first employee
    /// Console.WriteLine(context.ObjectStateManager.Dump(firstEmployee.EntityKey));
    ///  
    /// // Dump tracking info of all added products
    /// IEnumerable&amp;lt;ObjectStateEntry&amp;gt; objectStateEntries =
    ///   context.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Where(e =&amp;gt; e.Entity is Product);
    /// Console.WriteLine(context.ObjectStateManager.Dump(objectStateEntries));
    /// </code></example>
    public static string Dump(this ObjectStateManager manager)
    {
      return Dump(manager, null, null, false);
    }

    /// <summary>
    /// Extension method for ObjectStateManager class
    /// Dump all tracking info about the given ObjectStateEntries to a string 
    /// </summary>
    /// <param name="manager">ObjectStateManager</param>
    /// <param name="objectStateEntries">Collection of ObjectStateEntries. If null, then all entities will be displayed</param>
    /// <returns>String with tracking info about entries</returns>
    /// <example><code>
    /// NorthwindEntities context = new NorthwindEntities();
    ///  
    /// // Modify Employee
    /// Employee firstEmployee = context.Employees.First(e =&amp;gt; e.EmployeeID == 1);
    /// if (firstEmployee != null)
    /// {
    ///   firstEmployee.City = "San Francisco";
    ///   firstEmployee.Notes = "New notes for employee 1";
    /// }
    /// // Delete Employee
    /// context.DeleteObject(context.Employees.First(e =&amp;gt; e.EmployeeID == 2));
    /// // Add Employee
    /// context.AddToEmployees(new Employee() { EmployeeID = 1000, FirstName = "Jan", LastName = "Jansen" });
    /// // Add Employee
    /// context.AddToEmployees(new Employee() { EmployeeID = 1001, FirstName = "Piet", LastName = "Pieters" });
    /// // Add Product
    /// context.AddToProducts(new Product() { ProductID = 1000, ProductName = "Visual Studio 2008" });
    ///  
    /// // Dump all tracking info
    /// Console.WriteLine(context.ObjectStateManager.Dump());
    ///  
    /// // Dump tracking info of first employee
    /// Console.WriteLine(context.ObjectStateManager.Dump(firstEmployee.EntityKey));
    ///  
    /// // Dump tracking info of all added products
    /// IEnumerable&amp;lt;ObjectStateEntry&amp;gt; objectStateEntries =
    ///   context.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Where(e =&amp;gt; e.Entity is Product);
    /// Console.WriteLine(context.ObjectStateManager.Dump(objectStateEntries));
    /// </code></example>
    public static string Dump(this ObjectStateManager manager, IEnumerable<ObjectStateEntry> objectStateEntries)
    {
      return Dump(manager, objectStateEntries, null, false);
    }

    /// <summary>
    /// Extension method for ObjectStateManager class
    /// Dump all tracking info about the given Entity in the ObjectStateManager to a string 
    /// </summary>
    /// <param name="manager">ObjectStateManager</param>
    /// <param name="entityKey">Entity key of given entity. If null, then all entities will be displayed</param>
    /// <returns>String with tracking info about entry</returns>
    /// <example><code>
    /// NorthwindEntities context = new NorthwindEntities();
    ///  
    /// // Modify Employee
    /// Employee firstEmployee = context.Employees.First(e =&amp;gt; e.EmployeeID == 1);
    /// if (firstEmployee != null)
    /// {
    ///   firstEmployee.City = "San Francisco";
    ///   firstEmployee.Notes = "New notes for employee 1";
    /// }
    /// // Delete Employee
    /// context.DeleteObject(context.Employees.First(e =&amp;gt; e.EmployeeID == 2));
    /// // Add Employee
    /// context.AddToEmployees(new Employee() { EmployeeID = 1000, FirstName = "Jan", LastName = "Jansen" });
    /// // Add Employee
    /// context.AddToEmployees(new Employee() { EmployeeID = 1001, FirstName = "Piet", LastName = "Pieters" });
    /// // Add Product
    /// context.AddToProducts(new Product() { ProductID = 1000, ProductName = "Visual Studio 2008" });
    ///  
    /// // Dump all tracking info
    /// Console.WriteLine(context.ObjectStateManager.Dump());
    ///  
    /// // Dump tracking info of first employee
    /// Console.WriteLine(context.ObjectStateManager.Dump(firstEmployee.EntityKey));
    ///  
    /// // Dump tracking info of all added products
    /// IEnumerable&amp;lt;ObjectStateEntry&amp;gt; objectStateEntries =
    ///   context.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Where(e =&amp;gt; e.Entity is Product);
    /// Console.WriteLine(context.ObjectStateManager.Dump(objectStateEntries));
    /// </code></example>    
    public static string Dump(this ObjectStateManager manager, EntityKey entityKey)
    {
      return Dump(manager, null, entityKey, false);
    }

    /// <summary>
    /// Extension method for ObjectStateManager class
    /// Dump all tracking info about the entries in the ObjectStateManager to a HTML string 
    /// </summary>
    /// <param name="manager">ObjectStateManager</param>
    /// <returns>HTML string with tracking info about entries</returns>
    /// <example><code>
    /// Console.WriteLine(context.ObjectStateManager.DumpAsHtml());
    /// </code></example>
    public static string DumpAsHtml(this ObjectStateManager manager)
    {
      return Dump(manager, null, null, true);
    }

    /// <summary>
    /// Extension method for ObjectStateManager class
    /// Dump all info about the given ObjectStateEntries to a HTML string 
    /// </summary>
    /// <param name="manager">ObjectStateManager</param>
    /// <param name="objectStateEntries">Collection of ObjectStateEntries</param>
    /// <returns>HTML string with tracking info about entries</returns>
    /// <example><code>
    /// IEnumerable&amp;lt;ObjectStateEntry&amp;gt; objectStateEntries =
    ///   context.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Where(e =&amp;gt; e.Entity is Product);
    /// Console.WriteLine(context.ObjectStateManager.DumpAsHtml(objectStateEntries));
    /// </code></example>
    public static string DumpAsHtml(this ObjectStateManager manager, IEnumerable<ObjectStateEntry> objectStateEntries)
    {
      return Dump(manager, objectStateEntries, null, true);
    }

    /// <summary>
    /// Extension method for ObjectStateManager class
    /// Dump all tracking info about the given Entity in the ObjectStateManager to a HTML string 
    /// </summary>
    /// <param name="manager">ObjectStateManager</param>
    /// <param name="entityKey">Entity key of given entity</param>
    /// <returns>HTML string with tracking info about entry</returns>
    /// <example><code>
    /// Console.WriteLine(context.ObjectStateManager.DumpAsHtml(firstEmployee.EntityKey));
    /// </code></example>
    public static string DumpAsHtml(this ObjectStateManager manager, EntityKey entityKey)
    {
      return Dump(manager, null, entityKey, true);
    }
  }

}

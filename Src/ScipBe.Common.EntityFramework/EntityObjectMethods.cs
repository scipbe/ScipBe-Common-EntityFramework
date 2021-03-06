﻿// ==============================================================================================
// Namespace   : ScipBe.Common.EntityFramework
// Class(es)   : Extension methods for EntityObject
// Version     : 1.0
// Author      : Stefan Cruysberghs
// Website     : http://www.scip.be
// Date        : June 2008 - December 2008
// Status      : Open source - MIT License
// ==============================================================================================

using System;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Diagnostics;
using System.Linq;

namespace ScipBe.Common.EntityFramework
{
  public static partial class EntityFrameworkExtensionMethods
  {
    /// <summary>
    /// Get EntitySet name for given EntityType name
    /// </summary>
    /// <param name="entityTypeName">Name of EntityType (e.g. "Employee", "Category")</param>
    /// <param name="context">ObjectContext of entity</param>
    /// <returns>Name of EntitySet (e.g. "EmployeeSet", "Categories")</returns>
    public static string GetEntitySetName(string entityTypeName, ObjectContext context)
    {
      if (String.IsNullOrEmpty(entityTypeName))
      {
        throw new ArgumentNullException("entityTypeName", "EntityType name is required");
      }

      if (context == null)
      {
        throw new ArgumentNullException("context", "ObjectContext is required");
      }
      
      var container = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);
      string entitySetName = (from meta in container.BaseEntitySets
                              where meta.ElementType.Name == entityTypeName
                              select meta.Name).FirstOrDefault();

      return entitySetName ?? "";
    }

    /// <summary>
    /// Extension method for EntityObject which gets the EntitySet name
    /// </summary>
    /// <param name="entityObject">EntityObject (e.g. firstEmployee, firstCategory)</param>
    /// <param name="context">ObjectContext of entity</param>
    /// <returns>Name of EntitySet (e.g. "EmployeeSet", "Categories)</returns>
    /// <example><code>
    /// var emp = context.EmployeeSet.First();
    /// Console.WriteLine(emp.GetEntitySetName(context));
    /// 
    /// string entitySetName = entityObject.GetEntitySetName(context);
    /// context.AttachTo(entityObject.GetEntitySetName(context), entityObject);
    /// context.AddObject(entityObject.GetEntitySetName(context), entityObject);
    /// </code></example>
    public static string GetEntitySetName(this EntityObject entityObject, ObjectContext context)
    {
      if (entityObject == null)
      {
        throw new ArgumentNullException("entityObject", "EntityObject name is required");
      }

      if (entityObject.EntityKey != null)
      {
        return entityObject.EntityKey.EntitySetName;
      }

      if (context == null)
      {
        throw new ArgumentNullException("context", "ObjectContext is required");
      }

      string entityTypeName = GetEntityTypeName(entityObject);
      return GetEntitySetName(entityTypeName, context);
    }
    
    /// <summary>
    /// Extension method for EntityObject which gets the EntityType name
    /// </summary>
    /// <param name="entityObject">EntityObject (e.g. firstEmployee)</param>
    /// <returns>Name of EntityType (e.g. "Employee")</returns>
    /// <example><code>
    /// var emp = context.EmployeeSet.First();
    /// Console.WriteLine(emp.GetEntityTypeName());
    /// </code></example>
    public static string GetEntityTypeName(this EntityObject entityObject)
    {
      if (entityObject == null)
      {
        throw new ArgumentNullException("entityObject", "EntityObject name is required");
      }

      return entityObject.GetType().Name;
    }

    /// <summary>
    /// Extension method for EntityObject which gets ObjectQuery&lt;T&gt; 
    /// This is done by creating and returning an entity SQL query 
    /// which is generated by using the EntitySet name.
    /// </summary>
    /// <typeparam name="T">EntityObject class (e.g. Employee, Category)</typeparam>
    /// <param name="entityObject">EntityObject (e.g. firstEmployee, firstCategory)</param>
    /// <param name="context">ObjectContext of entity</param>
    /// <returns>ObjectQuery with collection of EntityObjects (e.g. ObjectQuery&lt;Employee&gt;, ObjectQuery&lt;Category&gt;)</returns>
    /// <example><code>
    /// var emp = context.EmployeeSet.First();
    /// var allEmployees = emp.GetEntitySet(context);
    /// </code></example>
    public static ObjectQuery<T> GetEntitySet<T>(this T entityObject, ObjectContext context) 
      where T : EntityObject
    {
      if (entityObject == null)
      {
        throw new ArgumentNullException("entityObject", "EntityObject name is required");
      }

      if (context == null)
      {
        throw new ArgumentNullException("context", "ObjectContext is required");
      }

      string entitySetName = GetEntitySetName(entityObject, context);
      string entityContainerName = context.DefaultContainerName;

      if (String.IsNullOrEmpty(entitySetName) || String.IsNullOrEmpty(entityContainerName))
      {
        Debug.Write("EntitySetName or EntityContainerName not found");
        return null;
      }
      
      string eSql = String.Format("SELECT VALUE a FROM {0}.{1} AS a", entityContainerName, entitySetName);
      return context.CreateQuery<T>(eSql);
    }
  }
}

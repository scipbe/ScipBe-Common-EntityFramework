// ==============================================================================================
// Namespace   : ScipBe.Common.EntityFramework
// Class(es)   : Extension methods for ObjectQuery
// Version     : 1.0
// Author      : Stefan Cruysberghs
// Website     : http://www.scip.be
// Date        : June 2008 - December 2008
// Status      : Open source - MIT License
// ==============================================================================================

using System;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Linq;

namespace ScipBe.Common.EntityFramework
{
  public static partial class EntityFrameworkExtensionMethods
  {
    /// <summary>
    /// Get EntityType name for given EntitySet name
    /// </summary>
    /// <param name="entitySetName">Name of EntitySet (e.g. "EmployeeSet", "Categories")</param>
    /// <param name="context">ObjectContext of entity</param>
    /// <returns>Name of EntityType (e.g. "Employee", "Category")</returns>
    public static string GetEntityTypeName(string entitySetName, ObjectContext context)
    {
      if (String.IsNullOrEmpty(entitySetName))
      {
        throw new ArgumentNullException("entitySetName", "EntitySet name is required");
      }

      if (context == null)
      {
        throw new ArgumentNullException("context", "ObjectContext is required");
      }

      var container = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);
      string entityTypeName = (from meta in container.BaseEntitySets
                              where meta.Name == entitySetName
                              select meta.ElementType.Name).FirstOrDefault();

      return entityTypeName ?? "";
    }

    /// <summary>
    /// Extension method for ObjectQuery which gets the EntitySet name
    /// </summary>
    /// <param name="objectQuery">ObjectQuery (e.g. context.EmployeeSet, context.Categories)</param>
    /// <param name="context">ObjectContext of entity</param>
    /// <returns>Name of EntitySet (e.g. "EmployeeSet", "Categories)</returns>
    /// <example><code>
    /// var employees = context.EmployeeSet;
    /// Console.WriteLine(employees.GetEntitySetName(context));
    /// </code></example>
    public static string GetEntitySetName(this ObjectQuery objectQuery, ObjectContext context)
    {
      if (objectQuery == null)
      {
        throw new ArgumentNullException("objectQuery", "ObjectQuery name is required");
      }

      if (context == null)
      {
        throw new ArgumentNullException("context", "ObjectContext is required");
      }

      string entityTypeName = GetEntityTypeName(objectQuery);

      return GetEntitySetName(entityTypeName, context);
    }

    /// <summary>
    /// Extension method for ObjectQuery which gets the EntityType name
    /// </summary>
    /// <param name="objectQuery">ObjectQuery (e.g. context.EmployeeSet, context.Categories)</param>
    /// <returns>Name of EntitySet (e.g. "Employee", "Category")</returns>
    /// <example><code>
    /// var employees = context.EmployeeSet;
    /// Console.WriteLine(employees.GetEntityTypeName());
    /// </code></example>
    public static string GetEntityTypeName(this ObjectQuery objectQuery)
    {
      if (objectQuery == null)
      {
        throw new ArgumentNullException("objectQuery", "ObjectQuery name is required");
      }

      return objectQuery.GetResultType().EdmType.Name;
    }
  }
}

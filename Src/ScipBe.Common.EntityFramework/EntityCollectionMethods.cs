// ==============================================================================================
// Namespace   : ScipBe.Common.EntityFramework
// Class(es)   : Extension methods for EntityCollection<TEntity>
// Version     : 1.0
// Author      : Stefan Cruysberghs
// Website     : http://www.scip.be
// Date        : June 2008 - December 2008
// Status      : Open source - MIT License
// ==============================================================================================

using System;
using System.Data.Objects;
using System.Data.Objects.DataClasses;

namespace ScipBe.Common.EntityFramework
{
  public static partial class EntityFrameworkExtensionMethods
  {
    /// <summary>
    /// Extension method for EntityCollection which gets the EntitySet name
    /// </summary>
    /// <param name="entityCollection">EntityCollection (e.g. employee.Orders, order.OrderDetails)</param>
    /// <param name="context">ObjectContext of entity</param>
    /// <returns>Name of EntitySet (e.g. "Orders", "OrderDetails)</returns>
    /// <example><code>
    /// var emp = context.EmployeeSet.First();
    /// Console.WriteLine(emp.Orders.GetEntitySetName(context));
    /// </code></example>
    public static string GetEntitySetName<TEntity>(this EntityCollection<TEntity> entityCollection, ObjectContext context)
      where TEntity : class, IEntityWithRelationships
    {
      if (entityCollection == null)
      {
        throw new ArgumentNullException("entityCollection", "EntityCollection<T> name is required");
      }

      if (context == null)
      {
        throw new ArgumentNullException("context", "ObjectContext is required");
      }

      string entityTypeName = GetEntityTypeName(entityCollection);

      return GetEntitySetName(entityTypeName, context);
    }

    /// <summary>
    /// Extension method for EntityCollection which gets the EntityType name
    /// </summary>
    /// <param name="entityCollection">EntityCollection (e.g. employee.Orders, order.OrderDetails)</param>
    /// <returns>Name of EntitySet (e.g. "Order", "OrderDetail)</returns>
    /// <example><code>
    /// var emp = context.EmployeeSet.First();
    /// Console.WriteLine(emp.Orders.GetEntityTypeName(context));
    /// </code></example>
    public static string GetEntityTypeName<TEntity>(this EntityCollection<TEntity> entityCollection)
      where TEntity : class, IEntityWithRelationships
    {
      if (entityCollection == null)
      {
        throw new ArgumentNullException("entityCollection", "EntityCollection<T> name is required");
      }

      return typeof(TEntity).Name;
    }
  }
}

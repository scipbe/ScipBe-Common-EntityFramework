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
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ScipBe.Common.EntityFramework
{
  /// <summary>
  /// Extension methods on ObjectQuery
  /// </summary>
  public static partial class EntityFrameworkExtensionMethods
  {
    private static void CollectRelationalMembers(Expression exp, ICollection<PropertyInfo> members)
    {
      if (exp.NodeType == ExpressionType.Lambda)
      {
        // At root, explore body
        CollectRelationalMembers(((LambdaExpression)exp).Body, members);
      }
      else if (exp.NodeType == ExpressionType.MemberAccess)
      {
        MemberExpression mexp = (MemberExpression)exp;
        CollectRelationalMembers(mexp.Expression, members);
        members.Add((PropertyInfo)mexp.Member);
      }
      else if (exp.NodeType == ExpressionType.Call)
      {
        MethodCallExpression cexp = (MethodCallExpression)exp;

        if (cexp.Method.IsStatic == false)
          throw new InvalidOperationException("Invalid type of expression.");

        foreach (var arg in cexp.Arguments)
          CollectRelationalMembers(arg, members);
      }
      else if (exp.NodeType == ExpressionType.Parameter)
      {
        // Reached the toplevel
        return;
      }
      else
      {
        throw new InvalidOperationException("Invalid type of expression.");
      }
    }

    /// <summary>
    /// Specifies the related objects to include in the query results using
    /// a lambda expression mentioning the path members.
    /// </summary>
    /// <remarks>
    /// Author: Rudi Breedenraedt
    /// Documentation: http://www.codetuning.net/blog/post/Entity-Framework-compile-safe-Includes.aspx
    /// </remarks>
    /// <returns>A new System.Data.Objects.ObjectQuery&gt;T&lt; with the defined query path.</returns>
    /// <example><code>
    /// var query = from p in context.Products  
    ///             .Include(p =&gt; p.PriceHistory)  
    ///             .Include(p =&gt; p.Supplier.Address) 
    ///             select p; 
    /// 
    /// var query = from p in context.Products 
    ///             .Include(p =&gt; p.PriceHistory)  
    ///             .Include(p =&gt; p.Suppliers.First().Address)  
    ///             select p; 
    /// </code></example>
    public static ObjectQuery<T> Include<T>(this ObjectQuery<T> objectQuery, Expression<Func<T, object>> path)
    {
      if (objectQuery == null)
      {
        throw new ArgumentNullException("objectQuery", "ObjectQuery is required");
      }

      if (path == null)
      {
        throw new ArgumentNullException("path", "Expression for path is required");
      }
      
      // Retrieve member path
      List<PropertyInfo> members = new List<PropertyInfo>();
      CollectRelationalMembers(path, members);

      // Build string path
      StringBuilder sb = new StringBuilder();
      string separator = "";
      foreach (PropertyInfo member in members)
      {
        sb.Append(separator);
        sb.Append(member.Name);
        separator = ".";
      }

      // Apply Include
      return objectQuery.Include(sb.ToString());
    }
  }
}

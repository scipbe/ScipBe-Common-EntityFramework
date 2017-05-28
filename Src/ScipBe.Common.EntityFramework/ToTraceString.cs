// ==============================================================================================
// Namespace   : ScipBe.Common.EntityFramework
// Class(es)   : ToTraceString extension method
// Version     : 1.0
// Author      : Stefan Cruysberghs
// Website     : http://www.scip.be
// Date        : June 2008 - December 2008
// Status      : Open source - MIT License
// ==============================================================================================

using System.Linq;

namespace ScipBe.Common.EntityFramework
{
  public static partial class EntityFrameworkExtensionMethods
  {
    /// <summary>
    /// Get trace string (=SQL statement) for given Entity SQL or LINQ to Entities query.
    /// </summary>
    /// <param name="query">Entity SQL or LINQ to Entities query</param>
    /// <returns>SQL statement for Entity SQL or LINQ to Entities query</returns>
    public static string ToTraceString(this IQueryable query)
    {
      System.Reflection.MethodInfo toTraceStringMethod = query.GetType().GetMethod("ToTraceString");

      if (toTraceStringMethod != null)
        return toTraceStringMethod.Invoke(query, null).ToString();
      return "";
    }
  }
}

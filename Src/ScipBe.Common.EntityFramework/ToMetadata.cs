// ==============================================================================================
// Namespace   : ScipBe.Common.EntityFramework
// Class(es)   : ToMetadata extension methods
// Version     : 1.0
// Author      : Stefan Cruysberghs
// Website     : http://www.scip.be
// Date        : June 2008 - December 2008
// Status      : Open source - MIT License
// ==============================================================================================
 
using System;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Diagnostics;
using System.Linq;
using System.Data.Objects.DataClasses;

namespace ScipBe.Common.EntityFramework
{
  /// <summary>
  /// Extension methods for ADO.NET Entity Framework classes
  /// </summary>
  public static partial class EntityFrameworkExtensionMethods
  {
    /// <summary>
    /// Convert System.Data.Metadata.Edm.BuiltInTypeKind to ScipBe.Common.EntityFramework.MetaBuiltInTypeKind
    /// which is a subset of the Edm.BuiltInTypeKind
    /// </summary>
    /// <param name="builtInTypeKind">BuiltInTypeKind</param>
    /// <returns>MetaBuiltInTypeKind</returns>
    private static MetaBuiltInTypeKind ConvertBuiltInTypeKind(BuiltInTypeKind builtInTypeKind)
    {
      switch (builtInTypeKind)
      {
        case BuiltInTypeKind.ComplexType:
          return MetaBuiltInTypeKind.ComplexType;
        case BuiltInTypeKind.EntitySet:
          return MetaBuiltInTypeKind.EntitySet;
        case BuiltInTypeKind.EntityType:
          return MetaBuiltInTypeKind.EntityType;
        case BuiltInTypeKind.PrimitiveType:
          return MetaBuiltInTypeKind.PrimitiveType;
        case BuiltInTypeKind.RowType:
          return MetaBuiltInTypeKind.RowType;
        case BuiltInTypeKind.CollectionType:
          return MetaBuiltInTypeKind.CollectionType;
        default:
          return MetaBuiltInTypeKind.Unknown;
      }
    }
    
    /// <summary>
    /// Query conceptual metadata about Entity Data Model and return a collection
    /// of metadata about the properties
    /// </summary>
    /// <param name="members">Edm members</param>
    /// <param name="keyMembers">Edm key members</param>
    /// <returns>Collection of metadata about the properties</returns>
    private static List<MetaProperty> ProcessProperties(IEnumerable<EdmMember> members, IEnumerable<EdmMember> keyMembers)
    {
      // Create small list with all key member names
      string[] metaKeyMembersNames = null;
      if (keyMembers != null)
      {
        metaKeyMembersNames = keyMembers.Select(m => m.Name).ToArray();
      }
      var metaProperties =
        from meta in members
        let prop = (meta as EdmProperty)
        let type = meta is EdmProperty ? meta.TypeUsage.EdmType : null
        where meta is EdmProperty
        select new MetaProperty()
        {
          Name = prop.Name,
          Nullable = prop.Nullable,
          // Check if member is also listed in arry with key member names
          IsKeyMember = ((metaKeyMembersNames != null) && metaKeyMembersNames.Where(keyName => keyName == prop.Name).Any()),
          BuiltInTypeKind = ConvertBuiltInTypeKind(type.BuiltInTypeKind),
          // Only filled for EntitySets
          Documentation = type.Documentation == null ? "" : type.Documentation.LongDescription,
          // Fill in type for primitive and entity types
          Type = ((type is PrimitiveType) || (type is EntityType)) ? 
            new MetaType()
            {
              Name = type.Name,
              NameSpace = type.NamespaceName,
              ClrEquivalentType = (type is PrimitiveType ? ((PrimitiveType)type).ClrEquivalentType : null)
            } : 
            null,
          // Recursive call to ProcessProperties to list properties of entity, row or collection
          Properties = 
            type is EntityType ?
              ProcessProperties(((EntityType)type).Members, ((EntityType)type).KeyMembers) :
            type is RowType ?
              ProcessProperties(((RowType)type).Members, null) :
            type is ComplexType ?
              ProcessProperties(((ComplexType)type).Members, null) :
            ((type is CollectionType) && (((CollectionType)type).TypeUsage.EdmType is EntityType)) ?
              ProcessProperties((((CollectionType) type).TypeUsage.EdmType as EntityType).Members,
              (((CollectionType)type).TypeUsage.EdmType as EntityType).KeyMembers) :
            ((type is CollectionType) && (((CollectionType)type).TypeUsage.EdmType is RowType)) ?
              ProcessProperties(((RowType)((CollectionType)type).TypeUsage.EdmType).Members, null) :
            ((type is CollectionType) && (((CollectionType)type).TypeUsage.EdmType is ComplexType)) ?
              ProcessProperties(((ComplexType)((CollectionType)type).TypeUsage.EdmType).Members, null) :
            null
        };

      return metaProperties.ToList();
    }

    /// <summary>
    /// Query conceptual metadata about Entity Data Model and return a collection
    /// of metadata about the navigation properties 
    /// </summary>
    /// <param name="members">Edm members</param>
    /// <returns>Collection of metadata about the navigation properties</returns>
    private static List<MetaNavigationProperty> ProcessNavigationProperties(IEnumerable<EdmMember> members)
    {
      var metaNavigationProperties =
        from meta in members
        let prop = (meta as NavigationProperty)
        where meta is NavigationProperty
        select new MetaNavigationProperty()
        {
       	  Name = prop.Name,
	        RelationshipTypeName = prop.RelationshipType.Name,
	        ToEndMemberName = prop.ToEndMember.Name,
	        FromEndMemberName = prop.FromEndMember.Name
        };

      return metaNavigationProperties.ToList(); 
    }

    /// <summary>
    /// Extension method ToMetadata() for an ADO.NET Entity Framework ObjectQuery object.
    /// Metadata describes how the entities and relations in the Entity Framework are named, typed and structured.
    /// This extension method will convert the complex and detailed metadata about the Entity Framework 
    /// into a simple structure with info about the properties and navigation properties.
    /// </summary>
    /// <param name="query">Entity SQL or LINQ to Entities query</param>
    /// <returns>IMetadata object with all metadata about the ObjectQuery</returns>
    public static Metadata ToMetadata(this ObjectQuery query)
    {
      if (query == null)
      {
        throw new ArgumentNullException("query", "Entity SQL or LINQ to Entities query is required");
      }
      
      IEnumerable<EdmMember> metaEdmMembers = null;
      try
      {
        metaEdmMembers =
          query.GetResultType().EdmType.MetadataProperties.First(p => p.Name == "Members").Value
          as IEnumerable<EdmMember>;
      }
      catch (Exception ex)
      {
        Debug.Write(ex.Message);
      }

      IEnumerable<EdmMember> metaEdmKeyMembers = null;
      try
      {
        metaEdmKeyMembers =
          query.GetResultType().EdmType.MetadataProperties.First(p => p.Name == "KeyMembers").Value
          as IEnumerable<EdmMember>;
      }
      catch (Exception ex)
      {
        Debug.Write(ex.Message);
      }

      var metaProperties = ProcessProperties(metaEdmMembers, metaEdmKeyMembers);

      var metaNavigationProperties = ProcessNavigationProperties(metaEdmMembers);

      return new Metadata()
               {
                 BuiltInTypeKind = ConvertBuiltInTypeKind(query.GetResultType().EdmType.BuiltInTypeKind),
                 Properties = metaProperties, 
                 NavigationProperties = metaNavigationProperties
               };
    }

    

    /// <summary>
    /// Extension method ToMetadata() for an ADO.NET Entity Framework EntityObject object.
    /// Metadata describes how the entities and relations in the Entity Framework are named, typed and structured.
    /// This extension method will convert the complex and detailed metadata about the Entity Framework 
    /// into a simple structure with info about the properties and navigation properties.
    /// </summary>
    /// <param name="entity">EntityObject</param>
    /// <param name="context">ObjectContext</param>
    /// <returns>IMetadata object with all metadata about the EntityObject</returns>
    public static Metadata ToMetadata(this EntityObject entity, ObjectContext context)
    {
      if (entity == null)
      {
        throw new ArgumentNullException("entity", "EntityObject is required");
      }

      if (context == null)
      {
        throw new ArgumentNullException("context", "ObjectContext is required");
      }
      
      EntityType entityMeta = (from meta in context.MetadataWorkspace.GetItems(DataSpace.CSpace)
                       where meta.BuiltInTypeKind == BuiltInTypeKind.EntityType
                       && ((EntityType)meta).Name == entity.GetType().Name
                       select meta).FirstOrDefault() as EntityType;

      if (entityMeta == null)
      {
        Debug.Write("No metadata found for entity");
        return null;
      }

      var metaProperties = ProcessProperties(entityMeta.Members, entityMeta.KeyMembers);

      var metaNavigationProperties = ProcessNavigationProperties(entityMeta.Members);

      return new Metadata()
      {
        BuiltInTypeKind = ConvertBuiltInTypeKind(BuiltInTypeKind.EntityType),
        Properties = metaProperties,
        NavigationProperties = metaNavigationProperties
      };
    }
  }
}

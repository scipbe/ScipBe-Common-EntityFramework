// ==============================================================================================
// Namespace   : ScipBe.Common.EntityFramework
// Author      : Stefan Cruysberghs
// Website     : http://www.scip.be
// Status      : Open source - MIT License
// ==============================================================================================
  
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ScipBe.Common.EntityFramework
{
  /// <summary>
  /// Enumeration for BuiltInTypeKind.
  /// Subset of System.Data.Metadata.Edm.BuiltInTypeKind
  /// </summary>
  public enum MetaBuiltInTypeKind
  {
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown,
    /// <summary>
    /// A ComplexType consists of one or more properties. 
    /// However unlike EntityType, ComplexType is not associated with an EntityKey. 
    /// In most cases it will be a part of an entity.
    /// </summary>
    ComplexType,
    /// <summary>
    /// An EntitySet is a collection which holds instances of EntityTypes.
    /// </summary>
    EntitySet,
    /// <summary>
    /// An EntityType defines the entities of the EDM which are mapped to a table in the database. 
    /// An EntityType has a EntityKey which makes each instance unique. 
    /// An entity is an instance of a EntityType.
    /// </summary>
    EntityType,
    /// <summary>
    /// A RowType is an anonymous type. 
    /// In most cases RowTypes are returned by LINQ to Entities or Entity SQL queries.
    /// </summary>
    RowType,
    /// <summary>
    /// PrimitiveTypes are types such as String, Bool, SByte, Int16, Int32, Byte, Float, Decimal, Xml, Guid, ... 
    /// Primitive types are properties in an EntityType, ComplexType or RowType.
    /// </summary>
    PrimitiveType,
    /// <summary>
    /// A CollectionType represents a collection of instances of a specific type like EntityType or RowType.
    /// </summary>
    CollectionType
  }

  /// <summary>
  /// Metadata about Type 
  /// </summary>
  [DataContract]
  public class MetaType
  {
    /// <summary>
    /// Name of type
    /// </summary>
    [DataMember]
    public string Name { get; internal set; }
    /// <summary>
    /// Namespace of type
    /// </summary>
    [DataMember]
    public string NameSpace { get; internal set; }
    /// <summary>
    /// CLR equivalent type (string, int, bool, ...)
    /// </summary>
    public Type ClrEquivalentType { get; internal set; }
  }

  /// <summary>
  /// Metadata about Property
  /// All important meta data about the definition of a property.
  /// </summary>
  [DataContract]
  public class MetaProperty
  {
    /// <summary>
    /// Name of the property
    /// </summary>    
    [DataMember]
    public string Name { get; internal set; }
    /// <summary>
    /// Is the property Nullable?
    /// </summary>
    [DataMember]
    public bool Nullable { get; internal set; }
    /// <summary>
    /// Is the property a key member?
    /// </summary>
    [DataMember]
    public bool IsKeyMember { get; internal set; }
    /// <summary>
    /// BuiltInTypeKind of property (EntitySet, EntityType, ComplexType, RowType, PrimitiveType, ...)
    /// </summary>
    [DataMember]
    public MetaBuiltInTypeKind BuiltInTypeKind { get; internal set; }
    /// <summary>
    /// XML documentation of property. This is only filled for properties of entity types.
    /// </summary>
    [DataMember]
    public string Documentation { get; internal set; }
    /// <summary>
    /// Type of property. This is only filled when the BuiltInTypeKind is a PrimitiveType.
    /// </summary>
    [DataMember]
    public MetaType Type { get; internal set; }
    /// <summary>
    /// Child properties. This is used in case when BuiltInTypeKind is EntityType, RowType or CollectionType.
    /// </summary>
    [DataMember]
    public List<MetaProperty> Properties { get; internal set; }
  }

  /// <summary>
  /// Metadata about Navigation Property (Relations)
  /// </summary>
  [DataContract]
  public class MetaNavigationProperty
  {
    /// <summary>
    /// Name of navigation property
    /// </summary>
    [DataMember]
    public string Name { get; internal set; }
    /// <summary>
    /// Type of relationship (1:n, 0:n, n:n, ...)
    /// </summary>
    [DataMember]
    public string RelationshipTypeName { get; internal set; }
    /// <summary>
    /// From member name
    /// </summary>
    [DataMember]
    public string FromEndMemberName { get; internal set; }
    /// <summary>
    /// End member name
    /// </summary>
    [DataMember]
    public string ToEndMemberName { get; internal set; }
  }

  /// <summary>
  /// Metadata about Entity Framework EntitySet, ObjectQuery, ...
  /// </summary>
  [DataContract]
  public class Metadata
  {
    /// <summary>
    /// BuiltInTypeKind (EntitySet, EntityType, RowType, ...)
    /// </summary>
    [DataMember]
    public MetaBuiltInTypeKind BuiltInTypeKind { get; internal set; }
    /// <summary>
    /// Collection of metadata about properties
    /// </summary>
    [DataMember]
    public List<MetaProperty> Properties { get; internal set; }
    /// <summary>
    /// Collection of metadata about navigation properties
    /// </summary>
    [DataMember]
    public List<MetaNavigationProperty> NavigationProperties { get; internal set; }
  }
}

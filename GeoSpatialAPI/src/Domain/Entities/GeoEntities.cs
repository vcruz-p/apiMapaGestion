using NetTopologySuite.Geometries;

namespace Domain.Entities;

public class Marker : BaseEntity
{
    private Point? _geometry;
    
    public Point? Geometry 
    { 
        get => _geometry; 
        set => _geometry = value; 
    }
}

public class AreaMapa : BaseEntity
{
    private NetTopologySuite.Geometries.Polygon? _geometry;
    
    public NetTopologySuite.Geometries.Polygon? Geometry 
    { 
        get => _geometry; 
        set => _geometry = value; 
    }
}

public class Route : BaseEntity
{
    private LineString? _geometry;
    
    public LineString? Geometry 
    { 
        get => _geometry; 
        set => _geometry = value; 
    }
}

public class Target : BaseEntity
{
    private Point? _geometry;
    
    public Point? Geometry 
    { 
        get => _geometry; 
        set => _geometry = value; 
    }
    
    // Dynamic JSON metadata
    public string? Metadata { get; set; }
    
    // Reference to parent entity (Marker, AreaMapa, or Route)
    public Guid? ParentId { get; set; }
    public string? ParentType { get; set; } // "Marker", "AreaMapa", "Route"
}

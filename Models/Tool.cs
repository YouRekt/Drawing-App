﻿namespace DrawingAppCG.Models
{
    public enum Tool
    {
        None,
        Line,
        Circle,
        Rectangle,
        Polygon,
        Pill,
        Move,
        Clip,
        Bucket,
    }
    public enum ClippingStage
    {
        None,
        SelectedSubjectPolygon,
        DefinedClippingPolygon
    }
    public enum FillMode
    {
        None,
        Color,
        Image
    }
}


public class Cell
{
    public bool TopWall { get; set; } = true;
    public bool BottomWall { get; set; } = true;
    public bool LeftWall { get; set; } = true;
    public bool RightWall { get; set; } = true;
    public bool FrontWall { get; set; } = true;
    public bool BackWall { get; set; } = true;
    public bool Visited { get; set; } = false;
}

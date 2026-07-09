using UnityEngine;

public interface IDragEventPass
{
    public bool enableDragEventPass { get; set; }
    public GameObject dragEventPassTarget { get; set; }
}
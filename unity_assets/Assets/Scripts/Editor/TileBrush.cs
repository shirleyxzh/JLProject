using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

[CreateAssetMenu(fileName = "tile Brush", menuName = "Brushes/Tile brush")]
[CustomGridBrush(false, true, false, "tile Brush")]
public class TileBrush : GameObjectBrush
{
}

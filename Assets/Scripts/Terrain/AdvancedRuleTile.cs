using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/Advanced Rule Tile")]
public class AdvancedRuleTile : RuleTile<AdvancedRuleTile.Neighbor>
{
	public bool alwaysConnect;
	public bool specifiedExcluding;
	public TileBase[] tilesToConnect;

	public class Neighbor : RuleTile.TilingRule.Neighbor
	{
		public const int Any = 3;
		public const int Specified = 4;
		public const int Nothing = 5;
	}

	public override bool RuleMatch(int neighbor, TileBase tile)
	{
		switch (neighbor)
		{
			case Neighbor.This: return Check_This(tile);
			case Neighbor.NotThis: return Check_NotThis(tile);
			case Neighbor.Any: return Check_Any(tile);
			case Neighbor.Specified: return Check_Specified(tile);
			case Neighbor.Nothing: return Check_Nothing(tile);
		}
		return base.RuleMatch(neighbor, tile);
	}

	bool Check_This(TileBase tile)
	{
		if (!alwaysConnect) return tile == this;
		else return tilesToConnect.Contains(tile) || tile == this;
	}
	bool Check_NotThis(TileBase tile)
	{
		if (!alwaysConnect) return tile != this;
		else return !tilesToConnect.Contains(tile) && tile != this;
	}
	bool Check_Any(TileBase tile)
	{
		return tile != null;
	}
	bool Check_Specified(TileBase tile)
	{
		if (specifiedExcluding) return tilesToConnect.Contains(tile);
		else return tilesToConnect.Contains(tile) || tile == this;
	}
	bool Check_Nothing(TileBase tile)
	{
		return tile == null;
	}
}
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _193396
{
	using BoundedTile = GridPreprocessorData.BoundedTile;
	using TilePrefabMapping = GridPreprocessorData.TilePrefabMapping;

	public abstract class GroundController : MonoBehaviour
	{
		public abstract Tilemap[] tilemaps { get; }
	}
	class ObjectInvalidation
	{
		public GameObject objectToDestroy;
		public Func<bool> condition;

		public ObjectInvalidation(GameObject _objectToDestroy, Func<bool> _condition)
		{
			objectToDestroy = _objectToDestroy;
			condition = _condition;
		}
	}

	[RequireComponent(typeof(Grid))]
	public class GridPreprocessor : MonoBehaviour
	{
		public GridPreprocessorData data;

		private Tilemap[] tilemaps;

		private Transform autogenGroup;
		private Tilemap slopesTilemap;
		private Tilemap bounceOnBreakTilemap;
		private Transform groundDroppingGroup;
		private Transform groundBreakingGroup;
		private Transform customRegionsGroup;
		private Transform enemiesGroup;
		private Transform collectiblesGroup;

		public List<TilemapHelper.Region> groundDroppingRegions;
		public List<TilemapHelper.Region> groundBreakingRegions;
		public List<TilemapHelper.Region> customRegions;


		private void Awake()
		{
			regenerate();
		}

		private void regenerate()
		{
			QolUtility.createIfNotExist(out autogenGroup, transform, "Autogen");

			autogenGroup.gameObject.SetActive(false);
			tilemaps = transform.GetComponentsInChildren<Tilemap>();
			autogenGroup.gameObject.SetActive(true);

			initializeRuntimeTilemaps();

			foreach (Tilemap tilemap in tilemaps)
			{
				tilemap.CompressBounds();

				foreach (Vector3Int coord in tilemap.cellBounds.allPositionsWithin)
				{
					TileBase tileBase = tilemap.GetTile(coord);
					if (tileBase == null)
						continue;

					Tile tile = tileBase as Tile;

					tryAddTiles(tile, coord, slopesTilemap, data.slopes.tiles);
					tryAddTiles(tile, coord, bounceOnBreakTilemap, data.bounceOnBreak.tiles);

					tryAddEntity(tile, tilemap, coord, enemiesGroup, data.enemies.mapping);
					tryAddEntity(tile, tilemap, coord, collectiblesGroup, data.collectibles.mapping);
				}
			}

			createRegions(ref groundDroppingRegions, groundDroppingGroup, 
				transform.GetComponentsInChildren<GroundDroppingController>(), data.terrainData.groundDropping.prefab);
			createRegions(ref groundBreakingRegions, groundBreakingGroup, 
				transform.GetComponentsInChildren<GroundBreakingController>(), data.terrainData.groundBreaking.prefab);
			createRegions(ref customRegions, customRegionsGroup,
				transform.GetComponentsInChildren<CustomRegionSource>(), null);
		}
		private void clear()
		{
			if (autogenGroup != null)
			{
				QolUtility.DestroyExecutableInEditMode(autogenGroup.gameObject);
				autogenGroup = null;
				groundDroppingRegions = null;
				groundBreakingRegions = null;
			}
		}

		private bool createTilemap(out Tilemap target, Transform parent, string name, Color color)
		{
			Transform tilemapTransform;
			if (!QolUtility.createIfNotExist(out tilemapTransform, parent, name))
			{
				target = tilemapTransform.GetComponent<Tilemap>();
				return false;
			}

			tilemapTransform.gameObject.SetActive(false);

			target = tilemapTransform.AddComponent<Tilemap>();
			target.color = color;

			TilemapRenderer tilemapRenderer = tilemapTransform.AddComponent<TilemapRenderer>();
			tilemapRenderer.sortingLayerName = data.sortingLayer;
			tilemapRenderer.sortingOrder = data.sortingOrder;

			TilemapCollider2D tilemapCollider = tilemapTransform.AddComponent<TilemapCollider2D>();
			tilemapCollider.isTrigger = true;

			return true;
		}
		private void createSlopeTilemap()
		{
			if (createTilemap(out slopesTilemap, autogenGroup, "Slope", data.slopes.color))
			{
				QolUtility.setTag(slopesTilemap.gameObject, RuntimeSettings.Tag.Slope);

				slopesTilemap.gameObject.SetActive(true);
			}
			else
				slopesTilemap.ClearAllTiles();
		}
		private void createBounceOnBreakTilemap()
		{
			if (createTilemap(out bounceOnBreakTilemap, autogenGroup, "Bounce On Break", data.bounceOnBreak.color))
			{
				QolUtility.setTag(bounceOnBreakTilemap.gameObject, RuntimeSettings.Tag.BreakableExplode);

				GroundBreakingController controller = bounceOnBreakTilemap.AddComponent<GroundBreakingController>();
				controller.data = data.terrainData;
				controller.breakableTilemaps = data.bounceOnBreak.breakableTilemaps;

				bounceOnBreakTilemap.gameObject.SetActive(true);
			}
			else
				bounceOnBreakTilemap.ClearAllTiles();
		}
		private void initializeRuntimeTilemaps()
		{
			createSlopeTilemap();
			createBounceOnBreakTilemap();

			QolUtility.createIfNotExist(out groundDroppingGroup, autogenGroup, "Ground Dropping");
			QolUtility.createIfNotExist(out groundBreakingGroup, autogenGroup, "Ground Breaking");
			QolUtility.createIfNotExist(out customRegionsGroup, autogenGroup, "Custom Regions");
			enemiesGroup = GameManager.instance.runtimeGroup[GameManager.RuntimeGroup.Enemies];
			collectiblesGroup = GameManager.instance.runtimeGroup[GameManager.RuntimeGroup.Collectibles];
		}

		private void tryAddTiles(Tile tile, Vector3Int coord, Tilemap parent, BoundedTile[] tiles)
		{
			foreach (var tileData in tiles)
				if (tile == tileData.tile)
				{
					foreach (Vector3Int offset in tileData.offsetBounds.allPositionsWithin)
						parent.SetTile(coord + offset, data.areaTile);
				}
		}
		private void tryAddEntity(Tile tile, Tilemap tilemap, Vector3Int coord, Transform parent, TilePrefabMapping[] mappings)
		{
			foreach (var mapping in mappings)
				if (tile == mapping.tile)
				{
					Transform newEntity;
					if (!QolUtility.createIfNotExist(out newEntity, parent,
						RuntimeDataManager.getUniqueName(mapping.prefab, tilemap, coord), mapping.prefab))
						continue;

					SpriteRenderer renderer = mapping.prefab.transform.Find("Sprite").GetComponent<SpriteRenderer>();

					Matrix4x4 tileTransform = tilemap.GetTransformMatrix(coord);
					Quaternion tileRotation = Quaternion.LookRotation(tileTransform.GetColumn(2), tileTransform.GetColumn(1));

					var sprite = tile.sprite;
					var rendererSprite = renderer.sprite;

					Vector2 pivotOffset = (sprite.pivot - rendererSprite.pivot) / sprite.pixelsPerUnit;
					Vector3 offset = renderer.transform.position;
					Quaternion rotation = tileRotation;
					if (renderer.flipX)
						rotation *= Quaternion.Euler(0, 180, 0);
					if (rotation.y > 0.5f)
						offset.x = -offset.x;

					Vector3 position = tilemap.CellToWorld(coord) + new Vector3(0.5f, 0.5f);
					position -= (Vector3)pivotOffset;
					position -= offset;
					position += (Vector3)tileTransform.GetColumn(3);

					newEntity.position = position;
					newEntity.rotation = rotation;

					tilemap.SetTile(coord, null);
				}
		}
		private void createRegions(ref List<TilemapHelper.Region> regions, Transform parent, 
			GroundController[] groundControllers, GameObject prefab)
		{
			var newRegions = new List<TilemapHelper.Region>();
			List<Vector3Int> ignore = new List<Vector3Int>();

			foreach (var groundController in groundControllers)
			{
				Tilemap triggerTilemap = groundController.GetComponent<Tilemap>();
				Tilemap[] tilemaps = groundController.tilemaps;

				foreach (Vector3Int coord in triggerTilemap.cellBounds.allPositionsWithin)
				{
					if (ignore.Contains(coord))
						continue;

					TileBase tileBase = triggerTilemap.GetTile(coord);
					if (tileBase == null)
						continue;

					List<Vector3Int> triggeredCoords = TilemapHelper.getTriggeredTiles(
						triggerTilemap, new List<Vector3Int> { coord });

					TilemapHelper.Region region = null;
					Transform regionTransform;
					if (!QolUtility.createIfNotExist(out regionTransform, parent,
						RuntimeDataManager.getUniqueName(triggerTilemap.gameObject, TilemapHelper.hash(triggeredCoords)), prefab))
						if (region != null)
							region = regions.Find(r => r.gameObject == regionTransform.gameObject);

					if (region == null)
					{
						List<TilemapHelper.TileData> tiles = TilemapHelper.getAllTiles(tilemaps, triggeredCoords);

						region = new TilemapHelper.Region(regionTransform.gameObject, tiles, triggeredCoords);
						regionTransform.gameObject.SetActive(false);
					}

					regionTransform.tag = groundController.tag;

					ignore.AddRange(region.coords);
					newRegions.Add(region);
				}
			}

			regions = newRegions;
		}
	}
}
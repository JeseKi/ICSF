using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace icsf.Systems;

public class HitboxOverlaySystem : ModSystem
{
	public static readonly Color DefaultHostileNpcHitboxColor = Color.IndianRed;
	public static readonly Color DefaultHostileProjectileHitboxColor = Color.OrangeRed;
	public static readonly Color DefaultPlayerHitboxColor = Color.LimeGreen;
	public const int DefaultHostileNpcLineThicknessInScreenPixels = 2;
	public const int DefaultHostileProjectileLineThicknessInScreenPixels = 2;
	public const int DefaultPlayerLineThicknessInScreenPixels = 2;
	public const bool DefaultHostileNpcFillRectangles = false;
	public const bool DefaultHostileProjectileFillRectangles = false;
	public const bool DefaultPlayerFillRectangles = false;

	public static bool ShowHostileNpcHitboxes { get; set; } = true;
	public static bool ShowHostileProjectileHitboxes { get; set; } = true;
	public static bool ShowPlayerHitbox { get; set; } = true;
	public static Color HostileNpcHitboxColor { get; set; } = DefaultHostileNpcHitboxColor;
	public static Color HostileProjectileHitboxColor { get; set; } = DefaultHostileProjectileHitboxColor;
	public static Color PlayerHitboxColor { get; set; } = DefaultPlayerHitboxColor;
	public static int HostileNpcLineThicknessInScreenPixels { get; set; } = DefaultHostileNpcLineThicknessInScreenPixels;
	public static int HostileProjectileLineThicknessInScreenPixels { get; set; } = DefaultHostileProjectileLineThicknessInScreenPixels;
	public static int PlayerLineThicknessInScreenPixels { get; set; } = DefaultPlayerLineThicknessInScreenPixels;
	public static bool HostileNpcFillRectangles { get; set; } = DefaultHostileNpcFillRectangles;
	public static bool HostileProjectileFillRectangles { get; set; } = DefaultHostileProjectileFillRectangles;
	public static bool PlayerFillRectangles { get; set; } = DefaultPlayerFillRectangles;

	public override void PostDrawInterface(SpriteBatch spriteBatch)
	{
		if (Main.gameMenu) {
			return;
		}

		if (ShowHostileNpcHitboxes) {
			DrawEnemyNpcHitboxes(spriteBatch);
		}

		if (ShowHostileProjectileHitboxes) {
			DrawHostileProjectileHitboxes(spriteBatch);
		}

		if (ShowPlayerHitbox) {
			DrawLocalPlayerHitbox(spriteBatch);
		}
	}

	public static void ResetVisualSettings()
	{
		HostileNpcHitboxColor = DefaultHostileNpcHitboxColor;
		HostileProjectileHitboxColor = DefaultHostileProjectileHitboxColor;
		PlayerHitboxColor = DefaultPlayerHitboxColor;
		HostileNpcLineThicknessInScreenPixels = DefaultHostileNpcLineThicknessInScreenPixels;
		HostileProjectileLineThicknessInScreenPixels = DefaultHostileProjectileLineThicknessInScreenPixels;
		PlayerLineThicknessInScreenPixels = DefaultPlayerLineThicknessInScreenPixels;
		HostileNpcFillRectangles = DefaultHostileNpcFillRectangles;
		HostileProjectileFillRectangles = DefaultHostileProjectileFillRectangles;
		PlayerFillRectangles = DefaultPlayerFillRectangles;
	}

	private static void DrawEnemyNpcHitboxes(SpriteBatch spriteBatch)
	{
		for (int i = 0; i < Main.maxNPCs; i++) {
			NPC npc = Main.npc[i];
			if (!IsHostileNpcTarget(npc)) {
				continue;
			}

			DrawWorldRectangle(
				spriteBatch,
				npc.Hitbox,
				HostileNpcHitboxColor,
				HostileNpcLineThicknessInScreenPixels,
				HostileNpcFillRectangles
			);
		}
	}

	private static void DrawHostileProjectileHitboxes(SpriteBatch spriteBatch)
	{
		for (int i = 0; i < Main.maxProjectiles; i++) {
			Projectile projectile = Main.projectile[i];
			if (!projectile.active || !projectile.hostile || projectile.damage <= 0) {
				continue;
			}

			DrawWorldRectangle(
				spriteBatch,
				projectile.Hitbox,
				HostileProjectileHitboxColor,
				HostileProjectileLineThicknessInScreenPixels,
				HostileProjectileFillRectangles
			);
		}
	}

	private static void DrawLocalPlayerHitbox(SpriteBatch spriteBatch)
	{
		Player player = Main.LocalPlayer;
		if (!player.active || player.dead) {
			return;
		}

		DrawWorldRectangle(
			spriteBatch,
			player.Hitbox,
			PlayerHitboxColor,
			PlayerLineThicknessInScreenPixels,
			PlayerFillRectangles
		);
	}

	private static bool IsHostileNpcTarget(NPC npc)
	{
		if (!npc.active || npc.friendly || npc.life <= 0) {
			return false;
		}

		if (npc.CanBeChasedBy()) {
			return true;
		}

		if (npc.boss || npc.rarity > 0) {
			return true;
		}

		if (npc.realLife >= 0 && npc.realLife < Main.npc.Length) {
			NPC root = Main.npc[npc.realLife];
			if (root.active && (root.boss || root.rarity > 0 || root.CanBeChasedBy())) {
				return true;
			}
		}

		if (npc.type == NPCID.EaterofWorldsHead
			|| npc.type == NPCID.EaterofWorldsBody
			|| npc.type == NPCID.EaterofWorldsTail) {
			return true;
		}

		if (!npc.friendly) {
			return true;
		}

		return false;
	}

	private static void DrawWorldRectangle(
		SpriteBatch spriteBatch,
		Rectangle worldRectangle,
		Color color,
		int lineThicknessInScreenPixels,
		bool fillRectangle
	)
	{
		Vector2 worldTopLeft = new(worldRectangle.Left - Main.screenPosition.X, worldRectangle.Top - Main.screenPosition.Y);
		Vector2 worldBottomRight = new(worldRectangle.Right - Main.screenPosition.X, worldRectangle.Bottom - Main.screenPosition.Y);

		Vector2 screenTopLeft = Vector2.Transform(worldTopLeft, Main.GameViewMatrix.TransformationMatrix);
		Vector2 screenBottomRight = Vector2.Transform(worldBottomRight, Main.GameViewMatrix.TransformationMatrix);

		// PostDrawInterface uses Main.UIScaleMatrix, so convert from screen pixels to UI coordinates.
		float inverseUiScale = 1f / Main.UIScale;
		screenTopLeft *= inverseUiScale;
		screenBottomRight *= inverseUiScale;

		int left = (int)System.MathF.Round(System.MathF.Min(screenTopLeft.X, screenBottomRight.X));
		int top = (int)System.MathF.Round(System.MathF.Min(screenTopLeft.Y, screenBottomRight.Y));
		int width = System.Math.Max(1, (int)System.MathF.Round(System.MathF.Abs(screenBottomRight.X - screenTopLeft.X)));
		int height = System.Math.Max(1, (int)System.MathF.Round(System.MathF.Abs(screenBottomRight.Y - screenTopLeft.Y)));
		Rectangle screenRectangle = new(left, top, width, height);

		DrawRectangle(spriteBatch, screenRectangle, color, lineThicknessInScreenPixels, fillRectangle);
	}

	private static void DrawRectangle(
		SpriteBatch spriteBatch,
		Rectangle rectangle,
		Color color,
		int lineThicknessInScreenPixels,
		bool fillRectangle
	)
	{
		Texture2D pixel = TextureAssets.MagicPixel.Value;
		int lineThickness = System.Math.Max(1, (int)System.MathF.Round(lineThicknessInScreenPixels / Main.UIScale));

		if (fillRectangle) {
			spriteBatch.Draw(pixel, rectangle, color * 0.24f);
		}

		spriteBatch.Draw(pixel, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, lineThickness), color);
		spriteBatch.Draw(pixel, new Rectangle(rectangle.Left, rectangle.Bottom - lineThickness, rectangle.Width, lineThickness), color);
		spriteBatch.Draw(pixel, new Rectangle(rectangle.Left, rectangle.Top, lineThickness, rectangle.Height), color);
		spriteBatch.Draw(pixel, new Rectangle(rectangle.Right - lineThickness, rectangle.Top, lineThickness, rectangle.Height), color);
	}
}

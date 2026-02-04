using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace icsf.Systems;

public class HitboxOverlaySystem : ModSystem
{
	public static readonly Color DefaultHostileNpcHitboxColor = Color.IndianRed;
	public static readonly Color DefaultHostileProjectileHitboxColor = Color.OrangeRed;
	public static readonly Color DefaultPlayerHitboxColor = Color.LimeGreen;
	public const int DefaultLineThicknessInScreenPixels = 2;
	public const bool DefaultFillRectangles = false;

	public static bool ShowHostileNpcHitboxes { get; set; } = true;
	public static bool ShowHostileProjectileHitboxes { get; set; } = true;
	public static bool ShowPlayerHitbox { get; set; } = true;
	public static Color HostileNpcHitboxColor { get; set; } = DefaultHostileNpcHitboxColor;
	public static Color HostileProjectileHitboxColor { get; set; } = DefaultHostileProjectileHitboxColor;
	public static Color PlayerHitboxColor { get; set; } = DefaultPlayerHitboxColor;
	public static int LineThicknessInScreenPixels { get; set; } = DefaultLineThicknessInScreenPixels;
	public static bool FillRectangles { get; set; } = DefaultFillRectangles;

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
		LineThicknessInScreenPixels = DefaultLineThicknessInScreenPixels;
		FillRectangles = DefaultFillRectangles;
	}

	private static void DrawEnemyNpcHitboxes(SpriteBatch spriteBatch)
	{
		for (int i = 0; i < Main.maxNPCs; i++) {
			NPC npc = Main.npc[i];
			if (!npc.active || !npc.CanBeChasedBy()) {
				continue;
			}

			DrawWorldRectangle(spriteBatch, npc.Hitbox, HostileNpcHitboxColor);
		}
	}

	private static void DrawHostileProjectileHitboxes(SpriteBatch spriteBatch)
	{
		for (int i = 0; i < Main.maxProjectiles; i++) {
			Projectile projectile = Main.projectile[i];
			if (!projectile.active || !projectile.hostile || projectile.damage <= 0) {
				continue;
			}

			DrawWorldRectangle(spriteBatch, projectile.Hitbox, HostileProjectileHitboxColor);
		}
	}

	private static void DrawLocalPlayerHitbox(SpriteBatch spriteBatch)
	{
		Player player = Main.LocalPlayer;
		if (!player.active || player.dead) {
			return;
		}

		DrawWorldRectangle(spriteBatch, player.Hitbox, PlayerHitboxColor);
	}

	private static void DrawWorldRectangle(SpriteBatch spriteBatch, Rectangle worldRectangle, Color color)
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

		DrawRectangle(spriteBatch, screenRectangle, color);
	}

	private static void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
	{
		Texture2D pixel = TextureAssets.MagicPixel.Value;
		int lineThickness = System.Math.Max(1, (int)System.MathF.Round(LineThicknessInScreenPixels / Main.UIScale));

		if (FillRectangles) {
			spriteBatch.Draw(pixel, rectangle, color * 0.24f);
		}

		spriteBatch.Draw(pixel, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, lineThickness), color);
		spriteBatch.Draw(pixel, new Rectangle(rectangle.Left, rectangle.Bottom - lineThickness, rectangle.Width, lineThickness), color);
		spriteBatch.Draw(pixel, new Rectangle(rectangle.Left, rectangle.Top, lineThickness, rectangle.Height), color);
		spriteBatch.Draw(pixel, new Rectangle(rectangle.Right - lineThickness, rectangle.Top, lineThickness, rectangle.Height), color);
	}
}

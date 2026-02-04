using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using System;

namespace icsf.UI;

public class DraggableUIPanel : UIPanel
{
	public float DragAreaHeight { get; set; } = 28f;

	private bool _dragging;
	private bool _wasMouseLeftDown;
	private Vector2 _dragOffset;

	public override void LeftMouseDown(UIMouseEvent evt)
	{
		base.LeftMouseDown(evt);

		CalculatedStyle dimensions = GetDimensions();
		if (evt.MousePosition.Y > dimensions.Y + DragAreaHeight) {
			return;
		}

		_dragging = true;
		_dragOffset = evt.MousePosition - new Vector2(Left.Pixels, Top.Pixels);
		Main.LocalPlayer.mouseInterface = true;
	}

	public override void LeftMouseUp(UIMouseEvent evt)
	{
		base.LeftMouseUp(evt);
		_dragging = false;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		bool mouseLeftDown = Main.mouseLeft;
		CalculatedStyle dimensions = GetDimensions();
		bool insidePanel = ContainsPoint(Main.MouseScreen);
		bool insideDragArea = insidePanel && Main.MouseScreen.Y <= dimensions.Y + DragAreaHeight;

		// Robust fallback: start dragging on the frame left mouse is pressed in the drag area,
		// even when child elements consume LeftMouseDown.
		if (!_dragging && mouseLeftDown && !_wasMouseLeftDown && insidePanel) {
			if (insideDragArea) {
				_dragging = true;
				_dragOffset = Main.MouseScreen - new Vector2(Left.Pixels, Top.Pixels);
			}
		}
		_wasMouseLeftDown = mouseLeftDown;

		if (insidePanel) {
			Main.LocalPlayer.mouseInterface = true;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_dragging) {
			if (!Main.mouseLeft) {
				_dragging = false;
			}
			else {
				float newLeft = Main.mouseX - _dragOffset.X;
				float newTop = Main.mouseY - _dragOffset.Y;
				ClampToScreen(newLeft, newTop);
				Recalculate();
			}
		}

		base.Draw(spriteBatch);
	}

	private void ClampToScreen(float desiredLeft, float desiredTop)
	{
		CalculatedStyle dimensions = GetDimensions();
		float maxX = Math.Max(0f, Main.screenWidth - dimensions.Width);
		float maxY = Math.Max(0f, Main.screenHeight - dimensions.Height);
		float clampedX = Math.Clamp(desiredLeft, 0f, maxX);
		float clampedY = Math.Clamp(desiredTop, 0f, maxY);
		Left.Set(clampedX, 0f);
		Top.Set(clampedY, 0f);
	}
}

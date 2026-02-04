using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace icsf.UI;

public class SimpleSlider : UIElement
{
	private readonly float _minValue;
	private readonly float _maxValue;
	private readonly System.Action<float> _onValueChanged;
	private float _value;
	private bool _dragging;
	private bool _wasMouseLeftDown;

	public float Value => _value;

	public SimpleSlider(float minValue, float maxValue, float initialValue, System.Action<float> onValueChanged)
	{
		_minValue = minValue;
		_maxValue = maxValue;
		_onValueChanged = onValueChanged;
		SetValue(initialValue, notify: false);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		bool mouseLeftDown = Main.mouseLeft;
		if (!_dragging && mouseLeftDown && !_wasMouseLeftDown && ContainsPoint(Main.MouseScreen)) {
			_dragging = true;
		}

		if (_dragging) {
			SetFromMouseX(Main.MouseScreen.X);
		}

		if (!mouseLeftDown) {
			_dragging = false;
		}

		_wasMouseLeftDown = mouseLeftDown;
	}

	public void SetValue(float value, bool notify)
	{
		float clamped = MathHelper.Clamp(value, _minValue, _maxValue);
		if (System.MathF.Abs(clamped - _value) < 0.001f) {
			return;
		}

		_value = clamped;
		if (notify) {
			_onValueChanged(_value);
		}
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		CalculatedStyle dimensions = GetDimensions();
		Texture2D pixel = TextureAssets.MagicPixel.Value;

		Rectangle background = new((int)dimensions.X, (int)dimensions.Y + 7, (int)dimensions.Width, 6);
		spriteBatch.Draw(pixel, background, new Color(52, 52, 52, 220));

		float percent = (_value - _minValue) / (_maxValue - _minValue);
		Rectangle fill = new((int)dimensions.X, background.Y, (int)(dimensions.Width * percent), background.Height);
		spriteBatch.Draw(pixel, fill, new Color(110, 180, 255, 230));

		int knobCenterX = (int)(dimensions.X + dimensions.Width * percent);
		Rectangle knob = new(knobCenterX - 5, (int)dimensions.Y + 2, 10, 16);
		spriteBatch.Draw(pixel, knob, new Color(235, 235, 235, 255));
	}

	private void SetFromMouseX(float mouseX)
	{
		CalculatedStyle dimensions = GetDimensions();
		if (dimensions.Width <= 0f) {
			return;
		}

		float percent = (mouseX - dimensions.X) / dimensions.Width;
		float value = _minValue + (_maxValue - _minValue) * MathHelper.Clamp(percent, 0f, 1f);
		SetValue(value, notify: true);
	}
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using icsf.Systems;

namespace icsf.UI;

public class HitboxOverlayUiState : UIState
{
	private readonly List<ToggleEntry> _toggleEntries = new();
	private readonly List<UIText> _sliderLabelTexts = new();
	private readonly List<SimpleSlider> _sliders = new();
	private DraggableUIPanel _panel = null!;
	private UIPanel _colorPreviewPanel = null!;
	private UIText _title = null!;
	private UIText _colorValueText = null!;
	private UIText _editingTargetText = null!;
	private UITextPanel<string> _closeButton = null!;
	private UITextPanel<string> _fillToggleButton = null!;
	private UITextPanel<string> _resetButton = null!;
	private SimpleSlider _redSlider = null!;
	private SimpleSlider _greenSlider = null!;
	private SimpleSlider _blueSlider = null!;
	private SimpleSlider _lineThicknessSlider = null!;
	private ColorTarget _selectedEditTarget = ColorTarget.HostileNpc;

	public override void OnInitialize()
	{
		_panel = new DraggableUIPanel {
			DragAreaHeight = 44f
		};
		_panel.Width.Set(460f, 0f);
		_panel.Height.Set(512f, 0f);
		_panel.HAlign = 0f;
		_panel.VAlign = 0f;
		_panel.Left.Set((Main.screenWidth - 460f) * 0.5f, 0f);
		_panel.Top.Set((Main.screenHeight - 512f) * 0.5f, 0f);
		Append(_panel);

		UIPanel dragBar = new UIPanel();
		dragBar.Width.Set(444f, 0f);
		dragBar.Height.Set(24f, 0f);
		dragBar.Left.Set(8f, 0f);
		dragBar.Top.Set(8f, 0f);
		dragBar.BackgroundColor = new Color(43, 54, 84, 220);
		dragBar.BorderColor = new Color(92, 109, 169, 235);
		_panel.Append(dragBar);

		_title = new UIText(string.Empty, 0.95f, true);
		_title.HAlign = 0.5f;
		_title.Top.Set(14f, 0f);
		_panel.Append(_title);

		AddToggleButton(
			top: 48f,
			labelKey: "Mods.icsf.UI.HostileNpc",
			getter: () => HitboxOverlaySystem.ShowHostileNpcHitboxes,
			setter: value => HitboxOverlaySystem.ShowHostileNpcHitboxes = value
		);

		AddToggleButton(
			top: 84f,
			labelKey: "Mods.icsf.UI.HostileProjectile",
			getter: () => HitboxOverlaySystem.ShowHostileProjectileHitboxes,
			setter: value => HitboxOverlaySystem.ShowHostileProjectileHitboxes = value
		);

		AddToggleButton(
			top: 120f,
			labelKey: "Mods.icsf.UI.Player",
			getter: () => HitboxOverlaySystem.ShowPlayerHitbox,
			setter: value => HitboxOverlaySystem.ShowPlayerHitbox = value
		);

		_editingTargetText = new UIText(string.Empty, 0.85f);
		_editingTargetText.HAlign = 0.5f;
		_editingTargetText.Top.Set(164f, 0f);
		_panel.Append(_editingTargetText);

		UITextPanel<string> changeTargetButton = new(string.Empty, 0.85f, false);
		changeTargetButton.Width.Set(280f, 0f);
		changeTargetButton.Height.Set(30f, 0f);
		changeTargetButton.HAlign = 0.5f;
		changeTargetButton.Top.Set(188f, 0f);
		changeTargetButton.OnLeftClick += (_, _) => {
			_selectedEditTarget = (ColorTarget)(((int)_selectedEditTarget + 1) % 3);
			SoundEngine.PlaySound(SoundID.MenuTick);
			RefreshText();
		};
		_panel.Append(changeTargetButton);
		_toggleEntries.Add(new ToggleEntry(changeTargetButton, "Mods.icsf.UI.ChangeEditedTarget", () => true, false));

		_colorPreviewPanel = new UIPanel();
		_colorPreviewPanel.Width.Set(36f, 0f);
		_colorPreviewPanel.Height.Set(36f, 0f);
		_colorPreviewPanel.Left.Set(30f, 0f);
		_colorPreviewPanel.Top.Set(228f, 0f);
		_panel.Append(_colorPreviewPanel);

		_colorValueText = new UIText(string.Empty, 0.85f);
		_colorValueText.Left.Set(78f, 0f);
		_colorValueText.Top.Set(238f, 0f);
		_panel.Append(_colorValueText);

		AddSliderWithLabel(
			top: 280f,
			slider: new SimpleSlider(0f, 255f, 0f, _ => ApplySelectedColorFromSliders())
		);

		AddSliderWithLabel(
			top: 318f,
			slider: new SimpleSlider(0f, 255f, 0f, _ => ApplySelectedColorFromSliders())
		);

		AddSliderWithLabel(
			top: 356f,
			slider: new SimpleSlider(0f, 255f, 0f, _ => ApplySelectedColorFromSliders())
		);

		_redSlider = _sliders[0];
		_greenSlider = _sliders[1];
		_blueSlider = _sliders[2];

		AddSliderWithLabel(
			top: 394f,
			slider: new SimpleSlider(1f, 12f, GetSelectedLineThickness(), value => {
				SetSelectedLineThickness(Math.Max(1, (int)MathF.Round(value)));
				RefreshText();
			})
		);
		_lineThicknessSlider = _sliders[3];

		_fillToggleButton = new UITextPanel<string>(string.Empty, 0.8f, false);
		_fillToggleButton.Width.Set(140f, 0f);
		_fillToggleButton.Height.Set(30f, 0f);
		_fillToggleButton.Left.Set(16f, 0f);
		_fillToggleButton.Top.Set(430f, 0f);
		_fillToggleButton.OnLeftClick += (_, _) => {
			SetSelectedFillRectangles(!GetSelectedFillRectangles());
			SoundEngine.PlaySound(SoundID.MenuTick);
			RefreshText();
		};
		_panel.Append(_fillToggleButton);

		_resetButton = new UITextPanel<string>(string.Empty, 0.8f, false);
		_resetButton.Width.Set(170f, 0f);
		_resetButton.Height.Set(30f, 0f);
		_resetButton.Left.Set(164f, 0f);
		_resetButton.Top.Set(430f, 0f);
		_resetButton.OnLeftClick += (_, _) => {
			HitboxOverlaySystem.ResetVisualSettings();
			SoundEngine.PlaySound(SoundID.MenuOpen);
			RefreshText();
		};
		_panel.Append(_resetButton);

		_closeButton = new UITextPanel<string>(string.Empty, 0.85f, false);
		_closeButton.Width.Set(110f, 0f);
		_closeButton.Height.Set(30f, 0f);
		_closeButton.Left.Set(338f, 0f);
		_closeButton.Top.Set(430f, 0f);
		_closeButton.OnLeftClick += (_, _) => {
			SoundEngine.PlaySound(SoundID.MenuClose);
			HitboxOverlayUiSystem.CloseUi();
		};
		_panel.Append(_closeButton);

		RefreshText();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (_panel.ContainsPoint(Main.MouseScreen)) {
			Main.LocalPlayer.mouseInterface = true;
		}
	}

	public void RefreshText()
	{
		_title.SetText(Language.GetTextValue("Mods.icsf.UI.HitboxOverlayTitle"));
		_closeButton.SetText(Language.GetTextValue("Mods.icsf.UI.Close"));

		foreach (ToggleEntry entry in _toggleEntries) {
			string label = Language.GetTextValue(entry.LabelKey);
			if (!entry.IsStatusToggle) {
				entry.Button.SetText(label);
				continue;
			}

			string status = entry.Getter() ? Language.GetTextValue("Mods.icsf.UI.ToggleOn") : Language.GetTextValue("Mods.icsf.UI.ToggleOff");
			entry.Button.SetText($"{label}: {status}");
		}

		string currentTargetText = Language.GetTextValue(GetColorTargetTextKey(_selectedEditTarget));
		_editingTargetText.SetText(Language.GetTextValue("Mods.icsf.UI.EditingTarget", currentTargetText));

		Color selectedColor = GetSelectedColor();
		_colorPreviewPanel.BackgroundColor = selectedColor;
		_colorValueText.SetText(Language.GetTextValue("Mods.icsf.UI.ColorValue", selectedColor.R, selectedColor.G, selectedColor.B));
		_redSlider.SetValue(selectedColor.R, notify: false);
		_greenSlider.SetValue(selectedColor.G, notify: false);
		_blueSlider.SetValue(selectedColor.B, notify: false);

		_sliderLabelTexts[0].SetText($"{Language.GetTextValue("Mods.icsf.UI.Red")} ({selectedColor.R})");
		_sliderLabelTexts[1].SetText($"{Language.GetTextValue("Mods.icsf.UI.Green")} ({selectedColor.G})");
		_sliderLabelTexts[2].SetText($"{Language.GetTextValue("Mods.icsf.UI.Blue")} ({selectedColor.B})");

		int selectedLineThickness = GetSelectedLineThickness();
		_lineThicknessSlider.SetValue(selectedLineThickness, notify: false);
		_sliderLabelTexts[3].SetText($"{Language.GetTextValue("Mods.icsf.UI.LineThickness")} ({selectedLineThickness})");

		bool selectedFillRectangles = GetSelectedFillRectangles();
		string fillStatus = selectedFillRectangles ? Language.GetTextValue("Mods.icsf.UI.ToggleOn") : Language.GetTextValue("Mods.icsf.UI.ToggleOff");
		_fillToggleButton.SetText($"{Language.GetTextValue("Mods.icsf.UI.FilledRectangle")}: {fillStatus}");
		_resetButton.SetText(Language.GetTextValue("Mods.icsf.UI.ResetVisualDefaults"));
	}

	private void AddToggleButton(float top, string labelKey, Func<bool> getter, Action<bool> setter)
	{
		UITextPanel<string> button = new(string.Empty, 0.85f, false);
		button.Width.Set(420f, 0f);
		button.Height.Set(30f, 0f);
		button.HAlign = 0.5f;
		button.Top.Set(top, 0f);
		button.OnLeftClick += (_, _) => {
			setter(!getter());
			RefreshText();
			SoundEngine.PlaySound(SoundID.MenuTick);
		};

		_panel.Append(button);
		_toggleEntries.Add(new ToggleEntry(button, labelKey, getter, true));
	}

	private void AddSliderWithLabel(float top, SimpleSlider slider)
	{
		UIText label = new(string.Empty, 0.8f);
		label.Left.Set(16f, 0f);
		label.Top.Set(top, 0f);
		_panel.Append(label);
		_sliderLabelTexts.Add(label);

		slider.Left.Set(144f, 0f);
		slider.Top.Set(top + 2f, 0f);
		slider.Width.Set(300f, 0f);
		slider.Height.Set(20f, 0f);
		_panel.Append(slider);
		_sliders.Add(slider);
	}

	private void ApplySelectedColorFromSliders()
	{
		Color color = new(
			(byte)Math.Clamp((int)MathF.Round(_redSlider.Value), 0, 255),
			(byte)Math.Clamp((int)MathF.Round(_greenSlider.Value), 0, 255),
			(byte)Math.Clamp((int)MathF.Round(_blueSlider.Value), 0, 255)
		);
		SetSelectedColor(color);
		RefreshText();
	}

	private Color GetSelectedColor()
	{
		return _selectedEditTarget switch {
			ColorTarget.HostileNpc => HitboxOverlaySystem.HostileNpcHitboxColor,
			ColorTarget.HostileProjectile => HitboxOverlaySystem.HostileProjectileHitboxColor,
			ColorTarget.Player => HitboxOverlaySystem.PlayerHitboxColor,
			_ => HitboxOverlaySystem.HostileNpcHitboxColor
		};
	}

	private void SetSelectedColor(Color color)
	{
		switch (_selectedEditTarget) {
			case ColorTarget.HostileNpc:
				HitboxOverlaySystem.HostileNpcHitboxColor = color;
				break;
			case ColorTarget.HostileProjectile:
				HitboxOverlaySystem.HostileProjectileHitboxColor = color;
				break;
			case ColorTarget.Player:
				HitboxOverlaySystem.PlayerHitboxColor = color;
				break;
		}
	}

	private int GetSelectedLineThickness()
	{
		return _selectedEditTarget switch {
			ColorTarget.HostileNpc => HitboxOverlaySystem.HostileNpcLineThicknessInScreenPixels,
			ColorTarget.HostileProjectile => HitboxOverlaySystem.HostileProjectileLineThicknessInScreenPixels,
			ColorTarget.Player => HitboxOverlaySystem.PlayerLineThicknessInScreenPixels,
			_ => HitboxOverlaySystem.HostileNpcLineThicknessInScreenPixels
		};
	}

	private void SetSelectedLineThickness(int lineThickness)
	{
		switch (_selectedEditTarget) {
			case ColorTarget.HostileNpc:
				HitboxOverlaySystem.HostileNpcLineThicknessInScreenPixels = lineThickness;
				break;
			case ColorTarget.HostileProjectile:
				HitboxOverlaySystem.HostileProjectileLineThicknessInScreenPixels = lineThickness;
				break;
			case ColorTarget.Player:
				HitboxOverlaySystem.PlayerLineThicknessInScreenPixels = lineThickness;
				break;
		}
	}

	private bool GetSelectedFillRectangles()
	{
		return _selectedEditTarget switch {
			ColorTarget.HostileNpc => HitboxOverlaySystem.HostileNpcFillRectangles,
			ColorTarget.HostileProjectile => HitboxOverlaySystem.HostileProjectileFillRectangles,
			ColorTarget.Player => HitboxOverlaySystem.PlayerFillRectangles,
			_ => HitboxOverlaySystem.HostileNpcFillRectangles
		};
	}

	private void SetSelectedFillRectangles(bool fillRectangles)
	{
		switch (_selectedEditTarget) {
			case ColorTarget.HostileNpc:
				HitboxOverlaySystem.HostileNpcFillRectangles = fillRectangles;
				break;
			case ColorTarget.HostileProjectile:
				HitboxOverlaySystem.HostileProjectileFillRectangles = fillRectangles;
				break;
			case ColorTarget.Player:
				HitboxOverlaySystem.PlayerFillRectangles = fillRectangles;
				break;
		}
	}

	private static string GetColorTargetTextKey(ColorTarget target)
	{
		return target switch {
			ColorTarget.HostileNpc => "Mods.icsf.UI.HostileNpc",
			ColorTarget.HostileProjectile => "Mods.icsf.UI.HostileProjectile",
			ColorTarget.Player => "Mods.icsf.UI.Player",
			_ => "Mods.icsf.UI.HostileNpc"
		};
	}

	private enum ColorTarget
	{
		HostileNpc,
		HostileProjectile,
		Player
	}

	private sealed class ToggleEntry
	{
		public UITextPanel<string> Button { get; }
		public string LabelKey { get; }
		public Func<bool> Getter { get; }
		public bool IsStatusToggle { get; }

		public ToggleEntry(UITextPanel<string> button, string labelKey, Func<bool> getter, bool isStatusToggle)
		{
			Button = button;
			LabelKey = labelKey;
			Getter = getter;
			IsStatusToggle = isStatusToggle;
		}
	}
}

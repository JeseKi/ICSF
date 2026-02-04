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
	private UIPanel _panel = null!;
	private UIText _title = null!;
	private UITextPanel<string> _closeButton = null!;

	public override void OnInitialize()
	{
		_panel = new UIPanel();
		_panel.Width.Set(340f, 0f);
		_panel.Height.Set(220f, 0f);
		_panel.HAlign = 0.5f;
		_panel.VAlign = 0.5f;
		Append(_panel);

		_title = new UIText(string.Empty, 0.95f, true);
		_title.HAlign = 0.5f;
		_title.Top.Set(16f, 0f);
		_panel.Append(_title);

		AddToggleButton(
			top: 54f,
			labelKey: "Mods.icsf.UI.HostileNpc",
			getter: () => HitboxOverlaySystem.ShowHostileNpcHitboxes,
			setter: value => HitboxOverlaySystem.ShowHostileNpcHitboxes = value
		);

		AddToggleButton(
			top: 94f,
			labelKey: "Mods.icsf.UI.HostileProjectile",
			getter: () => HitboxOverlaySystem.ShowHostileProjectileHitboxes,
			setter: value => HitboxOverlaySystem.ShowHostileProjectileHitboxes = value
		);

		AddToggleButton(
			top: 134f,
			labelKey: "Mods.icsf.UI.Player",
			getter: () => HitboxOverlaySystem.ShowPlayerHitbox,
			setter: value => HitboxOverlaySystem.ShowPlayerHitbox = value
		);

		_closeButton = new UITextPanel<string>(string.Empty, 0.85f, false);
		_closeButton.Width.Set(140f, 0f);
		_closeButton.Height.Set(34f, 0f);
		_closeButton.HAlign = 0.5f;
		_closeButton.Top.Set(176f, 0f);
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
			string status = entry.Getter() ? Language.GetTextValue("Mods.icsf.UI.ToggleOn") : Language.GetTextValue("Mods.icsf.UI.ToggleOff");
			entry.Button.SetText($"{label}: {status}");
		}
	}

	private void AddToggleButton(float top, string labelKey, Func<bool> getter, Action<bool> setter)
	{
		UITextPanel<string> button = new(string.Empty, 0.85f, false);
		button.Width.Set(300f, 0f);
		button.Height.Set(34f, 0f);
		button.HAlign = 0.5f;
		button.Top.Set(top, 0f);
		button.OnLeftClick += (_, _) => {
			setter(!getter());
			RefreshText();
			SoundEngine.PlaySound(SoundID.MenuTick);
		};

		_panel.Append(button);
		_toggleEntries.Add(new ToggleEntry(button, labelKey, getter));
	}

	private sealed class ToggleEntry
	{
		public UITextPanel<string> Button { get; }
		public string LabelKey { get; }
		public Func<bool> Getter { get; }

		public ToggleEntry(UITextPanel<string> button, string labelKey, Func<bool> getter)
		{
			Button = button;
			LabelKey = labelKey;
			Getter = getter;
		}
	}
}

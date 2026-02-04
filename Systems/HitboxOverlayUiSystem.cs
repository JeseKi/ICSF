using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using icsf.UI;

namespace icsf.Systems;

public class HitboxOverlayUiSystem : ModSystem
{
	internal static UserInterface UserInterface;
	internal static HitboxOverlayUiState UiState;

	public override void Load()
	{
		if (Main.dedServ) {
			return;
		}

		UserInterface = new UserInterface();
		UiState = new HitboxOverlayUiState();
		UiState.Activate();
	}

	public override void Unload()
	{
		UserInterface = null;
		UiState = null;
	}

	public override void UpdateUI(GameTime gameTime)
	{
		if (UserInterface?.CurrentState is not null && !Main.playerInventory) {
			UserInterface.SetState(null);
			return;
		}

		UserInterface?.Update(gameTime);
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		if (mouseTextIndex == -1) {
			return;
		}

		layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
			"icsf: Hitbox Overlay UI",
			DrawInterface,
			InterfaceScaleType.UI
		));
	}

	private static bool DrawInterface()
	{
		if (UserInterface?.CurrentState is not null) {
			UserInterface.Draw(Main.spriteBatch, new GameTime());
		}

		return true;
	}

	public static void ToggleUi()
	{
		if (Main.dedServ || UserInterface is null || UiState is null) {
			return;
		}

		if (UserInterface.CurrentState is null) {
			OpenUi();
		}
		else {
			CloseUi();
		}
	}

	public static void OpenUi()
	{
		if (Main.dedServ || UserInterface is null || UiState is null) {
			return;
		}

		UiState.RefreshText();
		UserInterface.SetState(UiState);
		Main.playerInventory = true;
	}

	public static void CloseUi()
	{
		if (Main.dedServ || UserInterface is null) {
			return;
		}

		UserInterface.SetState(null);
	}
}

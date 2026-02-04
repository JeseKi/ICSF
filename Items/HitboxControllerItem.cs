using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using icsf.Systems;

namespace icsf.Items;

public class HitboxControllerItem : ModItem
{
	public override string Texture => $"Terraria/Images/Item_{ItemID.MechanicalLens}";

	public override void SetDefaults()
	{
		Item.width = 20;
		Item.height = 20;
		Item.maxStack = 1;
		Item.useStyle = ItemUseStyleID.HoldUp;
		Item.useTime = 20;
		Item.useAnimation = 20;
		Item.UseSound = SoundID.MenuOpen;
		Item.rare = ItemRarityID.Blue;
		Item.value = Item.buyPrice(silver: 50);
		Item.consumable = false;
	}

	public override bool ConsumeItem(Player player) => false;

	public override bool? UseItem(Player player)
	{
		if (player.whoAmI == Main.myPlayer) {
			HitboxOverlayUiSystem.ToggleUi();
		}

		return true;
	}

	public override bool CanRightClick() => true;

	public override void RightClick(Player player)
	{
		// Some right-click flows treat right-click items as consumables; force this tool to stay.
		if (Item.type == ItemID.None) {
			Item.SetDefaults(Type);
		}

		if (Item.stack < 1) {
			Item.stack = 1;
		}

		if (player.whoAmI == Main.myPlayer) {
			HitboxOverlayUiSystem.ToggleUi();
		}
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Wood, 10)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}

using Sexy;
using Sexy.TodLib;

namespace Lawn;

internal class ConfirmPurchaseDialog : LawnDialog
{
	public StoreItem mItemType;

	public int mQuantity;

	public int mCost;

	public string mDesc = string.Empty;

	public ConfirmPurchaseDialog(LawnApp theApp, StoreItem theItemType, int aQuantity, int aCost, string theDesc)
		: base(theApp, null, 46, isModal: true, "[BUY_ITEM_HEADER]", string.Empty, string.Empty, 1)
	{
		mItemType = theItemType;
		mQuantity = aQuantity;
		mCost = aCost;
		mDesc = theDesc;
	}

	public override int GetPreferredHeight(int theWidth)
	{
		return (int)Constants.InvertAndScale(400f);
	}

	public override void Resize(int theX, int theY, int theWidth, int theHeight)
	{
		base.Resize(theX, theY, theWidth, theHeight);
	}

	public override void Draw(Graphics g)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		base.Draw(g);
		TRect confirmPurchaseDialog_Background = Constants.ConfirmPurchaseDialog_Background;
		g.SetColor(new SexyColor(171, 159, 207));
		g.SetColorizeImages(colorizeImages: true);
		LawnCommon.DrawImageBox(g, confirmPurchaseDialog_Background, AtlasResources.IMAGE_ALMANAC_PAPER);
		g.SetColorizeImages(colorizeImages: false);
		GlobalMembersStoreScreen.DrawStoreItem(g, Constants.ConfirmPurchaseDialog_Item_Pos.X, Constants.ConfirmPurchaseDialog_Item_Pos.Y, mItemType, isComingSoon: false, isSoldOut: false, mQuantity, mCost);
		confirmPurchaseDialog_Background = Constants.ConfirmPurchaseDialog_Text;
		TodStringFile.TodDrawStringWrapped(g, mDesc, confirmPurchaseDialog_Background, Resources.FONT_BRIANNETOD12, new SexyColor(40, 50, 90), DrawStringJustification.DS_ALIGN_CENTER_VERTICAL_MIDDLE);
	}
}

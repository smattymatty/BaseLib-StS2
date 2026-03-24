using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace BaseLib.Abstracts;

/// <summary>
/// A generic version of the base games Temporary Strength and Dexterity Power with small functionality improvements
/// </summary>
public abstract class CustomTemporaryPowerModel : CustomPowerModel, ITemporaryPower
{
     private const string LocTurnEndBoolVar = "UntilEndOfOtherSideTurn";

    protected abstract Func<Creature, decimal, Creature?, CardModel?, bool, Task> ApplyPowerFunc { get; }
    public abstract PowerModel InternallyAppliedPower { get; }
    public abstract AbstractModel OriginModel { get; }
    protected virtual bool UseOwnLocalization => false;
    protected virtual bool UntilEndOfOtherSideTurn => false;
    protected virtual int LastForXExtraTurns => 0;
    
    public override PowerType Type => InternallyAppliedPower.Type;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => true;
    public override bool IsInstanced => true;
    
    
    // The whole IgnoreNextInstance thing ONLY exists because of the Misery card
    // Check Misery.DoHackyThingsForSpecificPowers() for usage
    private bool _shouldIgnoreNextInstance;
    public void IgnoreNextInstance() => _shouldIgnoreNextInstance = true;
    


    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        var powerSource = this;
        if (InternallyAppliedPower is CustomTemporaryPowerModel)
        {
            // This could lead to infinite recursion if someone makes a mistake and publishes it. So just say no to any attempt.
            MainFile.Logger.Warn($"Don't put TemporaryPowerModels into a TemporaryPowerModel. Attempted to apply power '{InternallyAppliedPower.GetType().Name}' in power '{this.GetType().Name}'. Power will not be applied!");
            return;
        }
        if (_shouldIgnoreNextInstance)
        {
            _shouldIgnoreNextInstance = false;
        }
        else
        {
            DynamicVars.Repeat.BaseValue = LastForXExtraTurns;
            DynamicVars[LocTurnEndBoolVar].BaseValue = Convert.ToDecimal(UntilEndOfOtherSideTurn);
            await ApplyPowerFunc(target, amount, applier, cardSource, true);
        }
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        var powerSource = this;
        if (InternallyAppliedPower is CustomTemporaryPowerModel)
        {
            await PowerCmd.Remove(powerSource);
            return;
        }
        if ((!UntilEndOfOtherSideTurn && side != powerSource.Owner.Side) || (UntilEndOfOtherSideTurn && side == powerSource.Owner.Side))
            return;
        if (powerSource.DynamicVars.Repeat.BaseValue > 0)
        {
            powerSource.DynamicVars.Repeat.UpgradeValueBy(-1);
            return;
        }

        powerSource.Flash();
        await ApplyPowerFunc(powerSource.Owner, -powerSource.Amount, powerSource.Owner, null, true);
        await PowerCmd.Remove(powerSource);
    }


    #region Localization

    // Only used for localization purposes
    protected override IEnumerable<DynamicVar> CanonicalVars => [new RepeatVar(0), new BoolVar(LocTurnEndBoolVar, false)];

    public override LocString Title
    {
        get
        {
            switch (OriginModel)
            {
                case CardModel cardModel:
                    return cardModel.TitleLocString;
                case PotionModel potionModel:
                    return potionModel.Title;
                case RelicModel relicModel:
                    return relicModel.Title;
                case PowerModel powerModel:
                    return powerModel.Title;
                case OrbModel orbModel:
                    return orbModel.Title;
                case CharacterModel characterModel:
                    return characterModel.Title;
                case MonsterModel monsterModel:
                    return monsterModel.Title;
                default:
                    MainFile.Logger.Warn($"Getting the 'Title' for the base model type of '{OriginModel.GetType().Name}' has not been implemented yet. Using default title.");
                    return new LocString("powers",  "BASELIB-CUSTOM_TEMPORARY_POWER_MODEL.title");
            }
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            List<IHoverTip> items;
            switch (OriginModel)
            {
                case CardModel card:
                    items = [HoverTipFactory.FromCard(card)];
                    break;
                case PotionModel model:
                    items = [HoverTipFactory.FromPotion(model)];
                    break;
                case RelicModel relic:
                    items = HoverTipFactory.FromRelic(relic).ToList();
                    break;
                case PowerModel power:
                    items = [HoverTipFactory.FromPower(power)];
                    break;
                default:
                    MainFile.Logger.Warn($"Getting the Hover Tips for the base model type of '{OriginModel.GetType().Name}' has not been implemented yet.");
                    items = [];
                    break;
            }
            items.Add(HoverTipFactory.FromPower(InternallyAppliedPower));
            return items;
        }
    }

    public override LocString Description
    {
        get
        {
            if (UseOwnLocalization)
                return new LocString("powers", Amount > 0 ? $"{Id.Entry}.UP.description" : $"{Id.Entry}.DOWN.description");
            return new LocString("powers", Amount > 0 ? "BASELIB-CUSTOM_TEMPORARY_POWER_MODEL.UP.description" : "BASELIB-CUSTOM_TEMPORARY_POWER_MODEL.DOWN.description");
        }
    }
    
    protected override string SmartDescriptionLocKey
    {
        get
        {
            if (UseOwnLocalization)
                return Amount > 0 ? $"{Id.Entry}.UP.smartDescription" : $"{Id.Entry}.DOWN.smartDescription";
            return Amount > 0 ? "BASELIB-CUSTOM_TEMPORARY_POWER_MODEL.UP.smartDescription" : "BASELIB-CUSTOM_TEMPORARY_POWER_MODEL.DOWN.smartDescription";
        }
    }

    #endregion Localization

}
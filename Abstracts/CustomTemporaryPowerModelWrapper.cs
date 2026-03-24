using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace BaseLib.Abstracts;

/// <summary>
/// An ease of use wrapper for CustomTemporaryPowerModel to simplify the process
/// </summary>
/// <typeparam name="TModel">The source of the power</typeparam>
/// <typeparam name="TPower">The power that will be applied to the target</typeparam>
public abstract class CustomTemporaryPowerModelWrapper<TModel, TPower> : CustomTemporaryPowerModel  where TModel : AbstractModel where TPower : PowerModel
{
    public override string CustomBigBetaIconPath => this.Amount >= 0 ? "BaseLib/images/powers/baselib-power_temp_up.png" : "BaseLib/images/powers/baselib-power_temp_down.png";
    public override string CustomPackedIconPath => this.Amount >= 0 ? "BaseLib/images/powers/baselib-power_temp_up.png" : "BaseLib/images/powers/baselib-power_temp_down.png";
    public override string CustomBigIconPath => this.Amount >= 0 ? "BaseLib/images/powers/big/baselib-power_temp_up_big.png" : "BaseLib/images/powers/big/baselib-power_temp_down_big.png";

    public override AbstractModel OriginModel => ModelDb.GetById<AbstractModel>(ModelDb.GetId<TModel>());
    public override PowerModel InternallyAppliedPower => ModelDb.Power<TPower>();
    protected override Func<Creature, decimal, Creature?, CardModel?, bool, Task> ApplyPowerFunc => PowerCmd.Apply<TPower>;
}
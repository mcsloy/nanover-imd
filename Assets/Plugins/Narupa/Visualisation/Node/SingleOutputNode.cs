using JetBrains.Annotations;
using Narupa.Visualisation.Property;

namespace Narupa.Visualisation.Node
{
    public abstract class SingleOutputNode<TProperty> : GenericOutputNode where TProperty : IProperty, new()
    {
        [NotNull]
        protected TProperty output;

        protected override void ClearOutput()
        {
            output.UndefineValue();
        }
    }
}
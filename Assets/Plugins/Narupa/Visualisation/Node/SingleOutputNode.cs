using Narupa.Visualisation.Property;

namespace Narupa.Visualisation.Node
{
    public abstract class SingleOutputNode<TProperty> : GenericOutputNode where TProperty : IProperty, new()
    {
        protected TProperty output;

        public override void Setup()
        {
            base.Setup();
            output = new TProperty();
        }

        protected override void ClearOutput()
        {
            output.UndefineValue();
        }
    }
}
namespace DDEngine.GUI.Injection
{

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class InjectIntoControlAttribute : System.Attribute
    {
        private GUIControlID controlToInject;

        public GUIControlID ControlToInject => controlToInject;

        private static bool DoesControlIdSupportInjecting(GUIControlID control)
        {
            return ((control == GUIControlID.Menu) || (control == GUIControlID.Layer));
        }

        public InjectIntoControlAttribute(GUIControlID control)
        {
            if (!DoesControlIdSupportInjecting(control))
            {
                throw new System.NotSupportedException("The requested control does not support injecting!");
            }

            this.controlToInject = control;
        }
    }
}

namespace Framework.Runtime.UI
{
    public class DefaultUTabDisplayUnit : UListDisplayUnit
    {
        private UTMPText m_Uptxt;

        public DefaultUTabDisplayUnit(object data) : base(data)
        {
        }

        protected override void OnGUI(object data)
        {
            base.OnGUI(data);
            m_Uptxt = DisplayGO.transform.GetComponentInChild<UTMPText>("UTMText");
            if (data is string str)
            {
                m_Uptxt.text = str;
            }
        }
    }
}